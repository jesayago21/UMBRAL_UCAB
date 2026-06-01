# Iteración E1-8 — Application CRUD Trivia (CatalogoTrivia)

**Fase:** Entrega 1 — capa Application (Trivia)
**Fecha:** 2026-05-29
**Fuentes de verdad:** `docs/entrega-1/PLAN.md §5.1`, ERS **HU-24..31**, `iter-e1-07-trivia-dominio.md`, `umbral-backend-spec.md`

> **Metodología:** TDD estricto — 28 tests escritos **antes** de los handlers;
> luego commands/queries/validators/DTOs hasta verde.

---

## Contexto

Con el dominio de Trivia listo (E1-7), esta iteración agrega la capa **Application**
(CQRS + MediatR + FluentValidation) para el banco de preguntas. Sigue el mismo
patrón que `Misiones/` y `Sesion/`: handler `internal`, repos mockeados con
NSubstitute, validadores registrados por assembly scan.

**No incluye** Infrastructure (E1-9) ni API (E1-10).

---

## Casos de uso implementados

### Categorías (HU-28..31)

| Caso de uso | Command/Query | Handler | Validator |
|-------------|---------------|---------|-----------|
| Crear categoría | `CrearCategoriaCommand` | ✅ | ✅ |
| Renombrar categoría | `ActualizarCategoriaCommand` | ✅ | ✅ |
| Eliminar categoría (soft) | `EliminarCategoriaCommand` | ✅ | ✅ |
| Listar categorías | `ListCategoriasQuery` | ✅ | — |
| Obtener por id | `GetCategoriaByIdQuery` | ✅ | — |

### Preguntas (HU-24..27, HU-25 parcial)

| Caso de uso | Command/Query | Handler | Validator |
|-------------|---------------|---------|-----------|
| Crear pregunta | `CrearPreguntaCommand` | ✅ | ✅ |
| Modificar pregunta | `ActualizarPreguntaCommand` | ✅ | ✅ |
| Eliminar pregunta (soft) | `EliminarPreguntaCommand` | ✅ | ✅ |
| Listar preguntas (filtros) | `ListPreguntasQuery` | ✅ | — |
| Obtener por id | `GetPreguntaByIdQuery` | ✅ | — |

---

## Estructura de archivos

```
src/backend/Umbral.Application/CatalogoTrivia/
├── Models/
│   ├── TriviaDtos.cs          ← CategoriaDto, PreguntaDto, OpcionRespuestaInput
│   └── TriviaMappings.cs      ← ToDto, ParseDificultad, ToOpciones
├── Categorias/
│   ├── Commands/{Crear,Actualizar,Eliminar}Categoria/
│   └── Queries/{List,Get}Categoria*/
└── Preguntas/
    ├── Commands/{Crear,Actualizar,Eliminar}Pregunta/
    └── Queries/{List,Get}Pregunta*/

tests/Umbral.Application.Tests/CatalogoTrivia/
├── Builders/TriviaTestBuilder.cs
├── Categorias/{Commands,Queries}/*Tests.cs
└── Preguntas/{Commands,Queries}/*Tests.cs
```

---

## Reglas de negocio cubiertas en Application

| ID | Regla | Dónde |
|----|-------|-------|
| RB-15 | Nombre de categoría único | `CrearCategoria` / `ActualizarCategoria` → `ExistsByNombreAsync` |
| RB-14 | Al eliminar categoría, preguntas quedan "Sin Categoría" | `EliminarCategoria` → `QuitarCategoria()` en cada pregunta |
| RB-28 | Exactamente una opción correcta | Validators + dominio |
| — | Soft delete no visible en listados/get | Handlers filtran `Eliminada == true` → `NotFoundException` |
| — | Categoría debe existir al crear/asignar pregunta | `CrearPregunta` / `ActualizarPregunta` |

> **Diferida (Entrega 2):** HU-26 *"no editar pregunta en sesión activa"* —
> requiere puerto `HasSesionesActivasAsync` en trivia (patrón Misiones).

---

## Tests agregados

| Área | Archivos | Tests |
|------|----------|-------|
| Categorías — commands | 4 archivos | 10 |
| Categorías — queries | 2 archivos | 3 |
| Preguntas — commands | 5 archivos | 12 |
| Preguntas — queries | 2 archivos | 3 |
| Builder | `TriviaTestBuilder.cs` | — |

**Total Application tras la iteración: 79/79 ✅** (51 previos + 28 nuevos)
**Total solución: 330/330 ✅**

```powershell
dotnet test tests/Umbral.Application.Tests --filter "FullyQualifiedName~CatalogoTrivia"
# Passed!  Failed: 0, Passed: 28
```

---

## Decisiones de diseño

1. **Mismo patrón CQRS que Misiones:** `Result<Guid>` en commands, DTOs en queries,
   `NotFoundException` / `DomainException` propagadas al middleware API.
2. **`OpcionRespuestaInput`** en Application (no en dominio): el dominio recibe
   `OpcionRespuesta` VOs construidos por `TriviaMappings.ToOpciones`.
3. **`EliminarCategoria` orquesta RB-14:** el handler itera preguntas de la
   categoría y llama `QuitarCategoria()` antes del soft delete.
4. **Listados excluyen eliminados:** coherente con soft delete de auditoría.
5. **MediatR auto-registra handlers** vía `AddApplication()` — no hay wiring manual.

---

## Historias de usuario (ERS) — esta iteración

| HU | Título | Capa esta iteración | Estado |
|----|--------|---------------------|--------|
| HU-24 | Registrar pregunta | Application + tests | 🔶 (falta Infra/API) |
| HU-25 | Consultar preguntas | Queries + tests | 🔶 |
| HU-26 | Modificar pregunta | Application + tests | 🔶 |
| HU-27 | Eliminar pregunta | Application + tests | 🔶 |
| HU-28 | Registrar categoría | Application + tests | 🔶 |
| HU-29 | Consultar categorías | Queries + tests | 🔶 |
| HU-30 | Modificar categoría | Application + tests | 🔶 |
| HU-31 | Eliminar categoría | Application + tests | 🔶 |

---

## Pendiente — siguiente iteración

- **E1-9 Infrastructure:** EF configs, `CategoriaRepository`, `PreguntaRepository`,
  migración `AddCatalogoTrivia` + tests con Testcontainers.
- **E1-10 API:** `CategoriasController`, `PreguntasController`
  (`[Authorize(Roles="Administrador")]`) + tests de integración.
