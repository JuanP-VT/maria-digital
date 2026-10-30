import { useState } from 'react'
import { toast } from 'sonner'
import { Skeleton } from '@/components/ui/skeleton'
import { CaptureWizard } from './CaptureWizard'
import { PreviewScreen } from './PreviewScreen'
import { saveStudent, type UploadProgress } from './saveStudent'
import { StartScreen } from './StartScreen'
import type { Student } from './types'
import { useStudentDraft } from './useStudentDraft'
import { useWakeLock } from './useWakeLock'

type DocumentWizardPageProps = {
  onSave?: (student: Student, onProgress: (progress: UploadProgress) => void) => unknown | Promise<unknown>
}

function exitWizard() {
  window.location.hash = ''
}

export function DocumentWizardPage({ onSave = saveStudent }: DocumentWizardPageProps) {
  const wizard = useStudentDraft()
  const [saving, setSaving] = useState(false)
  const [failed, setFailed] = useState(false)
  const [progress, setProgress] = useState<UploadProgress | null>(null)
  const { draft } = wizard

  useWakeLock(draft !== null)

  async function handleSave() {
    if (!draft) return
    setSaving(true)
    setFailed(false)
    setProgress(null)
    try {
      await onSave(draft.student, setProgress)
      toast.success('Estudiante guardado')
      wizard.finish()
    } catch (error) {
      setFailed(true)
      toast.error(error instanceof Error ? error.message : 'No se pudo guardar el estudiante. Intenta de nuevo.')
    } finally {
      setSaving(false)
      setProgress(null)
    }
  }

  if (!wizard.loaded) {
    return (
      <div className="fixed inset-0 z-40 grid place-items-center bg-background" aria-label="Cargando">
        <Skeleton className="h-12 w-56" />
      </div>
    )
  }

  if (!draft) return <StartScreen onStart={wizard.start} onExit={exitWizard} />

  if (draft.mode === 'preview') {
    return (
      <PreviewScreen
        slots={draft.student.slots}
        studentNumber={wizard.studentNumber}
        saving={saving}
        failed={failed}
        progress={progress}
        onRetake={wizard.retake}
        onSave={() => void handleSave()}
        onExit={exitWizard}
      />
    )
  }

  return (
    <CaptureWizard
      draft={draft}
      studentNumber={wizard.studentNumber}
      onCapture={wizard.capture}
      onSkip={wizard.skip}
      onGoTo={wizard.goTo}
      onAdvance={wizard.advance}
      onExit={exitWizard}
    />
  )
}
