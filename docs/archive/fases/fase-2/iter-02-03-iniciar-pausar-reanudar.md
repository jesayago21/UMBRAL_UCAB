# Iteración 02-03 — Iniciar / Pausar / Reanudar sesión (Application)

**Fase:** 2 · **HU-14**, **HU-15** · **RB-18**  
**Rama:** `feature/fase2-application`

---

## Commands

| Command | Dominio | Transición |
|---------|---------|------------|
| `IniciarSesionCommand` | `Sesion.Iniciar()` | EnPreparacion → Activa |
| `PausarSesionCommand` | `Sesion.Pausar()` | Activa → Pausada |
| `ReanudarSesionCommand` | `Sesion.Reanudar()` | Pausada → Activa |

Retorno: `Result<Guid>` (id de sesión).

**Prerrequisito operador (web):** sesión en `EnPreparacion` con ≥1 equipo antes de `Iniciar` (tras `AbrirParaRegistro` + `RegistrarEquipo` en iteraciones previas).

---

## Roles en esta iteración

La capa Application **no valida roles**. Quien puede ejecutar estos commands en producción será **Operador** o **Administrador** vía JWT en API (fase posterior). Ver `docs/fase-2/roles-y-autenticacion.md` si existe.

---

## Tests

10 handler + 3 validator = **13** nuevos (total Application acumulado en tracker).

---

## Verificación

```bash
dotnet test tests/Umbral.Application.Tests/Umbral.Application.Tests.csproj
```
