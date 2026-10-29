import { describe, expect, it } from 'vitest'
import { capturedCount } from './progress'
import type { Slot } from './types'

const slot = (status: Slot['status']): Slot => ({ docType: 'x', status, qualityFlags: [] })

describe('capturedCount', () => {
  it('counts only captured slots', () => {
    expect(capturedCount([slot('captured'), slot('skipped'), slot('empty'), slot('captured')])).toBe(2)
  })

  it('returns 0 for no slots', () => {
    expect(capturedCount([])).toBe(0)
  })
})
