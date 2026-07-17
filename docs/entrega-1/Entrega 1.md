# **Entrega 1 — Bloque A completado**

**HU Realizadas:**

| HU | Titulo | Evidencia en el proyecto |
| :---: | ----- | ----- |
| HU-01 | Creación de misión  | API \+ MisionesPage \+ dominio RB-10  |
| HU-02 | Consulta de misiones | Listado y filtros en admin \+ queries  |
| HU-03 | Modificación de misión | Renombrar \+ editar/agregar/eliminar etapas (BT/Trivia) con RB-11; activar/desactivar es HU-04 |
| HU-04 | Eliminación / desactivación de misión | EliminarMision / desactivar  |
| HU-05 | Configuración de etapas (BT / Trivia) | EtapaBusquedaTesoro, EtapaTrivia, RB-33  |
| HU-06 | Registro de pistas en etapa BT | AgregarPistaEtapa \+ administración  |
| HU-07 | Modificación de pistas | `EditarPistaEtapa` \+ PUT pistas \+ UI `MisionEtapasPanel` (RB-11) |
| HU-08 | Eliminación de pistas | `EliminarPistaEtapa` \+ DELETE pistas \+ UI admin |
| HU-09 | Liberación automática por tiempo | Dominio `LiberarPistasPorTiempoVencidas` \+ background poll ~5s; notifica vía SignalR |
| HU-10 | Liberación por ganador de etapa BT | `LiberarPistasPorGanadorEtapaSiguiente` en evidencia válida; notifica vía SignalR |
| RF-15 | Pista manual del operador (ad-hoc) | `LiberarPistaManual` + panel operador; notifica vía SignalR |
| HU-12 | Creación de sesión | CrearSesionMision, código de acceso, snapshot RB-11, nombre de instancia  |
| HU-13 | Inscripción de participantes | UnirseSesion, RB-02/03, web participante  |
| HU-14 | Inicio de sesión de la mision  | IniciarSesion, RB-18 |
| HU-15 | Pausa y reanudación de sesión | \`PausarSesion\` / \`ReanudarSesion\` + SignalR \`SesionEstadoCambiado\` (banner “Reconectando…”) |
| HU-16 | Aplicación de penalizaciones | `AplicarPenalizacion` \+ `PenalizarParticipantePanel` (RB-20) + SignalR `RankingActualizado` |
| HU-17 | Tablero en tiempo real (BT) | `SesionHub` `EtapaAvanzada` / `RankingActualizado` + `ParticipanteTableroHeader` (TriviaHub pendiente) |
| HU-18 | Envío de evidencias (QR) | `SubmitEvidencia` \+ `ParticipanteGameplayShell` (RB-22) |
| HU-19 | Validación ganador único de etapa | Dominio BT \+ feedback UI tras envío evidencia |
| HU-20 | Transición automática de fase | Avance etapa en dominio \+ refetch ranking/etapas |
| HU-21 | Ranking en sesión (tiempo real) | `SesionHub` `RankingActualizado` con snapshot + `SesionRankingPanel` “En vivo” |
| HU-22 | Historial de auditoría | `GetHistorialSesion` \+ `SesionHistorialPanel` (RF-21) |
| HU-23 | Reporte final / cerrar sesión | `GetReporteFinalSesion` \+ `ReporteFinalPanel` (RB-26) |
| HU-24 | Registro de pregunta de trivia | Banco de trivia (CRUD)  |
| HU-25 | Consulta y filtrado de preguntas | PreguntasPage  |
| HU-26 | Modificación de pregunta | CRUD banco  |
| HU-27 | Eliminación lógica de pregunta | RB-16  |
| HU-28  | Registro de categoría | CRUD categorías |
| HU-29 | Consulta de categorías | Listado admin  |
| HU-30 | Modificación de categoría | RB-15  |
| HU-31 | Eliminación de categoría | RB-14  |
| HU-32 | Control de sala de espera | Lista en vivo (`ParticipantesActualizados`) \+ `ExpulsarParticipante` (solo `EnPreparacion`) \+ SignalR `ParticipanteExpulsado` \+ UI operador |
| HU-33 / HU-38 | Secuencia automática trivia | Operador inicia etapa (`POST …/trivia/lanzar`); `TriviaCicloBackgroundService` cierra al vencer timer y lanza la siguiente; SignalR + UI live |
| HU-34 | Envío de respuesta trivia | `POST …/trivia/respuestas` (sync MediatR); UI confirma opción; 0 pts si tarde (RB-12); una sola confirmación (RB-13); ranking SignalR (cola RabbitMQ = HU-35) |
| HU-40 | Etapa Trivia en la misión | EtapaTrivia \+ categorías RB-33  |
| HU-41 | Usuario administrable (Keycloak \+ BD) | UsuariosPage, RF-31, RB-37; alta sin password → email `UPDATE_PASSWORD` (bandeja local: http://localhost:8025) |
| HU-42 | Cuentas Participante e inscripción a sesión | Auto-registro `/registro` + rol Participante; Admin no asigna Participante (RB-35); juego con código RB-02 |

**RNF Cumplidos:**

| RNF | Descripcion |
| :---: | ----- |
| RNF-01 | React \+ .NET  |
| RNF-02 | PostgreSQL \+ Entity Framework Core  |
| RNF-03 | Tiempo real (SignalR `SesionHub`) — estado, pistas, etapa, ranking (BT), trivia pregunta/transición (HU-33) |
| RNF-04  | MediatR / CQRS |
| RNF-06 | Arquitectura hexagonal  |
| RNF-07 | Dominio aislado de infraestructura  |
| RNF-08 | Logging, excepciones y validación  |
| RNF-09 | Cobertura de pruebas backend ≥ 90% (CI)  |
| RNF-10 | Docker Compose  |
| RNF-11 | Pipeline de integración continua  |
| RNF-12  | Interfaz usable (alcance Entrega 1\) |

**Demo:**

| Paso | Actor | Accion |
| :---: | ----- | ----- |
| 1 | Todos | Login Keycloak, o participante nuevo: Login → Crear cuenta (`/registro`) |
| 2 | Administrador | Misión polimórfica: etapas BT \+ Trivia, pistas y banco trivia  |
| 2b | Administrador | Crear usuario administrable → abrir **Mailpit** http://localhost:8025 → link del correo → fijar contraseña en Keycloak |
| 3 | Operador | Crear sesión con nombre, abrir inscripción, unir participantes, iniciar, evidencia/ranking, penalizar, historial, reporte final  |
| 4 | Participante | Unirse con código, tablero en vivo (etapa/pistas/ranking sin F5), evidencia QR |
| 5 | Repositorio | `dotnet test`, reporte cobertura, pipeline CI  |

**Infra local útil (emails de Keycloak):**

| Servicio | URL |
| ---- | ---- |
| Mailpit (bandeja SMTP de prueba) | http://localhost:8025 |
| Keycloak admin | http://localhost:8080 (`admin` / `admin`) |

