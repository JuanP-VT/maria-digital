import { describe, expect, it } from 'vitest'
import type { WizardStep } from './config'
import { capturedCount, describeFlags, missingRequiredSteps } from './progress'
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

describe('missingRequiredSteps', () => {
  const steps: WizardStep[] = [
    { id: 'a', label: 'A', hintText: '', required: true },
    { id: 'b', label: 'B', hintText: '', required: true },
    { id: 'c', label: 'C', hintText: '', required: false },
  ]

  it('lists required steps without a captured photo, including skipped ones', () => {
    const missing = missingRequiredSteps(steps, [slot('captured'), slot('skipped'), slot('empty')])
    expect(missing.map((step) => step.id)).toEqual(['b'])
  })

  it('ignores optional steps', () => {
    expect(missingRequiredSteps(steps, [slot('captured'), slot('captured'), slot('empty')])).toEqual([])
  })

  it('treats a missing slot as missing', () => {
    expect(missingRequiredSteps(steps, [slot('captured')]).map((step) => step.id)).toEqual(['b'])
  })
})

describe('describeFlags', () => {
  it('describes each combination of quality flags', () => {
    expect(describeFlags(['blur'])).toBe('La foto se ve borrosa.')
    expect(describeFlags(['dark'])).toBe('La foto se ve oscura.')
    expect(describeFlags(['dark', 'blur'])).toBe('La foto se ve borrosa y oscura.')
  })
})
