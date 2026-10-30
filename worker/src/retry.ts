export type Verdict = 'ack' | 'retry'

// Only a 2xx settles a message. Everything else is retried, including 401/403 (a wrong secret is
// fixed by a deploy, and the message must still be there) and 404 (the row is created before the
// URL is issued, but a replica can lag). Exhausted retries land in the dead-letter queue.
export function classifyStatus(status: number): Verdict {
  return status >= 200 && status < 300 ? 'ack' : 'retry'
}

const BASE_DELAY_SECONDS = 5
const MAX_DELAY_SECONDS = 600

/**
 * Exponential backoff (5s, 15s, 45s, ... capped at 10 min) with jitter between 50% and 100% so a
 * burst of failures does not come back as one burst. `attempts` is Cloudflare's 1-based count.
 * The long tail gives a sleeping free-tier backend time to wake up.
 */
export function retryDelaySeconds(attempts: number, random: () => number = Math.random): number {
  const step = Math.max(0, Math.floor(attempts) - 1)
  const delay = Math.min(MAX_DELAY_SECONDS, BASE_DELAY_SECONDS * 3 ** step)
  return Math.max(1, Math.round(delay * (0.5 + random() * 0.5)))
}
