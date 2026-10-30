import { describe, expect, it, vi } from 'vitest'
import { NOTHING_CAPTURED_MESSAGE, saveStudent } from './saveStudent'
import type { Student } from './types'

const blob = new Blob(['x'], { type: 'image/jpeg' })
const student = (slots: Student['slots']): Student => ({ id: '1', createdAt: 0, slots })

describe('saveStudent', () => {
  it('uploads only the captured photos and forwards progress', async () => {
    const upload = vi.fn(async (_files, onProgress?: (done: number, total: number) => void) => {
      onProgress?.(1, 1)
      return { submissionId: 's1' }
    })
    const onProgress = vi.fn()

    const result = await saveStudent(
      student([
        { docType: 'registration', status: 'captured', image: blob, qualityFlags: [] },
        { docType: 'birth', status: 'skipped', qualityFlags: [] },
      ]),
      onProgress,
      upload,
    )

    expect(result).toEqual({ submissionId: 's1' })
    expect(upload.mock.calls[0][0]).toEqual([{ type: 'registration', blob }])
    expect(onProgress).toHaveBeenCalledWith({ done: 1, total: 1 })
  })

  it('refuses to upload when nothing was captured', async () => {
    const upload = vi.fn()
    await expect(
      saveStudent(student([{ docType: 'birth', status: 'skipped', qualityFlags: [] }]), vi.fn(), upload),
    ).rejects.toThrow(NOTHING_CAPTURED_MESSAGE)
    expect(upload).not.toHaveBeenCalled()
  })

  it('lets upload errors through unchanged', async () => {
    const upload = vi.fn().mockRejectedValue(new Error('Sin conexión.'))
    await expect(
      saveStudent(student([{ docType: 'birth', status: 'captured', image: blob, qualityFlags: [] }]), vi.fn(), upload),
    ).rejects.toThrow('Sin conexión.')
  })
})
