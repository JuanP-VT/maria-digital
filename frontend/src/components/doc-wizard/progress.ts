import type { WizardStep } from './config'
import type { QualityFlag, Slot } from './types'

export function capturedCount(slots: Slot[]) {
  return slots.filter((slot) => slot.status === 'captured').length
}

/** Required steps that have no captured photo (skipped counts as missing). */
export function missingRequiredSteps(steps: WizardStep[], slots: Slot[]) {
  return steps.filter((step, index) => step.required && slots[index]?.status !== 'captured')
}

export function describeFlags(flags: QualityFlag[]) {
  if (flags.includes('blur') && flags.includes('dark')) return 'La foto se ve borrosa y oscura.'
  return flags.includes('blur') ? 'La foto se ve borrosa.' : 'La foto se ve oscura.'
}
