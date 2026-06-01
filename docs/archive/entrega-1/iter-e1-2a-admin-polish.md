# Iteración E1-2a — Afinado UI catálogo admin

**Fase:** Entrega 1 — Fase D
**Fecha:** 2026-05-30
**Depende de:** E1-2, E1-K4

---

## Objetivo

Mejorar UX del catálogo **Administrador** y dejar **separación clara de roles** en navegación.

---

## Cambios

### Roles (navegación)

- Eliminado enlace «Operador» del header admin.
- Eliminado «Catálogo admin» del header operador.
- `/operador/*` solo rol **Operador** (admin recibe 403 si entra por URL).
- Badge de rol en cada layout (Administrador / Operador).
- `AccessDenied` con enlace «Volver a mi inicio».

### UX compartida

| Componente | Uso |
|------------|-----|
| `PageHeader` | Título + descripción + acción |
| `EmptyState` | Listas vacías / sin filtros |
| `SuccessAlert` | Feedback tras crear/editar/eliminar (auto-dismiss 4s) |
| `ErrorState` | + botón **Reintentar** (refetch) |
| `styles/ui.ts` | Botones, inputs, cards consistentes |
| `useSuccessMessage` | Mensajes de éxito |

### Por pantalla

- **Misiones / Categorías / Preguntas:** limpiar filtros, estados de carga en mutaciones, hover en filas, textos de ayuda.
- **Preguntas:** grupos radio distintos en crear vs editar (`correcta-create` / `correcta-edit`).

---

## Verificación

1. Login `admin` → solo nav Misiones / Categorías / Preguntas.
2. Crear/editar → banner verde de éxito.
3. Error de red → Reintentar.
4. Login `operador` → solo Sesiones; `/admin/misiones` → 403 con volver a operador.

---

## Siguiente

**E1-2b** — UI operador (sesiones BT).
