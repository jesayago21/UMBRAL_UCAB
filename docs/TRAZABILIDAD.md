# UMBRAL — Trazabilidad canónica (ERS ↔ `.cursor` ↔ Fase 1)

> **Resumen operativo recomendado:** `docs/RESUMEN-COMPACTO-E1-E2.md`.  
> Este documento queda como referencia normativa de códigos HU/RB/RNF.

**Fuente normativa del proyecto:** `.cursor/specs/umbral-product-spec.md` (producto + RB/RF/RNF) y este documento para códigos unificados.

**Documento académico base:** `docs/ERS_Proyecto_UMBRAL_UCAB.md` (historias HU-01…HU-40; sección §8–§7 alineadas con esta tabla).

**Historias TO-BE (formato académico):** `docs/HU-TO-BE-UMBRAL.md` (misiones polimórficas, Participante, Identidad).

**Seguimiento de implementación dominio:** `docs/fase-1/TRACKER.md` (usa numeración **HU del ERS**, no la antigua tabla comprimida de entregas).

> Los códigos `RB-13-01`, `RB-16-01`, etc. en iteraciones de Fase 1 son **criterios de aceptación locales** de esa iteración, no la regla global `RB-13` / `RB-16`.

### Convención en código C#

| Prefijo | Significado | Ejemplo |
|---------|-------------|---------|
| `RB-NN` | Regla global (esta tabla) | `RB-18` al iniciar sesión |
| `RB-NN-xx` | Criterio de iteración Fase 1 | `RB-14-02` en `iter-03` → global **RB-18** |
| `HU-NN` | Historia ERS | `HU-16` penalización |

En `Sesion.cs` y tests, citar **ambos** cuando aplique: criterio local + RB global.

---

## Alcance de clientes

| Rol | Cliente | Stack |
|-----|---------|-------|
| Administrador | Web | React + Vite (`src/frontend/umbral-web`) |
| Operador | Web | React + Vite |
| Equipo participante | Mobile | React Native + Expo (`src/mobile/umbral-mobile`) |

Queda fuera de alcance: apps nativas puras (Swift/Kotlin sin Expo), cobros, IoT, IA de contenido, analítica histórica avanzada, geolocalización.

---

## Reglas de negocio (RB-01 … RB-37)

| Código | Regla | Modo |
|--------|-------|------|
| RB-01 | Solo se crea sesión desde una misión con estado `Activa`. | Ambos |
| RB-02 | El nombre de equipo es único dentro de la misma sesión. | Ambos |
| RB-03 | Un equipo no puede registrarse en sesión `Finalizada` o `Cancelada`. | Ambos |
| RB-04 | Al validar la primera evidencia correcta, los demás no puntúan en esa etapa. | BT |
| RB-05 | Al completarse un nodo, todos avanzan a la siguiente etapa a la vez. | BT |
| RB-06 | Solo se aceptan evidencias para la etapa marcada como activa. | BT |
| RB-07 | Pistas `PorTiempo` se liberan al transcurrir el tiempo configurado (p. ej. 15 min). | BT |
| RB-08 | Ranking por puntaje desc.; desempate: menor suma de tiempos de respuesta. | Ambos |
| RB-09 |   Una misión necesita al menos una etapa válida para activarse (BT: QR; Trivia: ≥1 categoría). | Ambos |
| RB-10 | El nombre de misión es único en el sistema. | Ambos |
| RB-11 | El `MisionSnapshot` es inmutable desde que se crea la sesión. | Ambos |
| RB-12 | Respuesta de trivia después del cierre del timer → 0 puntos. | Trivia |
| RB-13 | Respuesta de trivia no modificable tras confirmar o expirar el timer. | Trivia |
| RB-14 | Si se elimina una categoría, sus preguntas pasan a «Sin Categoría». | Trivia |
| RB-15 | Los nombres de categoría son únicos. | Trivia |
| RB-16 | Las preguntas se eliminan lógicamente (`isDeleted`), nunca físicamente. | Trivia |
| RB-17 | Puntaje de trivia solo si la respuesta es correcta y llegó antes del cierre del timer. | Trivia |
| RB-18 | Una sesión no puede iniciarse sin al menos un equipo registrado. | Ambos |
| RB-19 | No se aceptan evidencias si la sesión está `Pausada`, `Finalizada` o `Cancelada`. | BT |
| RB-20 | Toda penalización exige motivo de texto obligatorio. | Ambos |
| RB-21 | Una misma pista no se entrega dos veces al mismo equipo en la misma etapa. | BT |
| RB-22 | QR válido solo si coincide con el código del nodo activo. | BT |
| RB-23 | Una pista solo se libera si el equipo está en el nodo correspondiente. | BT |
| RB-24 | El puntaje total del equipo nunca es inferior a cero. | Ambos |
| RB-25 | Todo cambio de puntaje registra `OperadorId` o evento de sistema. | Ambos |
| RB-26 | Sesión `Finalizada` o `Cancelada` → solo lectura (auditoría). | Ambos |
| RB-27 | El operador solo administra sesiones que tiene asignadas. | Ambos |
| RB-28 | Pregunta de trivia: ≥1 opción correcta y ≥2 incorrectas. | Trivia |
| RB-29 | Puntos de trivia se suman al puntaje global de la sesión. | Trivia |
| RB-30 | No se aceptan respuestas si el timer de la pregunta ya inició al conectar. | Trivia |
| RB-31 | En trivia, todos los equipos pueden puntuar si responden correctamente. | Trivia |
| RB-32 | Pregunta lanzada no se cancela hasta que expire el timer o todos respondan. | Trivia |
| RB-33 | Etapa `Trivia` debe tener al menos un `CategoriaId` al configurar la misión. | Trivia |
| RB-34 | Progresión secuencial: al completar una etapa, todos avanzan al siguiente nodo de la misión. | Ambos |
| RB-35 | Rol `Participante` no se asigna vía administración de usuarios (solo inscripción a sesión). | Identidad |
| RB-36 | Email y username únicos en tabla espejo de usuarios administrables. | Identidad |
| RB-37 | Si falla persistencia local tras registro en Keycloak, compensar eliminando el usuario en identity server. | Identidad |

---

## Requerimientos funcionales ampliados (RF-31 … RF-35)

| Código | Requerimiento |
|--------|---------------|
| RF-31 | Registrar usuario administrable en Keycloak y tabla espejo local con invariantes. |
| RF-32 | Asignar/revocar roles `Administrador` y `Operador` (no `Participante` arbitrario). |
| RF-33 | Bloquear/activar usuario (estado sincronizado con Keycloak). |
| RF-34 | Listar y consultar usuarios administrables (solo Administrador). |
| RF-35 | Sesión de misión ejecuta etapas en orden; motor aplica reglas BT o Trivia según `TipoEtapa` activa. |

### Historias nuevas (TO-BE)

| HU | Título |
|----|--------|
| HU-A1 | Configuración de etapa Trivia en misión polimórfica |
| HU-I1 | Registro de usuario administrable (doble commit Keycloak + BD) |
| HU-I2 | Restricción de rol Participante en administración |

---

## Requerimientos no funcionales (RNF-01 … RNF-14)

| Código | Descripción |
|--------|-------------|
| RNF-01 | Frontend React + TypeScript; backend .NET 8. |
| RNF-02 | PostgreSQL + Entity Framework Core. |
| RNF-03 | Tiempo real sobre WebSockets (SignalR). |
| RNF-04 | MediatR + CQRS. |
| RNF-05 | Procesos asíncronos con RabbitMQ. |
| RNF-06 | Arquitectura hexagonal / limpia. |
| RNF-07 | Dominio sin dependencia de infraestructura ni del framework web. |
| RNF-08 | Logging, excepciones globales y FluentValidation. |
| RNF-09 | Cobertura de pruebas backend ≥ 90 %. |
| RNF-10 | Ejecución local con `docker compose up`. |
| RNF-11 | Pipeline CI: compilación y pruebas. |
| RNF-12 | Interfaz clara, usable y coherente con los flujos principales. |
| RNF-13 | Soportar picos de concurrencia de escritura en PostgreSQL (p. ej. trivia simultánea). |
| RNF-14 | Latencia de actualización en tiempo real &lt; 1 s en condiciones normales. |

---

## Historias de usuario — numeración ERS (HU-01 … HU-40)

Usar **siempre** esta numeración en `TRACKER.md`, iteraciones de Fase 1 y documentación académica.

### Catálogo BT (admin)

| HU | Título (ERS) |
|----|----------------|
| HU-01 | Creación de misión |
| HU-02 | Consulta de misiones |
| HU-03 | Modificación de misión |
| HU-04 | Eliminación / desactivación de misión |
| HU-05 | Configuración de nodos (tesoros) |
| HU-06 | Registro de pistas en el nodo |
| HU-07 | Modificación de pistas |
| HU-08 | Eliminación de pistas |
| HU-09 | Liberación automática por tiempo |
| HU-10 | Liberación por ganador de etapa |

### Sesión y juego BT

| HU | Título (ERS) |
|----|----------------|
| HU-11 | Visualización de pistas disponibles (equipo) |
| HU-12 | Creación de sesión |
| HU-13 | Inscripción de equipos |
| HU-14 | Control de inicio de sesión |
| HU-15 | Pausa y reanudación |
| HU-16 | Aplicación de penalizaciones |
| HU-17 | Tablero en tiempo real (equipo) |
| HU-18 | Envío de evidencias (QR) |
| HU-19 | Validación ganador único de etapa |
| HU-20 | Transición automática de fase |
| HU-21 | Ranking y trazabilidad |
| HU-22 | Historial de auditoría |
| HU-23 | Reporte final / cerrar sesión |

### Sala de espera / misión (ERS)

| HU | Título (ERS) |
|----|----------------|
| HU-32 | Control de sala de espera (lista en vivo + expulsar en `EnPreparacion`) |

### Trivia (admin, operador, sistema)

| HU | Título (ERS) |
|----|----------------|
| HU-24 … HU-31 | Banco de preguntas y categorías |
| HU-33 … HU-40 | Sesión trivia jugable, rondas, ranking, transiciones |

---

## Mapeo Entrega 1 (quality-spec) → HU ERS

La tabla antigua de `umbral-quality-spec.md` §14 usaba HU-01…HU-14 **propias de la entrega**. Equivalencias:

| Antigua (entrega) | HU ERS canónica |
|-------------------|-----------------|
| HU-01 Crear misión | HU-01 |
| HU-02 Etapas y pistas | HU-05, HU-06 |
| HU-03 Activar misión | HU-01 / HU-04 |
| HU-04 Crear sesión BT | **HU-12** |
| HU-05 Registrar equipo | **HU-13** |
| HU-06 Iniciar / pausar / finalizar | **HU-14**, **HU-15**, **HU-23** |
| HU-07 Unirse con código | HU-13 (acceso equipo) |
| HU-08 Ver pistas | **HU-11** |
| HU-09 Enviar evidencia | **HU-18** |
| HU-10 Validar evidencia | **HU-19** |
| HU-11 Liberar pistas manual | **RF-15** pista ad-hoc operador (`POST …/pistas-manuales`) |
| HU-12 Penalización | **HU-16** |
| HU-13 Ranking WebSocket | **HU-21**, **HU-17** |
| HU-14 Consumer RabbitMQ | RF-19 |

---

## Fase 1 (dominio) — HUs en curso

| Iteración | HU principal ERS | Estado |
|-----------|------------------|--------|
| iter-01 | HU-12 | ✅ |
| iter-02 | HU-13 | ✅ |
| iter-03 | HU-14, HU-15 | ✅ (+ SignalR SesionHub para estado) |
| iter-04 | HU-16 | ✅ (Bloque A) |
| iter-05 | HU-18 | ✅ (Bloque A) |
| iter-06 | HU-19, HU-20 | ✅ (Bloque A) |
| iter-07 | HU-22, HU-23 | ✅ (Bloque A) |
| Bloque A catálogo | HU-03, HU-07, HU-08 | ✅ |
| Bloque B (parcial) | HU-09 | ✅ dominio + background + SignalR `PistaLiberada` |
| Bloque B (parcial) | HU-10 | ✅ dominio en SubmitEvidencia + SignalR `PistaLiberada` |
| Bloque B (parcial) | RF-15 | ✅ pista ad-hoc operador + SignalR `PistaLiberada` |
| Bloque B (parcial) | HU-17 | ✅ tablero BT: `EtapaAvanzada` + `RankingActualizado` + UI participante (TriviaHub pendiente) |
| Bloque B (parcial) | HU-21 | ✅ ranking en vivo: payload completo en `RankingActualizado` (evidencia / penalización / finalizar) |
| Bloque B (parcial) | HU-32 | ✅ sala de espera: `ParticipantesActualizados` + `ExpulsarParticipante` + SignalR `ParticipanteExpulsado` + UI operador/participante |
| Bloque B (parcial) | HU-33 / HU-38 | ✅ secuencia trivia auto: operador inicia etapa; timer 30 s → transición 5 s → siguiente (BackgroundService); UI live (respuesta = HU-34) |
| Bloque B (parcial) | HU-34 | ✅ encola respuesta: `POST …/trivia/respuestas` → 202 Accepted; UI confirma opción (RB-12/13) |
| Bloque B (parcial) | HU-35 | ✅ MassTransit + RabbitMQ: `ProcesarRespuestaTriviaConsumer` cola `umbral.respuestas-trivia`; retry ×3; mensajes malformados descartados; Testing = bus InMemory |
| Bloque B (parcial) | HU-36 | ✅ puntaje dinámico trivia: `CalculoPuntajeTriviaService` (base 100 + bonus velocidad); solo correcta y a tiempo (RB-17/RB-12); suma a `PuntajeTotal` de sesión (RB-29); persiste `TimestampServidor` + `TiempoRespuestaMs` (insumo HU-39) |
| Bloque B (parcial) | HU-37 | ✅ ranking parcial + feedback: `RankingActualizado` tras cada respuesta (SignalR); feedback personal (correcta/tarde/pts vía poll `EstadoTrivia`); puntajes en BD + fallback REST `GET /ranking`. Opcional no hecho: revelar opción correcta a toda la sala al cerrar ronda |
| Bloque B (parcial) | HU-39 | ✅ desempate RB-08: `RankingService` ordena por puntaje ↓, luego menor `TiempoAcumuladoMs` (solo respuestas a tiempo; fuera de tiempo = 0 útil, RB-12), luego nombre; expuesto en DTO/API/SignalR |
| Bloque B (parcial) | HU-40 | ✅ etapa Trivia en misión: `EtapaTrivia` + `CategoriaId[]` (RB-33); admin `EtapasEditor`/`MisionEtapasPanel`; categorías deben existir al guardar; preguntas activas al activar (RB-09 vía `MisionTriviaValidacion`); snapshot en creación de sesión |
| Bloque C (mobile) | umbral-mobile | ✅ scaffold Expo + Keycloak `umbral-mobile` + lobby/partida (BT QR manual, trivia, ranking, SignalR) en `src/mobile/umbral-mobile` |

**Bloque C:** participante oficial en mobile; web desactivada (`VITE_PARTICIPANTE_WEB_ENABLED=false`). Pendiente: validar escáner QR en dispositivo físico.
