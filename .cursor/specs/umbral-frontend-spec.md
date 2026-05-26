# UMBRAL — Especificación Técnica Frontend

## 1. Visión general

El frontend de UMBRAL está dividido en dos aplicaciones independientes:

| App            | Tecnología          | Usuarios             | Responsabilidad                        |
|----------------|---------------------|----------------------|----------------------------------------|
| `umbral-web`   | React + Vite + TS   | Admin, Operador      | Configuración, gestión y supervisión   |
| `umbral-mobile`| React Native + Expo | Equipo Participante  | Experiencia de juego (BT y Trivia)     |

Ambas comparten:
- La misma API REST del backend
- El mismo servidor SignalR
- Las mismas definiciones de tipos TypeScript
- Los mismos patrones de hooks y services

---

## 2. Stack por aplicación

### umbral-web (React)

| Tecnología            | Versión | Uso                              |
|-----------------------|---------|----------------------------------|
| React                 | 18.x    | Framework UI                     |
| TypeScript            | 5.x     | Tipado estático                  |
| Vite                  | 5.x     | Build tool y dev server          |
| React Router          | 6.x     | Enrutamiento SPA                 |
| Axios                 | 1.x     | Cliente HTTP                     |
| @microsoft/signalr    | 8.x     | Cliente WebSocket                |
| Zustand               | 4.x     | Estado global                    |
| React Query           | 5.x     | Cache de datos del servidor      |
| Tailwind CSS          | 3.x     | Estilos utilitarios              |
| Vitest                | 1.x     | Pruebas unitarias                |
| React Testing Library | 14.x    | Pruebas de componentes           |
| Playwright            | 1.x     | Pruebas E2E                      |

### umbral-mobile (React Native)

| Tecnología                  | Versión | Uso                              |
|-----------------------------|---------|----------------------------------|
| React Native                | 0.74.x  | Framework mobile                 |
| Expo                        | 51.x    | Toolchain y SDK                  |
| TypeScript                  | 5.x     | Tipado estático                  |
| React Navigation            | 6.x     | Navegación entre screens         |
| Axios                       | 1.x     | Cliente HTTP                     |
| @microsoft/signalr          | 8.x     | Cliente WebSocket (mismo paquete)|
| Zustand                     | 4.x     | Estado global                    |
| AsyncStorage                | 1.x     | Persistencia de token            |
| expo-camera / expo-barcode  | 13.x    | Escáner QR                       |
| NativeWind                  | 4.x     | Tailwind para React Native       |
| Jest                        | 29.x    | Pruebas unitarias                |
| React Native Testing Library| 12.x    | Pruebas de componentes nativos   |

---

## 3. Tipos TypeScript compartidos

Los tipos se definen una sola vez y son usados por ambas apps.
Si en el futuro se extrae a un paquete compartido, la estructura ya está lista.

```typescript
// types/sesion.types.ts
export type TipoSesion = 'BusquedaTesoro' | 'Trivia';

export type EstadoSesion =
  | 'Programada'
  | 'EnPreparacion'
  | 'Activa'
  | 'Pausada'
  | 'Finalizada'
  | 'Cancelada';

export interface SesionDto {
  id: string;
  tipoSesion: TipoSesion;
  estado: EstadoSesion;
  operadorId: string;
  iniciadaEn: string | null;
  finalizadaEn: string | null;
  totalEquipos: number;
}

export interface EstadoSesionDetalleDto {
  sesionId: string;
  tipoSesion: TipoSesion;
  estado: EstadoSesion;
  etapaActualIndex?: number;       // solo BusquedaTesoro
  preguntaActualIndex?: number;    // solo Trivia
  equipos: EquipoResumenDto[];
  ranking: PosicionRankingDto[];
}

// types/equipo.types.ts
export interface EquipoResumenDto {
  equipoId: string;
  nombre: string;
  puntajeTotal: number;
  tiempoAcumuladoMs: number;
  bloqueadoParaRondaActual: boolean;
}

export interface PosicionRankingDto {
  posicion: number;
  nombreEquipo: string;
  puntajeTotal: number;
  tiempoAcumuladoMs: number;
}

// types/trivia.types.ts
export type EstadoRespuesta =
  | 'Pendiente'
  | 'Correcta'
  | 'Incorrecta'
  | 'FueraDeTiempo';

export interface PreguntaActivaDto {
  preguntaId: string;
  enunciado: string;
  opciones: OpcionDto[];
  timerMs: number;
  lanzadaEn: string;
}

export interface OpcionDto {
  opcionId: string;
  texto: string;
}

export interface ResultadoRondaDto {
  preguntaId: string;
  opcionCorrectaId: string;
  ranking: PosicionRankingDto[];
}

// types/mision.types.ts
export type EstadoMision = 'Activa' | 'Inactiva';
export type TipoLiberacion = 'PorTiempo' | 'PorGanador';

export interface MisionDto {
  id: string;
  nombre: string;
  descripcion: string;
  nivelDificultad: string;
  tiempoMaximoSeg: number;
  estado: EstadoMision;
  totalEtapas: number;
}

export interface EtapaDto {
  etapaId: string;
  orden: number;
  tiempoMaximoSeg: number;
  pistas: PistaDto[];
}

export interface PistaDto {
  pistaId: string;
  contenido: string;
  orden: number;
  tipoLiberacion: TipoLiberacion;
}

export interface PistaHabilitadaDto {
  pistaId: string;
  contenido: string;
  orden: number;
  habilitadaEn: string;
}
```

---

## 4. Cliente HTTP — patrón compartido

Ambas apps usan el mismo patrón. La única diferencia es el almacenamiento del token.

```typescript
// Patrón base compartido
import axios, { AxiosError } from 'axios';

const BASE_URL = /* web: import.meta.env.VITE_API_URL
                   mobile: process.env.EXPO_PUBLIC_API_URL */

export const apiClient = axios.create({
  baseURL: `${BASE_URL}/api/v1`,
  headers: { 'Content-Type': 'application/json' },
  timeout: 10000,
});

// Interceptor request → adjunta token
apiClient.interceptors.request.use((config) => {
  const token = authStore.getState().token;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// Interceptor response → manejo centralizado
apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError<ApiErrorResponse>) => {
    if (error.response?.status === 401) {
      authStore.getState().logout();
      // web: window.location.href = '/login'
      // mobile: NavigationRef.navigate('Join')
    }
    return Promise.reject(error);
  }
);
```

---

## 5. Conexión SignalR — patrón compartido

`@microsoft/signalr` funciona igual en React y React Native.

```typescript
// lib/signalr.ts (mismo patrón en ambas apps)
import * as signalR from '@microsoft/signalr';
import { authStore } from '@/store/authStore';

const BASE_URL = /* env var según plataforma */

export const crearConexionSesion = () =>
  new signalR.HubConnectionBuilder()
    .withUrl(`${BASE_URL}/hubs/sesion`, {
      accessTokenFactory: () => authStore.getState().token ?? '',
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Warning)
    .build();

export const crearConexionTrivia = () =>
  new signalR.HubConnectionBuilder()
    .withUrl(`${BASE_URL}/hubs/trivia`, {
      accessTokenFactory: () => authStore.getState().token ?? '',
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000])
    .configureLogging(signalR.LogLevel.Warning)
    .build();
```

---

## 6. Hooks compartidos (misma lógica, ambas apps)

### useSessionSocket

```typescript
// hooks/useSessionSocket.ts
import { useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { crearConexionSesion } from '@/lib/signalr';

export type EstadoConexion =
  | 'conectando'
  | 'conectado'
  | 'reconectando'
  | 'desconectado';

interface UseSessionSocketOptions {
  sesionId: string;
  esOperador?: boolean;
  onEstadoCambiado?: (payload: any) => void;
  onRankingActualizado?: (payload: any) => void;
  onPistaLiberada?: (payload: any) => void;
  onEtapaAvanzada?: (payload: any) => void;
}

export const useSessionSocket = ({
  sesionId,
  esOperador = false,
  onEstadoCambiado,
  onRankingActualizado,
  onPistaLiberada,
  onEtapaAvanzada,
}: UseSessionSocketOptions) => {
  const conexionRef = useRef<signalR.HubConnection | null>(null);
  const [estadoConexion, setEstadoConexion] =
    useState<EstadoConexion>('conectando');

  useEffect(() => {
    const conexion = crearConexionSesion();
    conexionRef.current  = conexion;

    if (onEstadoCambiado)
      conexion.on('sesion:estado-cambiado', onEstadoCambiado);
    if (onRankingActualizado)
      conexion.on('sesion:ranking-actualizado', onRankingActualizado);
    if (onPistaLiberada)
      conexion.on('sesion:pista-liberada', onPistaLiberada);
    if (onEtapaAvanzada)
      conexion.on('sesion:etapa-avanzada', onEtapaAvanzada);

    conexion.onreconnecting(() => setEstadoConexion('reconectando'));
    conexion.onreconnected(() => setEstadoConexion('conectado'));
    conexion.onclose(() => setEstadoConexion('desconectado'));

    conexion.start()
      .then(async () => {
        setEstadoConexion('conectado');
        await conexion.invoke('UnirseASesion', sesionId);
        if (esOperador)
          await conexion.invoke('UnirseComoOperador', sesionId);
      })
      .catch(() => setEstadoConexion('desconectado'));

    return () => { conexion.stop(); };
  }, [sesionId]);

  return { estadoConexion };
};
```

### useTimer

```typescript
// hooks/useTimer.ts
import { useState, useEffect, useRef } from 'react';

interface UseTimerOptions {
  duracionMs: number;
  onExpira?: () => void;
  autoStart?: boolean;
}

export const useTimer = ({
  duracionMs,
  onExpira,
  autoStart = true,
}: UseTimerOptions) => {
  const [tiempoRestanteMs, setTiempoRestanteMs] = useState(duracionMs);
  const [corriendo, setCorriendo] = useState(autoStart);
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  useEffect(() => {
    if (!corriendo) return;

    intervalRef.current = setInterval(() => {
      setTiempoRestanteMs((prev) => {
        if (prev <= 100) {
          clearInterval(intervalRef.current!);
          setCorriendo(false);
          onExpira?.();
          return 0;
        }
        return prev - 100;
      });
    }, 100);

    return () => clearInterval(intervalRef.current!);
  }, [corriendo]);

  const reiniciar = (nuevaDuracionMs?: number) => {
    clearInterval(intervalRef.current!);
    setTiempoRestanteMs(nuevaDuracionMs ?? duracionMs);
    setCorriendo(true);
  };

  return {
    tiempoRestanteMs,
    segundosRestantes: Math.ceil(tiempoRestanteMs / 1000),
    porcentaje: Math.max(0, (tiempoRestanteMs / duracionMs) * 100),
    corriendo,
    reiniciar,
  };
};
```

### useTrivia

```typescript
// hooks/useTrivia.ts
import { useState, useCallback, useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { crearConexionTrivia } from '@/lib/signalr';
import { sesionService } from '@/services/sesionService';
import type { PreguntaActivaDto, ResultadoRondaDto } from '@/types/trivia.types';

export type FaseTrivia =
  | 'esperando'
  | 'pregunta-activa'
  | 'respondido'
  | 'resultado'
  | 'transicion'
  | 'finalizada';

export const useTrivia = (sesionId: string) => {
  const [fase, setFase] = useState<FaseTrivia>('esperando');
  const [preguntaActiva, setPreguntaActiva] =
    useState<PreguntaActivaDto | null>(null);
  const [opcionSeleccionada, setOpcionSeleccionada] =
    useState<string | null>(null);
  const [bloqueado, setBloqueado] = useState(false);
  const [resultadoRonda, setResultadoRonda] =
    useState<ResultadoRondaDto | null>(null);
  const conexionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    const conexion = crearConexionTrivia();
    conexionRef.current = conexion;

    conexion.on('trivia:pregunta-lanzada', (pregunta: PreguntaActivaDto) => {
      setPreguntaActiva(pregunta);
      setOpcionSeleccionada(null);
      setBloqueado(false);
      setResultadoRonda(null);
      setFase('pregunta-activa');
    });

    conexion.on('trivia:tiempo-agotado', () => {
      setBloqueado(true);
    });

    conexion.on('trivia:resultado-ronda', (resultado: ResultadoRondaDto) => {
      setResultadoRonda(resultado);
      setFase('resultado');
    });

    conexion.on('trivia:transicion', () => setFase('transicion'));

    conexion.on('sesion:estado-cambiado', ({ nuevoEstado }: any) => {
      if (nuevoEstado === 'Finalizada') setFase('finalizada');
    });

    conexion.start()
      .then(() => conexion.invoke('UnirseASesion', sesionId))
      .catch(console.error);

    return () => { conexion.stop(); };
  }, [sesionId]);

  const confirmarRespuesta = useCallback(async (opcionId: string) => {
    if (bloqueado || !preguntaActiva) return;

    setOpcionSeleccionada(opcionId);
    setBloqueado(true);
    setFase('respondido');

    try {
      await sesionService.submitRespuestaTrivia(
        sesionId, preguntaActiva.preguntaId, opcionId);
    } catch {
      // Respuesta no llegó a tiempo — el backend la marcará FueraDeTiempo
    }
  }, [bloqueado, preguntaActiva, sesionId]);

  return {
    fase,
    preguntaActiva,
    opcionSeleccionada,
    bloqueado,
    resultadoRonda,
    confirmarRespuesta,
  };
};
```

### useRanking

```typescript
// hooks/useRanking.ts
import { useState, useCallback, useEffect } from 'react';
import type { PosicionRankingDto } from '@/types/sesion.types';
import { sesionService } from '@/services/sesionService';
import { useSessionSocket } from './useSessionSocket';

export const useRanking = (sesionId: string) => {
  const [ranking, setRanking] = useState<PosicionRankingDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const cargarRanking = useCallback(async () => {
    try {
      const data = await sesionService.getRanking(sesionId);
      setRanking(data);
    } finally {
      setIsLoading(false);
    }
  }, [sesionId]);

  useEffect(() => { cargarRanking(); }, [cargarRanking]);

  useSessionSocket({
    sesionId,
    onRankingActualizado: ({ ranking: nuevoRanking }) => {
      setRanking(nuevoRanking);
    },
  });

  return { ranking, isLoading, recargar: cargarRanking };
};
```

---

## 7. Store global (Zustand)

```typescript
// store/authStore.ts — WEB
import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';

type Rol = 'Administrador' | 'Operador' | 'EquipoParticipante';

interface AuthState {
  token: string | null;
  userId: string | null;
  rol: Rol | null;
  sesionId: string | null;
  equipoId: string | null;
  login: (params: LoginParams) => void;
  logout: () => void;
  estaAutenticado: () => boolean;
}

// WEB → usa localStorage
export const authStore = create<AuthState>()(
  persist(
    (set, get) => ({
      token: null, userId: null, rol: null,
      sesionId: null, equipoId: null,
      login: ({ token, userId, rol, sesionId, equipoId }) =>
        set({ token, userId, rol,
              sesionId: sesionId ?? null,
              equipoId: equipoId ?? null }),
      logout: () => set({
        token: null, userId: null, rol: null,
        sesionId: null, equipoId: null }),
      estaAutenticado: () => !!get().token,
    }),
    { name: 'umbral-auth',
      storage: createJSONStorage(() => localStorage) }
  )
);

// MOBILE → usa AsyncStorage (mismo store, distinto storage)
import AsyncStorage from '@react-native-async-storage/async-storage';
export const authStore = create<AuthState>()(
  persist(
    // ... misma implementación ...
    { name: 'umbral-auth',
      storage: createJSONStorage(() => AsyncStorage) }
  )
);
```

---

## 8. umbral-web — detalle de implementación

### 8.1 Estructura de carpetas

umbral-web/src/
├── components/
│   ├── shared/
│   │   ├── Button/
│   │   ├── RankingList/
│   │   ├── Timer/
│   │   ├── Badge/
│   │   └── LoadingSpinner/
│   ├── operator/
│   │   ├── SessionControl/      → botones iniciar/pausar/finalizar
│   │   ├── PenaltyForm/         → formulario de penalización
│   │   ├── HintReleasePanel/    → liberar pistas manualmente
│   │   └── TeamStatusCard/      → estado por equipo
│   └── admin/
│       ├── MisionForm/
│       ├── EtapaForm/
│       └── PreguntaForm/
├── pages/
│   ├── admin/
│   │   ├── MisionesPage.tsx
│   │   ├── MisionDetailPage.tsx
│   │   ├── PreguntasPage.tsx
│   │   └── CategoriasPage.tsx
│   ├── operator/
│   │   ├── SesionesPage.tsx
│   │   ├── OperatorDashboardPage.tsx  → panel principal
│   │   └── WaitingRoomPage.tsx        → sala de espera
│   └── auth/
│       └── LoginPage.tsx
├── hooks/
│   ├── useSessionSocket.ts
│   ├── useRanking.ts
│   └── useAuth.ts
├── services/
│   ├── apiClient.ts
│   ├── sesionService.ts
│   ├── misionService.ts
│   ├── preguntaService.ts
│   ├── categoriaService.ts
│   └── authService.ts
├── store/
│   ├── authStore.ts
│   └── sesionStore.ts
├── types/
├── router/
│   └── AppRouter.tsx
├── lib/
│   └── signalr.ts
├── App.tsx
└── main.tsx

### 8.2 Enrutamiento y protección

```typescript
// router/AppRouter.tsx
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { authStore } from '@/store/authStore';

const RutaProtegida = ({
  children,
  roles,
}: {
  children: React.ReactNode;
  roles: string[];
}) => {
  const { estaAutenticado, rol } = authStore();
  if (!estaAutenticado()) return <Navigate to="/login" replace />;
  if (!roles.includes(rol ?? '')) return <Navigate to="/no-autorizado" replace />;
  return <>{children}</>;
};

export const AppRouter = () => (
  <BrowserRouter>
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      {/* Admin */}
      <Route path="/admin/misiones" element={
        <RutaProtegida roles={['Administrador']}>
          <MisionesPage />
        </RutaProtegida>
      } />
      <Route path="/admin/misiones/:id" element={
        <RutaProtegida roles={['Administrador']}>
          <MisionDetailPage />
        </RutaProtegida>
      } />
      <Route path="/admin/preguntas" element={
        <RutaProtegida roles={['Administrador']}>
          <PreguntasPage />
        </RutaProtegida>
      } />
      <Route path="/admin/categorias" element={
        <RutaProtegida roles={['Administrador']}>
          <CategoriasPage />
        </RutaProtegida>
      } />

      {/* Operador */}
      <Route path="/operador/sesiones" element={
        <RutaProtegida roles={['Operador', 'Administrador']}>
          <SesionesPage />
        </RutaProtegida>
      } />
      <Route path="/operador/sesiones/:id" element={
        <RutaProtegida roles={['Operador', 'Administrador']}>
          <OperatorDashboardPage />
        </RutaProtegida>
      } />
      <Route path="/operador/sesiones/:id/sala-espera" element={
        <RutaProtegida roles={['Operador', 'Administrador']}>
          <WaitingRoomPage />
        </RutaProtegida>
      } />

      <Route path="/" element={<Navigate to="/login" replace />} />
    </Routes>
  </BrowserRouter>
);
```

### 8.3 OperatorDashboardPage — estructura

```tsx
// pages/operator/OperatorDashboardPage.tsx
export const OperatorDashboardPage = () => {
  const { id: sesionId } = useParams<{ id: string }>();
  const { sesionActiva } = sesionStore();
  const { ranking } = useRanking(sesionId!);

  // Escucha eventos de operador
  useSessionSocket({
    sesionId: sesionId!,
    esOperador: true,
    onEstadoCambiado: ({ nuevoEstado }) =>
      sesionStore.getState().actualizarEstado(nuevoEstado),
  });

  if (!sesionActiva) return <LoadingSpinner />;

  return (
    <div className="grid grid-cols-3 gap-6 p-6">
      {/* Col 1: Control de sesión */}
      <SessionControl sesion={sesionActiva} />

      {/* Col 2: Ranking en tiempo real */}
      <div>
        <h2 className="font-bold text-lg mb-3">Ranking</h2>
        <RankingList ranking={ranking} />
      </div>

      {/* Col 3: Panel específico por tipo */}
      {sesionActiva.tipoSesion === 'BusquedaTesoro'
        ? <HintReleasePanel sesionId={sesionId!} equipos={sesionActiva.equipos} />
        : <TriviaOperatorPanel sesionId={sesionId!} />
      }
    </div>
  );
};
```

---

## 9. umbral-mobile — detalle de implementación

### 9.1 Estructura de carpetas

umbral-mobile/src/
├── components/
│   ├── shared/
│   │   ├── RankingList/
│   │   ├── Timer/
│   │   ├── ScoreDisplay/
│   │   └── ConnectionBadge/
│   ├── busqueda/
│   │   ├── HintList/
│   │   └── QRScanner/
│   └── trivia/
│       ├── TriviaQuestion/
│       ├── OptionButton/
│       └── RoundResult/
├── screens/
│   ├── auth/
│   │   └── JoinScreen.tsx        → ingresar código de acceso
│   ├── busqueda/
│   │   └── BusquedaDashboardScreen.tsx
│   ├── trivia/
│   │   ├── WaitingScreen.tsx     → sala de espera
│   │   └── TriviaDashboardScreen.tsx
│   └── shared/
│       ├── RankingScreen.tsx
│       └── SessionEndScreen.tsx
├── hooks/
│   ├── useSessionSocket.ts       → mismo que web
│   ├── useTimer.ts               → mismo que web
│   ├── useTrivia.ts              → mismo que web
│   └── useBusquedaTesoro.ts
├── services/
│   ├── apiClient.ts
│   ├── sesionService.ts
│   └── equipoService.ts
├── store/
│   └── authStore.ts              → AsyncStorage
├── types/                        → mismos tipos que web
└── navigation/
└── AppNavigator.tsx


### 9.2 Navegación

```typescript
// navigation/AppNavigator.tsx
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { authStore } from '@/store/authStore';

const Stack = createNativeStackNavigator();

export const AppNavigator = () => {
  const { estaAutenticado, sesionId, tipoSesion } = authStore();

  return (
    <NavigationContainer>
      <Stack.Navigator screenOptions={{ headerShown: false }}>
        {!estaAutenticado() ? (
          // No autenticado
          <Stack.Screen name="Join" component={JoinScreen} />
        ) : tipoSesion === 'BusquedaTesoro' ? (
          // Sesión BT
          <Stack.Screen
            name="BusquedaDashboard"
            component={BusquedaDashboardScreen}
          />
        ) : (
          // Sesión Trivia
          <>
            <Stack.Screen name="Waiting"  component={WaitingScreen} />
            <Stack.Screen name="Trivia"   component={TriviaDashboardScreen} />
          </>
        )}
        {/* Compartidas */}
        <Stack.Screen name="Ranking"    component={RankingScreen} />
        <Stack.Screen name="SessionEnd" component={SessionEndScreen} />
      </Stack.Navigator>
    </NavigationContainer>
  );
};
```

### 9.3 JoinScreen — autenticación del equipo

```typescript
// screens/auth/JoinScreen.tsx
import { useState } from 'react';
import { View, Text, TextInput, TouchableOpacity } from 'react-native';
import { equipoService } from '@/services/equipoService';
import { authStore } from '@/store/authStore';

export const JoinScreen = () => {
  const [codigo, setCodigo] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [cargando, setCargando] = useState(false);

  const handleUnirse = async () => {
    if (!codigo.trim()) return;
    setCargando(true);
    setError(null);

    try {
      const { token, equipoId, sesionId, tipoSesion } =
        await equipoService.unirse(codigo.trim());

      authStore.getState().login({
        token,
        userId: equipoId,
        rol: 'EquipoParticipante',
        sesionId,
        equipoId,
        tipoSesion,
      });
    } catch {
      setError('Código inválido o sesión no disponible.');
    } finally {
      setCargando(false);
    }
  };

  return (
    <View className="flex-1 justify-center px-8">
      <Text className="text-3xl font-bold text-center mb-8">UMBRAL</Text>
      <TextInput
        value={codigo}
        onChangeText={setCodigo}
        placeholder="Código de acceso"
        autoCapitalize="characters"
        className="border rounded-xl px-4 py-3 text-lg mb-4"
      />
      {error && <Text className="text-red-500 text-sm mb-3">{error}</Text>}
      <TouchableOpacity
        onPress={handleUnirse}
        disabled={cargando || !codigo.trim()}
        className="bg-blue-600 rounded-xl py-4"
      >
        <Text className="text-white font-bold text-center text-lg">
          {cargando ? 'Conectando…' : 'Unirse a la sesión'}
        </Text>
      </TouchableOpacity>
    </View>
  );
};
```

### 9.4 TriviaDashboardScreen

```typescript
// screens/trivia/TriviaDashboardScreen.tsx
import { View, Text } from 'react-native';
import { useTrivia } from '@/hooks/useTrivia';
import { useTimer } from '@/hooks/useTimer';
import { authStore } from '@/store/authStore';

export const TriviaDashboardScreen = () => {
  const { sesionId } = authStore();
  const {
    fase,
    preguntaActiva,
    opcionSeleccionada,
    bloqueado,
    resultadoRonda,
    confirmarRespuesta,
  } = useTrivia(sesionId!);

  const timer = useTimer({
    duracionMs: preguntaActiva?.timerMs ?? 0,
    autoStart: fase === 'pregunta-activa',
  });

  if (fase === 'esperando' || fase === 'transicion') {
    return (
      <View className="flex-1 justify-center items-center">
        <Text className="text-xl text-gray-500">
          {fase === 'transicion'
            ? 'Preparando siguiente pregunta…'
            : 'Esperando inicio…'}
        </Text>
      </View>
    );
  }

  if (fase === 'resultado' && resultadoRonda) {
    return <RoundResult resultado={resultadoRonda} />;
  }

  if (fase === 'finalizada') {
    return <SessionEndScreen />;
  }

  if (!preguntaActiva) return null;

  return (
    <View className="flex-1 p-6">
      <TriviaQuestion
        pregunta={preguntaActiva}
        opcionSeleccionada={opcionSeleccionada}
        bloqueado={bloqueado}
        tiempoRestanteMs={timer.tiempoRestanteMs}
        timerTotalMs={preguntaActiva.timerMs}
        onSeleccionar={confirmarRespuesta}
      />
    </View>
  );
};
```

### 9.5 BusquedaDashboardScreen

```typescript
// screens/busqueda/BusquedaDashboardScreen.tsx
import { useState } from 'react';
import { View, Text, ScrollView } from 'react-native';
import { useBusquedaTesoro } from '@/hooks/useBusquedaTesoro';
import { authStore } from '@/store/authStore';

export const BusquedaDashboardScreen = () => {
  const { sesionId, equipoId } = authStore();
  const {
    pistas,
    puntaje,
    etapaActual,
    bloqueado,
    enviarEvidencia,
    enviando,
    resultadoUltimaEvidencia,
  } = useBusquedaTesoro(sesionId!, equipoId!);

  return (
    <ScrollView className="flex-1 p-6">
      {/* Puntaje */}
      <ScoreDisplay puntaje={puntaje} />

      {/* Estado de etapa */}
      {bloqueado && (
        <Text className="text-center text-orange-500 font-semibold my-3">
          Otro equipo encontró el tesoro primero. ¡Sigue en la próxima etapa!
        </Text>
      )}

      {/* Pistas habilitadas */}
      <HintList pistas={pistas} />

      {/* Escáner QR */}
      {!bloqueado && (
        <QRScanner
          onEvidenciaEnviada={enviarEvidencia}
          enviando={enviando}
          resultado={resultadoUltimaEvidencia}
        />
      )}
    </ScrollView>
  );
};
```

---

## 10. Variables de entorno

```bash
# umbral-web/.env.example
VITE_API_URL=http://localhost:5000
VITE_APP_NAME=UMBRAL

# umbral-mobile/.env.example  (Expo usa EXPO_PUBLIC_)
EXPO_PUBLIC_API_URL=http://localhost:5000
EXPO_PUBLIC_APP_NAME=UMBRAL
```

---

## 11. Reglas del frontend — resumen completo

| Regla | Web | Mobile |
|---|---|---|
| HTTP solo en `services/` | ✅ | ✅ |
| Sin lógica en componentes/screens | ✅ | ✅ |
| Sin `any` en TypeScript | ✅ | ✅ |
| Hooks consumen services | ✅ | ✅ |
| Cleanup en `useEffect` | ✅ | ✅ |
| Token en `localStorage` | ✅ | ❌ |
| Token en `AsyncStorage` | ❌ | ✅ |
| Rutas con React Router | ✅ | ❌ |
| Navegación con React Navigation | ❌ | ✅ |
| Estilos con Tailwind CSS | ✅ | ❌ |
| Estilos con NativeWind | ❌ | ✅ |
| Fallback REST si SignalR falla | ✅ | ✅ |
| Roles permitidos | Admin, Operador | EquipoParticipante |