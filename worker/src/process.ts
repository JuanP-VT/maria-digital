import { parseMessage } from './message'
import { classifyStatus, retryDelaySeconds } from './retry'
import { sign } from './signature'

export const CONFIRM_PATH = '/api/internal/uploads/confirmed'

// A sleeping free-tier backend can take a while to boot; a timeout is retried like any failure.
export const REQUEST_TIMEOUT_MS = 30_000

export interface Deps {
  fetch: typeof fetch
  apiBaseUrl: string
  secret: string
  now: () => number
  random: () => number
}

export type Outcome = { action: 'ack' } | { action: 'retry'; delaySeconds: number }

/**
 * Decides what to do with one queue message. The backend deduplicates by object key, so this keeps
 * no state of its own and is safe to run any number of times for the same message.
 */
export async function processMessage(
  body: unknown,
  attempts: number,
  deps: Deps,
): Promise<Outcome> {
  const parsed = parseMessage(body)
  if (parsed.kind === 'ignore') return { action: 'ack' }
  if (parsed.kind === 'invalid') {
    // Retrying cannot repair a message we cannot read.
    console.error('Dropping unreadable queue message', JSON.stringify(body))
    return { action: 'ack' }
  }

  const retry: Outcome = {
    action: 'retry',
    delaySeconds: retryDelaySeconds(attempts, deps.random),
  }

  const payload = JSON.stringify({ key: parsed.key, size: parsed.size })
  const timestamp = Math.floor(deps.now() / 1000)
  try {
    const response = await deps.fetch(deps.apiBaseUrl.replace(/\/+$/, '') + CONFIRM_PATH, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-Timestamp': String(timestamp),
        'X-Signature': await sign(deps.secret, timestamp, payload),
      },
      body: payload,
      signal: AbortSignal.timeout(REQUEST_TIMEOUT_MS),
    })
    if (classifyStatus(response.status) === 'ack') return { action: 'ack' }
    console.warn(`Backend answered ${response.status} for ${parsed.key} (attempt ${attempts})`)
    return retry
  } catch (error) {
    console.warn(`Could not reach backend for ${parsed.key} (attempt ${attempts})`, error)
    return retry
  }
}
