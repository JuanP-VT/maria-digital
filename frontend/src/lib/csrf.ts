export async function getCsrfToken() {
  const response = await fetch('/api/auth/csrf', { credentials: 'include' })
  if (!response.ok) throw new Error('No se pudo iniciar una sesión segura.')
  const data = await response.json() as { requestToken: string }
  return data.requestToken
}
