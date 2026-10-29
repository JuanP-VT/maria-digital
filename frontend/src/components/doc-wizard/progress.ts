import type { Slot } from './types'

export function capturedCount(slots: Slot[]) {
  return slots.filter((slot) => slot.status === 'captured').length
}
