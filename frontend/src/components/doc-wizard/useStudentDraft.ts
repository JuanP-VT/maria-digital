import { useCallback, useEffect, useReducer } from 'react'
import { idbDelete, idbGet, idbSet } from '@/lib/idb'
import { WIZARD_STEPS } from './config'
import type { Draft, QualityFlag, Slot, WizardMode } from './types'

const DRAFT_KEY = 'doc-wizard-draft'
const COUNTER_KEY = 'doc-wizard-saved-count'
const LAST_STEP = WIZARD_STEPS.length - 1

type State = { loaded: boolean; draft: Draft | null; savedCount: number }

type Action =
  | { type: 'hydrate'; draft: Draft | null; savedCount: number }
  | { type: 'start' }
  | { type: 'capture'; index: number; image: Blob; flags: QualityFlag[] }
  | { type: 'skip'; index: number }
  | { type: 'goto'; index: number }
  | { type: 'mode'; mode: WizardMode }
  | { type: 'retake'; index: number }
  | { type: 'advance'; from: number }
  | { type: 'finish' }

function emptySlots(): Slot[] {
  return WIZARD_STEPS.map((step) => ({ docType: step.id, status: 'empty', qualityFlags: [] }))
}

function newDraft(): Draft {
  return {
    student: { id: crypto.randomUUID?.() ?? String(Date.now()), createdAt: Date.now(), slots: emptySlots() },
    stepIndex: 0,
    mode: 'capture',
    returnTo: null,
  }
}

// Keep a restored draft in line with the current config (steps may have been added or reordered).
function reconcile(draft: Draft): Draft {
  const slots = WIZARD_STEPS.map(
    (step): Slot => draft.student.slots.find((slot) => slot.docType === step.id) ?? { docType: step.id, status: 'empty', qualityFlags: [] },
  )
  return { ...draft, student: { ...draft.student, slots }, stepIndex: Math.min(draft.stepIndex, LAST_STEP) }
}

function setSlot(draft: Draft, index: number, slot: Partial<Slot>): Draft {
  const slots = draft.student.slots.map((current, i) => (i === index ? { ...current, ...slot } : current))
  return { ...draft, student: { ...draft.student, slots } }
}

function reducer(state: State, action: Action): State {
  if (action.type === 'hydrate') return { loaded: true, draft: action.draft && reconcile(action.draft), savedCount: action.savedCount }
  if (action.type === 'start') return { ...state, draft: newDraft() }
  if (action.type === 'finish') return { ...state, draft: null, savedCount: state.savedCount + 1 }

  const draft = state.draft
  if (!draft) return state

  switch (action.type) {
    case 'capture':
      return { ...state, draft: setSlot(draft, action.index, { status: 'captured', image: action.image, qualityFlags: action.flags }) }
    case 'skip':
      return { ...state, draft: setSlot(draft, action.index, { status: 'skipped', image: undefined, qualityFlags: [] }) }
    case 'goto':
      return { ...state, draft: { ...draft, stepIndex: action.index } }
    case 'mode':
      return { ...state, draft: { ...draft, mode: action.mode, returnTo: null } }
    case 'retake':
      return { ...state, draft: { ...draft, stepIndex: action.index, mode: 'capture', returnTo: 'preview' } }
    case 'advance': {
      if (draft.stepIndex !== action.from) return state
      if (draft.returnTo || draft.stepIndex >= LAST_STEP) return { ...state, draft: { ...draft, mode: 'preview', returnTo: null } }
      return { ...state, draft: { ...draft, stepIndex: draft.stepIndex + 1 } }
    }
  }
}

export function useStudentDraft() {
  const [state, dispatch] = useReducer(reducer, { loaded: false, draft: null, savedCount: 0 })

  useEffect(() => {
    let active = true
    Promise.all([idbGet<Draft>(DRAFT_KEY), idbGet<number>(COUNTER_KEY)])
      .then(([draft, savedCount]) => active && dispatch({ type: 'hydrate', draft: draft ?? null, savedCount: savedCount ?? 0 }))
      .catch(() => active && dispatch({ type: 'hydrate', draft: null, savedCount: 0 }))
    return () => { active = false }
  }, [])

  useEffect(() => {
    if (!state.loaded) return
    const write = state.draft ? idbSet(DRAFT_KEY, state.draft) : idbDelete(DRAFT_KEY)
    write.catch((error) => console.warn('No se pudo guardar el borrador del estudiante.', error))
  }, [state.loaded, state.draft])

  useEffect(() => {
    if (!state.loaded) return
    idbSet(COUNTER_KEY, state.savedCount).catch(() => undefined)
  }, [state.loaded, state.savedCount])

  const start = useCallback(() => dispatch({ type: 'start' }), [])
  const capture = useCallback((index: number, image: Blob, flags: QualityFlag[]) => dispatch({ type: 'capture', index, image, flags }), [])
  const skip = useCallback((index: number) => dispatch({ type: 'skip', index }), [])
  const goTo = useCallback((index: number) => dispatch({ type: 'goto', index }), [])
  const setMode = useCallback((mode: WizardMode) => dispatch({ type: 'mode', mode }), [])
  const retake = useCallback((index: number) => dispatch({ type: 'retake', index }), [])
  const advance = useCallback((from: number) => dispatch({ type: 'advance', from }), [])
  const finish = useCallback(() => dispatch({ type: 'finish' }), [])

  return {
    loaded: state.loaded,
    draft: state.draft,
    studentNumber: state.savedCount + 1,
    start, capture, skip, goTo, setMode, retake, advance, finish,
  }
}
