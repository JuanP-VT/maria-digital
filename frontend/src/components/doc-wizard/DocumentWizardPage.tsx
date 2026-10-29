import { useState } from 'react'
import { toast } from 'sonner'
import { Skeleton } from '@/components/ui/skeleton'
import { CaptureWizard } from './CaptureWizard'
import { PreviewScreen } from './PreviewScreen'
import { StartScreen } from './StartScreen'
import type { Student } from './types'
import { useStudentDraft } from './useStudentDraft'
import { useWakeLock } from './useWakeLock'

type DocumentWizardPageProps = {
  onSave?: (student: Student) => void | Promise<void>
}

// Stub until the upload endpoint exists.
function saveStudentStub(student: Student) {
  console.info('Estudiante guardado (simulado)', student)
  toast.success('Estudiante guardado')
}

function exitWizard() {
  window.location.hash = ''
}

export function DocumentWizardPage({ onSave = saveStudentStub }: DocumentWizardPageProps) {
  const wizard = useStudentDraft()
  const [saving, setSaving] = useState(false)
  const { draft } = wizard

  useWakeLock(draft !== null)

  async function handleSave() {
    if (!draft) return
    setSaving(true)
    try {
      await onSave(draft.student)
      wizard.finish()
    } catch {
      toast.error('No se pudo guardar el estudiante. Intenta de nuevo.')
    } finally {
      setSaving(false)
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
