import { useEffect, useState, type FormEvent, type ReactNode } from 'react'
import { Church, KeyRound, LogIn } from 'lucide-react'
import { Button } from './ui/button'
import { Input } from './ui/input'
import { Label } from './ui/label'

type AdminSession = {
  displayName: string
  email: string
}

type AdminAuthGateProps = {
  children: (admin: AdminSession, onLogout: () => Promise<void>) => ReactNode
}

async function getCsrfToken() {
  const response = await fetch('/api/auth/csrf', { credentials: 'include' })
  if (!response.ok) throw new Error('No se pudo iniciar una sesión segura.')
  const data = await response.json() as { requestToken: string }
  return data.requestToken
}

export function AdminAuthGate({ children }: AdminAuthGateProps) {
  const [admin, setAdmin] = useState<AdminSession | null>(null)
  const [csrfToken, setCsrfToken] = useState('')
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true
    async function restoreSession() {
      try {
        const token = await getCsrfToken()
        if (!active) return
        setCsrfToken(token)
        const response = await fetch('/api/auth/me', { credentials: 'include' })
        if (response.ok) setAdmin(await response.json() as AdminSession)
      } catch {
        if (active) setError('No fue posible conectar con el servidor.')
      } finally {
        if (active) setLoading(false)
      }
    }
    void restoreSession()
    return () => { active = false }
  }, [])

  async function handleLogin(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSubmitting(true)
    setError('')
    try {
      const token = csrfToken || await getCsrfToken()
      setCsrfToken(token)
      const response = await fetch('/api/auth/login', {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': token },
        body: JSON.stringify({ email, password }),
      })
      if (!response.ok) {
        setError(response.status === 429 ? 'Demasiados intentos. Espera un minuto y vuelve a intentar.' : 'Correo o contraseña incorrectos.')
        return
      }
      setAdmin(await response.json() as AdminSession)
      setPassword('')
    } catch {
      setError('No fue posible conectar con el servidor.')
    } finally {
      setSubmitting(false)
    }
  }

  async function handleLogout() {
    const token = csrfToken || await getCsrfToken()
    await fetch('/api/auth/logout', {
      method: 'POST',
      credentials: 'include',
      headers: { 'X-CSRF-TOKEN': token },
    })
    setAdmin(null)
    setPassword('')
  }

  if (loading) {
    return <main className="auth-loading" aria-label="Verificando sesión"><span /></main>
  }

  if (admin) return children(admin, handleLogout)

  return (
    <main className="auth-screen">
      <div className="auth-decoration" aria-hidden="true"><span /><span /><span /></div>
      <section className="auth-panel" aria-labelledby="auth-title">
        <div className="auth-brand">
          <span className="auth-brand-mark"><Church size={22} strokeWidth={1.8} /></span>
          <span>MARIA <b>DIGITAL</b></span>
        </div>
        <p className="auth-eyebrow">ACCESO DE ADMINISTRACIÓN</p>
        <h1 id="auth-title">Bienvenido de nuevo</h1>
        <p className="auth-intro">Inicia sesión para continuar con la administración parroquial.</p>

        <form className="auth-form" onSubmit={handleLogin}>
          <div className="auth-field">
            <Label htmlFor="admin-email">Correo electrónico</Label>
            <Input id="admin-email" type="email" autoComplete="username" required value={email} onChange={(event) => setEmail(event.target.value)} placeholder="admin@parroquia.mx" />
          </div>
          <div className="auth-field">
            <Label htmlFor="admin-password">Contraseña</Label>
            <Input id="admin-password" type="password" autoComplete="current-password" required value={password} onChange={(event) => setPassword(event.target.value)} placeholder="Tu contraseña" />
          </div>
          {error && <p className="auth-error" role="alert">{error}</p>}
          <Button className="auth-submit" type="submit" disabled={submitting}>
            {submitting ? <KeyRound size={16} /> : <LogIn size={16} />}
            {submitting ? 'Verificando…' : 'Iniciar sesión'}
          </Button>
        </form>
        <p className="auth-footnote">Solo personal autorizado</p>
      </section>
    </main>
  )
}