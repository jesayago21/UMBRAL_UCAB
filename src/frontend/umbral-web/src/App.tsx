import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter } from 'react-router-dom'
import { OidcAuthBridge } from '@/auth/OidcAuthBridge'
import { AppRouter } from '@/router/AppRouter'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      retry: 1,
    },
  },
})

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <OidcAuthBridge>
          <AppRouter />
        </OidcAuthBridge>
      </BrowserRouter>
    </QueryClientProvider>
  )
}
