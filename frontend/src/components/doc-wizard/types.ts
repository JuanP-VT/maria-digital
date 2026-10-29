export type SlotStatus = 'empty' | 'captured' | 'skipped'
export type QualityFlag = 'blur' | 'dark'

export type Slot = {
  docType: string
  status: SlotStatus
  image?: Blob
  qualityFlags: QualityFlag[]
}

export type Student = {
  id: string
  createdAt: number
  slots: Slot[]
}

export type WizardMode = 'capture' | 'preview'

export type Draft = {
  student: Student
  stepIndex: number
  mode: WizardMode
  /** When set, the next capture/skip returns here instead of advancing to the next step. */
  returnTo: WizardMode | null
}
