import { renderHook, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { useWakeLock } from './useWakeLock'

function stubWakeLock() {
  const release = vi.fn().mockResolvedValue(undefined)
  const request = vi.fn().mockResolvedValue({ release })
  Object.defineProperty(navigator, 'wakeLock', { value: { request }, configurable: true })
  return { request, release }
}

describe('useWakeLock', () => {
  afterEach(() => {
    Reflect.deleteProperty(navigator, 'wakeLock')
  })

  it('requests a screen lock while active and releases it on unmount', async () => {
    const { request, release } = stubWakeLock()
    const { unmount } = renderHook(() => useWakeLock(true))
    await waitFor(() => expect(request).toHaveBeenCalledWith('screen'))
    await Promise.resolve()
    unmount()
    expect(release).toHaveBeenCalled()
  })

  it('does nothing when inactive', () => {
    const { request } = stubWakeLock()
    renderHook(() => useWakeLock(false))
    expect(request).not.toHaveBeenCalled()
  })

  it('does not throw where the API is missing', () => {
    expect(() => renderHook(() => useWakeLock(true))).not.toThrow()
  })
})
