const memoryStorage = new Map();
const warnedStorageTypes = new Set();

function getWindowStorage(storageType) {
  if (typeof window === 'undefined') {
    return null;
  }

  try {
    const storage = window[storageType];
    const probeKey = `__hycoat.storage.probe.${storageType}`;
    storage.setItem(probeKey, '1');
    storage.removeItem(probeKey);
    return storage;
  } catch (error) {
    if (!warnedStorageTypes.has(storageType)) {
      warnedStorageTypes.add(storageType);
      console.warn(`Browser ${storageType} is unavailable for this document.`, error);
    }

    return null;
  }
}

export function isPersistentStorageAvailable() {
  return getWindowStorage('localStorage') !== null;
}

export function getPersistentItem(key) {
  const storage = getWindowStorage('localStorage');
  if (storage) {
    return storage.getItem(key);
  }

  return memoryStorage.get(key) ?? null;
}

export function setPersistentItem(key, value) {
  const storage = getWindowStorage('localStorage');
  if (storage) {
    storage.setItem(key, value);
    return true;
  }

  memoryStorage.set(key, value);
  return false;
}

export function removePersistentItem(key) {
  const storage = getWindowStorage('localStorage');
  if (storage) {
    storage.removeItem(key);
  }

  memoryStorage.delete(key);
}

export function isEmbeddedBrowserContext() {
  if (typeof window === 'undefined') {
    return false;
  }

  try {
    return window.self !== window.top;
  } catch {
    return true;
  }
}