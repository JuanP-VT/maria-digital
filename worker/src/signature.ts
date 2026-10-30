const encoder = new TextEncoder()

// Must match WebhookSignature.Sign in the backend: hex HMAC-SHA256 of "{timestamp}.{body}".
export async function sign(secret: string, timestamp: number, body: string): Promise<string> {
  const key = await crypto.subtle.importKey(
    'raw',
    encoder.encode(secret),
    { name: 'HMAC', hash: 'SHA-256' },
    false,
    ['sign'],
  )
  const mac = await crypto.subtle.sign('HMAC', key, encoder.encode(`${timestamp}.${body}`))
  return [...new Uint8Array(mac)].map((byte) => byte.toString(16).padStart(2, '0')).join('')
}
