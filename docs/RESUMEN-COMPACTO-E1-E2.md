# UMBRAL — Resumen compacto de fases e iteraciones

Documento único de referencia para estado funcional, trazabilidad y alcance por entrega.

- Históricos detallados: `docs/fase-*`, `docs/entrega-1/iter-*`, `docs/TRAZABILIDAD.md`
- Documento académico base: `docs/ERS_Proyecto_UMBRAL_UCAB.md`
- Este resumen prioriza "lo esencial para construir, demostrar y defender".

---

## 1) Objetivo del proyecto (esencial)

UMBRAL gestiona sesiones de Búsqueda del Tesoro y Trivia con tres roles:

- `Administrador`: catálogo (misiones, etapas, pistas, categorías, preguntas)
- `Operador`: creación y orquestación de sesiones
- `Participante`: unión a sesión y vista de partida (web temporal en E1)

Arquitectura y stack:

- Backend .NET 8 con enfoque hexagonal (Domain/Application/Infrastructure/API)
- PostgreSQL + EF Core
- Frontend React/Vite (`umbral-web`)
- Auth Keycloak OIDC

---

## 2) Estado global por entregas

| Entrega | Estado | Resultado clave |
|---|---|---|
| E1 | En cierre | Frontend + backend + persistencia demostrables, cobertura backend >= 90% |
| E2 | Pendiente | Tiempo real, gameplay completo, mobile final, E2E |

---

## 3) Fases e iteraciones (resumen ejecutivo)

## Fases 1-4 (base backend)

| Fase | Objetivo | Estado |
|---|---|---|
| Fase 1 | Dominio sesiones BT (ciclo base y reglas) | Completada |
| Fase 2 | Application (commands/queries/handlers/validación) | Completada |
| Fase 3 | Infrastructure (EF, repos, migraciones, tests) | Completada |
| Fase 4 | API + integración end-to-end backend | Completada |

## Entrega 1 (iteraciones E1)

| Iteración | Tema | Estado |
|---|---|---|
| E1-01 / E1-04 | Cobertura backend + cierre de brecha | Completada (>= 90%) |
| E1-02 | Frontend CRUD admin (misiones + trivia banco) | Completada |
| E1-02a | Polish admin | Completada |
| E1-02b | Operador sesiones (BT mínimo en plan, luego expandido) | Completada y ampliada |
| E1-03 | CI con gate de cobertura | Completada |
| E1-07..10 | Trivia backend (dominio -> API) | Completada |
| E1-K1..K4 | Keycloak (infra + backend + web OIDC) | Completada |
| E1-M1 | Mobile login opcional | No ejecutada (reemplazada por web participante temporal) |

---

## 4) Historias de usuario (HU) cubiertas en E1

Fuente de códigos: `docs/TRAZABILIDAD.md` y `docs/ERS_Proyecto_UMBRAL_UCAB.md`.

| Bloque HU | Cobertura E1 |
|---|---|
| HU-01..HU-04 (CRUD misiones) | Cubiertas |
| HU-05..HU-10 (etapas/pistas de misión) | Cubiertas en administración base |
| HU-12..HU-16 (crear sesión, unir participantes, iniciar/pausar/reanudar, penalización) | Cubiertas en backend y operador |
| HU-18..HU-23 (evidencia, ranking, cierre/auditoría) | Backend cubierto; UI parcial según alcance E1 |
| HU-24..HU-31 (CRUD trivia banco) | Cubiertas |
| HU-32 (sala de espera + expulsar) | ✅ E1 (lista viva + `ExpulsarParticipante`) |
| HU-33 (secuencia trivia + timer) | ✅ parcial E1 (secuencia auto + live UI) |
| HU-34 (enviar respuesta) | ✅ (202 + UI; cola = HU-35) |
| HU-35, HU-36 (cola + puntaje dinámico) | ✅ hechas |
| HU-37 (ranking parcial + feedback) | ✅ (SignalR ranking + feedback personal; revelación grupal de opción correcta = opcional no hecha) |
| HU-39 (desempate por tiempo acumulado) | ✅ |
| HU-40 (config etapa Trivia en misión) | ✅ (admin + RB-09 al activar) |
| Mobile participante (`umbral-mobile`) | ✅ scaffold + flujo (QR cámara pendiente) |

Notas:

- En E1 se incorporó sesión de trivia en operador y flujo de participante web para unión/visualización de preguntas.
- App Expo en `src/mobile/umbral-mobile` (Keycloak + lobby + partida). Web participante sigue habilitado hasta apagar el flag.

---

## 5) Requisitos funcionales (RF) y no funcionales (RNF)

## RF (alto nivel)

| Grupo RF ERS | Estado E1 |
|---|---|
| Gestión de catálogo (misiones y trivia banco) | Cumplido |
| Gestión de sesiones y ciclo de vida | Cumplido (sin tiempo real) |
| Inscripción y control de acceso por código | Cumplido |
| Ranking y cierre de sesión | Cumplido en backend; actualización no realtime |
| Eventos en vivo por WebSocket / colas demo operativa | Diferido a E2 |

## RNF clave

| RNF | Estado |
|---|---|
| RNF-06/07 Arquitectura limpia | Cumplido |
| RNF-08 Manejo de errores/validación | Cumplido |
| RNF-09 Cobertura backend >= 90% | Cumplido |
| RNF-10 Ejecución local por Docker | Cumplido |
| RNF-11 CI build+test | Cumplido |
| RNF-03/14 tiempo real < 1s | Diferido a E2 |

---

## 6) Qué se entrega realmente en E1 (demo)

1. Login OIDC por rol (`admin`, `operador`, `equipo`) con Keycloak
2. Admin web:
   - CRUD misiones (incluye etapas/pistas)
   - CRUD categorías/preguntas de trivia
3. Operador web:
   - Crear sesiones BT y Trivia
   - Abrir inscripción, iniciar/pausar/reanudar/finalizar/cancelar
   - Ver participantes y ranking por refresco manual
4. Equipo web (temporal):
   - Ver sesiones abiertas BT/Trivia
   - Unirse con código de sesión
   - Ver partida base y preguntas de trivia en modo lectura

---

## 7) Diferido a E2 (sin ambigüedad)

- SignalR/WebSockets para eventos en vivo (ranking, timers, transición en cliente)
- Gameplay completo BT (QR/evidencias en experiencia final)
- Trivia jugable completa (lanzamiento de pregunta, respuesta, cierre, desempate)
- Mobile final como cliente principal de equipo
- E2E Playwright y endurecimiento UX de tiempo real

---

## 8) Evidencias y comandos de verificación

## Backend

- Build: `dotnet build Umbral.sln`
- Tests: `dotnet test Umbral.sln`
- Cobertura local: `.\scripts\run-coverage.ps1 -Threshold 90`
- Cobertura CI/Linux: `bash scripts/run-coverage.sh --threshold 90`

## Infra

- Levantar stack: `docker compose up -d`
- Servicios mínimos para demo: Postgres + Keycloak + API + `umbral-web`

---

## 9) Reorganización documental sugerida

Para evitar duplicidad entre "plan", "trazabilidad" e "iteraciones":

1. Mantener este documento como "fuente única operativa".
2. Mantener `TRAZABILIDAD.md` solo como matriz normativa de códigos (HU/RB/RNF).
3. Mantener `PLAN.md` como backlog vivo corto (pendientes y orden), no como histórico largo.
4. Conservar `iter-*` y `fase-*` como historial técnico de detalle (no lectura obligatoria).

---

## 10) Próximo paso recomendado

Si deseas, el siguiente paso es aplicar la poda completa:

- mover iteraciones cerradas a `docs/archive/`
- dejar en `docs/` solo:
  - `RESUMEN-COMPACTO-E1-E2.md`
  - `TRAZABILIDAD.md`
  - `README.md` (uso/demostración)
  - `ERS_Proyecto_UMBRAL_UCAB.md` (marco académico)
