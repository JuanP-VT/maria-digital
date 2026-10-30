import { describe, expect, it } from 'vitest'
import type { WizardStep } from './config'
import { capturedCount, describeFlags, missingRequiredSteps, saveButtonLabel, uploadFilesFromSlots } from './progress'
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

describe('uploadFilesFromSlots', () => {
  const blob = new Blob(['x'], { type: 'image/jpeg' })

  it('keeps captured slots with their doc type and drops skipped, empty and imageless ones', () => {
    const slots: Slot[] = [
      { docType: 'registration', status: 'captured', image: blob, qualityFlags: [] },
      { docType: 'birth', status: 'skipped', qualityFlags: [] },
      { docType: 'baptism', status: 'empty', qualityFlags: [] },
      { docType: 'address', status: 'captured', qualityFlags: [] },
    ]
    expect(uploadFilesFromSlots(slots)).toEqual([{ type: 'registration', blob }])
  })
})

describe('saveButtonLabel', () => {
  it('shows the file being uploaded, never past the total', () => {
    expect(saveButtonLabel(true, false, null)).toBe('Preparando…')
    expect(saveButtonLabel(true, false, { done: 0, total: 3 })).toBe('Subiendo 1 de 3…')
    expect(saveButtonLabel(true, false, { done: 3, total: 3 })).toBe('Subiendo 3 de 3…')
  })

  it('offers a retry after a failure and the normal label otherwise', () => {
    expect(saveButtonLabel(false, true, null)).toBe('Reintentar')
    expect(saveButtonLabel(false, false, null)).toBe('Guardar estudiante')
  })
})

describe('describeFlags', () => {
  it('describes each combination of quality flags', () => {
    expect(describeFlags(['blur'])).toBe('La foto se ve borrosa.')
    expect(describeFlags(['dark'])).toBe('La foto se ve oscura.')
    expect(describeFlags(['dark', 'blur'])).toBe('La foto se ve borrosa y oscura.')
  })
})
