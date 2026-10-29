import { useRef, useState, type PointerEvent, type WheelEvent } from 'react'
import { X } from 'lucide-react'
import { Button } from '@/components/ui/button'

const MIN_SCALE = 1
const MAX_SCALE = 6
const DOUBLE_TAP_MS = 300

type Transform = { scale: number; x: number; y: number }
type Point = { x: number; y: number }

const clamp = (value: number, min: number, max: number) => Math.min(max, Math.max(min, value))
const distance = (a: Point, b: Point) => Math.hypot(a.x - b.x, a.y - b.y)

type ImageViewerProps = { src: string; title: string; onClose: () => void }

/** Full-screen photo with pinch-zoom, drag-to-pan and double-tap zoom (Pointer Events, no library). */
export function ImageViewer({ src, title, onClose }: ImageViewerProps) {
  const [transform, setTransform] = useState<Transform>({ scale: 1, x: 0, y: 0 })
  const pointers = useRef(new Map<number, Point>())
  const pinch = useRef<{ startDistance: number; startScale: number } | null>(null)
  const lastTap = useRef(0)

  function update(next: Transform) {
    const scale = clamp(next.scale, MIN_SCALE, MAX_SCALE)
    setTransform(scale === MIN_SCALE ? { scale, x: 0, y: 0 } : { ...next, scale })
  }

  function handleDown(event: PointerEvent<HTMLDivElement>) {
    event.currentTarget.setPointerCapture(event.pointerId)
    pointers.current.set(event.pointerId, { x: event.clientX, y: event.clientY })
    if (pointers.current.size === 2) {
      const [a, b] = [...pointers.current.values()]
      pinch.current = { startDistance: distance(a, b), startScale: transform.scale }
    } else if (pointers.current.size === 1) {
      const now = Date.now()
      if (now - lastTap.current < DOUBLE_TAP_MS) update(transform.scale > 1 ? { scale: 1, x: 0, y: 0 } : { scale: 2.5, x: 0, y: 0 })
      lastTap.current = now
    }
  }

  function handleMove(event: PointerEvent<HTMLDivElement>) {
    const previous = pointers.current.get(event.pointerId)
    if (!previous) return
    const current = { x: event.clientX, y: event.clientY }
    pointers.current.set(event.pointerId, current)

    if (pointers.current.size === 2 && pinch.current) {
      const [a, b] = [...pointers.current.values()]
      update({ ...transform, scale: pinch.current.startScale * (distance(a, b) / pinch.current.startDistance) })
    } else if (pointers.current.size === 1 && transform.scale > 1) {
      update({ ...transform, x: transform.x + current.x - previous.x, y: transform.y + current.y - previous.y })
    }
  }

  function handleUp(event: PointerEvent<HTMLDivElement>) {
    pointers.current.delete(event.pointerId)
    pinch.current = null
  }

  function handleWheel(event: WheelEvent<HTMLDivElement>) {
    update({ ...transform, scale: transform.scale * (event.deltaY < 0 ? 1.15 : 1 / 1.15) })
  }

  return (
    <div className="fixed inset-0 z-50 flex flex-col bg-black text-white" role="dialog" aria-modal="true" aria-label={title}>
      <div className="flex items-center justify-between gap-2 px-3 pt-[max(0.5rem,env(safe-area-inset-top))] pb-2">
        <p className="m-0 truncate text-sm font-medium">{title}</p>
        <Button variant="ghost" size="icon" className="text-white hover:bg-white/15 hover:text-white" aria-label="Cerrar" onClick={onClose}>
          <X />
        </Button>
      </div>
      <div
        className="min-h-0 flex-1 touch-none overflow-hidden select-none"
        onPointerDown={handleDown}
        onPointerMove={handleMove}
        onPointerUp={handleUp}
        onPointerCancel={handleUp}
        onWheel={handleWheel}
      >
        <img
          className="size-full object-contain will-change-transform"
          style={{ transform: `translate(${transform.x}px, ${transform.y}px) scale(${transform.scale})` }}
          src={src}
          alt={title}
          draggable={false}
        />
      </div>
      <p className="m-0 px-3 pt-2 pb-[max(0.75rem,env(safe-area-inset-bottom))] text-center text-xs text-white/70">Pellizca o toca dos veces para acercar</p>
    </div>
  )
}
