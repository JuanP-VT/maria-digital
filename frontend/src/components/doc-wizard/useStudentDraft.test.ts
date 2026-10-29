import { act, renderHook, waitFor } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { idbGet, idbSet } from '@/lib/idb'
import { capturedCount } from './progress'
import { WIZARD_STEPS } from './config'
import { useStudentDraft } from './useStudentDraft'

async function ready() {
  const hook = renderHook(() => useStudentDraft())
  await waitFor(() => expect(hook.result.current.loaded).toBe(true))
  return hook
}

describe('useStudentDraft', () => {
  it('starts with no draft and student number 1', async () => {
    const { result } = await ready()
    expect(result.current.draft).toBeNull()
    expect(result.current.studentNumber).toBe(1)
  })

  it('start creates an empty draft with one slot per step', async () => {
    const { result } = await ready()
    act(() => result.current.start())
    expect(result.current.draft?.student.slots).toHaveLength(WIZARD_STEPS.length)
    expect(capturedCount(result.current.draft!.student.slots)).toBe(0)
    expect(result.current.draft?.stepIndex).toBe(0)
  })

  it('capture and skip update the slot', async () => {
    const { result } = await ready()
    act(() => result.current.start())
    act(() => result.current.capture(0, new Blob(['x']), ['blur']))
    act(() => result.current.skip(1))
    const [first, second] = result.current.draft!.student.slots
    expect(first).toMatchObject({ status: 'captured', qualityFlags: ['blur'] })
    expect(second).toMatchObject({ status: 'skipped', qualityFlags: [] })
  })

  it('advance moves to the next step and ignores stale calls', async () => {
    const { result } = await ready()
    act(() => result.current.start())
    act(() => result.current.advance(0))
    expect(result.current.draft?.stepIndex).toBe(1)
    act(() => result.current.advance(0))
    expect(result.current.draft?.stepIndex).toBe(1)
  })

  it('advance from the last step switches to preview', async () => {
    const { result } = await ready()
    act(() => result.current.start())
    const last = WIZARD_STEPS.length - 1
    act(() => result.current.goTo(last))
    act(() => result.current.advance(last))
    expect(result.current.draft?.mode).toBe('preview')
  })

  it('retake returns to preview after the next advance', async () => {
    const { result } = await ready()
    act(() => result.current.start())
    act(() => result.current.retake(0))
    expect(result.current.draft).toMatchObject({ stepIndex: 0, mode: 'capture', returnTo: 'preview' })
    act(() => result.current.advance(0))
    expect(result.current.draft).toMatchObject({ mode: 'preview', returnTo: null })
  })

  it('finish clears the draft and bumps the student number', async () => {
    const { result } = await ready()
    act(() => result.current.start())
    act(() => result.current.finish())
    expect(result.current.draft).toBeNull()
    expect(result.current.studentNumber).toBe(2)
  })

  it('restores a saved draft and reconciles it with the current steps', async () => {
    await idbSet('doc-wizard-draft', {
      student: { id: 'abc', createdAt: 1, slots: [] },
      stepIndex: 999,
      mode: 'capture',
      returnTo: null,
    })
    const { result } = await ready()
    expect(result.current.draft?.student.id).toBe('abc')
    expect(result.current.draft?.student.slots).toHaveLength(WIZARD_STEPS.length)
    expect(result.current.draft?.stepIndex).toBe(WIZARD_STEPS.length - 1)
  })

  it('persists the draft to IndexedDB', async () => {
    const { result } = await ready()
    act(() => result.current.start())
    await waitFor(async () => expect(await idbGet('doc-wizard-draft')).toBeDefined())
  })
})
