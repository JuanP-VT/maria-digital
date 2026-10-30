import { useState } from 'react'
import { cn } from 'cn'
import { Ban, Camera, ImageOff, Loader2, RotateCcw, TriangleAlert, X } from 'lucide-react'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { WIZARD_STEPS, type WizardStep } from './config'
import { ImageViewer } from './ImageViewer'
import { capturedCount, missingRequiredSteps, saveButtonLabel } from './progress'
import type { Slot } from './types'
import { useObjectUrl } from './useObjectUrl'

type PreviewScreenProps = {
  slots: Slot[]
  studentNumber: number
  saving: boolean
  failed?: boolean
  progress?: { done: number; total: number } | null
  onRetake: (index: number) => void
  onSave: () => void
  onExit: () => void
}

type CellProps = { step: WizardStep; slot: Slot; disabled: boolean; onOpen: (url: string) => void; onRetake: () => void }

function Cell({ step, slot, disabled, onOpen, onRetake }: CellProps) {
  const url = useObjectUrl(slot.image)
  const missing = slot.status !== 'captured'

  return (
    <li className="flex min-w-0 flex-col gap-2">
      {slot.status === 'captured' && url ? (
        <button
          type="button"
          className="relative aspect-[3/4] overflow-hidden rounded-lg border border-border bg-muted outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
          aria-label={`Ver ${step.label} a pantalla completa`}
          onClick={() => onOpen(url)}
        >
          <img className="size-full object-cover" src={url} alt={step.label} />
          {slot.qualityFlags.length > 0 && (
            <Badge variant="secondary" className="absolute top-1.5 left-1.5 gap-1">
              <TriangleAlert /> {slot.qualityFlags.includes('blur') ? 'Borrosa' : 'Oscura'}
            </Badge>
          )}
        </button>
      ) : (
        <div
          className={cn(
            'flex aspect-[3/4] flex-col items-center justify-center gap-1.5 rounded-lg border-2 p-2 text-center text-xs',
            slot.status === 'skipped'
              ? 'border-solid border-secondary bg-secondary/60 text-secondary-foreground'
              : 'border-dashed border-destructive/60 bg-muted text-muted-foreground',
          )}
        >
          {slot.status === 'skipped' ? <Ban size={22} aria-hidden="true" /> : <ImageOff size={22} aria-hidden="true" />}
          <strong className="text-sm">{slot.status === 'skipped' ? 'No disponible' : 'Falta'}</strong>
          {slot.status === 'empty' && step.required && <span className="text-destructive">Obligatorio</span>}
        </div>
      )}

      <div className="flex items-center justify-between gap-1">
        <span className="min-w-0 truncate text-sm font-medium">{step.label}</span>
      </div>
      <Button variant="outline" className="h-10" disabled={disabled} onClick={onRetake}>
        {missing ? <Camera /> : <RotateCcw />}
        {missing ? 'Capturar' : 'Repetir'}
      </Button>
    </li>
  )
}

export function PreviewScreen({ slots, studentNumber, saving, failed = false, progress = null, onRetake, onSave, onExit }: PreviewScreenProps) {
  const [viewing, setViewing] = useState<{ url: string; title: string } | null>(null)
  const [confirmOpen, setConfirmOpen] = useState(false)

  const missingRequired = missingRequiredSteps(WIZARD_STEPS, slots)

  function handleSave() {
    if (missingRequired.length > 0) setConfirmOpen(true)
    else onSave()
  }

  return (
    <div className="fixed inset-0 z-40 flex flex-col bg-background text-foreground">
      <header className="flex items-center gap-2 border-b border-border px-3 pt-[max(0.5rem,env(safe-area-inset-top))] pb-2">
        <Button variant="ghost" size="icon" aria-label="Salir" disabled={saving} onClick={onExit}>
          <X />
        </Button>
        <div className="min-w-0">
          <h1 className="m-0 font-heading text-lg leading-tight">Revisar documentos</h1>
          <p className="m-0 text-xs text-muted-foreground">Estudiante {studentNumber} · {capturedCount(slots)} de {WIZARD_STEPS.length} capturados</p>
        </div>
      </header>

      <main className="min-h-0 flex-1 overflow-y-auto px-4 py-4" aria-busy={saving}>
        <ul className="m-0 mx-auto grid max-w-md list-none grid-cols-2 gap-3 p-0">
          {WIZARD_STEPS.map((step, index) => (
            <Cell
              key={step.id}
              step={step}
              slot={slots[index]}
              disabled={saving}
              onOpen={(url) => setViewing({ url, title: step.label })}
              onRetake={() => onRetake(index)}
            />
          ))}
        </ul>
      </main>

      <footer className="border-t border-border bg-card px-4 pt-3 pb-[max(0.75rem,env(safe-area-inset-bottom))]">
        {saving && (
          <div
            className="mx-auto mb-3 h-1.5 w-full max-w-md overflow-hidden rounded-full bg-muted"
            role="progressbar"
            aria-label="Subiendo documentos"
            aria-valuemin={0}
            aria-valuemax={progress?.total ?? 0}
            aria-valuenow={progress?.done ?? 0}
          >
            <div
              className="h-full rounded-full bg-primary transition-[width] duration-300 ease-out"
              style={{ width: `${progress && progress.total > 0 ? Math.max(8, (progress.done / progress.total) * 100) : 8}%` }}
            />
          </div>
        )}
        <Button className="mx-auto flex h-12 w-full max-w-md text-base" disabled={saving} onClick={handleSave}>
          {saving && <Loader2 className="animate-spin" aria-hidden="true" />}
          {saveButtonLabel(saving, failed, progress)}
        </Button>
      </footer>

      {viewing && <ImageViewer src={viewing.url} title={viewing.title} onClose={() => setViewing(null)} />}

      <AlertDialog open={confirmOpen} onOpenChange={setConfirmOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Faltan documentos obligatorios</AlertDialogTitle>
            <AlertDialogDescription>
              Falta: {missingRequired.map((step) => step.label).join(', ')}. ¿Guardar de todas formas?
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Seguir revisando</AlertDialogCancel>
            <AlertDialogAction onClick={() => { setConfirmOpen(false); onSave() }}>Guardar de todas formas</AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  )
}
