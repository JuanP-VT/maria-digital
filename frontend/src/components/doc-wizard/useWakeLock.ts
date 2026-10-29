import { useEffect } from 'react'

/** Keeps the screen on while `active`. Silently does nothing where the Wake Lock API is missing. */
export function useWakeLock(active: boolean) {
  useEffect(() => {
    if (!active || !('wakeLock' in navigator)) return
    let sentinel: WakeLockSentinel | null = null
    let released = false

    async function request() {
      try {
        const next = await navigator.wakeLock.request('screen')
        if (released) void next.release()
        else sentinel = next
      } catch {
        // Denied (e.g. low battery); the wizard works without it.
      }
    }
    function onVisibility() {
      if (document.visibilityState === 'visible') void request()
    }

    void request()
    document.addEventListener('visibilitychange', onVisibility)
    return () => {
      released = true
      document.removeEventListener('visibilitychange', onVisibility)
      void sentinel?.release()
    }
  }, [active])
}
