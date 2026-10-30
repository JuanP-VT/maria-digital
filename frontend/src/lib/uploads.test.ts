import { afterEach, describe, expect, it, vi } from 'vitest'
import { buildPresignRequest, uploadDocuments } from './uploads'

const json = (body: unknown, status = 200) => new Response(JSON.stringify(body), { status })
const jpeg = (size: number) => new Blob([new Uint8Array(size)], { type: 'image/jpeg' })

function mockFetch(handlers: Record<string, (init?: RequestInit) => Response>) {
  const fetchMock = vi.fn((url: string, init?: RequestInit) =>
    Promise.resolve(handlers[url]?.(init) ?? new Response(null, { status: 404 })))
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

const presigned = {
  submissionId: 's1',
  uploads: [{ type: 'birth', url: 'https://storage.test/k?sig', headers: { 'Content-Type': 'image/jpeg' } }],
}

describe('uploads', () => {
  afterEach(() => vi.unstubAllGlobals())

  it('describes each blob by type, content type and size, never by a key', () => {
    expect(buildPresignRequest([{ type: 'birth', blob: jpeg(10) }])).toEqual({
      files: [{ type: 'birth', contentType: 'image/jpeg', sizeBytes: 10 }],
    })
  })

  it('presigns with the CSRF token, then PUTs the blob with the returned headers and no cookies', async () => {
    const fetchMock = mockFetch({
      '/api/auth/csrf': () => json({ requestToken: 'tok' }),
      '/api/uploads/presign': () => json(presigned),
      'https://storage.test/k?sig': () => new Response(null, { status: 200 }),
    })
    const blob = jpeg(10)

    const result = await uploadDocuments([{ type: 'birth', blob }])

    expect(result).toEqual({ submissionId: 's1' })
    const presignInit = fetchMock.mock.calls.find((c) => c[0] === '/api/uploads/presign')![1]!
    expect(presignInit.headers).toMatchObject({ 'X-CSRF-TOKEN': 'tok' })
    const putInit = fetchMock.mock.calls.find((c) => c[0] === 'https://storage.test/k?sig')![1]!
    expect(putInit).toMatchObject({ method: 'PUT', headers: { 'Content-Type': 'image/jpeg' }, body: blob })
    expect(putInit.credentials).toBeUndefined()
  })

  it('shows the server validation messages', async () => {
    mockFetch({
      '/api/auth/csrf': () => json({ requestToken: 't' }),
      '/api/uploads/presign': () => json({ errors: ['Archivo muy grande.', 'Otro error.'] }, 400),
    })

    await expect(uploadDocuments([{ type: 'birth', blob: jpeg(1) }])).rejects.toThrow('Archivo muy grande. Otro error.')
  })

  it.each([[429, /demasiados/i], [401, /sesión/i]])('maps status %i to a clear message', async (status, message) => {
    mockFetch({
      '/api/auth/csrf': () => json({ requestToken: 't' }),
      '/api/uploads/presign': () => new Response(null, { status }),
    })

    await expect(uploadDocuments([{ type: 'birth', blob: jpeg(1) }])).rejects.toThrow(message)
  })

  it('reports progress as each file finishes', async () => {
    mockFetch({
      '/api/auth/csrf': () => json({ requestToken: 't' }),
      '/api/uploads/presign': () => json({
        submissionId: 's1',
        uploads: [
          { type: 'birth', url: 'https://storage.test/a', headers: {} },
          { type: 'baptism', url: 'https://storage.test/b', headers: {} },
        ],
      }),
      'https://storage.test/a': () => new Response(null, { status: 200 }),
      'https://storage.test/b': () => new Response(null, { status: 200 }),
    })
    const onProgress = vi.fn()

    await uploadDocuments([{ type: 'birth', blob: jpeg(1) }, { type: 'baptism', blob: jpeg(1) }], onProgress)

    expect(onProgress.mock.calls).toEqual([[0, 2], [1, 2], [2, 2]])
  })

  it('shows an offline message when a request never reaches the server', async () => {
    vi.stubGlobal('fetch', vi.fn((url: string) =>
      url === '/api/auth/csrf' ? Promise.resolve(json({ requestToken: 't' })) : Promise.reject(new TypeError('Failed to fetch'))))

    await expect(uploadDocuments([{ type: 'birth', blob: jpeg(1) }])).rejects.toThrow(/sin conexión/i)
  })

  it('fails when storage rejects the upload', async () => {
    mockFetch({
      '/api/auth/csrf': () => json({ requestToken: 't' }),
      '/api/uploads/presign': () => json(presigned),
      'https://storage.test/k?sig': () => new Response(null, { status: 403 }),
    })

    await expect(uploadDocuments([{ type: 'birth', blob: jpeg(1) }])).rejects.toThrow(/subir/i)
  })
})
