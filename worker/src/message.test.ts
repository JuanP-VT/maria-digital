import { describe, expect, it } from 'vitest'
import { parseMessage } from './message'

const event = (action: string, key = 'submissions/abc/registration/def', size = 5000) => ({
  account: 'acc',
  bucket: 'digital-guadalupe-docs',
  action,
  object: { key, size, eTag: 'x' },
  eventTime: '2026-10-29T12:00:00Z',
})

describe('parseMessage', () => {
  it.each(['PutObject', 'CopyObject', 'CompleteMultipartUpload'])('confirms %s', (action) => {
    expect(parseMessage(event(action))).toEqual({
      kind: 'confirm',
      key: 'submissions/abc/registration/def',
      size: 5000,
    })
  })

  it('ignores deletions and other non-create actions', () => {
    expect(parseMessage(event('DeleteObject'))).toEqual({ kind: 'ignore' })
    expect(parseMessage(event('LifecycleDeletion'))).toEqual({ kind: 'ignore' })
  })

  it('ignores objects outside the submissions prefix', () => {
    expect(parseMessage(event('PutObject', 'other/file'))).toEqual({ kind: 'ignore' })
  })

  it('accepts an empty object', () => {
    expect(parseMessage(event('PutObject', undefined, 0))).toMatchObject({ size: 0 })
  })

  it.each([
    ['null', null],
    ['a string', 'PutObject'],
    ['no action', { object: { key: 'submissions/a', size: 1 } }],
    ['no object', { action: 'PutObject' }],
    ['null object', { action: 'PutObject', object: null }],
    ['empty key', event('PutObject', '')],
    ['non-string key', { action: 'PutObject', object: { key: 7, size: 1 } }],
    ['negative size', event('PutObject', undefined, -1)],
    ['fractional size', event('PutObject', undefined, 1.5)],
    ['string size', { action: 'PutObject', object: { key: 'submissions/a', size: '1' } }],
  ])('rejects %s as invalid', (_name, body) => {
    expect(parseMessage(body)).toEqual({ kind: 'invalid' })
  })
})
