import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import worker, { type Env } from './index'

const env: Env = {
  API_BASE_URL: 'https://api.example.org',
  UPLOAD_WEBHOOK_SECRET: '0123456789abcdef0123456789abcdef',
}

const put = (key: string) => ({ action: 'PutObject', object: { key, size: 10 } })

function fakeMessage(body: unknown, attempts = 1) {
  return { body, attempts, ack: vi.fn(), retry: vi.fn() }
}

function run(messages: ReturnType<typeof fakeMessage>[], e: Env = env) {
  return worker.queue({ messages } as unknown as MessageBatch<unknown>, e)
}

describe('queue handler', () => {
  beforeEach(() => {
    vi.spyOn(console, 'warn').mockImplementation(() => {})
    vi.spyOn(console, 'error').mockImplementation(() => {})
  })
  afterEach(() => vi.restoreAllMocks())

  it('settles every message on its own: one failure does not replay the others', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn(async (_url: string, init: RequestInit) =>
        String(init.body).includes('bad')
          ? new Response(null, { status: 503 })
          : new Response(null, { status: 204 }),
      ),
    )
    const good = fakeMessage(put('submissions/a/birth/good'))
    const bad = fakeMessage(put('submissions/a/birth/bad'), 2)
    const ignored = fakeMessage({ action: 'DeleteObject', object: { key: 'x', size: 0 } })

    await run([good, bad, ignored])

    expect(good.ack).toHaveBeenCalledOnce()
    expect(good.retry).not.toHaveBeenCalled()
    expect(bad.ack).not.toHaveBeenCalled()
    expect(bad.retry).toHaveBeenCalledWith({ delaySeconds: expect.any(Number) })
    expect(ignored.ack).toHaveBeenCalledOnce()
  })

  it('retries when processing throws unexpectedly', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn(async () => {
        throw new Error('boom')
      }),
    )
    const message = fakeMessage(put('submissions/a/birth/x'))

    await run([message])

    expect(message.retry).toHaveBeenCalledOnce()
    expect(message.ack).not.toHaveBeenCalled()
  })

  it('retries everything, without calling the backend, when it is not configured', async () => {
    const fetchMock = vi.fn()
    vi.stubGlobal('fetch', fetchMock)
    const message = fakeMessage(put('submissions/a/birth/x'))

    await run([message], { ...env, UPLOAD_WEBHOOK_SECRET: '' })

    expect(fetchMock).not.toHaveBeenCalled()
    expect(message.retry).toHaveBeenCalledOnce()
    expect(message.ack).not.toHaveBeenCalled()
  })
})
