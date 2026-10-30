import { describe, expect, it } from 'vitest'
import { sign } from './signature'

describe('sign', () => {
  // Same vector as WebhookSignatureTests in the backend, so the two sides cannot drift apart.
  it('matches the backend for the pinned vector', async () => {
    expect(await sign('0123456789abcdef0123456789abcdef', 1_700_000_000, '{}')).toBe(
      '417adf36b901772f658597a76d329694e335e65fc0849d7715c3ca33406309b5',
    )
  })

  it('changes when the body, timestamp or secret changes', async () => {
    const base = await sign('s'.repeat(32), 1, 'a')
    expect(await sign('s'.repeat(32), 1, 'b')).not.toBe(base)
    expect(await sign('s'.repeat(32), 2, 'a')).not.toBe(base)
    expect(await sign('t'.repeat(32), 1, 'a')).not.toBe(base)
  })
})
