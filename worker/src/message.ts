export type ParsedMessage =
  | { kind: 'confirm'; key: string; size: number }
  | { kind: 'ignore' }
  | { kind: 'invalid' }

// R2 event notifications: https://developers.cloudflare.com/r2/buckets/event-notifications/
const CREATE_ACTIONS = new Set(['PutObject', 'CopyObject', 'CompleteMultipartUpload'])

// Presigned uploads always write under this prefix (StudentDocument.BuildStorageKey).
export const KEY_PREFIX = 'submissions/'

export function parseMessage(body: unknown): ParsedMessage {
  if (typeof body !== 'object' || body === null) return { kind: 'invalid' }
  const { action, object } = body as { action?: unknown; object?: unknown }
  if (typeof action !== 'string') return { kind: 'invalid' }
  if (!CREATE_ACTIONS.has(action)) return { kind: 'ignore' }

  if (typeof object !== 'object' || object === null) return { kind: 'invalid' }
  const { key, size } = object as { key?: unknown; size?: unknown }
  if (typeof key !== 'string' || key.length === 0) return { kind: 'invalid' }
  if (typeof size !== 'number' || !Number.isSafeInteger(size) || size < 0) {
    return { kind: 'invalid' }
  }
  if (!key.startsWith(KEY_PREFIX)) return { kind: 'ignore' }

  return { kind: 'confirm', key, size }
}
