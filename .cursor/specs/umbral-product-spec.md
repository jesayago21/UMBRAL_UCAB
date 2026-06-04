# UMBRAL — Especificación de Producto

> **Normativa:** Esta spec y `.cursor/` son la referencia técnica actualizada. Los códigos **RB**, **RF**, **RNF** y **HU-01…HU-40** deben coincidir con `docs/TRAZABILIDAD.md` y `docs/ERS_Proyecto_UMBRAL_UCAB.md` (§7–§8 sincronizados).

## 1. Visión del producto

UMBRAL es una plataforma web para diseñar y operar experiencias de investigación
inmersiva en tiempo real. Soporta dos modos de juego independientes:

- **Búsqueda del Tesoro**: participantes compiten encontrando y escaneando códigos QR
  por etapas. Gana quien complete cada etapa primero.
- **Trivia**: participantes responden preguntas sincronizadas con timer. Gana quien
  acumule más puntos por exactitud y velocidad.

Ambos modos comparten la infraestructura de sesión, participantes, ranking y
notificaciones en tiempo real. Una **misión** puede combinar etapas de
Búsqueda del Tesoro y Trivia en secuencia. Una **sesión de misión** recorre
esas etapas en orden; el motor aplica las reglas del tipo de etapa activa.

---

## 2. Actores del sistema

| Actor               | Responsabilidades principales                                      |
|---------------------|--------------------------------------------------------------------|
| Administrador       | Configura misiones (BT), gestiona banco de preguntas (Trivia) y usuarios. |
| Operador            | Crea y controla sesiones en vivo de cualquier tipo. Libera pistas, penaliza participantes y supervisa la ejecución. |
| Equipo Participante | Se conecta a su sesión, recibe pistas (BT) o responde preguntas (Trivia), ve su puntaje y ranking. |

---

## 3. Bounded Contexts

### BC 1 — Catálogo de Misiones (polimórfico)

Gestiona el diseño de misiones: etapas tipadas (`BusquedaTesoro` | `Trivia`),
pistas/QR en etapas BT y `CategoriaId` en etapas Trivia.

Lenguaje ubicuo: Mision, Etapa, TipoEtapa, EtapaBusquedaTesoro, EtapaTrivia,
Pista, CodigoQR, TipoLiberacion, MisionSnapshot.

### BC 2 — Catálogo de Trivia (banco)

Gestiona el banco de preguntas y categorías (referenciadas por ID desde etapas).

Lenguaje ubicuo: Pregunta, OpcionRespuesta, Categoria, Dificultad, Timer.

### BC 3 — Ejecución de Sesión

Gestiona el ciclo de vida de sesiones de misión, progresión secuencial de etapas,
participantes, ranking, penalizaciones e historial.

Lenguaje ubicuo: Sesion, ContextoMision, MisionId, ParticipanteSesion, Puntaje,
Penalizacion, EventoSesion, RankingService.

### BC 4 — Identidad y Accesos

Sincronización sombra con Keycloak: usuarios administrables, roles y estado.

Lenguaje ubicuo: UsuarioAdministrable, KeycloakUserId, EmailAddress, RolSistema,
EstadoUsuario, IIdentityService.

---

## 4. Módulos funcionales

### MÓDULO 1 — Gestión de Misiones (BC: Catálogo de Misiones)

**Quién:** Administrador
**Aplica a:** Diseño de misiones (etapas BT y/o Trivia)

Flujo principal:
Crear misión → Agregar etapas tipadas → Configurar pistas/QR (BT) o categorías (Trivia) → Activar misión

Reglas de negocio:
- Una misión necesita al menos una etapa para activarse (RB-09).
- Solo las misiones en estado `Activa` pueden usarse para crear sesiones (RB-01).
- El nombre de misión es único en el sistema (RB-10).
- Cada etapa tiene su propio `CodigoQR` que los participantes deben encontrar y escanear.
- Las pistas de una etapa tienen orden y tipo de liberación:
  - `PorTiempo`: se libera automáticamente al transcurrir N segundos (RB-07).
  - `PorGanador`: se libera cuando un participante completa la etapa anterior.
- Al crear una sesión desde una misión se genera un `MisionSnapshot` inmutable.
  Los cambios posteriores a la misión no afectan sesiones en curso (RB-11).
- Desactivar una misión no elimina ni afecta sesiones históricas.

---

### MÓDULO 2 — Banco de Preguntas (BC: Catálogo de Trivia)

**Quién:** Administrador
**Aplica a:** Sesiones tipo Trivia únicamente

Reglas de negocio:
- Cada pregunta tiene: enunciado, 2 o más opciones (exactamente una correcta),
  categoría, dificultad y timer en ms (RF-23, RF-24, RF-25).
- Las preguntas se eliminan lógicamente (`isDeleted = true`), nunca físicamente (RB-16).
- Los nombres de categoría son únicos. No se pueden duplicar (RB-15).
- Si se elimina una categoría, sus preguntas se mueven a "Sin Categoría" (RB-14).
- Al renombrar una categoría el cambio se refleja automáticamente en todas
  sus preguntas asociadas.
- Si no hay categorías, el sistema muestra invitación a crear la primera.

---

### MÓDULO 3 — Gestión de Sesiones (BC: Ejecución de Sesión)

**Quién:** Operador
**Aplica a:** Ambos modos

El operador crea una **sesión de misión** seleccionando una `Mision` activa.
La sesión ejecuta las etapas en orden según la plantilla del administrador.

Estados válidos y transiciones:

Programada → EnPreparacion → Activa ⇄ Pausada → Finalizada
↘                   ↗
Cancelada (desde cualquier estado no terminal)

Reglas de negocio:
- Solo se puede crear sesión BusquedaTesoro desde una misión `Activa` (RB-01).
- Una sesión `Finalizada` o `Cancelada` no admite más operaciones.
- Al crear la sesión se genera un código de acceso único para que los participantes
  se unan a la sala de espera.
- Una sesión no puede iniciarse sin al menos un participante registrado (RB-18).

---

### MÓDULO 4 — Gestión de Participantes (BC: Ejecución de Sesión)

**Quién:** Operador (registra), Equipo Participante (se autentifica)
**Aplica a:** Ambos modos

Reglas de negocio:
- El nombre de participante es único dentro de la sesión (RB-02).
- Al registrarse el participante recibe un `CodigoAcceso` generado automáticamente.
- El participante usa ese código para autenticarse y acceder a su panel.
- No se puede registrar un participante en sesión `Finalizada` o `Cancelada` (RB-03).
- El puntaje inicial de todo participante es cero.
- El operador puede expulsar participantes durante la sala de espera si el nombre
  es inapropiado (HU-33).

---

### MÓDULO 5 — Panel del Operador (BC: Ejecución de Sesión)

**Quién:** Operador
**Aplica a:** Ambos modos (con secciones específicas por modo)

Capacidades compartidas:
- Ver estado global de la sesión y tiempo transcurrido.
- Ver ranking en tiempo real con puntajes y posiciones.
- Aplicar penalizaciones con motivo justificado (RF-13, RB-20).
- Cambiar estado de la sesión (pausar, reanudar, finalizar).
- Ver historial completo de eventos.

Capacidades exclusivas BusquedaTesoro:
- Ver etapa activa y progreso por participante.
- Liberar pistas manualmente para participante específico o para todos (RF-15).

Capacidades exclusivas Trivia:
- Ver pregunta activa y timer en curso.
- Ver respuestas recibidas por participante en tiempo real.
- Ver sala de espera con participantes conectados antes de iniciar (HU-33).

---

### MÓDULO 6 — Panel del Equipo (BC: Ejecución de Sesión)

**Quién:** Equipo Participante
**Aplica a:** Ambos modos (con vistas distintas)

Vista BusquedaTesoro:
- Pistas habilitadas para la etapa actual (solo las propias).
- Temporizador de la etapa.
- Puntaje acumulado.
- Formulario para escanear/ingresar código QR y enviar evidencia.

Vista Trivia:
- Enunciado de la pregunta activa.
- Opciones de respuesta.
- Timer de la pregunta en cuenta regresiva.
- Puntaje acumulado.
- Aviso "Preparando siguiente pregunta" durante transición.

Comportamiento compartido:
- Notificaciones instantáneas vía WebSocket de cambios de estado.
- Reconexión automática si se pierde la conexión durante la sesión.
- Si el participante entra con la sesión ya iniciada, se sincroniza al estado actual.

---

### MÓDULO 7 — Flujo BusquedaTesoro (BC: Ejecución de Sesión)

**Gestiona:** `ContextoBusquedaTesoro` dentro de `Sesion`

Flujo por etapa:

Sesión inicia en Etapa 1 → Participantes buscan el QR físico →
Equipo escanea QR → Sistema valida evidencia →
Si es el primero válido:
→ Equipo gana la etapa (puntaje completo)
→ Se liberan pistas PorGanador para los demás
→ Sesión avanza a la siguiente etapa para todos
Si no es el primero:
→ Equipo bloqueado para puntuar en esta etapa
→ Puede seguir buscando pero sin puntos disponibles

Reglas de negocio:
- Solo se aceptan evidencias para la etapa marcada como activa en `ContextoBusquedaTesoro` (RB-06).
- La validación compara el QR escaneado con el `CodigoQRSolucion` de la etapa activa (RF-20).
- La cadena de validación (Chain of Responsibility):
  1. ValidarSesionActiva
  2. ValidarEtapaActiva
  3. ValidarQRCoincide
  4. ValidarNoDuplicada (mismo participante no puede ganar dos veces)
- El primer participante en validar correctamente es el ganador único de la etapa (RB-04).
- Al completar la última etapa, la sesión finaliza automáticamente.
- Cada evidencia se registra con: timestamp servidor, participante, sesión y resultado (RF-11).
- El puntaje se recalcula al validar (RF-12) vía evento de dominio → consumer RabbitMQ.

---

### MÓDULO 8 — Flujo Trivia (BC: Ejecución de Sesión)

**Gestiona:** `ContextoTrivia` dentro de `Sesion`

Flujo por ronda:

Sistema lanza pregunta (broadcast) →
Participantes ven enunciado + opciones + timer en cuenta regresiva →
Cada participante confirma su respuesta (se bloquea al confirmar o al llegar a 0s) →
Timer expira → Sistema cierra ventana de recepción (timerCerradoEn) →
Sistema valida todas las respuestas recibidas antes del cierre →
Broadcast: respuesta correcta + ranking actualizado →
Pausa de transición (N segundos, aviso "Preparando siguiente pregunta") →
Si hay más preguntas → Sistema lanza siguiente automáticamente
Si era la última → Sistema finaliza la sesión

Reglas de negocio:
- La validación ocurre al expirar el timer, no al momento de confirmar (RF-26).
- Una respuesta confirmada o con timer expirado no puede modificarse (RF-30, RB-13).
- Una respuesta que llega al servidor después del cierre del timer recibe 0 puntos,
  independientemente de si era correcta (RB-12).
- El timestamp de llegada al servidor es el criterio de desempate en ranking (HU-40).
- Si dos participantes tienen timestamp idéntico, comparten posición hasta la siguiente ronda.
- Los eventos publicados en RabbitMQ: `PreguntaLanzada`, `RespuestaTriviaRecibida`,
  `TiempoAgotado` (RF-29).
- El puntaje de trivia se basa en corrección y opcionalmente en velocidad (RF-28).

---

### MÓDULO 9 — Ranking y Tiempo Real (BC: Ejecución de Sesión)

**Quién:** Sistema → todos los clientes
**Aplica a:** Ambos modos

Reglas de ranking (RB-08):
- Ordenado por puntaje total descendente.
- Desempate primario: menor suma de tiempos de respuesta (más rápido primero).
- Desempate secundario: si los tiempos son idénticos, comparten posición.

Actualización en tiempo real:
- El ranking se emite vía WebSocket en cada cambio de puntaje.
- Si SignalR falla, el cliente recupera el ranking mediante Query REST de respaldo.
- Latencia máxima aceptable de actualización: < 1 segundo en condiciones normales (RNF-14).

---

### MÓDULO 10 — Procesamiento Asíncrono (RabbitMQ)

**Quién:** Sistema (interno)
**Aplica a:** Ambos modos

| Evento                    | Consumer(s)                                  | Modo      |
|---------------------------|----------------------------------------------|-----------|
| `SesionCreada`            | Auditoría                                    | Ambos     |
| `SesionIniciada`          | Auditoría, Notificaciones                    | Ambos     |
| `SesionFinalizada`        | Auditoría, Consolidación historial           | Ambos     |
| `PenalizacionAplicada`    | Recálculo de puntaje, Auditoría              | Ambos     |
| `EvidenciaValidada`       | Recálculo de puntaje, Auditoría              | BT        |
| `EtapaCompletada`         | Transición de etapa, Notificaciones          | BT        |
| `PistaLiberada`           | Notificaciones a participantes                     | BT        |
| `PreguntaLanzada`         | Auditoría de trivia                          | Trivia    |
| `RespuestaTriviaRecibida` | Validación de respuesta, Puntaje             | Trivia    |
| `TiempoAgotado`           | Cierre de ronda, lanzamiento siguiente       | Trivia    |

Resiliencia:
- 3 reintentos por mensaje fallido.
- Tras el tercer fallo → Dead Letter Queue (DLQ) para auditoría.
- El flujo principal no se detiene por fallos en la DLQ.

---

## 5. Requerimientos funcionales

| Código | Descripción                                                                        | Modo  |
|--------|------------------------------------------------------------------------------------|-------|
| RF-01  | Crear, editar, consultar y desactivar misiones.                                    | BT    |
| RF-02  | Registrar etapas, pistas y código QR por etapa en cada misión.                    | BT    |
| RF-03  | Crear sesión BusquedaTesoro a partir de una misión activa.                        | BT    |
| RF-04  | Manejar estados: Programada, EnPreparacion, Activa, Pausada, Finalizada, Cancelada.| Ambos |
| RF-05  | Registrar participantes y asociarlos a una sesión.                                       | Ambos |
| RF-06  | Cada participante visualiza temporizador, puntaje y pistas habilitadas.                  | BT    |
| RF-07  | Participantes envían evidencias QR vinculadas a la etapa activa.                        | BT    |
| RF-08  | Restringir recepción de evidencias únicamente a la etapa activa.                  | BT    |
| RF-09  | Al validar la primera evidencia correcta, asignar puntaje y bloquear a los demás. | BT    |
| RF-10  | Al completar el nodo, avanzar automáticamente a la siguiente etapa para todos.    | BT    |
| RF-11  | Registrar cada evidencia con fecha, hora, participante, sesión y resultado.             | BT    |
| RF-12  | Recalcular puntaje automáticamente al validar evidencia o aplicar penalización.   | Ambos |
| RF-13  | El operador aplica penalizaciones justificadas que restan puntos.                 | Ambos |
| RF-14  | Liberar pistas automáticamente por tiempo o al ganar etapa anterior.              | BT    |
| RF-15  | El operador libera pistas manualmente para participante específico o todos.             | BT    |
| RF-16  | Mostrar ranking actualizado automáticamente con criterios de desempate.           | Ambos |
| RF-17  | Notificar en tiempo real cuando se habilite pista o cambie estado.                | BT    |
| RF-18  | Reflejar historial de eventos y actividad en tiempo real.                         | Ambos |
| RF-19  | Publicar eventos en RabbitMQ al registrar evidencias o cambios de estado.         | Ambos |
| RF-20  | Verificar evidencia comparando QR escaneado con código del nodo activo.           | BT    |
| RF-21  | Restringir acceso según rol: Admin, Operador, Equipo.                             | Ambos |
| RF-22  | Consultar misiones, sesiones y rankings con modelos de lectura independientes.    | Ambos |
| RF-23  | CRUD de banco de preguntas categorizadas.                                          | Trivia|
| RF-24  | Cada pregunta tiene múltiples opciones con exactamente una correcta.              | Trivia|
| RF-25  | Configurar timer de respuesta por pregunta de forma independiente.                | Trivia|
| RF-26  | Registrar respuestas en tiempo real y validar al expirar el timer.                | Trivia|
| RF-27  | Lanzar siguiente pregunta automáticamente tras el tiempo de transición.           | Trivia|
| RF-28  | Calcular puntaje de trivia por corrección y opcionalmente por velocidad.          | Trivia|
| RF-29  | Publicar en RabbitMQ: PreguntaLanzada, RespuestaRecibida, TiempoAgotado.         | Trivia|
| RF-30  | Bloquear cambio de respuesta al confirmar o expirar el tiempo.                    | Trivia|

---

## 6. Requerimientos no funcionales

| Código | Descripción                                                             |
|--------|-------------------------------------------------------------------------|
| RNF-01 | Frontend en React + TypeScript. Backend en .NET 8.                      |
| RNF-02 | Persistencia con PostgreSQL y Entity Framework Core.                    |
| RNF-03 | Comunicación en tiempo real sobre WebSockets (SignalR).                 |
| RNF-04 | Lógica de aplicación con MediatR y CQRS.                               |
| RNF-05 | Procesos asíncronos desacoplados mediante RabbitMQ.                     |
| RNF-06 | Arquitectura hexagonal: puertos y adaptadores.                          |
| RNF-07 | El dominio no depende de infraestructura ni del framework web.          |
| RNF-08 | Logging, manejo global de excepciones y validaciones con FluentValidation.|
| RNF-09 | Cobertura de pruebas backend ≥ 90%.                                     |
| RNF-10 | Ejecutable localmente con un solo `docker compose up`.                  |
| RNF-11 | Pipeline CI con compilación y ejecución de pruebas automáticas.         |
| RNF-12 | Interfaz clara, usable y coherente con los flujos principales del sistema. |
| RNF-13 | Soportar picos de concurrencia de escritura en PostgreSQL (p. ej. respuestas trivia simultáneas). |
| RNF-14 | Latencia de actualización en tiempo real < 1 segundo en condiciones normales.|

---

## 7. Reglas de negocio consolidadas

Tabla completa en `docs/TRAZABILIDAD.md`. Resumen:

| Código | Regla                                                                               | Modo  |
|--------|-------------------------------------------------------------------------------------|-------|
| RB-01  | Solo se crea sesión BT desde una misión con estado Activa.                         | BT    |
| RB-02  | El nombre de participante es único dentro de la misma sesión.                            | Ambos |
| RB-03  | Un participante no puede registrarse en sesión Finalizada o Cancelada.                   | Ambos |
| RB-04  | Al validar la primera evidencia correcta, los demás participantes no pueden puntuar en esa etapa. | BT |
| RB-05  | Al completarse un nodo, la sesión avanza a la siguiente etapa para todos a la vez. | BT    |
| RB-06  | Solo se aceptan evidencias para la etapa marcada como activa.                      | BT    |
| RB-07  | Las pistas PorTiempo se liberan automáticamente al transcurrir el tiempo configurado.| BT  |
| RB-08  | Ranking: puntaje desc. Desempate: menor suma de tiempos de respuesta.              | Ambos |
| RB-09  | Una misión necesita al menos una etapa para poder activarse.                       | BT    |
| RB-10  | El nombre de misión es único en el sistema.                                        | BT    |
| RB-11  | El MisionSnapshot es inmutable desde el momento de crear la sesión.               | BT    |
| RB-12  | Una respuesta de trivia que llega después del cierre del timer recibe 0 puntos.   | Trivia|
| RB-13  | Una respuesta de trivia no puede modificarse una vez confirmada o expirado el timer.| Trivia|
| RB-14  | Si se elimina una categoría, sus preguntas se mueven a "Sin Categoría".           | Trivia|
| RB-15  | Los nombres de categoría son únicos. No se pueden duplicar.                        | Trivia|
| RB-16  | Las preguntas se eliminan lógicamente, nunca físicamente.                          | Trivia|
| RB-17  | El puntaje de trivia se otorga solo si la respuesta es correcta y llegó antes del cierre del timer. | Trivia|
| RB-18  | Una sesión no puede iniciarse sin al menos un participante registrado.                   | Ambos |
| RB-19  | No se aceptan evidencias si la sesión está Pausada, Finalizada o Cancelada.      | BT    |
| RB-20  | Toda penalización exige motivo de texto obligatorio.                               | Ambos |
| RB-21  | Una misma pista no se entrega dos veces al mismo participante en la misma etapa.        | BT    |
| RB-22  | QR válido solo si coincide con el código del nodo activo.                          | BT    |
| RB-23  | Una pista solo se libera si el participante está en el nodo correspondiente.             | BT    |
| RB-24  | El puntaje total del participante nunca es inferior a cero.                              | Ambos |
| RB-25  | Todo cambio de puntaje registra OperadorId o evento de sistema.                  | Ambos |
| RB-26  | Sesión Finalizada o Cancelada → solo lectura (auditoría).                          | Ambos |
| RB-27  | El operador solo administra sesiones que tiene asignadas.                          | Ambos |
| RB-28  | Pregunta de trivia: ≥1 correcta y ≥2 incorrectas.                                  | Trivia|
| RB-29  | Puntos de trivia se suman al puntaje global de la sesión.                          | Trivia|
| RB-30  | No respuestas trivia si el timer ya inició al conectar el participante.                  | Trivia|
| RB-31  | En trivia, todos los participantes pueden puntuar si responden correctamente.             | Trivia|
| RB-32  | Pregunta lanzada no cancelable hasta expirar timer o que todos respondan.          | Trivia|

---

## 8. Patrones de diseño requeridos

| Patrón                  | Aplicación concreta en UMBRAL                                               |
|-------------------------|-----------------------------------------------------------------------------|
| Strategy                | `ICalculoPuntajeStrategy` con `BusquedaTesoroStrategy` y `TriviaStrategy`. El modo de la sesión determina cuál se usa. |
| Composite               | `Mision` contiene `List<Etapa>`, cada `Etapa` contiene `List<Pista>`.      |
| Facade                  | `SessionOperationFacade`: coordina cambio de estado + publicación de evento + notificación SignalR. |
| Proxy                   | `PistaAccessProxy`: verifica que el participante tiene permiso antes de exponer el contenido de una pista. |
| Template Method         | `EvidenciaProcessorBase`: Recibir → Validar → CalcularPuntaje → Persistir → Notificar. Cada paso es sobreescribible. |
| State                   | Métodos `Iniciar/Pausar/Reanudar/Finalizar/Cancelar` en `Sesion` validan la transición según el estado actual. |
| Chain of Responsibility | `EvidenciaValidationChain`: ValidarSesionActiva → ValidarEtapaActiva → ValidarQRCoincide → ValidarNoDuplicada. |