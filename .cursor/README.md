# Configuración Cursor — UMBRAL

Documentación de agentes, reglas y specs para el asistente de IA.

## Fuente normativa

| Documento | Contenido |
|-----------|-----------|
| [`docs/TRAZABILIDAD.md`](../docs/TRAZABILIDAD.md) | RB-01…32, RNF-01…14, HU ERS, mapeo entregas |
| [`specs/umbral-product-spec.md`](specs/umbral-product-spec.md) | Producto, módulos, RF, patrones |
| [`docs/ERS_Proyecto_UMBRAL_UCAB.md`](../docs/ERS_Proyecto_UMBRAL_UCAB.md) | ERS académico (HU narrativas) |
| [`docs/fase-1/TRACKER.md`](../docs/fase-1/TRACKER.md) | Progreso dominio Fase 1 |
| [`docs/entrega-1/PLAN.md`](../docs/entrega-1/PLAN.md) | **Alcance y backlog Entrega 1** (vigente) |

## Estructura

- `rules/` — convenciones de repo y código
- `specs/` — arquitectura, backend, frontend, calidad
- `skills/` — guías por tecnología (DDD, CQRS, EF, SignalR, RabbitMQ, tests)
- `agents/` — prompts por rol (architect, backend, frontend, qa, devops)

## Clientes

- **Web:** Admin + Operador (`src/frontend/umbral-web`)
- **Mobile:** Equipo participante — React Native + Expo (`src/mobile/umbral-mobile`)
