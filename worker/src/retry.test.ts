import { describe, expect, it } from 'vitest'
import { classifyStatus, retryDelaySeconds } from './retry'

describe('classifyStatus', () => {
  it.each([200, 201, 204])('acks %i', (status) => {
    expect(classifyStatus(status)).toBe('ack')
  })

  it.each([301, 400, 401, 403, 404, 408, 422, 429, 500, 502, 503, 504])(
    'retries %i so the message ends in the dead-letter queue instead of being lost',
    (status) => {
      expect(classifyStatus(status)).toBe('retry')
    },
  )
})

describe('retryDelaySeconds', () => {
  const noJitter = () => 1
  const maxJitter = () => 0

  it('grows exponentially', () => {
    expect([1, 2, 3, 4].map((n) => retryDelaySeconds(n, noJitter))).toEqual([5, 15, 45, 135])
  })

  it('is capped at 10 minutes', () => {
    expect(retryDelaySeconds(8, noJitter)).toBe(600)
    expect(retryDelaySeconds(50, noJitter)).toBe(600)
  })

  it('jitters between half and the full delay', () => {
    expect(retryDelaySeconds(3, maxJitter)).toBe(23)
    expect(retryDelaySeconds(3, () => 0.5)).toBe(34)
    expect(retryDelaySeconds(3, noJitter)).toBe(45)
  })

  it('never returns less than one second, even for bad attempt counts', () => {
    expect(retryDelaySeconds(0, maxJitter)).toBeGreaterThanOrEqual(1)
    expect(retryDelaySeconds(-3, maxJitter)).toBeGreaterThanOrEqual(1)
  })
})
