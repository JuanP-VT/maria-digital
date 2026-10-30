import { uploadDocuments } from '@/lib/uploads'
import { uploadFilesFromSlots } from './progress'
import type { Student } from './types'

export type UploadProgress = { done: number; total: number }

export const NOTHING_CAPTURED_MESSAGE = 'Captura al menos un documento antes de guardar.'

/** Uploads the captured photos of a student. Throws a Spanish message when it cannot. */
export async function saveStudent(
  student: Student,
  onProgress: (progress: UploadProgress) => void,
  upload: typeof uploadDocuments = uploadDocuments,
) {
  const files = uploadFilesFromSlots(student.slots)
  if (files.length === 0) throw new Error(NOTHING_CAPTURED_MESSAGE)
  return upload(files, (done, total) => onProgress({ done, total }))
}
