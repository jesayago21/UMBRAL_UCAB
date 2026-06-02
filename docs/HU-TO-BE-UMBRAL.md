UMBRAL — Historias de usuario (TO-BE)

Versión alineada con misiones polimórficas, sesión de misión unificada e Identidad y Accesos.  
Trazabilidad: `TRAZABILIDAD.md` · Modelo: `domain-model.md`

Convención: una sesión de misión recorre en orden las etapas definidas en la plantilla (Búsqueda del tesoro y/o Trivia). El banco de preguntas (`Categoria`, `Pregunta`) no sustituye el diseño de la misión.

---

Catálogo de misiones (Administrador)

HU-01: Creación de misión

Definición  
Como Administrador, quiero registrar una nueva misión (plantilla de juego) con nombre y descripción, para construir después una secuencia de etapas de Búsqueda del tesoro y/o Trivia.

Criterios de aceptación

1. El nombre de la misión debe ser único en el sistema (RB-10).
2. El estado inicial debe ser Borrador (o equivalente no activo).
3. La misión puede crearse sin etapas; no podrá activarse hasta cumplir RB-09.
4. La API responde con DTO (no entidades de dominio expuestas).

Flujos alternos

- Si el nombre ya existe, el sistema impide el guardado y notifica al usuario.
- Si faltan campos obligatorios (nombre vacío), error de validación.

Trazabilidad: RF-01 · RB-10, RB-09

---

HU-02: Consulta de misiones

Definición  
Como Administrador, quiero visualizar el listado de misiones existentes con resumen de etapas, para gestionar el catálogo de plantillas.

Criterios de aceptación

1. Permitir filtrar por estado (Borrador / Activa / Inactiva) y por nombre.
2. Mostrar, en lectura, cantidad o tipo de etapas (p. ej. “2 BT, 1 Trivia”) cuando esté disponible en el DTO (RF-22).
3. Si no hay misiones, mensaje: “No hay registros disponibles”.

Flujos alternos

- Filtros sin resultados → listado vacío con mensaje informativo.

Trazabilidad: RF-01, RF-22 · RB-10

---

HU-03: Modificación de misión

Definición  
Como Administrador, quiero editar los datos generales de una misión y sus etapas mientras la política lo permita, para corregir o ampliar la plantilla sin afectar sesiones en curso.

Criterios de aceptación

1. Solo se permiten cambios estructurales si la misión no tiene sesiones activas en curso que dependan de su snapshot.
2. Cambios en el catálogo (preguntas, categorías) no alteran sesiones ya creadas (RB-11).
3. Edición de metadatos (nombre, etc.) según reglas de negocio y unicidad (RB-10).

Flujos alternos

- Si la misión está Activa con sesión en curso vinculada, bloquear edición de etapas y mostrar advertencia.
- Intento de nombre duplicado → rechazo (RB-10).

Trazabilidad: RF-01 · RB-10, RB-11

---

HU-04: Eliminación / desactivación de misión

Definición  
Como Administrador, quiero desactivar o eliminar lógicamente misiones que ya no se utilicen, para mantener el catálogo sin perder historial.

Criterios de aceptación

1. Si la misión tiene historial de sesiones, solo desactivación lógica (estado Inactiva), no borrado físico.
2. Para activar, la misión debe cumplir RB-09 (≥1 etapa válida según tipo).
3. Misión activa con sesiones programadas/en curso → restricciones según política de HU-03.

Flujos alternos

- Intento de activar misión sin etapas válidas → error RB-09.
- Eliminación con sesiones históricas → cambio automático a Inactiva en lugar de borrar.

Trazabilidad: RF-01 · RB-09, RB-01

---

HU-05: Configuración de etapas (nodos BT y etapas Trivia)

Definición  
Como Administrador, quiero agregar etapas ordenadas a una misión: de tipo Búsqueda del tesoro (descripción, código QR) o de tipo Trivia (categorías del banco), para definir el recorrido que ejecutará la sesión (RF-02).

Criterios de aceptación

1. El orden de etapas es correlativo (1, 2, 3…) en una misma misión.
2. Etapa BT: descripción y código QR de solución obligatorios.
3. Etapa Trivia: al menos un CategoriaId asignado (RB-33).
4. No mezclar comportamientos: pistas y QR solo en etapas BT.

Flujos alternos

- Etapa Trivia sin categorías → validación; la misión no cambia.
- Misión en estado que impide edición (sesión en curso) → bloqueo con advertencia.
- Orden duplicado o inválido → error de validación.

Trazabilidad: RF-02 · RB-09, RB-33 · Ver también HU-A1

---

HU-06: Registro de pistas en etapa BT

Definición  
Como Administrador, quiero crear y asociar pistas (texto/enlace) a una etapa de Búsqueda del tesoro, para alimentar el banco de ayudas de esa etapa.

Criterios de aceptación

1. Cada pista está vinculada obligatoriamente a una etapa BT.
2. Permitir tipo de liberación: Por tiempo o Por ganador (RB-07).
3. Si aplica Por tiempo, configurar segundos de liberación.
4. Las etapas Trivia no admiten pistas.

Flujos alternos

- Intento de pista en etapa Trivia → rechazo.
- Misión Activa con sesión en curso que ya congeló snapshot → edición bloqueada (RB-11).

Trazabilidad: RF-02, RF-14 · RB-21

---

HU-07: Modificación de pistas

Definición  
Como Administrador, quiero editar el contenido de pistas de etapas BT para corregir errores antes de usar la misión en sesiones.

Criterios de aceptación

1. Edición permitida solo si no viola RB-11 (sesiones activas con snapshot que incluye la pista).
2. Mantener tipo de liberación y reglas de HU-06.

Flujos alternos

- Pista ya entregada en sesión activa → bloqueo de edición y aviso.
- Misión bloqueada por sesión en curso → botón editar deshabilitado.

Trazabilidad: RF-02 · RB-11, RB-21

---

HU-08: Eliminación de pistas

Definición  
Como Administrador, quiero eliminar pistas de etapas BT para corregir el diseño de la misión.

Criterios de aceptación

1. Al eliminar una pista, reordenar o ajustar prioridades restantes del nodo si aplica.
2. Advertencia si se elimina la última pista de una etapa BT (dificultad del reto).

Flujos alternos

- Última pista de etapa → advertencia, no impedir si la política lo permite.
- Sesión activa con snapshot → bloqueo (RB-11).

Trazabilidad: RF-02 · RB-11

---

HU-09: Liberación automática de pistas por tiempo

Definición  
Como Sistema, quiero liberar pistas configuradas como Por tiempo cuando transcurra el intervalo definido, durante una etapa BT activa de una sesión de misión.

Criterios de aceptación

1. Ejecutar de forma asíncrona y notificar a participantes (WebSockets / RF-17).
2. Solo durante etapa activa de tipo BT (RB-07, RB-23).
3. No liberar la misma pista dos veces al mismo participante en la misma etapa (RB-21).

Flujos alternos

- Si no quedan pistas por tiempo en la etapa, no disparar notificación vacía.
- Sesión pausada/finalizada → no liberar (RB-19, RB-26).

Trazabilidad: RF-14 · RB-07, RB-21, RB-19

---

HU-10: Liberación de pistas por ganador de etapa BT

Definición  
Como Sistema, quiero liberar pistas asociadas al avance cuando un participante gana la etapa BT actual (primera evidencia válida), según diseño de la misión.

Criterios de aceptación

1. Disparo inmediatamente después de evidencia válida (RF-24, RB-04).
2. La pista liberada corresponde a reglas de diseño (p. ej. pista de avance en etapa BT siguiente, si está configurada).
3. Si la siguiente etapa no es BT, no aplicar lógica de “pista de nodo BT siguiente”.
4. Si es la última etapa de la misión, no liberar pistas de avance.

Flujos alternos

- Última etapa de la misión → no liberación; puede disparar finalización (HU-23).
- Etapa actual no BT → esta HU no aplica.

Trazabilidad: RF-14, RF-15 · RB-04, RB-07

---

Catálogo Trivia — banco (Administrador)

HU-24: Registro de nueva pregunta

Definición  
Como Administrador, quiero crear una pregunta de trivia con opciones y categoría, para alimentar el banco usado por etapas Trivia en las misiones.

Criterios de aceptación

1. Enunciado no vacío.
2. Al menos 3 opciones; exactamente una correcta (RB-28).
3. Asignación a CategoriaId (o flujo “Sin categoría” según RB-14).
4. Tiempo de respuesta configurable por pregunta (RF-25).

Flujos alternos

- Sin opción correcta marcada → bloqueo de guardado.
- Menos de 3 opciones → error de validación.

Trazabilidad: RF-23, RF-25 · RB-28, RB-16

---

HU-25: Visualización y filtrado de preguntas

Definición  
Como Administrador, quiero listar y filtrar preguntas del banco para revisar el contenido disponible para etapas Trivia.

Criterios de aceptación

1. Búsqueda por texto en enunciado.
2. Filtro por categoría y dificultad.
3. La consulta de administración puede mostrar cuál opción es correcta (no aplica al panel de participante).

Flujos alternos

- Sin resultados → “No se encontraron resultados”.

Trazabilidad: RF-23, RF-22 · RB-16

---

HU-26: Modificación de contenido de pregunta

Definición  
Como Administrador, quiero editar enunciados u opciones de preguntas existentes en el banco.

Criterios de aceptación

1. Validar RB-28 al guardar.
2. No editar si la pregunta está en un snapshot de sesión de misión Activa/Pausada (RB-11).
3. Actualizar versión persistida en BD.

Flujos alternos

- Pregunta en sesión en curso → campos en solo lectura con aviso.
- Sin respuesta correcta → bloqueo (RB-28).

Trazabilidad: RF-24 · RB-11, RB-28, RB-16

---

HU-27: Eliminación lógica de preguntas

Definición  
Como Administrador, quiero desactivar lógicamente preguntas del banco que ya no sean útiles.

Criterios de aceptación

1. Eliminación lógica (`isDeleted`), nunca física (RB-16).
2. Doble confirmación recomendada en UI.
3. No eliminar si está en snapshot de sesión en curso.

Flujos alternos

- Pregunta en sesión activa → acción bloqueada.
- Confirmación cancelada → sin cambios.

Trazabilidad: RF-24 · RB-16, RB-11

---

HU-28: Registro de categoría

Definición  
Como Administrador, quiero crear categorías temáticas para organizar el banco de preguntas.

Criterios de aceptación

1. Nombre de categoría único (RB-15).
2. Descripción breve opcional.

Flujos alternos

- Nombre duplicado → impedir guardado y sugerir otro nombre.

Trazabilidad: RF-23 · RB-15

---

HU-29: Consulta de categorías

Definición  
Como Administrador, quiero listar categorías con conteo de preguntas asociadas.

Criterios de aceptación

1. Mostrar número de preguntas activas por categoría.
2. Listado vacío con mensaje informativo si aplica.

Flujos alternos

- Sin categorías → invitación a crear la primera (útil en onboarding de HU-A1).

Trazabilidad: RF-23 · RB-15

---

HU-30: Modificación de categoría

Definición  
Como Administrador, quiero editar nombre o descripción de una categoría existente.

Criterios de aceptación

1. Unicidad de nombre (RB-15).
2. Cambio reflejado en listados; preguntas mantienen referencia por id.

Flujos alternos

- Nombre duplicado al editar → bloqueo.

Trazabilidad: RF-23 · RB-15

---

HU-31: Eliminación de categoría

Definición  
Como Administrador, quiero eliminar categorías que ya no se necesiten.

Criterios de aceptación

1. Al eliminar categoría, las preguntas asociadas pasan a “Sin categoría” (RB-14), no se borran.
2. Confirmación indicando cuántas preguntas se ven afectadas.
3. Etapas Trivia en misiones Borrador que usaban la categoría deben revalidarse (RB-33).

Flujos alternos

- Categoría en uso en misión Activa → advertencia o bloqueo según política.

Trazabilidad: RF-23 · RB-14, RB-33

---

HU-A1: Configuración de etapa Trivia en la misión

Definición  
Como Administrador, quiero agregar una etapa de tipo Trivia a una misión y seleccionar categorías del banco en ese momento, para que el operador lance sesiones sin reconfigurar el contenido trivia al crear la sesión.

Criterios de aceptación

1. Etapa Trivia persistida con `TipoEtapa = Trivia` y lista de `CategoriaId` (RB-33).
2. Categorías seleccionadas deben existir y, para activar misión, tener preguntas activas disponibles (RB-09).
3. Respuesta API solo con DTO polimórfico de etapas.
4. Orden de etapa coherente con HU-05.

Flujos alternos

- Sin categorías → error de validación; misión no cambia.
- Categorías sin preguntas activas al activar misión → error RB-09 (no al guardar etapa si la política lo permite en Borrador).

Trazabilidad: RF-02 · RB-33, RB-09 · Sustituye función de selección de categorías que antes estaba en HU-32

---

Identidad y accesos (Administrador)

HU-I1: Registro de usuario administrable

Definición  
Como Administrador, quiero registrar un usuario con email, nombre y roles Administrador u Operador, para que pueda autenticarse en Keycloak y quede reflejado en la tabla espejo local.

Criterios de aceptación

1. Doble commit: primero Keycloak, luego persistencia local (RF-31).
2. Email y username únicos (RB-36).
3. Respuesta con DTO (`UsuarioResponse`), sin exponer agregados de dominio.
4. Si falla BD tras éxito en Keycloak → compensación eliminando usuario en identity server (RB-37).

Flujos alternos

- Email duplicado → error claro; sin usuario huérfano en Keycloak (compensación si aplica).
- Fallo de red con Keycloak → no persistir localmente.

Trazabilidad: RF-31, RF-32 · RB-36, RB-37

---

HU-I2: Restricción de rol Participante

Definición  
Como Administrador, quiero que el sistema impida asignar el rol Participante desde el panel de usuarios, para que ese rol solo exista al inscribirse a una sesión con código de acceso.

Criterios de aceptación

1. Alta o edición de usuario administrable con rol Participante → rechazo (RB-35).
2. Keycloak no recibe ese rol desde este flujo administrativo.

Flujos alternos

- Intento vía API directa → mismo error de dominio RB-35.

Trazabilidad: RF-32 · RB-35

---

Sesión de misión y juego BT (Operador, Participante, Sistema)

HU-11: Visualización de pistas disponibles (participante)

Definición  
Como Participante, quiero visualizar las pistas que me fueron otorgadas y mi historial, cuando la etapa activa es de Búsqueda del tesoro.

Criterios de aceptación

1. Mostrar solo pistas liberadas para mi participante en la etapa/nodo actual.
2. No recibir pistas duplicadas para el mismo nodo (RB-21).
3. En etapa Trivia, esta vista cede al flujo de preguntas (HU-34/35).

Flujos alternos

- Participante se desconecta y vuelve → consulta de recuperación de pistas ya otorgadas.
- Etapa activa no BT → mensaje o redirección al modo trivia.

Trazabilidad: RF-06 · RB-21, RB-23

---

HU-12: Creación de sesión de misión

Definición  
Como Operador, quiero crear una sesión en vivo seleccionando únicamente una misión en estado Activa, para que el juego ejecute todas sus etapas en orden sin elegir modo BT/Trivia por separado (RF-03, RF-35).

Criterios de aceptación

1. Solo misiones Activas en el listado (RB-01).
2. Generar código de acceso único por sesión.
3. Materializar MisionSnapshot inmutable (RB-11) con etapas BT y Trivia resueltas (preguntas de trivia resueltas en aplicación al crear sesión).
4. Estado inicial Programada; tipo de sesión informativo Mision.
5. Respuesta API: identificador de sesión, código de acceso, nombre de misión (DTO).

Flujos alternos

- Misión inactiva → no aparece en listado de creación.
- Misión sin etapas válidas → no debería estar Activa (RB-09).
- Fallo al resolver preguntas para etapa Trivia → error antes de persistir sesión.

Trazabilidad: RF-03, RF-35 · RB-01, RB-11, RB-34 · No usar HU-32

---

HU-13: Inscripción de participantes

Definición  
Como Operador o Participante, quiero registrar participantes en una sesión en preparación usando el código de acceso, para participar cuando el operador inicie el juego.

Criterios de aceptación

1. Código de acceso válido para la sesión.
2. Nombre de participante único en la sesión (RB-02).
3. No inscribir en sesión Finalizada o Cancelada (RB-03).
4. El jugador obtiene contexto de Participante por inscripción, no por HU-I1.

Flujos alternos

- Nombre duplicado → notificación de duplicidad.
- Código incorrecto → rechazo.
- Sesión ya Activa → política: no nuevas inscripciones (según reglas de dominio actuales).

Trazabilidad: RF-05 · RB-02, RB-03, RB-35

---

HU-14: Control de inicio de sesión

Definición  
Como Operador, quiero cambiar el estado de la sesión a Activa para que comience el juego en la primera etapa de la misión.

Criterios de aceptación

1. Bloquear inicio si hay 0 participantes (RB-18).
2. La plantilla debe cumplir RB-09 (etapas válidas).
3. Primera etapa activa según índice 0 en `ContextoMision`.

Flujos alternos

- 0 participantes → error RB-18.
- Configuración inválida de misión (etapa trivia sin preguntas resueltas) → impedir inicio.
- Participante ya registrado → aviso de duplicidad (si aplica en transición de estados).

Trazabilidad: RF-04 · RB-18, RB-09, RB-34

---

HU-15: Pausa y reanudación de sesión

Definición  
Como Operador, quiero pausar y reanudar la sesión de misión para detener temporalmente cronómetros y reglas de juego.

Criterios de aceptación

1. Participantes ven bloqueo en interfaz vía tiempo real (RNF-03).
2. En pausa: no evidencias BT (RB-19); trivia según RB-32 (timer ya lanzado).
3. Publicar eventos de dominio / notificaciones coherentes.

Flujos alternos

- Pérdida de WebSocket → indicador “Reconectando” en cliente.
- Intento de enviar evidencia en pausa → rechazo RB-19.

Trazabilidad: RF-04 · RB-19, RB-26

---

HU-16: Aplicación de penalizaciones

Definición  
Como Operador, quiero restar puntos a un participante con motivo obligatorio, durante la sesión de misión.

Criterios de aceptación

1. Motivo de texto obligatorio (RB-20).
2. Puntaje total no inferior a cero (RB-24).
3. Registrar trazabilidad / evento (RB-25).
4. Aplica en etapas BT y Trivia (puntaje global de sesión).

Flujos alternos

- Penalización mayor al puntaje actual → ajustar a cero (RB-24).
- Sesión finalizada → solo lectura (RB-26).

Trazabilidad: RF-11, RF-13 · RB-20, RB-24, RB-25, RB-26

---

HU-17: Visualización del tablero en tiempo real (participante)

Definición  
Como Participante, quiero ver en mi panel el estado del juego sin refrescar, según la etapa activa (BT o Trivia).

Criterios de aceptación

1. Etapa BT: nodo actual, pistas, temporizador de liberación (RB-07), puntaje.
2. Etapa Trivia: pregunta/timer según HU-34.
3. Actualización por WebSockets (RNF-03, RNF-14).
4. Al avanzar de etapa (RB-34), el tablero cambia de modo en la misma sesión.

Flujos alternos

- Socket caído → “Reconectando” y estimación local si aplica.
- Reconexión → sincronizar etapa y pregunta actual.

Trazabilidad: RF-17, RF-35 · RB-34, RB-07 / RB-12…17

---

HU-18: Envío de evidencias (QR)

Definición  
Como Participante, quiero registrar una evidencia escaneando el QR de la etapa BT activa, para competir por ser el primero en completarla.

Criterios de aceptación

1. Solo si la etapa activa es Búsqueda del tesoro.
2. QR debe coincidir con el de la etapa activa (RB-22, RF-19).
3. Sesión no pausada/finalizada/cancelada (RB-19).
4. Registrar timestamp servidor y resultado de validación (RF-11).

Flujos alternos

- QR de etapa anterior o futura → rechazo con mensaje.
- Etapa activa Trivia → error “no es etapa de evidencias”.
- Sesión pausada → RB-19.

Trazabilidad: RF-07, RF-08 · RB-06, RB-19, RB-22

---

HU-19: Validación de ganador único de etapa BT

Definición  
Como Sistema, quiero identificar al primer participante con evidencia válida en la etapa BT activa y asignar puntos exclusivos en esa etapa.

Criterios de aceptación

1. Solo el primero puntúa en esa etapa BT (RB-04).
2. Bloquear puntos posteriores de otros participantes en la misma etapa BT.
3. En etapa Trivia aplica RB-31 (varios pueden puntuar); esta HU no aplica allí.

Flujos alternos

- Dos envíos concurrentes → persistencia determina primero; segundo sin puntos de ganador.
- Evidencia inválida → sin ganador hasta envío válido.

Trazabilidad: RF-09, RF-20 · RB-04, RB-22

---

HU-20: Transición automática de etapa

Definición  
Como Sistema, quiero avanzar la sesión a la siguiente etapa de la misión cuando la etapa activa se complete, moviendo a todos los participantes al mismo tiempo (RB-34).

Criterios de aceptación

1. Etapa BT completada: tras ganador/evidencia según reglas → incrementar índice de etapa (RB-05, RB-34).
2. Etapa Trivia completada: tras agotar ronda/preguntas según reglas → mismo avance global.
3. Notificar a todos los clientes en tiempo real.
4. Si era la última etapa → disparar flujo de finalización (HU-23).

Flujos alternos

- Última etapa → finalizar sesión, no avanzar índice.
- Error de sincronización → reconciliar por consulta de estado de sesión.

Trazabilidad: RF-09, RF-35 · RB-05, RB-34

---

HU-21: Ranking y trazabilidad

Definición  
Como Operador o Participante, quiero ver el ranking actualizado tras cada evento relevante, con puntaje acumulado de todas las etapas (BT + Trivia).

Criterios de aceptación

1. Orden por puntaje descendente (RB-08).
2. Desempate por menor tiempo acumulado de respuesta cuando aplique (RB-08, HU-40).
3. Puntos de trivia suman al total de sesión (RB-29).
4. Actualización en tiempo real cuando esté implementado (RNF-03).

Flujos alternos

- Empate técnico en timestamp → misma posición hasta siguiente desempate (HU-40).
- Servicio de ranking no disponible → mostrar última versión cacheada en cliente.

Trazabilidad: RF-12, RF-16 · RB-08, RB-29

---

HU-22: Consulta de historial de auditoría

Definición  
Como Administrador u Operador autorizado, quiero consultar el log de eventos de una sesión de misión para verificar integridad del juego.

Criterios de aceptación

1. Listar eventos con tipo, payload y timestamp.
2. Incluir eventos procesados vía mensajería cuando aplique (RF-19, RNF-05).
3. Sesión finalizada en solo lectura (RB-26).

Flujos alternos

- Historial muy extenso → paginación.
- Sin permisos → acceso denegado (RF-21).

Trazabilidad: RF-18, RF-19 · RB-26, RB-25

---

HU-23: Reporte final y cierre de sesión

Definición  
Como Operador, quiero cerrar la sesión de misión y obtener el listado definitivo de posiciones.

Criterios de aceptación

1. Cambiar estado a Finalizada (RB-26).
2. Invalidar envíos posteriores (evidencias, respuestas trivia).
3. Emitir ranking final (HU-21).

Flujos alternos

- Cierre accidental → política de confirmación en UI (si se define).
- Sesión ya finalizada → operación idempotente o rechazada.

Trazabilidad: RF-04 · RB-26, RB-08

---

Trivia en vivo — dentro de sesión de misión (etapa activa Trivia)

Aplican cuando `ContextoMision` tiene etapa activa de tipo Trivia. No existe sesión trivia independiente (ver HU-32 deprecada).

HU-32: Creación de sesión de Trivia — DEPRECADA

Definición  
Como Operador, quiero crear una sesión exclusiva de trivia seleccionando categorías.  

Estado: Deprecada. Sustituida por HU-12 (crear sesión de misión) + HU-A1 (categorías en diseño de misión).

Criterios de aceptación

- No implementar flujo separado de “sesión trivia por categorías”.
- Conservar solo endpoints legacy marcados obsoletos si existen, hasta retirada planificada.

Flujos alternos

- N/A — redirigir documentación y capacitación a HU-12 y HU-A1.

---

HU-33: Control de sala de espera

Definición  
Como Operador, quiero ver en tiempo real qué participantes se han unido antes de iniciar la sesión (o antes de pasar a estado Activa).

Criterios de aceptación

1. Lista de participantes vía WebSockets (RNF-03).
2. Opción de expulsar participante por nombre inapropiado (si la política lo permite).
3. Aplica a sesión de misión completa, no solo “sala trivia”.

Flujos alternos

- Caída de conexión del operador → reintento sin cerrar sesión en preparación.

Trazabilidad: RF-04, RF-05 · RB-18

---

HU-34: Recepción de secuencia automática de preguntas

Definición  
Como Participante, quiero que las preguntas de la etapa Trivia activa aparezcan en secuencia, sincronizadas por el servidor.

Criterios de aceptación

1. Aviso “Preparando siguiente pregunta” entre rondas.
2. Sincronización vía WebSockets.
3. Si el participante entra tarde, sincronizar con la pregunta en curso (RB-30 según política).

Flujos alternos

- Entrada con secuencia ya iniciada → alinear a pregunta actual.
- Fin de etapa trivia → transición HU-20 (siguiente etapa o cierre).

Trazabilidad: RF-27, RF-35 · RB-32, RB-34, RB-30

---

HU-35: Envío de respuesta seleccionada

Definición  
Como Participante, quiero confirmar mi respuesta en la etapa Trivia activa antes de que expire el timer.

Criterios de aceptación

1. Bloquear opciones al confirmar o al llegar a 0 s (RF-30, RB-13).
2. Publicar respuesta al servidor / cola (RNF-05).
3. Respuesta fuera de tiempo → 0 puntos (RB-12).

Flujos alternos

- Fallo de red → reintentos en cliente hasta cierre de ronda si la política lo permite.
- Intento de cambio tras confirmar → rechazo RB-13.

Trazabilidad: RF-26, RF-30 · RB-12, RB-13, RB-17

---

HU-36: Procesamiento asíncrono de respuestas

Definición  
Como Sistema, quiero consumir mensajes de respuestas trivia desde RabbitMQ y validarlas sin bloquear el hilo principal.

Criterios de aceptación

1. Extraer `SesionId`, `ParticipanteId`, `PreguntaId`, opción seleccionada.
2. Validar contra dominio y persistir.
3. Mensaje mal formado → DLQ sin detener el motor.

Flujos alternos

- Pregunta ya cerrada → marcar fuera de tiempo / ignorar puntos (RB-12).

Trazabilidad: RF-26, RNF-05 · RB-12, RB-17

---

HU-37: Cálculo dinámico de puntuación (trivia)

Definición  
Como Sistema, quiero asignar puntos por respuesta correcta en etapa Trivia según reglas de la pregunta y tiempo de llegada.

Criterios de aceptación

1. Puntos solo si correcta y antes del cierre del timer (RB-17).
2. Sumar al puntaje global de la sesión (RB-29).
3. Registrar timestamp servidor para desempate (HU-40).

Flujos alternos

- Respuesta correcta pero tardía → 0 puntos (RB-12).

Trazabilidad: RF-28, RF-29 · RB-17, RB-29, RB-12

---

HU-38: Emisión de ranking parcial y feedback

Definición  
Como Sistema, quiero notificar resultados de ronda y ranking global de la sesión tras cada pregunta o al cerrar ronda trivia.

Criterios de aceptación

1. Broadcast de respuesta correcta (si la dinámica lo prevé).
2. Actualizar componente de ranking vía SignalR/WebSockets.
3. Persistir ranking en BD para recuperación si falla tiempo real.

Flujos alternos

- WebSockets caído → cliente recupera por consulta REST.

Trazabilidad: RF-16, RF-28 · RB-08, RB-29

---

HU-39: Transición y lanzamiento automático entre preguntas

Definición  
Como Sistema, quiero gestionar el tiempo de descanso entre preguntas de la etapa Trivia activa y lanzar la siguiente automáticamente.

Criterios de aceptación

1. Cronómetro de transición configurable (p. ej. 10 s).
2. Lanzar siguiente pregunta si quedan en la etapa actual.
3. Al agotar preguntas de la etapa → HU-20 (avanzar a siguiente etapa de misión o HU-23 si era la última).

Flujos alternos

- Última pregunta de la etapa trivia → no lanzar más; completar etapa.
- Última etapa de la misión → finalizar sesión en lugar de transición trivia.

Trazabilidad: RF-27, RF-35 · RB-32, RB-34

---

HU-40: Criterio técnico de desempate

Definición  
Como Sistema, quiero registrar el timestamp de servidor al recibir cada respuesta trivia para desempatar participantes con igual puntaje (RB-08).

Criterios de aceptación

1. Marca de tiempo en backend al recibir mensaje válido.
2. Ranking ordena por suma de tiempos de respuesta en desempate (menor tiempo total gana).
3. Respuesta después de cierre de validación → “Fuera de tiempo”, sin puntos ni tiempo útil (RB-12).

Flujos alternos

- Marcas idénticas → misma posición hasta siguiente evento que desempate.
- Latencia extrema post-cierre → 0 puntos.

Trazabilidad: RF-28 · RB-08, RB-12, RB-17

---

Índice rápido

 Rango  Bloque 
 HU-01 … HU-10  Catálogo misión y pistas (BT) 
 HU-A1  Etapa Trivia en misión 
 HU-I1, HU-I2  Usuarios administrables 
 HU-11 … HU-23  Sesión de misión y juego BT 
 HU-24 … HU-31  Banco trivia (catálogo) 
 HU-32  Deprecada 
 HU-33 … HU-40  Trivia en vivo (etapa activa) 

---

