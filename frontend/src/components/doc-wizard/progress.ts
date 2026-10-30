import type { UploadFile } from '@/lib/uploads'
import type { WizardStep } from './config'
import type { QualityFlag, Slot } from './types'

export function capturedCount(slots: Slot[]) {
  return slots.filter((slot) => slot.status === 'captured').length
}

/** Required steps that have no captured photo (skipped counts as missing). */
export function missingRequiredSteps(steps: WizardStep[], slots: Slot[]) {
  return steps.filter((step, index) => step.required && slots[index]?.status !== 'captured')
}

/** Photos to upload: captured slots only; skipped and empty ones are left out. */
export function uploadFilesFromSlots(slots: Slot[]): UploadFile[] {
  return slots.flatMap((slot) => (slot.status === 'captured' && slot.image ? [{ type: slot.docType, blob: slot.image }] : []))
}

export function saveButtonLabel(saving: boolean, failed: boolean, progress: { done: number; total: number } | null) {
  if (saving) return progress && progress.total > 0 ? `Subiendo ${Math.min(progress.done + 1, progress.total)} de ${progress.total}…` : 'Preparando…'
  return failed ? 'Reintentar' : 'Guardar estudiante'
}

export function describeFlags(flags: QualityFlag[]) {
  if (flags.includes('blur') && flags.includes('dark')) return 'La foto se ve borrosa y oscura.'
  return flags.includes('blur') ? 'La foto se ve borrosa.' : 'La foto se ve oscura.'
}
