import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { AdminAuthGate } from './AdminAuthGate'

const json = (body: unknown, status = 200) => new Response(JSON.stringify(body), { status })
const admin = { displayName: 'Padre Juan', email: 'a@b.mx' }

function mockFetch(routes: Record<string, () => Response>) {
  const fetchMock = vi.fn((...args: Parameters<typeof fetch>) => {
    const handler = routes[String(args[0])]
    return Promise.resolve(handler ? handler() : new Response(null, { status: 404 }))
  })
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

async function signIn(password: string) {
  await userEvent.type(await screen.findByLabelText('Correo electrónico'), 'a@b.mx')
  await userEvent.type(screen.getByLabelText('Contraseña'), password)
  await userEvent.click(screen.getByRole('button', { name: /iniciar sesión/i }))
}

describe('AdminAuthGate', () => {
  afterEach(() => vi.unstubAllGlobals())

  it('shows the children when a session already exists', async () => {
    mockFetch({ '/api/auth/csrf': () => json({ requestToken: 't' }), '/api/auth/me': () => json(admin) })
    render(<AdminAuthGate>{(a) => <p>hola {a.displayName}</p>}</AdminAuthGate>)
    expect(await screen.findByText('hola Padre Juan')).toBeInTheDocument()
  })

  it('shows the login form and signs in with the CSRF token', async () => {
    const fetchMock = mockFetch({
      '/api/auth/csrf': () => json({ requestToken: 't' }),
      '/api/auth/me': () => json({}, 401),
      '/api/auth/login': () => json(admin),
    })
    render(<AdminAuthGate>{(a) => <p>hola {a.displayName}</p>}</AdminAuthGate>)
    await signIn('secret')
    expect(await screen.findByText('hola Padre Juan')).toBeInTheDocument()

    const login = fetchMock.mock.calls.find(([url]) => url === '/api/auth/login')!
    expect(login[1]?.headers).toMatchObject({ 'X-CSRF-TOKEN': 't' })
  })

  it('shows an error on bad credentials', async () => {
    mockFetch({
      '/api/auth/csrf': () => json({ requestToken: 't' }),
      '/api/auth/me': () => json({}, 401),
      '/api/auth/login': () => json({}, 401),
    })
    render(<AdminAuthGate>{() => <p>dentro</p>}</AdminAuthGate>)
    await signIn('bad')
    await waitFor(() => expect(screen.getByRole('alert')).toHaveTextContent('Correo o contraseña incorrectos.'))
  })

  it('shows a rate-limit message on 429', async () => {
    mockFetch({
      '/api/auth/csrf': () => json({ requestToken: 't' }),
      '/api/auth/me': () => json({}, 401),
      '/api/auth/login': () => json({}, 429),
    })
    render(<AdminAuthGate>{() => <p>dentro</p>}</AdminAuthGate>)
    await signIn('x')
    await waitFor(() => expect(screen.getByRole('alert')).toHaveTextContent('Demasiados intentos'))
  })
})
