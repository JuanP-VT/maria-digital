import { act, renderHook } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { useHashRoute } from './useHashRoute'

describe('useHashRoute', () => {
  it('follows the URL hash', () => {
    window.location.hash = '#/one'
    const { result } = renderHook(() => useHashRoute())
    expect(result.current).toBe('#/one')

    act(() => {
      window.location.hash = '#/two'
      window.dispatchEvent(new HashChangeEvent('hashchange'))
    })
    expect(result.current).toBe('#/two')
  })
})
