# Entrega 1 — Plan de control y alcance

> **Estado:** plan vigente (alcance reducido aprobado)
> **Última actualización:** 2026-05-28
> **Decisión base:** Entrega 1 demuestra **comunicación frontend ↔ backend ↔ persistencia** con **cobertura backend ≥ 90%**, usando **CRUD de Misiones + Login** como historias de usuario mínimas. Las sesiones en vivo, ranking, "jugar", RabbitMQ, SignalR, mobile, Trivia y E2E se mueven a **Entrega 2**.

---

## 1. Mínimo exigido por el profesor

1. Comunicación real entre **frontend, backend y persistencia**.
2. **Cobertura de pruebas ≥ 90%** (backend) — RNF-09.
3. Algunas **historias de usuario mínimas** demostrables (CRUD).

Todo lo demás (sesiones en vivo, ranking, gameplay) **no se muestra** en Entrega 1.

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
| Dominio (`Umbral.Domain`) | ✅ Fase 1 completa | `docs/fase-1/TRACKER.md` — 170 tests |
| Application (`Umbral.Application`) | ✅ Fase 2 completa | `docs/fase-2/TRACKER.md` — 50 tests |
| Infrastructure (`Umbral.Infrastructure`) | ✅ Fase 3 completa | EF Core + PostgreSQL + Testcontainers |
| API (`Umbral.API`) | ✅ Fase 4 completa | CRUD Misiones, Auth JWT, endpoints sesión |
| Persistencia (PostgreSQL + migraciones) | ✅ | `Persistence/Migrations` |
| Proyectos de test | ✅ 4 proyectos | `Domain/Application/Infrastructure/API .Tests` |
| **Frontend web** | ❌ **No existe** | `src/frontend/` ausente |
| **CI (cobertura)** | ❌ **No existe** | `.github/workflows/` ausente |
| Cobertura real ≥ 90% | ⚠️ **Sin medir** | pendiente correr coverlet |
| CatalogoTrivia (Pregunta/Categoria) | ❌ Vacío (solo `.gitkeep`) | contingencia E1 → §5.1 |
| Mobile / SignalR / RabbitMQ demo / Trivia jugable / E2E | ❌ Fuera de alcance E1 | → Entrega 2 |

---

## 4. Alcance de Entrega 1

### 4.1 DENTRO de alcance

- **HU-01..04 — CRUD Misiones** (crear, listar/consultar, modificar, desactivar). Backend ✅; falta UI.
- **Login con JWT por rol** (admin/operador) y demostración de **403 por rol**. Backend ✅; falta UI.
- **Persistencia real** en PostgreSQL (ya operativa).
- **Frontend web mínimo** que consume la API real (login + pantallas CRUD Misiones).
- **Cobertura backend ≥ 90%** medida y reportada en CI.
- **README** de arranque local + **guion de demo**.
- **CRUD Trivia (contingencia, backend) — HU-24..31.** Banco de preguntas y
  categorías. Se construye desde cero para tener respaldo si el profesor lo
  pide. Detalle y mínimo en **§5.1**.

### 4.2 FUERA de alcance (→ Entrega 2)

- Sesiones en vivo, control inicio/pausa, evidencia, penalización **mostradas en UI**.
- **Ranking en tiempo real / SignalR**.
- **Consumers RabbitMQ**.
- **React Native (mobile)**.
- **Modo Trivia jugable** (sala de espera, rondas, respuestas, ranking, transición) — HU-32..40.
- **E2E con Playwright**.

> Nota: el backend de sesiones (crear/iniciar/pausar/evidencia/penalización/ranking) **ya está implementado y probado**, pero **no se demuestra** en Entrega 1. Es "crédito adelantado" para Entrega 2.

---

## 5. Gaps a cerrar (lo que falta de verdad)

1. **Frontend web** (gap #1, bloqueante para demostrar las 3 capas).
2. **Pipeline CI** que ejecuta tests y reporta cobertura.
3. **Medición + cierre de la brecha de cobertura** hasta ≥ 90%.
4. **README + guion de demo** orientado al profesor.
5. **CRUD Trivia backend** (contingencia): hoy `CatalogoTrivia` está vacío
   (solo `.gitkeep`). Es un vertical completo desde cero.

---

## 5.1 CRUD Trivia — alcance mínimo (contingencia, HU-24..31)

> **Estado actual:** `src/backend/Umbral.Domain/CatalogoTrivia/{Pregunta,Categoria}`
> contienen solo `.gitkeep`. **No hay nada implementado.** Es el ítem más caro
> de Entrega 1 porque atraviesa las 4 capas + migración + tests al 90%.

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
- UI de trivia en el frontend (en Entrega 1, si se pide, se demuestra por
  **Swagger / REST**; la UI mínima de E1 sigue siendo Misiones).

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

## 6. Backlog de acciones (control)

| # | Acción | Tipo | Dep. | Estado |
|---|--------|------|------|--------|
| E1-0 | Oficializar alcance reducido en `quality-spec §13/§14` | doc | — | ✅ |
| E1-0b | Alinear naming de proyectos de test (`quality-spec §2` ✅; `project-rules §1` ⬜) | doc | — | 🔶 |
| E1-1 | Medir cobertura backend real (coverlet) y registrar brecha | build | — | ⬜ |
| E1-2 | Frontend web mínimo: login + CRUD Misiones contra API real | código | E1-1 | ⬜ |
| E1-3 | Pipeline CI: `dotnet test` + reporte cobertura, gate ≥ 90% | devops | E1-1 | ⬜ |
| E1-4 | Cerrar brecha de cobertura con tests faltantes (TDD) | código | E1-1 | ⬜ |
| E1-5 | README arranque local (backend + db + front) | doc | E1-2 | ⬜ |
| E1-6 | Guion de demo para el profesor | doc | E1-2 | ⬜ |
| E1-7 | CRUD Trivia — Dominio `Categoria`/`Pregunta` + tests (TDD) — [doc](iter-e1-07-trivia-dominio.md) | código | — | ✅ (28 tests, 198 total verde) |
| E1-8 | CRUD Trivia — Application (commands/queries/handlers) + tests | código | E1-7 | ⬜ |
| E1-9 | CRUD Trivia — Infrastructure (EF + repos + migración) + tests | código | E1-8 | ⬜ |
| E1-10 | CRUD Trivia — API (`CategoriasController`, `PreguntasController`) + tests | código | E1-9 | ⬜ |

> Marca cada fila a medida que se completa. Este es el tablero de control de Entrega 1.
> **E1-7..E1-10** es la contingencia de Trivia (ver §5.1); se puede posponer si no la piden.

---

## 7. Reordenamiento de fases (efecto del recorte)

El alcance original de la spec metía demasiado en Entrega 1. Con el recorte:

| Fase (implícita) | Contenido | Antes | Ahora |
|------------------|-----------|-------|-------|
| 1–4 | Backend (Dominio → API) | E1 | ✅ E1 (hecho) |
| 5 (nueva) | Frontend web mínimo + CI + cobertura | parte de E1 | **E1** |
| 5b (nueva) | CRUD Trivia backend (HU-24..31, contingencia) | E2 | **E1 (opcional)** |
| 6 | SignalR / ranking tiempo real | E1 | **E2** |
| 7 | Mobile (React Native) | E1 | **E2** |
| 8 | RabbitMQ consumers (demo) | E1 | **E2** |
| 9–10 | Trivia jugable (HU-32..40) + E2E Playwright | E2 | **E2** |

---

## 8. Definition of Done — Entrega 1

- [ ] Solución .NET compila sin warnings; 4 proyectos de test en verde.
- [ ] **Cobertura backend ≥ 90%** sobre el código implementado, reportada por CI.
- [ ] Frontend web hace **login real** y **CRUD de Misiones** contra la API (persistencia real).
- [ ] Se puede demostrar **403 por rol** (operador no crea misiones).
- [ ] PostgreSQL levanta vía docker-compose y la app persiste datos.
- [ ] README permite a un tercero levantar el entorno local.
- [ ] Guion de demo ensayado.

---

## 9. Inconsistencias detectadas (registro)

| # | Inconsistencia | Resolución acordada |
|---|----------------|---------------------|
| 1 | Spec/`project-rules` nombran `Umbral.Integration.Tests` y `Umbral.E2E.Tests`; el repo tiene `Umbral.API.Tests` e `Umbral.Infrastructure.Tests`, sin E2E | Actualizar la spec a la realidad (E1-0b) |
| 2 | `quality-spec §13/§14` describe una Entrega 1 mucho más amplia (SignalR, RabbitMQ, mobile, E2E) que el mínimo del profesor | Oficializar alcance reducido (E1-0) |
| 3 | No existen pruebas de carga / "clickload" en ninguna spec | Fuera de alcance; agregar solo si el profesor lo exige |
| 4 | No existe un documento único de "10 fases"; solo el "Plan de 10 días" (`§15`) y el mapa implícito en `fase-1/TRACKER` | Este PLAN.md y la tabla §7 hacen las veces de roadmap de fases |

---

## 10. Riesgos

- **Cobertura < 90%**: hasta no medir (E1-1) no se conoce la brecha. Riesgo principal de la entrega.
- **Frontend desde cero**: es el mayor esfuerzo nuevo; conviene mantenerlo mínimo (login + CRUD).
- **Docker requerido** para tests de integración (Testcontainers) y para la demo (PostgreSQL).
