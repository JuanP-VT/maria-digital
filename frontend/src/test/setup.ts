import '@testing-library/jest-dom/vitest'
import 'fake-indexeddb/auto'
import { cleanup } from '@testing-library/react'
import { afterEach, vi } from 'vitest'

// jsdom has no object URL support; hand out predictable fakes.
let urlCounter = 0
URL.createObjectURL = vi.fn(() => `blob:test/${++urlCounter}`)
URL.revokeObjectURL = vi.fn()

afterEach(() => {
  cleanup()
  vi.clearAllMocks()
})
