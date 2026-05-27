# UMBRAL — Trazabilidad canónica (ERS ↔ `.cursor` ↔ Fase 1)

**Fuente normativa del proyecto:** `.cursor/specs/umbral-product-spec.md` (producto + RB/RF/RNF) y este documento para códigos unificados.

**Documento académico base:** `docs/ERS_Proyecto_UMBRAL_UCAB.md` (historias HU-01…HU-40; sección §8–§7 alineadas con esta tabla).

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

## Reglas de negocio (RB-01 … RB-32)

| Código | Regla | Modo |
|--------|-------|------|
| RB-01 | Solo se crea sesión BT desde una misión con estado `Activa`. | BT |
| RB-02 | El nombre de equipo es único dentro de la misma sesión. | Ambos |
| RB-03 | Un equipo no puede registrarse en sesión `Finalizada` o `Cancelada`. | Ambos |
| RB-04 | Al validar la primera evidencia correcta, los demás no puntúan en esa etapa. | BT |
| RB-05 | Al completarse un nodo, todos avanzan a la siguiente etapa a la vez. | BT |
| RB-06 | Solo se aceptan evidencias para la etapa marcada como activa. | BT |
| RB-07 | Pistas `PorTiempo` se liberan al transcurrir el tiempo configurado (p. ej. 15 min). | BT |
| RB-08 | Ranking por puntaje desc.; desempate: menor suma de tiempos de respuesta. | Ambos |
| RB-09 | Una misión necesita al menos una etapa para activarse. | BT |
| RB-10 | El nombre de misión es único en el sistema. | BT |
| RB-11 | El `MisionSnapshot` es inmutable desde que se crea la sesión. | BT |
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

### Trivia (admin, operador, sistema)

| HU | Título (ERS) |
|----|----------------|
| HU-24 … HU-31 | Banco de preguntas y categorías |
| HU-32 … HU-40 | Sesión trivia, sala de espera, rondas, ranking, transiciones |

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
| HU-11 Liberar pistas manual | RF-15 / panel operador |
| HU-12 Penalización | **HU-16** |
| HU-13 Ranking WebSocket | **HU-21**, **HU-17** |
| HU-14 Consumer RabbitMQ | RF-19 |

---

## Fase 1 (dominio) — HUs en curso

| Iteración | HU principal ERS |
|-----------|------------------|
| iter-01 | HU-12 |
| iter-02 | HU-13 |
| iter-03 | HU-14, HU-15 |
| iter-04 | HU-16 |
| iter-05 (plan) | HU-18 |
| iter-06 (plan) | HU-19, HU-20 |
| iter-07 (plan) | HU-23 |
