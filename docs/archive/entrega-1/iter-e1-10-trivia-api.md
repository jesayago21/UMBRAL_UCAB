# Iteración E1-10 — API CRUD Trivia (CatalogoTrivia)

**Fase:** Entrega 1 — capa API (Trivia)
**Fecha:** 2026-05-29
**Fuentes de verdad:** `iter-e1-09-trivia-infrastructure.md`, `MisionesController` (patrón), `PLAN.md §5.1`

> **Metodología:** TDD — 14 tests de integración (WebApplicationFactory +
> Testcontainers) escritos antes de controllers/contracts.

---

## Endpoints implementados

### Categorías (`/api/v1/categorias`) — HU-28..31

| Método | Ruta | Rol | Respuesta |
|--------|------|-----|-----------|
| POST | `/api/v1/categorias` | Administrador | 201 `{ id }` |
| GET | `/api/v1/categorias?nombre=` | Administrador | 200 `CategoriaResponse[]` |
| GET | `/api/v1/categorias/{id}` | Administrador | 200 / 404 |
| PUT | `/api/v1/categorias/{id}` | Administrador | 204 / 400 / 404 |
| DELETE | `/api/v1/categorias/{id}` | Administrador | 204 (soft delete) |

### Preguntas (`/api/v1/preguntas`) — HU-24..27

| Método | Ruta | Rol | Respuesta |
|--------|------|-----|-----------|
| POST | `/api/v1/preguntas` | Administrador | 201 `{ id }` |
| GET | `/api/v1/preguntas?categoriaId=&dificultad=&enunciado=` | Administrador | 200 `PreguntaResponse[]` |
| GET | `/api/v1/preguntas/{id}` | Administrador | 200 / 404 |
| PUT | `/api/v1/preguntas/{id}` | Administrador | 204 / 400 / 404 |
| DELETE | `/api/v1/preguntas/{id}` | Administrador | 204 (soft delete) |

> **403 por rol:** Operador recibe `403 Forbidden` en POST (demostrable para el profesor).

---

## Archivos

```
src/backend/Umbral.API/
├── Contracts/Trivia/TriviaContracts.cs
├── Controllers/CategoriasController.cs
└── Controllers/PreguntasController.cs

tests/Umbral.API.Tests/Controllers/
├── CategoriasControllerTests.cs   (7 tests)
└── PreguntasControllerTests.cs    (7 tests)
```

Los controllers delegan en MediatR (commands/queries de E1-8); no hay lógica de
negocio en la API.

---

## Tests de integración

| Escenario | Categorías | Preguntas |
|-----------|------------|-----------|
| Admin POST → 201 | ✅ | ✅ |
| Operador POST → 403 | ✅ | ✅ |
| GET listado | ✅ | ✅ |
| PUT actualiza | ✅ | ✅ |
| DELETE soft → GET 404 | ✅ | ✅ |
| GET id inexistente → 404 | ✅ | ✅ |
| Nombre duplicado → 400 DomainError | ✅ | — |
| POST con categoría válida | — | ✅ |

**Total API: 60/60 ✅** (46 previos + 14 nuevos)
**Total solución: 351/351 ✅**

```powershell
dotnet test tests/Umbral.API.Tests --filter "FullyQualifiedName~Categorias|FullyQualifiedName~Preguntas"
# Passed!  Failed: 0, Passed: 14
```

---

## Backend Trivia — slice completo

```
E1-7 Dominio     ✅
E1-8 Application ✅
E1-9 Infrastructure ✅
E1-10 API        ✅  ← esta iteración
```

---

## Pendiente

- **E1-1 / E1-4:** medir cobertura y cerrar brecha ≥ 90%.
- **E1-2:** frontend CRUD Misiones + Trivia.
- **E1-K4:** login OIDC Keycloak en el frontend.
