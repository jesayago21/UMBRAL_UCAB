# Iteración E1-7 — Dominio CRUD Trivia (CatalogoTrivia)

**Fase:** Entrega 1 — capa Dominio (contingencia Trivia)
**Fecha:** 2026-05-28
**Fuentes de verdad:** `docs/TRAZABILIDAD.md` (RB-13..15, RB-28), ERS **HU-24..31**, `umbral-quality-spec.md`, `docs/entrega-1/PLAN.md §5.1`

> **Nota de alcance / naming:** esta iteración pertenece al alcance redefinido de
> **Entrega 1** (ver `PLAN.md`). El identificador `E1-7` reemplaza la numeración
> de fases previa; lo construido en las fases 1–4 se mantiene intacto. Aquí solo
> se agrega el **dominio** del banco de preguntas de Trivia; el resto del CRUD
> (Application, Infrastructure, API, UI) son E1-8..E1-10.

---

## Contexto

`CatalogoTrivia/{Pregunta,Categoria}` existía solo como carpetas con `.gitkeep`
(sin código). Esta iteración crea el dominio mínimo del banco de preguntas para
tener listo el CRUD de Trivia como respaldo de la entrega, siguiendo el mismo
patrón del BC `CatalogoBusquedaTesoro` (AggregateRoot + factory `Crear` + eventos
+ soft delete).

**No incluye** gameplay de Trivia (sesión, sala de espera, rondas, respuestas,
ranking, transición, desempate) — eso es HU-32..40, Entrega 2.

---

## Objetos implementados

### BC CatalogoTrivia (nuevo)

```
CatalogoTrivia/
├── Categoria/
│   ├── CategoriaId.cs            ← record CategoriaId(Guid Valor)
│   ├── Categoria.cs              ← AggregateRoot (Crear / Renombrar / Eliminar)
│   ├── ICategoriaRepository.cs   ← port de salida
│   └── Events/
│       └── CategoriaCreada.cs
└── Pregunta/
    ├── PreguntaId.cs             ← record PreguntaId(Guid Valor)
    ├── Dificultad.cs             ← enum: Facil | Media | Dificil
    ├── OpcionRespuesta.cs        ← ValueObject (Texto + EsCorrecta)
    ├── Pregunta.cs               ← AggregateRoot (Crear / ModificarContenido / Categoría / Eliminar)
    ├── IPreguntaRepository.cs    ← port de salida
    └── Events/
        └── PreguntaCreada.cs
```

---

## Reglas de negocio cubiertas

| ID | Regla | Dónde |
|----|-------|-------|
| R-CAT-01 | `Categoria.Crear` valida nombre no vacío → `DomainException` | `Categoria` |
| R-CAT-02 | `Categoria.Crear` emite `CategoriaCreada` | `Categoria` |
| R-CAT-03 | `Categoria.Eliminar` aplica **soft delete** (`Eliminada = true`) | `Categoria` |
| RB-15 | Nombre de categoría único | **diferida a Application** (`ExistsByNombreAsync` en el puerto) |
| R-PRE-01 | `Pregunta.Crear` valida enunciado no vacío | `Pregunta` |
| R-PRE-02 | Mínimo **3 opciones** de respuesta | `Pregunta` |
| RB-28 | Exactamente **una** opción correcta | `Pregunta` |
| R-PRE-03 | `Pregunta.Crear` emite `PreguntaCreada` | `Pregunta` |
| R-PRE-04 | `ModificarContenido` revalida las mismas invariantes | `Pregunta` |
| RB-14 | Quitar categoría deja la pregunta "Sin Categoría" (`CategoriaId = null`) | `Pregunta.QuitarCategoria` |
| R-PRE-05 | `Pregunta.Eliminar` aplica **soft delete** | `Pregunta` |
| R-OPC-01 | `OpcionRespuesta.Crear` valida texto no vacío; VO inmutable con igualdad estructural | `OpcionRespuesta` |

> **Diferidas (gameplay, no aplican a CRUD E1):** HU-26 *"no editar pregunta si
> pertenece a una sesión de trivia activa"* y **RB-13** *"respuesta no modificable
> tras confirmar/expirar timer"*. Se resolverán en Application (patrón
> `HasSesionesActivasAsync`, como en Misiones) o en Entrega 2.

---

## Tests agregados (`tests/Umbral.Domain.Tests/CatalogoTrivia/`)

| Archivo | Tests | Cubre |
|---------|-------|-------|
| `OpcionRespuestaTests.cs` | 5 | texto válido/ vacío, trim, igualdad estructural |
| `CategoriaTests.cs` | 9 | crear, evento, nombre vacío, trim, renombrar, soft delete |
| `PreguntaTests.cs` | 14 | crear (con/sin categoría), evento, enunciado vacío, <3 opciones, 0 / >1 correctas, modificar, asignar/quitar categoría, soft delete |

**Total dominio tras la iteración: 198/198 ✅** (170 previos + 28 nuevos)

```powershell
dotnet test tests/Umbral.Domain.Tests/Umbral.Domain.Tests.csproj
# Passed!  Failed: 0, Passed: 198, Skipped: 0
```

---

## Decisiones de diseño

1. **Mismo patrón que `Mision`:** AggregateRoot con factory `Crear`, IDs como
   `record`, eventos en `Events/`, igualdad por ID (`IdEquals`/`GetIdHashCode`).
2. **Soft delete** (`Eliminada`) por recomendación de HU-27 (preservar auditoría).
3. **"Sin Categoría" = `CategoriaId` nullable** en `Pregunta` (RB-14): al eliminar
   una categoría, el handler de Application pondrá `CategoriaId = null` en sus
   preguntas vía `QuitarCategoria()`.
4. **Unicidad de nombre (RB-15) fuera del dominio:** el agregado no conoce a sus
   hermanos; se valida en Application con `ICategoriaRepository.ExistsByNombreAsync`.
5. **`OpcionRespuesta` como ValueObject** (no entidad): no tiene identidad propia,
   se compara por `Texto` + `EsCorrecta`.

---

## Historias de usuario (ERS) — esta iteración

| HU | Título | Capa esta iteración | Estado |
|----|--------|---------------------|--------|
| HU-24 | Registrar pregunta | Dominio + tests | 🔶 (falta App/Infra/API) |
| HU-26 | Modificar pregunta | Dominio + tests | 🔶 |
| HU-27 | Eliminar pregunta (soft) | Dominio + tests | 🔶 |
| HU-28 | Registrar categoría | Dominio + tests | 🔶 |
| HU-30 | Modificar categoría | Dominio + tests | 🔶 |
| HU-31 | Eliminar categoría (soft) | Dominio + tests | 🔶 |

(HU-25/HU-29 consulta/filtrado se cubren en E1-8 Application + E1-10 API.)

---

## Pendiente — siguientes iteraciones

- **E1-8 Application:** commands/queries/validators/handlers (CrearCategoria,
  ActualizarCategoria, EliminarCategoria, ListCategorias, CrearPregunta,
  ActualizarPregunta, EliminarPregunta, GetPreguntaById, ListPreguntas con
  filtro por categoría/dificultad) + tests con Moq.
- **E1-9 Infrastructure:** configuraciones EF Core, repositorios, **migración**
  nueva + tests con Testcontainers.
- **E1-10 API:** `CategoriasController`, `PreguntasController`
  (`[Authorize(Roles="Administrador")]`) + tests con WebApplicationFactory.
