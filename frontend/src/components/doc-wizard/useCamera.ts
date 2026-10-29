import { useCallback, useEffect, useState } from 'react'

export type CameraStatus = 'starting' | 'ready' | 'denied' | 'unsupported'

// `torch` is not part of the standard TS media-track types yet.
type TorchCapabilities = MediaTrackCapabilities & { torch?: boolean }

const supported = typeof navigator !== 'undefined' && !!navigator.mediaDevices?.getUserMedia

/** Owns the rear-camera stream while `active`, so it survives step changes inside the wizard. */
export function useCamera(active: boolean) {
  const [stream, setStream] = useState<MediaStream | null>(null)
  const [denied, setDenied] = useState(false)
  const [torchSupported, setTorchSupported] = useState(false)
  const [torchOn, setTorchOn] = useState(false)

  useEffect(() => {
    if (!active || !supported) return
    let cancelled = false
    let acquired: MediaStream | null = null

    navigator.mediaDevices
      .getUserMedia({
        audio: false,
        video: { facingMode: { ideal: 'environment' }, width: { ideal: 4096 }, height: { ideal: 2160 } },
      })
      .then((next) => {
        if (cancelled) return next.getTracks().forEach((track) => track.stop())
        acquired = next
        const capabilities = next.getVideoTracks()[0]?.getCapabilities?.() as TorchCapabilities | undefined
        setTorchSupported(!!capabilities?.torch)
        setStream(next)
      })
      .catch(() => {
        if (!cancelled) setDenied(true)
      })

    return () => {
      cancelled = true
      acquired?.getTracks().forEach((track) => track.stop())
      setStream(null)
      setDenied(false)
      setTorchSupported(false)
      setTorchOn(false)
    }
  }, [active])

  const toggleTorch = useCallback(async () => {
    const track = stream?.getVideoTracks()[0]
    if (!track) return
    const next = !torchOn
    try {
      await track.applyConstraints({ advanced: [{ torch: next } as MediaTrackConstraintSet] })
      setTorchOn(next)
    } catch {
      setTorchSupported(false)
    }
  }, [stream, torchOn])

  const status: CameraStatus = !supported ? 'unsupported' : denied ? 'denied' : stream ? 'ready' : 'starting'
  return { stream, status, torchSupported, torchOn, toggleTorch }
}
