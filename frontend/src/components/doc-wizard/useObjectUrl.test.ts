import { renderHook } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { useObjectUrl } from './useObjectUrl'

describe('useObjectUrl', () => {
  beforeEach(() => vi.useFakeTimers())
  afterEach(() => vi.useRealTimers())

  it('returns undefined without a blob', () => {
    const { result } = renderHook(() => useObjectUrl(undefined))
    expect(result.current).toBeUndefined()
    expect(URL.createObjectURL).not.toHaveBeenCalled()
  })

  it('shares one URL per blob and revokes it after the last user unmounts', () => {
    const blob = new Blob(['a'])
    const first = renderHook(() => useObjectUrl(blob))
    const second = renderHook(() => useObjectUrl(blob))
    expect(first.result.current).toBe(second.result.current)
    expect(URL.createObjectURL).toHaveBeenCalledTimes(1)

    first.unmount()
    vi.runAllTimers()
    expect(URL.revokeObjectURL).not.toHaveBeenCalled()

    second.unmount()
    vi.runAllTimers()
    expect(URL.revokeObjectURL).toHaveBeenCalledWith(second.result.current)
  })
})
