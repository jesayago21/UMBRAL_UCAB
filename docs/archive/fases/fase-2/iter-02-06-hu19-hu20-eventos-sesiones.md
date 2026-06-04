# Iteración 02-06 — Consolidación HU-19/HU-20 en Application

**Fase:** 2 · **HU-19**, **HU-20**  
**Rama:** `feature/fase2-application`

---

## Objetivo

Fortalecer la capa Application sobre el flujo `SubmitEvidencia` para validar que, ante una evidencia ganadora:

- se emiten los eventos de ganador y cierre de etapa;
- la sesión avanza automáticamente de etapa;
- intentos posteriores con QR de etapa anterior quedan inválidos.

---

## Alcance técnico

Sin cambios de reglas de negocio en dominio.  
Se agregan pruebas de orquestación en `SubmitEvidenciaCommandHandlerTests`:

1. Evidencia válida inicial -> `EvidenciaRegistrada` + `EvidenciaValidada` + `EtapaCompletada`.
2. Avance de `ContextoBT.EtapaActualIndex`.
3. Segundo envío con QR de etapa anterior -> `ResultadoValidacion.Invalida` y solo `EvidenciaRegistrada`.

---

## Resultado

Application mantiene trazabilidad de HU-19/20 desde el caso de uso HU-18 ya implementado, sin introducir infraestructura ni API en esta fase.
