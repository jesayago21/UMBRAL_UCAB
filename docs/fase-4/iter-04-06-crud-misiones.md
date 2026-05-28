# Iteración 04-06 — CRUD misiones (HU-01..04)

## Objetivo

Cerrar la Fase 4 exponiendo CRUD de misiones para consumo desde front web admin, con autorización por rol y tests de integración.

## Endpoints implementados

| Método | Ruta | Rol | Resultado |
|--------|------|-----|-----------|
| `POST` | `/api/v1/misiones` | Administrador | `201 Created` |
| `GET` | `/api/v1/misiones` | Administrador | `200 OK` |
| `GET` | `/api/v1/misiones/{id}` | Administrador | `200 OK` |
| `PUT` | `/api/v1/misiones/{id}` | Administrador | `204 NoContent` |
| `DELETE` | `/api/v1/misiones/{id}` | Administrador | `204 NoContent` (desactivación lógica) |

## Qué cambió por capa

### API

- Nuevo `MisionesController` con acciones CRUD delgadas vía MediatR.
- Nuevos contratos:
  - `CrearMisionRequest`
  - `ActualizarMisionRequest`
  - `MisionResponse` (+ DTOs de etapa/pista)
- Seguridad: `[Authorize(Roles = "Administrador")]` para todo el controller.

### Application

Se creó el módulo `Misiones` con commands/queries:

- `CrearMisionCommand` (+ validator + handler)
- `ActualizarMisionCommand` (+ validator + handler)
- `DesactivarMisionCommand` (+ validator + handler)
- `ListMisionesQuery` (+ handler)
- `GetMisionByIdQuery` (+ handler)

DTOs compartidos:

- `MisionDto`
- `EtapaMisionDto`
- `PistaMisionDto`

### Domain / Infrastructure

- `Mision` ahora soporta:
  - `Renombrar(string nombre)`
  - `Desactivar()` (Activa -> Borrador)
- `IMisionRepository` agrega `FindAllAsync()`.
- `MisionRepository` implementa `FindAllAsync()` con etapas y pistas incluidas.

## Notas de modelado

- El dominio actual usa `Borrador` / `Activa`. Para API/front se mapea:
  - `Activa` -> `"Activa"`
  - `Borrador` -> `"Inactiva"`
- `DELETE` no elimina físicamente; aplica desactivación lógica.
- Se mantuvo compatibilidad con reglas existentes de sesión (`CrearSesion` requiere misión activa).

## Ajustes de consistencia aplicados (spec/rules)

- **HU-01 (nombre único):**
  - Validación en create/update para impedir duplicados.
  - Índice único en BD sobre `misiones.nombre` (migración `AddUniqueIndexMisionNombre`).
- **HU-03 (no editar con sesiones activas):**
  - `ActualizarMisionCommandHandler` bloquea cambios si existe sesión activa de esa misión.
- **Contrato frontend (`MisionDto`):**
  - Se agregaron campos de compatibilidad en respuesta (`descripcion`, `nivelDificultad`, `tiempoMaximoSeg`, `totalEtapas`).
  - En el modelo actual, `descripcion` usa `nombre`, `nivelDificultad` se expone como `"NoDefinida"` y `tiempoMaximoSeg` como `0`.

## Tests de integración agregados

Archivo: `tests/Umbral.API.Tests/Controllers/MisionesControllerTests.cs`

- `POST_misiones_CuandoAdmin_Retorna201`
- `POST_misiones_CuandoOperador_Retorna403`
- `GET_misiones_CuandoAdmin_Retorna200`
- `PUT_misiones_CuandoAdmin_ActualizaNombreYEstado`
- `DELETE_misiones_CuandoActiva_DesactivaYRetorna204`
- `GET_misiones_por_id_CuandoNoExiste_Retorna404`

## Ajuste para pruebas de roles

`TestAuthHandler` ahora acepta header `X-Test-Role` en entorno `Testing`, para forzar rol por request y poder validar `403` en integración sin depender del login real.

## Validación ejecutada

```powershell
dotnet build Umbral.sln
dotnet test tests/Umbral.API.Tests/Umbral.API.Tests.csproj
```

Resultado: `46` tests API aprobados.

## Commit sugerido

`feat(api): iter-04-06 CRUD misiones con auth por rol`
