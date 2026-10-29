import { describe, expect, it } from 'vitest'
import { idbDelete, idbGet, idbSet } from './idb'

describe('idb', () => {
  it('returns undefined for a missing key', async () => {
    expect(await idbGet('missing')).toBeUndefined()
  })

  it('round-trips a value', async () => {
    await idbSet('k', { a: 1 })
    expect(await idbGet('k')).toEqual({ a: 1 })
  })

  it('deletes a value', async () => {
    await idbSet('gone', 'x')
    await idbDelete('gone')
    expect(await idbGet('gone')).toBeUndefined()
  })
})
