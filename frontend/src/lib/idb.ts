const DB_NAME = 'guadalupe-digital'
const STORE = 'kv'

let dbPromise: Promise<IDBDatabase> | null = null

function openDb() {
  dbPromise ??= new Promise<IDBDatabase>((resolve, reject) => {
    const request = indexedDB.open(DB_NAME, 1)
    request.onupgradeneeded = () => request.result.createObjectStore(STORE)
    request.onsuccess = () => resolve(request.result)
    request.onerror = () => {
      dbPromise = null
      reject(request.error)
    }
  })
  return dbPromise
}

async function run<T>(mode: IDBTransactionMode, action: (store: IDBObjectStore) => IDBRequest<T>) {
  const db = await openDb()
  return new Promise<T>((resolve, reject) => {
    const transaction = db.transaction(STORE, mode)
    const request = action(transaction.objectStore(STORE))
    transaction.oncomplete = () => resolve(request.result)
    transaction.onerror = () => reject(transaction.error)
    transaction.onabort = () => reject(transaction.error)
  })
}

export function idbGet<T>(key: string) {
  return run<T | undefined>('readonly', (store) => store.get(key))
}

export function idbSet(key: string, value: unknown) {
  return run('readwrite', (store) => store.put(value, key))
}

export function idbDelete(key: string) {
  return run('readwrite', (store) => store.delete(key))
}
