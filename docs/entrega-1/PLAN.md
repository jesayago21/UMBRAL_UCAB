# Entrega 1 — Plan de control y alcance

> **Estado:** plan vigente (alcance reducido aprobado)
> **Última actualización:** 2026-05-30
> **Decisión base:** Entrega 1 demuestra **comunicación frontend ↔ backend ↔ persistencia** con **cobertura backend ≥ 90%**: catálogo admin (misiones + trivia), **login OIDC**, **pantalla operador mínima** (sesión BT sin SignalR) y, si hay tiempo, **login mobile** (solo auth). Gameplay completo, SignalR, RabbitMQ demo y E2E → **Entrega 2**.

---

## 1. Mínimo exigido por el profesor

1. Comunicación real entre **frontend, backend y persistencia**.
2. **Cobertura de pruebas ≥ 90%** (backend) — RNF-09.
3. Algunas **historias de usuario mínimas** demostrables (CRUD).

Lo que **no** se muestra en E1: ranking en vivo (SignalR), trivia jugable, evidencia QR en mobile, E2E.

---

## 2. Estándar de desarrollo (TDD) — recordatorio operativo

Todas las HU siguen el ciclo TDD definido en `.cursor/specs/umbral-quality-spec.md`:

1. Escribir los **casos de prueba** de la HU (rojo).
2. Ejecutarlos y verlos fallar.
3. Implementar el método/HU **hasta que pasen** (verde).
4. Refactor manteniendo verde.

Reglas no negociables:

- Estructura **AAA** comentada en cada test.
- Nomenclatura `[Metodo]_[Escenario]_[ResultadoEsperado]`.
- **Builders** para construir entidades en pruebas (sin setup inline repetido).
- Los tests viven en **proyectos aparte** que referencian solo lo necesario.

---

## 3. Estado actual del repositorio (verificado)

| Capa / artefacto | Estado | Evidencia |
|------------------|--------|-----------|
| Dominio (`Umbral.Domain`) | ✅ Fase 1 + Trivia | 198 tests Domain |
| Application (`Umbral.Application`) | ✅ Fase 2 + Trivia | 79 tests Application |
| Infrastructure (`Umbral.Infrastructure`) | ✅ EF + Trivia + Keycloak | 14 tests Testcontainers |
| API (`Umbral.API`) | ✅ Misiones + Sesiones + Trivia + Keycloak | 60 tests API |
| Persistencia (PostgreSQL + migraciones) | ✅ | `Persistence/Migrations` + `AddCatalogoTrivia` |
| Proyectos de test | ✅ 4 proyectos | **385 tests** en verde |
| **Frontend web** (`umbral-web`) | 🔶 Admin CRUD ✅ | [E1-2](iter-e1-02-frontend-crud.md); falta OIDC, operador, afinado |
| **CI (cobertura)** | ✅ | [E1-3](iter-e1-03-ci-coverage.md) — `.github/workflows/ci.yml` |
| Cobertura real ≥ 90% | ✅ **96% total** | [E1-1/E1-4](iter-e1-01-cobertura-baseline.md) — `scripts/run-coverage.ps1` |
| CatalogoTrivia (Pregunta/Categoria) | ✅ Backend completo | E1-7..E1-10 |
| Keycloak OIDC | ✅ Backend + web OIDC | E1-K1..K4 — [doc](iter-e1-K4-oidc-web.md) |
| Pantalla operador (sesiones BT) | ⬜ | **E1-2b** — API sesiones ✅ |
| Mobile login (opcional) | ⬜ | **E1-M1** si hay tiempo |
| SignalR / RabbitMQ demo / Trivia jugable / E2E / mobile gameplay | ❌ E2 | → Entrega 2 |

---

## 4. Alcance de Entrega 1

### 4.1 DENTRO de alcance

- **HU-01..04 — CRUD Misiones** (crear, listar/consultar, modificar, desactivar). Backend ✅; falta UI.
- **Autenticación con Keycloak (OIDC)** por rol (admin/operador) y demostración de
  **403 por rol**. Reemplaza el JWT propio. Detalle en **§5.2** y
  `.cursor/skills/keycloak-auth-skill.md`.
- **Persistencia real** en PostgreSQL (ya operativa).
- **Frontend web** (`umbral-web`): **login OIDC** + CRUD **Misiones** + CRUD **Trivia**
  (categorías y preguntas) — E1-2 ✅ base; **E1-K4** cierra auth real.
- **Pantalla operador (mínima)** — E1-2b: crear sesión BT, equipos, iniciar/pausar/
  reanudar/finalizar/cancelar, ver **ranking por polling** (sin SignalR). Backend
  de sesiones ✅ (`SesionesController`).
- **Mobile login (opcional)** — E1-M1: scaffold Expo + OIDC Keycloak; **sin**
  gameplay ni QR (→ E2).
- **Afinado** del admin ya entregado (mensajes, UX, redirect por rol) — E1-2a,
  en paralelo con K4/2b.
- **Cobertura backend ≥ 90%** medida y reportada en CI.
- **README** de arranque local + **guion de demo**.
- **CRUD Trivia (contingencia, backend) — HU-24..31.** Banco de preguntas y
  categorías. Se construye desde cero para tener respaldo si el profesor lo
  pide. Detalle y mínimo en **§5.1**.

### 4.2 FUERA de alcance (→ Entrega 2)

- **Dashboard operador completo** (SignalR, sala de espera trivia, panel en vivo).
- **Ranking en tiempo real / SignalR** (en E1 solo GET `/ranking` con refresh manual).
- **Evidencia QR** y flujo **EquipoParticipante** en mobile.
- **Consumers RabbitMQ** en demo.
- **Modo Trivia jugable** (HU-32..40).
- **E2E con Playwright**.
- **Vitest / cobertura frontend ≥ 80%** (gate E2).

> El backend de sesiones (incl. evidencia y penalización) **ya está implementado**.
> E1-2b demuestra el **subconjunto operador** vía REST; el resto de la UX en vivo → E2.

---

## 5. Gaps a cerrar (lo que falta de verdad)

1. **E1-2a** — Afinar UI admin (E1-2).
3. **E1-2b** — Pantalla operador + lectura misiones activas para Operador (API mínima).
4. **E1-M1** *(opcional)* — Mobile solo login.
5. **README + guion de demo** (E1-5, E1-6).

---

## 5.1 CRUD Trivia — alcance mínimo (contingencia, HU-24..31)

> **Estado actual:** dominio ✅ ([iter-e1-07](iter-e1-07-trivia-dominio.md)).
> Faltan Application, Infrastructure (migración) y API antes de poder conectar
> el frontend de trivia.

### Qué SÍ entra (mínimo)

**Categoría — HU-28..31:**
- Crear categoría (nombre). **RB-15: nombre único.**
- Listar / consultar categorías.
- Modificar categoría (nombre).
- Eliminar categoría → **RB-14: sus preguntas pasan a "Sin Categoría"** (categoría opcional/nullable en la pregunta).

**Pregunta — HU-24..27:**
- Crear pregunta: enunciado, **opciones de respuesta**, **una opción correcta**, dificultad, categoría (opcional).
- Listar / consultar con **filtro por categoría y dificultad** (HU-25).
- Modificar pregunta (enunciado, opciones, correcta, dificultad, categoría).
- Eliminar pregunta.

### Qué NO entra (es Entrega 2 — HU-32..40)

- Jugar la trivia: sesión trivia, sala de espera, lanzar rondas, recibir
  respuestas, timer, puntaje, ranking, transición y desempate (**RB-13** y demás).
- **Gameplay** de trivia en UI (sala, rondas, timer) — Entrega 2. En Entrega 1
  la UI de trivia es solo **administración del banco** (CRUD categorías/preguntas),
  igual que el CRUD de misiones.

### Desglose TDD por capa (orden de construcción)

1. **Dominio** (`CatalogoTrivia`): agregados `Categoria` y `Pregunta`, VOs
   (`CategoriaId`, `PreguntaId`, `OpcionRespuesta`, dificultad), invariantes
   (RB-14, RB-15, exactamente una opción correcta) + `Umbral.Domain.Tests`.
2. **Application**: commands/queries + validators + handlers
   (`CrearCategoria`, `ActualizarCategoria`, `EliminarCategoria`,
   `ListCategorias`, `CrearPregunta`, `ActualizarPregunta`, `EliminarPregunta`,
   `GetPreguntaById`, `ListPreguntas`) + `Umbral.Application.Tests`.
3. **Infrastructure**: configuraciones EF Core, `IPreguntaRepository` /
   `ICategoriaRepository`, **migración nueva** + `Umbral.Infrastructure.Tests`.
4. **API**: `CategoriasController` y `PreguntasController`
   (`/api/v1/categorias`, `/api/v1/preguntas`), `[Authorize(Roles="Administrador")]`
   + `Umbral.API.Tests`.

> Cada paso sigue TDD (test primero). La cobertura del nuevo código debe entrar
> ≥ 90% para no bajar la cobertura total.

---

## 5.2 Autenticación con Keycloak (OIDC) — reemplaza el JWT propio

> Guía técnica completa: `.cursor/skills/keycloak-auth-skill.md`.

**Decisiones acordadas:**

| Tema | Decisión |
|------|----------|
| Flujo | Authorization Code + PKCE (el frontend redirige al login del realm) |
| Usuarios / roles | En el realm `umbral` de Keycloak (se elimina tabla `usuarios` + BCrypt) |
| API | Resource server: valida el token por `Authority`/JWKS; no emite tokens |
| Tests | `Umbral.API.Tests` siguen con `TestAuthHandler`; **no** se levanta Keycloak en CI |
| JWT propio | Se **elimina** (`AuthController.Login`, `JwtTokenIssuer`, `JwtOptions`, `Usuario`, seeder, BCrypt) |

**Qué se construye / cambia:**

1. **Keycloak en `docker-compose`** + realm export (`docker/keycloak/umbral-realm.json`)
   con roles (`Administrador`, `Operador`, `EquipoParticipante`), clients
   (`umbral-web`, `umbral-api`) y usuarios demo.
2. **API**: `AddJwtBearer` con `Authority` del realm + mapeo de `realm_access.roles`
   a claims `role`. Se eliminan los artefactos del JWT propio + migración que
   dropea la tabla `usuarios`.
3. **Frontend** (en E1-2): login OIDC (`oidc-client-ts` / `react-oidc-context`);
   el token se adjunta como `Bearer` igual que hoy.

> **Demostrable:** login real distinto admin/operador y **403 por rol** en catálogo.
> El smoke de Keycloak (login real) se documenta en README/guion; la cobertura ≥ 90%
> se mide sobre el código de negocio, no sobre el wiring de Keycloak.

---

## 6. Backlog de acciones (control)

### 6.1 Orden recomendado de ejecución (no seguir la tabla al pie de la letra)

El objetivo de Entrega 1 es **demostrar frontend ↔ backend ↔ persistencia**
con **cobertura ≥ 90%**, para **Misiones y Trivia (banco)**. Por eso **no**
conviene saltar a E1-8 sin medir cobertura ni dejar el frontend solo en misiones.

```
Fase A — Línea base (primero)
  E1-1   Medir cobertura actual (saber brecha antes y después de Trivia)
  E1-3   CI con reporte de cobertura (en paralelo si quieres)

Fase B — Backend listo para que el front consuma
  E1-8 → E1-9 → E1-10    Trivia: Application → Infra+migración → API
  E1-K1 → E1-K2 → E1-K3  Keycloak: compose+realm → API valida token → eliminar JWT propio
  (Misiones: API ya en main ✅)

Fase C — Cierre de calidad backend
  E1-4   Re-medir y cerrar brecha hasta ≥ 90% (después de E1-10 y E1-K3)

Fase D — Frontend web (orden acordado 2026-05-30)
  E1-2   CRUD Misiones + CRUD Trivia ✅
  E1-K4  Login OIDC Keycloak (redirect por rol)
  E1-2a  Afinar UI admin (mensajes, flujos, 403 claro)
  E1-2b  Pantalla operador — sesión BT mínima (sin SignalR)
  E1-M1  *(opcional, si hay tiempo)* Mobile — solo login OIDC

Fase E — Entrega
  E1-5   README (incluye arranque de Keycloak + web + operador)
  E1-6   Guion de demo (admin + operador + 403 + opcional mobile login)
```

**Siguiente paso inmediato:** **E1-2a/2b** → E1-5/E1-6. **E1-M1** solo si sobra tiempo.

### 6.2 Tabla de ítems

| # | Acción | Tipo | Dep. | Fase | Estado |
|---|--------|------|------|------|--------|
| E1-0 | Oficializar alcance reducido en `quality-spec §13/§14` | doc | — | — | ✅ |
| E1-0b | Alinear naming de proyectos de test en spec y `project-rules` | doc | — | — | ✅ |
| E1-1 | Medir cobertura backend real (coverlet) y registrar brecha — [doc](iter-e1-01-cobertura-baseline.md) | build | — | **A** | ✅ |
| E1-3 | Pipeline CI: `dotnet test` + reporte cobertura, gate ≥ 90% — [doc](iter-e1-03-ci-coverage.md) | devops | E1-1 | **A** | ✅ |
| E1-8 | CRUD Trivia — Application (commands/queries/handlers) + tests — [doc](iter-e1-08-trivia-application.md) | código | E1-7 | **B** | ✅ |
| E1-9 | CRUD Trivia — Infrastructure (EF + repos + migración) + tests — [doc](iter-e1-09-trivia-infrastructure.md) | código | E1-8 | **B** | ✅ |
| E1-10 | CRUD Trivia — API (`CategoriasController`, `PreguntasController`) + tests — [doc](iter-e1-10-trivia-api.md) | código | E1-9 | **B** | ✅ |
| E1-K1 | Keycloak en `docker-compose` + realm export (roles, clients, usuarios demo) — [doc](iter-e1-0K-keycloak.md) | devops | — | **B** | ✅ |
| E1-K2 | API valida token Keycloak (`Authority`/JWKS) + mapeo `realm_access.roles` | código | E1-K1 | **B** | ✅ |
| E1-K3 | Eliminar JWT propio (`AuthController`, `JwtTokenIssuer`, `Usuario`, BCrypt) + migración drop `usuarios` | código | E1-K2 | **B** | ✅ |
| E1-4 | Cerrar brecha de cobertura con tests faltantes (TDD) — total **96%** — [doc](iter-e1-01-cobertura-baseline.md) | código | E1-10, E1-K3 | **C** | ✅ |
| E1-2 | Frontend: CRUD **Misiones** + CRUD **Trivia** (categorías/preguntas) — [doc](iter-e1-02-frontend-crud.md) | código | E1-10 | **D** | ✅ |
| E1-K4 | Frontend login **OIDC** contra Keycloak (`react-oidc-context`); redirect admin/operador — [doc](iter-e1-K4-oidc-web.md) | código | E1-K2, E1-2 | **D** | ✅ |
| E1-2a | Afinar UI admin (E1-2): errores, estados vacíos, navegación | código | E1-2 | **D** | ⬜ |
| E1-2b | Pantalla **operador**: sesión BT mínima + ranking poll — [doc](iter-e1-02b-operador-sesiones.md) | código | E1-K4 | **D** | ⬜ |
| E1-M1 | **(Opcional)** Mobile `umbral-mobile`: solo login OIDC — [doc](iter-e1-M1-mobile-login-opcional.md) | código | E1-K4 | **D** | ⬜ |
| E1-5 | README arranque local (backend + db + Keycloak + web [+ mobile]) | doc | E1-K4, E1-2b | **E** | ⬜ |
| E1-6 | Guion de demo (admin + operador + 403; mobile opcional) | doc | E1-5 | **E** | ⬜ |
| E1-7 | CRUD Trivia — Dominio — [doc](iter-e1-07-trivia-dominio.md) | código | — | **B** | ✅ |

> Marca cada fila a medida que se completa. Usar **§6.1** como orden de trabajo.

---

## 7. Reordenamiento de fases (efecto del recorte)

El alcance original de la spec metía demasiado en Entrega 1. Con el recorte:

| Fase (implícita) | Contenido | Antes | Ahora |
|------------------|-----------|-------|-------|
| 1–4 | Backend (Dominio → API) | E1 | ✅ E1 (hecho) |
| 5 | Frontend admin (Misiones + Trivia) + CI + cobertura | E1 | **E1** (E1-2 ✅) |
| 5b | CRUD Trivia backend (HU-24..31) | E2 | **E1** ✅ |
| 5c | Operador sesiones BT mínimo (UI REST) | E2 | **E1** (E1-2b) |
| 5d | Mobile login opcional | E2 | **E1** opcional (E1-M1) |
| 6 | SignalR / ranking tiempo real | E1 | **E2** |
| 7 | Mobile gameplay (QR, trivia, sesión equipo) | E1 | **E2** |
| 8 | RabbitMQ consumers (demo) | E1 | **E2** |
| 9–10 | Trivia jugable (HU-32..40) + E2E Playwright | E2 | **E2** |

---

## 8. Definition of Done — Entrega 1

- [ ] Solución .NET compila sin warnings; 4 proyectos de test en verde.
- [ ] **Cobertura backend ≥ 90%** sobre el código implementado, reportada por CI.
- [x] **Login real con Keycloak (OIDC)** distinto admin/operador (**E1-K4**).
- [x] Frontend web: **CRUD Misiones** y **CRUD banco Trivia** (**E1-2**).
- [ ] **Pantalla operador** mínima: crear sesión BT, equipos, controles, ranking poll (**E1-2b**).
- [ ] Se puede demostrar **403 por rol** (operador no entra a `/admin/*`).
- [ ] *(Opcional)* **Mobile** solo login (**E1-M1**).
- [ ] PostgreSQL **y Keycloak** levantan vía docker-compose; la app persiste datos
      y valida tokens del realm.
- [ ] README permite a un tercero levantar el entorno local.
- [ ] Guion de demo ensayado.

---

## 9. Inconsistencias detectadas (registro)

| # | Inconsistencia | Resolución |
|---|----------------|------------|
| 1 | Spec/`project-rules` nombraban `Integration.Tests` / `E2E.Tests` | ✅ Alineado a `Infrastructure.Tests` + `API.Tests` (E1-0b) |
| 2 | `quality-spec` Entrega 1 demasiado amplia vs. profesor | ✅ Alcance en este PLAN + §13/§14 quality-spec (E1-0) |
| 3 | **"Clickload"** mal entendido como pruebas de carga | ✅ Es **Keycloak** (auth). **Decisión: Keycloak entra en Entrega 1** y reemplaza al JWT propio (E1-K1..K4). Ver `§5.2` y `quality-spec §13.1` |
| 4 | "Plan de 10 días / 10 fases" en quality-spec §15 | ✅ Eliminado; roadmap único = este `PLAN.md` §6.1 |

### Autenticación — Keycloak (decisión Entrega 1)

El **JWT propio** (`iter-04-05b`) **se reemplaza** por **Keycloak (OIDC)**. El
trabajo está en el backlog como **E1-K1..E1-K4** (§6.2) y detallado en **§5.2**.
El código del JWT propio se elimina en **E1-K3**.

---

## 10. Riesgos

- **Cobertura < 90%**: **resuelto** — E1-1 midió baseline (87.6%) y E1-4 la cerró a **96%** total (385 tests). Falta solo automatizar el gate en CI (E1-3).
- **Frontend desde cero**: es el mayor esfuerzo nuevo; conviene mantenerlo mínimo (login + CRUD).
- **Keycloak (nuevo)**: curva de aprendizaje (realm, clients, mapeo de roles, OIDC en
  el front). Mitigación: realm export versionado, `TestAuthHandler` en tests (no se
  levanta Keycloak en CI), y mantener el flujo simple (Auth Code + PKCE).
- **Docker requerido** para tests de integración (Testcontainers) y para la demo
  (PostgreSQL + Keycloak).
