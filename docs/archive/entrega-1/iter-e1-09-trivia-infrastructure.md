# Iteración E1-9 — Infrastructure CRUD Trivia (CatalogoTrivia)

**Fase:** Entrega 1 — capa Infrastructure (Trivia)
**Fecha:** 2026-05-29
**Fuentes de verdad:** `iter-e1-08-trivia-application.md`, `umbral-quality-spec.md §7`, `PLAN.md §5.1`

> **Metodología:** TDD con **Testcontainers + PostgreSQL real** (sin mocks de
> repositorio). 7 tests escritos antes de configs/repos/migración.

---

## Contexto

Con Application listo (E1-8), esta iteración persiste `Categoria` y `Pregunta`
en PostgreSQL: configuraciones EF Core, repositorios, migración y tests de
integración en `Umbral.Infrastructure.Tests`.

---

## Objetos implementados

### Persistencia EF Core

| Artefacto | Detalle |
|-----------|---------|
| `CategoriaConfiguration` | Tabla `categorias` (id, nombre, eliminada). Índice único parcial `nombre` WHERE `eliminada = false` (RB-15) |
| `PreguntaConfiguration` | Tabla `preguntas` (id, enunciado, dificultad, categoria_id FK nullable, eliminada, **opciones_json jsonb**) |
| `CategoriaIdValueConverter` / `PreguntaIdValueConverter` | Convención global en `UmbralDbContext` |
| `OpcionesRespuestaJsonValueConverter` | Serializa `_opciones` como JSON (VO inmutable; patrón similar a `MisionSnapshot`) |
| `OpcionesRespuestaPersistence` | Helper JSON ↔ `List<OpcionRespuesta>` |

### Repositorios

| Repositorio | Métodos |
|-------------|---------|
| `CategoriaRepository` | `FindById`, `FindAll`, `ExistsByNombre` (excluye eliminadas), `Save` |
| `PreguntaRepository` | `FindById`, `FindAll`, `FindByCategoria`, `Save` |

### Migración

- **`AddCatalogoTrivia`**: crea tablas `categorias` y `preguntas` + FK + índice único parcial.

### DI

- `ICategoriaRepository` → `CategoriaRepository`
- `IPreguntaRepository` → `PreguntaRepository`

---

## Decisiones de diseño

1. **Opciones como JSONB** en lugar de tabla hija: el VO `OpcionRespuesta` es
   inmutable y pertenece al agregado `Pregunta`; JSON evita tabla extra y
   constructor ORM en el dominio (mismo criterio que snapshots en sesiones).
2. **Índice único parcial** en `categorias.nombre`: permite reutilizar nombre
   tras soft delete; `ExistsByNombreAsync` también excluye eliminadas.
3. **Value comparer** en `_opciones`: EF detecta cambios al `ModificarContenido`.
4. **Tests con Postgres compartido** (fixture): cada test usa nombres únicos
   (`Guid`) cuando persiste categorías para evitar colisiones entre tests.

---

## Tests agregados (`Umbral.Infrastructure.Tests`)

| Archivo | Tests | Cubre |
|---------|-------|-------|
| `CategoriaRepositoryTests.cs` | 4 | save/find, unicidad activa, nombre reutilizable tras soft delete, find all |
| `PreguntaRepositoryTests.cs` | 3 | save/find con opciones JSON, find by categoría, update opciones |

**Total Infrastructure: 14/14 ✅** (7 previos + 7 nuevos)
**Total solución: 337/337 ✅**

```powershell
dotnet test tests/Umbral.Infrastructure.Tests
# Passed!  Failed: 0, Passed: 14
```

---

## Historias (ERS) — capa Infrastructure

| HU | Estado tras E1-9 |
|----|------------------|
| HU-24..31 (persistencia CRUD) | 🔶 falta API (E1-10) |

---

## Pendiente

- **E1-10 API:** `CategoriasController`, `PreguntasController` + tests
  `WebApplicationFactory` + Testcontainers.
- **E1-1 / E1-4:** medir y cerrar cobertura ≥ 90% tras E1-10.
