import { useEffect, useRef, useState, type ChangeEvent } from 'react'
import { TriangleAlert } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { fileToPhoto, frameToPhoto, type ProcessedPhoto } from '@/lib/image'
import { toast } from 'sonner'
import { WIZARD_STEPS } from './config'
import { describeFlags } from './progress'
import { StepHeader } from './StepHeader'
import type { Draft, QualityFlag } from './types'
import { useCamera } from './useCamera'
import { useObjectUrl } from './useObjectUrl'
import { Viewfinder } from './Viewfinder'

const FREEZE_MS = 600

type CaptureWizardProps = {
  draft: Draft
  studentNumber: number
  onCapture: (index: number, image: Blob, flags: QualityFlag[]) => void
  onSkip: (index: number) => void
  onGoTo: (index: number) => void
  onAdvance: (from: number) => void
  onExit: () => void
}

export function CaptureWizard({ draft, studentNumber, onCapture, onSkip, onGoTo, onAdvance, onExit }: CaptureWizardProps) {
  const { stepIndex } = draft
  const step = WIZARD_STEPS[stepIndex]
  const camera = useCamera(true)
  const videoRef = useRef<HTMLVideoElement>(null)
  const fileInputRef = useRef<HTMLInputElement>(null)
  const timerRef = useRef<number | undefined>(undefined)
  const [busy, setBusy] = useState(false)
  const [frozen, setFrozen] = useState<Blob>()
  const [warning, setWarning] = useState<{ index: number; flags: QualityFlag[] } | null>(null)
  const frozenUrl = useObjectUrl(frozen)
  const activeWarning = warning?.index === stepIndex ? warning : null

  useEffect(() => () => window.clearTimeout(timerRef.current), [])

  function handlePhoto({ blob, flags }: ProcessedPhoto) {
    const index = stepIndex
    onCapture(index, blob, flags)
    navigator.vibrate?.(30)
    setFrozen(blob)
    setWarning(flags.length ? { index, flags } : null)
    window.clearTimeout(timerRef.current)
    timerRef.current = window.setTimeout(() => {
      setFrozen(undefined)
      if (flags.length === 0) onAdvance(index)
    }, FREEZE_MS)
  }

  async function run(getPhoto: () => Promise<ProcessedPhoto>) {
    setBusy(true)
    try {
      handlePhoto(await getPhoto())
    } catch {
      toast.error('No se pudo procesar la foto. Intenta de nuevo.')
    } finally {
      setBusy(false)
    }
  }

  function handleShutter() {
    if (camera.status !== 'ready') return fileInputRef.current?.click()
    const video = videoRef.current
    if (!video || !video.videoWidth) return
    void run(() => frameToPhoto(video))
  }

  function handleFile(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0]
    event.target.value = ''
    if (file) void run(() => fileToPhoto(file))
  }

  function handleSkip() {
    onSkip(stepIndex)
    onAdvance(stepIndex)
  }

  return (
    <div className="fixed inset-0 z-40 flex flex-col bg-black">
      <StepHeader
        steps={WIZARD_STEPS}
        slots={draft.student.slots}
        current={stepIndex}
        studentNumber={studentNumber}
        onSelect={onGoTo}
        onExit={onExit}
      />

      <Viewfinder
        videoRef={videoRef}
        stream={camera.stream}
        status={camera.status}
        hintText={step.hintText}
        torchSupported={camera.torchSupported}
        torchOn={camera.torchOn}
        busy={busy}
        onToggleTorch={() => void camera.toggleTorch()}
        onShutter={handleShutter}
        onSkip={handleSkip}
      >
        {frozenUrl && (
          <div className="absolute inset-0 z-10 grid place-items-center bg-black/80">
            <img className="max-h-[70%] max-w-[80%] rounded-md border-2 border-white object-contain" src={frozenUrl} alt={`Foto de ${step.label}`} />
          </div>
        )}

        {activeWarning && (
          <div className="absolute inset-x-3 bottom-32 z-20 flex flex-col gap-2 rounded-lg bg-card p-3 text-card-foreground shadow-lg" role="alert">
            <p className="m-0 flex items-center gap-2 text-sm font-medium">
              <TriangleAlert size={16} className="shrink-0 text-highlight" aria-hidden="true" />
              {describeFlags(activeWarning.flags)} Conviene repetirla.
            </p>
            <div className="flex gap-2">
              <Button className="h-10 flex-1" onClick={() => { setWarning(null); setFrozen(undefined) }}>Repetir</Button>
              <Button variant="outline" className="h-10 flex-1" onClick={() => { setWarning(null); onAdvance(activeWarning.index) }}>Continuar</Button>
            </div>
          </div>
        )}
      </Viewfinder>

      <input ref={fileInputRef} className="hidden" type="file" accept="image/*" capture="environment" onChange={handleFile} />
    </div>
  )
}
