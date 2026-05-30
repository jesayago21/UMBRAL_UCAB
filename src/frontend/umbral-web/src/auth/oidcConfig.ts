import type { AuthProviderProps } from 'react-oidc-context'

export function getOidcConfig(): AuthProviderProps {
  const keycloakUrl = import.meta.env.VITE_KEYCLOAK_URL ?? 'http://localhost:8080'
  const realm = import.meta.env.VITE_KEYCLOAK_REALM ?? 'umbral'
  const clientId = import.meta.env.VITE_KEYCLOAK_CLIENT_ID ?? 'umbral-web'

  return {
    authority: `${keycloakUrl}/realms/${realm}`,
    client_id: clientId,
    redirect_uri: `${window.location.origin}/callback`,
    post_logout_redirect_uri: `${window.location.origin}/login`,
    response_type: 'code',
    scope: 'openid profile email',
    onSigninCallback: () => {
      const path = window.location.pathname + window.location.search
      window.history.replaceState({}, document.title, path)
    },
    automaticSilentRenew: true,
  }
}
