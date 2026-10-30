import { useEffect, type ReactNode, type RefObject } from 'react'
import { cn } from 'cn'
import { Camera, CameraOff, Zap, ZapOff } from 'lucide-react'
import { Button } from '@/components/ui/button'
import type { CameraStatus } from './useCamera'

type ViewfinderProps = {
  videoRef: RefObject<HTMLVideoElement | null>
  stream: MediaStream | null
  status: CameraStatus
  hintText: string
  torchSupported: boolean
  torchOn: boolean
  busy: boolean
  onToggleTorch: () => void
  onShutter: () => void
  onSkip: () => void
  children?: ReactNode
}

export function Viewfinder({ videoRef, stream, status, hintText, torchSupported, torchOn, busy, onToggleTorch, onShutter, onSkip, children }: ViewfinderProps) {
  useEffect(() => {
    const video = videoRef.current
    if (!video) return
    video.srcObject = stream
    if (stream) void video.play().catch(() => undefined)
  }, [videoRef, stream])

  const fallback = status === 'denied' || status === 'unsupported'

  return (
    <div className="relative min-h-0 flex-1 overflow-hidden bg-black text-white">
      <video
        ref={videoRef}
        className={cn('absolute inset-0 size-full object-cover', !stream && 'hidden')}
        playsInline
        muted
        autoPlay
      />

      {stream && (
        <div className="pointer-events-none absolute inset-0 grid place-items-center [container-type:size]" aria-hidden="true">
          {/* The photo is cropped to this rectangle (lib/guideCrop.ts): 68% tall, A4 ratio, at most 88% wide. */}
          <div className="aspect-[1/1.414] h-[min(68cqh,124.4cqw)] rounded-md border-2 border-white/90 shadow-[0_0_0_9999px_rgba(0,0,0,0.45)]" />
        </div>
      )}

      {fallback && (
        <div className="absolute inset-0 grid place-content-center justify-items-center gap-3 px-8 text-center">
          <CameraOff size={36} strokeWidth={1.5} aria-hidden="true" />
          <p className="m-0 text-sm">
            {status === 'denied'
              ? 'No hay acceso a la cámara. Se abrirá la cámara del teléfono para cada documento.'
              : 'Este navegador no permite usar la cámara aquí. Se abrirá la cámara del teléfono para cada documento.'}
          </p>
        </div>
      )}

      {status === 'starting' && (
        <p className="absolute inset-0 m-0 grid place-items-center text-sm text-white/80">Iniciando cámara…</p>
      )}

      <p className="absolute inset-x-3 top-3 m-0 rounded-lg bg-black/55 px-3 py-2 text-center text-sm backdrop-blur-sm" aria-live="polite">
        {hintText}
      </p>

      {children}

      <div className="absolute inset-x-0 bottom-0 flex items-center justify-between gap-3 px-5 pt-4 pb-[max(1.25rem,env(safe-area-inset-bottom))] bg-gradient-to-t from-black/70 to-transparent">
        <Button
          variant="ghost"
          className="h-11 min-w-24 text-white hover:bg-white/15 hover:text-white"
          onClick={onSkip}
        >
          No disponible
        </Button>

        <button
          type="button"
          className="grid size-[76px] shrink-0 place-items-center rounded-full border-4 border-white bg-white/20 outline-none transition-transform active:scale-95 focus-visible:ring-4 focus-visible:ring-ring disabled:opacity-50"
          aria-label="Tomar foto"
          disabled={busy || status === 'starting'}
          onClick={onShutter}
        >
          {fallback ? <Camera size={28} aria-hidden="true" /> : <span className="size-14 rounded-full bg-white" />}
        </button>

        <div className="flex min-w-24 justify-end">
          {torchSupported && (
            <Button
              variant="ghost"
              size="icon-lg"
              className="size-11 text-white hover:bg-white/15 hover:text-white"
              aria-label={torchOn ? 'Apagar linterna' : 'Encender linterna'}
              aria-pressed={torchOn}
              onClick={onToggleTorch}
            >
              {torchOn ? <Zap /> : <ZapOff />}
            </Button>
          )}
        </div>
      </div>
    </div>
  )
}
