/**
 * Limpia estado OIDC en el navegador (útil tras recrear Keycloak o error invalid_code).
 */
export function clearOidcBrowserState(): void {
  const prefixes = ['oidc.', 'oidc.user:']
  const sweep = (storage: Storage) => {
    const keys: string[] = []
    for (let i = 0; i < storage.length; i++) {
      const key = storage.key(i)
      if (key && prefixes.some((p) => key.startsWith(p))) keys.push(key)
    }
    keys.forEach((k) => storage.removeItem(k))
  }
  sweep(sessionStorage)
  sweep(localStorage)
}
