import { cn } from 'cn'
import { Ban, Check, X } from 'lucide-react'
import { Button } from '@/components/ui/button'
import type { WizardStep } from './config'
import { capturedCount } from './progress'
import type { Slot } from './types'
import { useObjectUrl } from './useObjectUrl'

type StepHeaderProps = {
  steps: WizardStep[]
  slots: Slot[]
  current: number
  studentNumber: number
  onSelect: (index: number) => void
  onExit: () => void
}

function Thumb({ slot, index }: { slot: Slot; index: number }) {
  const url = useObjectUrl(slot.image)
  if (slot.status === 'captured' && url) {
    return (
      <span className="relative block size-full">
        <img className="size-full rounded-[inherit] object-cover" src={url} alt="" />
        <span className="absolute -right-1 -bottom-1 grid size-4 place-items-center rounded-full bg-primary text-primary-foreground">
          <Check size={10} strokeWidth={3} />
        </span>
      </span>
    )
  }
  if (slot.status === 'skipped') return <Ban size={16} className="text-secondary" aria-hidden="true" />
  return <span className="text-sm font-semibold">{index + 1}</span>
}

export function StepHeader({ steps, slots, current, studentNumber, onSelect, onExit }: StepHeaderProps) {
  return (
    <header className="z-10 bg-black/60 px-3 pt-[max(0.5rem,env(safe-area-inset-top))] pb-2 text-white backdrop-blur-sm">
      <div className="mb-2 flex items-center gap-2">
        <Button variant="ghost" size="icon" className="text-white hover:bg-white/15 hover:text-white" aria-label="Salir" onClick={onExit}>
          <X />
        </Button>
        <p className="m-0 text-sm font-medium">Estudiante {studentNumber} · {capturedCount(slots)} de {steps.length} capturados</p>
      </div>
      <ol className="m-0 flex list-none gap-2 p-0">
        {steps.map((step, index) => {
          const slot = slots[index]
          return (
            <li className="min-w-0 flex-1" key={step.id}>
              <button
                type="button"
                className={cn(
                  'flex w-full flex-col items-center gap-1 rounded-lg p-1.5 text-center outline-none focus-visible:ring-2 focus-visible:ring-ring',
                  index === current ? 'bg-white/25' : 'hover:bg-white/10',
                )}
                aria-current={index === current ? 'step' : undefined}
                aria-label={`${step.label}: ${slot.status === 'captured' ? 'capturado' : slot.status === 'skipped' ? 'no disponible' : 'pendiente'}`}
                onClick={() => onSelect(index)}
              >
                <span
                  className={cn(
                    'grid h-11 w-9 place-items-center rounded-md border',
                    slot.status === 'skipped' ? 'border-dashed border-secondary/70' : 'border-white/40',
                    index === current && 'border-white',
                  )}
                >
                  <Thumb slot={slot} index={index} />
                </span>
                <span className="w-full truncate text-[10px] leading-tight">{step.label}</span>
              </button>
            </li>
          )
        })}
      </ol>
    </header>
  )
}
