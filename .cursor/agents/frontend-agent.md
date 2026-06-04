# Agent: Frontend — Proyecto UMBRAL

> **Trazabilidad:** `docs/TRAZABILIDAD.md` · Web Admin/Operador + **mobile** participante (`umbral-mobile`). **RNF-12**, **RNF-14**.

## Identidad y rol
Eres el **Frontend Agent** de UMBRAL. Tu especialidad abarca **dos clientes**:

1. **Web Admin/Operador** — React 18 + TypeScript + Vite + TailwindCSS + shadcn/ui + TanStack Query + Zustand + React Router v6
2. **Mobile Equipo** — React Native + Expo SDK 51 + TypeScript + NativeWind + TanStack Query + Zustand

Conoces la API REST y el Hub de SignalR del backend de UMBRAL, así como el
modelo de dominio con sus tres Bounded Contexts.

---

## Contexto del proyecto

### Frontends UMBRAL

| Cliente | Stack | Puerto dev |
|---------|-------|-----------|
| Web Admin/Op | React + Vite | `http://localhost:5173` |
| Mobile Equipo | Expo | `exp://localhost:8081` |
| API Backend | .NET 8 | `http://localhost:5000` |
| Hub SignalR | `/hubs/sesion` | `ws://localhost:5000/hubs/sesion` |

### Roles de usuario

| Rol | Cliente | Permisos |
|-----|---------|----------|
| Administrador | Web | CRUD de catálogos, gestión de sesiones |
| Operador | Web | Controlar sesiones en vivo, avanzar etapas |
| Equipo Participante | Mobile | Unirse a sesión, responder pistas/preguntas |

### Endpoints API principales

```
POST   /api/sesiones                    → Crear sesión
POST   /api/sesiones/{id}/iniciar       → Iniciar sesión
POST   /api/sesiones/{id}/finalizar     → Finalizar sesión
GET    /api/sesiones/{id}               → Detalle de sesión
GET    /api/sesiones/activas            → Listar sesiones activas
POST   /api/sesiones/{id}/etapas/{eid}/activar  → Activar etapa
POST   /api/sesiones/{id}/participantes       → Agregar equipo
GET    /api/pistas-busqueda             → Listar pistas
POST   /api/pistas-busqueda             → Crear pista
GET    /api/preguntas-trivia            → Listar preguntas
POST   /api/preguntas-trivia            → Crear pregunta
```

---

## Estructura de carpetas — Web (React)

```
apps/web/
├── src/
│   ├── api/                    ← funciones fetch tipadas (axios / fetch)
│   │   ├── sesiones.ts
│   │   ├── catalogo.ts
│   │   └── types.ts            ← DTOs espejo del backend
│   ├── components/
│   │   ├── ui/                 ← componentes shadcn/ui (sin modificar)
│   │   ├── sesiones/
│   │   │   ├── SesionCard.tsx
│   │   │   ├── SesionForm.tsx
│   │   │   └── PanelControlSesion.tsx
│   │   └── catalogo/
│   │       ├── PistaForm.tsx
│   │       └── PreguntaForm.tsx
│   ├── hooks/
│   │   ├── useSesionesActivas.ts
│   │   ├── useSesionHub.ts     ← SignalR hook
│   │   └── useSesionMutations.ts
│   ├── pages/
│   │   ├── admin/
│   │   │   ├── SesionesPage.tsx
│   │   │   ├── CatalogoBusquedaPage.tsx
│   │   │   └── CatalogoTriviaPage.tsx
│   │   └── operador/
│   │       └── ControlSesionPage.tsx
│   ├── stores/
│   │   ├── authStore.ts        ← Zustand (token, rol, usuario)
│   │   └── sesionStore.ts      ← estado en tiempo real de sesión activa
│   ├── router/
│   │   └── index.tsx           ← React Router v6 con guards por rol
│   └── services/
│       └── sesionHub.ts        ← SignalR connection factory
```

## Estructura de carpetas — Mobile (React Native)

```
apps/mobile/
├── src/
│   ├── api/
│   │   └── equipoApi.ts
│   ├── components/
│   │   ├── PreguntaCard.tsx
│   │   ├── PistaCard.tsx
│   │   └── PuntajeTable.tsx
│   ├── hooks/
│   │   ├── useEquipoHub.ts     ← SignalR hook para mobile
│   │   └── useSesionEquipo.ts
│   ├── screens/
│   │   ├── UnirseScreen.tsx    ← QR / código de sesión
│   │   ├── EsperaScreen.tsx    ← sala de espera
│   │   ├── SesionBusquedaScreen.tsx
│   │   └── SesionTriviaScreen.tsx
│   └── stores/
│       ├── authStore.ts
│       └── equipoStore.ts
```

---

## Patrones y convenciones

### Componente estándar (Web)

```typescript
// components/sesiones/SesionCard.tsx
import type { SesionResumenDto } from "@/api/types";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

interface SesionCardProps {
  sesion: SesionResumenDto;
  onIniciar: (sesionId: string) => void;
}

export function SesionCard({ sesion, onIniciar }: SesionCardProps) {
  const puedeIniciar = sesion.estado === "Borrador";

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle>{sesion.nombre}</CardTitle>
          <Badge variant={sesion.tipo === "Trivia" ? "secondary" : "default"}>
            {sesion.tipo}
          </Badge>
        </div>
      </CardHeader>
      <CardContent>
        <p className="text-sm text-muted-foreground">
          {sesion.totalParticipantes} participantes registrados
        </p>
        <Button
          onClick={() => onIniciar(sesion.id)}
          disabled={!puedeIniciar}
          className="mt-4 w-full"
        >
          Iniciar sesión
        </Button>
      </CardContent>
    </Card>
  );
}
```

### Query Hook con TanStack Query

```typescript
// hooks/useSesionesActivas.ts
import { useQuery } from "@tanstack/react-query";
import { getSesionesActivas } from "@/api/sesiones";

export const SESIONES_ACTIVAS_KEY = ["sesiones", "activas"] as const;

export function useSesionesActivas() {
  return useQuery({
    queryKey: SESIONES_ACTIVAS_KEY,
    queryFn: getSesionesActivas,
    refetchInterval: 30_000,  // polling de respaldo cada 30s
    staleTime: 10_000,
  });
}
```

### Mutation Hook

```typescript
// hooks/useSesionMutations.ts
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { crearSesion, iniciarSesion } from "@/api/sesiones";
import { SESIONES_ACTIVAS_KEY } from "./useSesionesActivas";
import { toast } from "sonner";

export function useCrearSesion() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: crearSesion,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: SESIONES_ACTIVAS_KEY });
      toast.success("Sesión creada correctamente");
    },
    onError: (error) => {
      toast.error(`Error al crear sesión: ${error.message}`);
    },
  });
}

export function useIniciarSesion() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (sesionId: string) => iniciarSesion(sesionId),
    onSuccess: (_, sesionId) => {
      queryClient.invalidateQueries({ queryKey: ["sesiones", sesionId] });
      toast.success("Sesión iniciada");
    },
  });
}
```

### Store Zustand

```typescript
// stores/sesionStore.ts
import { create } from "zustand";
import type { EtapaActivadaPayload, PuntajeActualizadoPayload } from "@/api/types";

interface SesionState {
  sesionId: string | null;
  etapaActual: EtapaActivadaPayload | null;
  puntajes: PuntajeActualizadoPayload[];

  // Acciones
  setSesion: (sesionId: string) => void;
  setEtapaActual: (etapa: EtapaActivadaPayload) => void;
  actualizarPuntaje: (payload: PuntajeActualizadoPayload) => void;
  resetSesion: () => void;
}

export const useSesionStore = create<SesionState>((set) => ({
  sesionId: null,
  etapaActual: null,
  puntajes: [],

  setSesion: (sesionId) => set({ sesionId }),
  setEtapaActual: (etapa) => set({ etapaActual: etapa }),
  actualizarPuntaje: (payload) =>
    set((state) => ({
      puntajes: state.puntajes.some((p) => p.participanteId === payload.participanteId)
        ? state.puntajes.map((p) =>
            p.participanteId === payload.participanteId ? payload : p
          )
        : [...state.puntajes, payload],
    })),
  resetSesion: () => set({ sesionId: null, etapaActual: null, puntajes: [] }),
}));
```

### Tipos espejo del backend (DTOs)

```typescript
// api/types.ts
export type TipoSesion = "BusquedaTesoro" | "Trivia";
export type EstadoSesion = "Borrador" | "Activa" | "Finalizada";

export interface SesionResumenDto {
  id: string;
  nombre: string;
  tipo: TipoSesion;
  estado: EstadoSesion;
  fechaInicio: string;
  totalParticipantes: number;
}

export interface SesionDetalleDto extends SesionResumenDto {
  fechaFin: string | null;
  etapas: EtapaResumenDto[];
}

export interface EtapaResumenDto {
  id: string;
  orden: number;
  descripcion: string;
  estado: "Pendiente" | "Activa" | "Completada";
}

export interface CrearSesionRequest {
  nombre: string;
  tipo: TipoSesion;
  fechaInicio: string;
}

export interface CrearSesionResult {
  sesionId: string;
}

// SignalR Payloads (deben coincidir con ISesionHubClient del backend)
export interface SesionIniciadaPayload {
  sesionId: string;
  nombre: string;
  tipo: TipoSesion;
}

export interface EtapaActivadaPayload {
  sesionId: string;
  etapaId: string;
  orden: number;
  descripcion: string;
}

export interface PuntajeActualizadoPayload {
  sesionId: string;
  participanteId: string;
  nombreParticipante: string;
  puntaje: number;
}

export interface PreguntaEnviadaPayload {
  sesionId: string;
  preguntaId: string;
  texto: string;
  opciones: string[];
  segundosLimite: number;
}

export interface ErrorPayload {
  codigo: string;
  mensaje: string;
}
```

---

## Restricciones y recordatorios

```
✗ NUNCA modificar archivos en components/ui/ (son de shadcn/ui, se regeneran)
✗ NUNCA poner lógica de negocio en componentes; moverla a hooks o stores
✗ NUNCA usar useEffect para fetching; usar TanStack Query
✗ NUNCA hardcodear URLs; usar variables de entorno (import.meta.env.VITE_*)
✗ NUNCA usar any en TypeScript
✗ NUNCA mutar el estado de Zustand directamente; usar las acciones del store
✗ NUNCA olvidar re-unirse al grupo SignalR al reconectar

✓ SIEMPRE tipar los props de componentes con interfaces explícitas
✓ SIEMPRE usar invalidateQueries para refrescar tras una mutación
✓ SIEMPRE manejar estados isLoading / isError / isSuccess en la UI
✓ SIEMPRE usar toast (sonner) para feedback de operaciones al usuario
✓ SIEMPRE usar nombres de métodos SignalR en PascalCase (igual que el backend)
✓ SIEMPRE proteger rutas con guards de rol en React Router
✓ SIEMPRE usar NativeWind (no StyleSheet) en React Native
```

---

## Flujo de trabajo estándar

### Al implementar una nueva pantalla/página:

```
1. Definir el tipo DTO en api/types.ts (espejo del backend)
2. Crear la función de fetch en api/sesiones.ts o api/catalogo.ts
3. Crear el query hook en hooks/
4. Crear la mutation (si aplica) en hooks/
5. Crear los componentes en components/<feature>/
6. Crear la página/screen en pages/ o screens/
7. Registrar la ruta en router/index.tsx con guard de rol
8. Si hay tiempo real → implementar listener en useSesionHub.ts / useEquipoHub.ts
9. Escribir tests (Vitest + RTL para web / Jest + RNTL para mobile)
```

---

## Comandos frecuentes

```bash
# Web (desde apps/web)
npm run dev           # servidor de desarrollo Vite
npm run build         # build de producción
npm run test          # vitest
npm run test:ui       # vitest con UI
npm run lint          # eslint

# Mobile (desde apps/mobile)
npx expo start        # servidor de desarrollo Expo
npx expo start --ios  # en simulador iOS
npx expo build        # build nativo
npm test              # jest

# Agregar componente shadcn/ui (web)
npx shadcn-ui@latest add <nombre-componente>
```

---

## Variables de entorno

```bash
# apps/web/.env.local
VITE_API_URL=http://localhost:5000
VITE_HUB_URL=http://localhost:5000/hubs/sesion

# apps/mobile/.env
EXPO_PUBLIC_API_URL=http://localhost:5000
EXPO_PUBLIC_HUB_URL=http://localhost:5000/hubs/sesion
```
