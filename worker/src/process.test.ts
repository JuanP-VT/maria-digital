import { describe, expect, it, vi } from 'vitest'
import { CONFIRM_PATH, processMessage, type Deps } from './process'
import { sign } from './signature'

const SECRET = '0123456789abcdef0123456789abcdef'
const KEY = 'submissions/abc/registration/def'
const body = { action: 'PutObject', object: { key: KEY, size: 5000 } }

function deps(fetchImpl: (...args: Parameters<typeof fetch>) => Promise<Response>) {
  const fetchMock = vi.fn(fetchImpl)
  const value: Deps = {
    fetch: fetchMock as unknown as typeof fetch,
    apiBaseUrl: 'https://api.example.org/',
    secret: SECRET,
    now: () => 1_700_000_000_500,
    random: () => 1,
  }
  return { value, fetchMock }
}

describe('processMessage', () => {
  it('posts a signed confirmation and acks on success', async () => {
    const { value, fetchMock } = deps(async () => new Response(null, { status: 204 }))

    const outcome = await processMessage(body, 1, value)

    expect(outcome).toEqual({ action: 'ack' })
    const [url, init] = fetchMock.mock.calls[0]!
    expect(url).toBe(`https://api.example.org${CONFIRM_PATH}`)
    const payload = JSON.stringify({ key: KEY, size: 5000 })
    expect(init?.body).toBe(payload)
    expect(init?.method).toBe('POST')
    const headers = init?.headers as Record<string, string>
    expect(headers['X-Timestamp']).toBe('1700000000')
    expect(headers['X-Signature']).toBe(await sign(SECRET, 1_700_000_000, payload))
  })

  it.each([404, 429, 500, 503])('retries with backoff when the backend answers %i', async (status) => {
    const { value } = deps(async () => new Response(null, { status }))

    expect(await processMessage(body, 2, value)).toEqual({ action: 'retry', delaySeconds: 15 })
  })

  it('retries when the backend is unreachable', async () => {
    const { value } = deps(async () => {
      throw new TypeError('fetch failed')
    })

    expect(await processMessage(body, 1, value)).toEqual({ action: 'retry', delaySeconds: 5 })
  })

  it('retries when the request times out (backend still waking up)', async () => {
    const { value } = deps(async () => {
      throw new DOMException('The operation timed out', 'TimeoutError')
    })

    expect(await processMessage(body, 3, value)).toEqual({ action: 'retry', delaySeconds: 45 })
  })

  it('passes a timeout signal to fetch', async () => {
    const { value, fetchMock } = deps(async () => new Response(null, { status: 204 }))

    await processMessage(body, 1, value)

    expect(fetchMock.mock.calls[0]![1]?.signal).toBeInstanceOf(AbortSignal)
  })

  it('acks without calling the backend for events it does not care about', async () => {
    const { value, fetchMock } = deps(async () => new Response(null, { status: 204 }))

    expect(await processMessage({ action: 'DeleteObject', object: { key: KEY, size: 0 } }, 1, value))
      .toEqual({ action: 'ack' })
    expect(fetchMock).not.toHaveBeenCalled()
  })

  it('acks unreadable messages instead of retrying them forever', async () => {
    const error = vi.spyOn(console, 'error').mockImplementation(() => {})
    const { value, fetchMock } = deps(async () => new Response(null, { status: 204 }))

    expect(await processMessage('garbage', 1, value)).toEqual({ action: 'ack' })
    expect(fetchMock).not.toHaveBeenCalled()
    error.mockRestore()
  })

  it('sends the same request every time, so redelivery is harmless', async () => {
    const { value, fetchMock } = deps(async () => new Response(null, { status: 204 }))

    await processMessage(body, 1, value)
    await processMessage(body, 4, value)

    expect(fetchMock.mock.calls[0]![1]?.body).toBe(fetchMock.mock.calls[1]![1]?.body)
  })
})
