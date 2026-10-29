import { useEffect, useMemo } from 'react'

type Entry = { url: string; refs: number }

// One URL per Blob, shared by every component showing it and revoked once nobody does.
const entries = new WeakMap<Blob, Entry>()

function entryFor(blob: Blob) {
  let entry = entries.get(blob)
  if (!entry) {
    entry = { url: URL.createObjectURL(blob), refs: 0 }
    entries.set(blob, entry)
  }
  return entry
}

/** Object URL for a Blob, revoked after the last component using it unmounts. */
export function useObjectUrl(blob: Blob | undefined) {
  const url = useMemo(() => (blob ? entryFor(blob).url : undefined), [blob])

  useEffect(() => {
    if (!blob) return
    const entry = entryFor(blob)
    entry.refs++
    return () => {
      entry.refs--
      // Deferred so a StrictMode remount (or a quick re-render) can re-acquire it first.
      setTimeout(() => {
        if (entry.refs === 0 && entries.get(blob) === entry) {
          URL.revokeObjectURL(entry.url)
          entries.delete(blob)
        }
      }, 0)
    }
  }, [blob])

  return url
}
