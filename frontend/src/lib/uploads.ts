import { getCsrfToken } from '@/lib/csrf'

export type UploadFile = { type: string; blob: Blob }

type PresignedFile = { type: string; url: string; headers: Record<string, string> }
type PresignResponse = { submissionId: string; uploads: PresignedFile[] }

export function buildPresignRequest(files: UploadFile[]) {
  return {
    files: files.map(({ type, blob }) => ({ type, contentType: blob.type, sizeBytes: blob.size })),
  }
}

async function presignErrorMessage(response: Response) {
  if (response.status === 429) return 'Demasiados intentos. Espera un momento e inténtalo de nuevo.'
  if (response.status === 401) return 'Tu sesión expiró. Inicia sesión de nuevo.'
  const body = await response.json().catch(() => null) as { errors?: string[] } | null
  return body?.errors?.join(' ') || 'No se pudieron preparar los archivos.'
}

const OFFLINE_MESSAGE = 'Sin conexión. Revisa tu internet e inténtalo de nuevo.'

// A rejected fetch means the request never reached the server (offline, blocked, CORS).
async function request(input: string, init: RequestInit) {
  try {
    return await fetch(input, init)
  } catch {
    throw new Error(OFFLINE_MESSAGE)
  }
}

// Asks the API for short-lived upload URLs, then sends the files straight to storage in parallel.
export async function uploadDocuments(files: UploadFile[], onProgress?: (done: number, total: number) => void) {
  const csrfToken = await getCsrfToken()
  const presign = await request('/api/uploads/presign', {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': csrfToken },
    body: JSON.stringify(buildPresignRequest(files)),
  })
  if (!presign.ok) throw new Error(await presignErrorMessage(presign))
  const { submissionId, uploads } = await presign.json() as PresignResponse

  let done = 0
  onProgress?.(done, uploads.length)
  await Promise.all(
    uploads.map(async (upload) => {
      const file = files.find((f) => f.type === upload.type)
      if (!file) throw new Error('Respuesta inesperada del servidor.')
      // Storage is another origin: no cookies. Content-Length is set by the browser from the body.
      const put = await request(upload.url, { method: 'PUT', headers: upload.headers, body: file.blob })
      if (!put.ok) throw new Error('No se pudo subir un archivo. Inténtalo de nuevo.')
      onProgress?.(++done, uploads.length)
    }),
  )
  return { submissionId }
}
