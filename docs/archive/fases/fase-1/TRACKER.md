# Fase 1 — Tracker de iteraciones ↔ HU (ERS)

**Alcance:** solo dominio + `Umbral.Domain.Tests`  
**Entrega:** 1 (criterio quality-spec)  
**Última actualización:** 2026-05-27 (iter-07 — Fase 1 completa)

**Trazabilidad:** HU **HU-01…HU-40** según ERS y `docs/TRAZABILIDAD.md`. Reglas globales **RB-01…RB-32** (no confundir con `RB-16-01` etc. de criterios locales en `iter-*.md`).

---

## Leyenda

| Símbolo | Significado |
|---------|-------------|
| ✅ | HU cubierta en dominio **con tests** de esa iteración |
| 🔶 | Soporte en código (métodos/VO presentes) pero **tests en iteración posterior** |
| ⬜ | Pendiente en Fase 1 (dominio) |
| — | Fuera de Fase 1 (UI, API, mobile, SignalR, etc.) |

> Una iteración **tacha** la HU cuando el caso de uso de dominio **+** los tests mínimos del `umbral-quality-spec.md` están implementados y en verde.

---

## Iteraciones

| Iter | Caso de uso (dominio) | HU ERS principal | HU soporte | Estado | Doc |
|------|-----------------------|------------------|------------|--------|-----|
| 01 | CrearSesionBusquedaTesoro | **HU-12** | HU-01, HU-05, HU-06 | ✅ | [iter-01](iter-01-crear-sesion-bt.md) |
| 02 | RegistrarEquipo | **HU-13** | — | ✅ | [iter-02](iter-02-registrar-participante.md) |
| 03 | IniciarSesion + Pausar/Reanudar | **HU-14**, **HU-15** | — | ✅ | [iter-03](iter-03-iniciar-pausar-reanudar.md) |
| 04 | AplicarPenalizacion | **HU-16** | — | ✅ | [iter-04](iter-04-aplicar-penalizacion.md) |
| 05 | RegistrarEvidencia (base) | **HU-18** | HU-19 (parcial) | ✅ | [iter-05](iter-05-registrar-evidencia.md) |
| 06 | Ganador único + transición etapa | **HU-19**, **HU-20** | HU-10 | ✅ | [iter-06](iter-06-ganador-transicion-etapa.md) |
| 07 | Cerrar/cancelar sesión | **HU-23** | — | ✅ | [iter-07](iter-07-cerrar-sesion.md) |

---

## Checklist global — HUs BT (Fase 1, capa dominio)

### Catálogo BusquedaTesoro

- [x] **HU-01** Crear misión — 🔶 `Mision.Crear` + `Activar` con tests (`MisionTests`)
- [ ] **HU-02** Consultar/listar misiones — ⬜ (query; Fase 2+)
- [ ] **HU-03** Modificar misión — ⬜
- [ ] **HU-04** Desactivar/eliminar misión — ⬜
- [x] **HU-05** Configurar etapas — 🔶 `AgregarEtapa` en `Mision`, tests en `MisionTests`
- [x] **HU-06** Registrar pistas en etapa — 🔶 `AgregarPista` en `Etapa`, `TipoLiberacion`
- [ ] **HU-07** Modificar pistas — ⬜
- [ ] **HU-08** Eliminar pistas — ⬜
- [ ] **HU-09** Liberación automática por tiempo — ⬜
- [ ] **HU-10** Liberación por ganador de etapa — ⬜

### Sesión Operador

- [x] **HU-12** Operador crea sesión BT — ✅ iter-01
- [x] **HU-13** Operador registra participantes — ✅ iter-02 (`SesionRegistrarEquipoTests`, 15 tests)
- [x] **HU-14** Operador inicia sesión — ✅ iter-03 (`SesionCicloVidaTests`, 32 tests)
- [x] **HU-15** Pausar / reanudar sesión — ✅ iter-03 (`SesionCicloVidaTests`, 32 tests)
- [x] **HU-16** Aplicar penalización — ✅ iter-04 (`PuntajeTests` + `SesionAplicarPenalizacionTests`, 30 tests)
- [x] **HU-23** Reporte final / cerrar sesión — ✅ iter-07 (`SesionCerrarSesionTests` + `RankingServiceTests`, 16 tests)

### Juego / Evidencia / Etapas

- [x] **HU-18** Equipo envía evidencia QR — ✅ iter-05 (`SesionRegistrarEvidenciaTests` + `ValidacionEvidenciaServiceTests`, 30 tests)
- [x] **HU-19** Validar ganador único de etapa — ✅ iter-06 (`SesionGanadorTransicionTests`, 10 tests)
- [x] **HU-20** Transición automática de etapa — ✅ iter-06 (`ContextoBT.AvanzarEtapa`, auto-finalizar)

### Dominio compartido / transversal

- [x] Equipo se une con código de acceso — 🔶 `CodigoAcceso` por participante en `RegistrarEquipo`; flujo mobile ⬜
- [ ] Liberación manual de pistas (RF-15) — ⬜

### Fuera de Fase 1

| HU | Fase |
|----|------|
| HU-11 Equipo ve pistas | 7 (Mobile) |
| HU-17 Tablero tiempo real | 4 + 7 |
| HU-21 Ranking tiempo real | 4 + 6 + 7 |
| HU-22 Historial auditoría | 4 + 6 (consulta); dominio `EventoSesion` ✅ iter-02 |
| HU-24…40 Trivia completo | 9 + 10 (Entrega 2) |
| Auth JWT | 8 |
| CI + cobertura ≥90% | 8 |
| E2E BT / Trivia | 10 |

---

## Progreso Fase 1

```
Iter 07 ██████████████  7/7 iteraciones (100%) — Fase 1 completa
```

**Tests dominio:** 170/170  
**HUs BT cerradas (dominio + tests):** HU-12, HU-13, HU-14, HU-15, HU-16, HU-18, HU-19, HU-20, HU-23  
**Siguiente fase:** Application (CQRS/MediatR) — ver `umbral-backend-spec.md`
