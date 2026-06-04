# **Entrega 1**

**HU Realizadas:**

| HU | Titulo | Evidencia en el proyecto |
| :---: | ----- | ----- |
| HU-01 | Creación de misión  | API \+ MisionesPage \+ dominio RB-10  |
| HU-02 | Consulta de misiones | Listado y filtros en admin \+ queries  |
| HU-03 | Modificación de misión | ActualizarMision \+ editor de etapas polimórfico  |
| HU-04 | Eliminación / desactivación de misión | EliminarMision / desactivar  |
| HU-05 | Configuración de etapas (BT / Trivia) | EtapaBusquedaTesoro, EtapaTrivia, RB-33  |
| HU-06 | Registro de pistas en etapa BT | AgregarPistaEtapa \+ administración  |
| HU-07 | Modificación de pistas | Catálogo de misión  |
| HU-08 | Eliminación de pistas | Catálogo de misión  |
| HU-12 | Creación de sesión | CrearSesionMision, código de acceso, snapshot RB-11, nombre de instancia  |
| HU-13 | Inscripción de participantes | UnirseSesion, RB-02/03, web participante  |
| HU-14 | Inicio de sesión de la mision  | IniciarSesion, RB-18 |
| HU-15 | Pausa y reanudación de sesión | \`PausarSesion\` / \`ReanudarSesion\`, panel operador (sin WebSocket) |
| HU-21 | Ranking en sesión | Operador y participante; refresco manual, solo se ve como consulta |
| HU-24 | Registro de pregunta de trivia | Banco de trivia (CRUD)  |
| HU-25 | Consulta y filtrado de preguntas | PreguntasPage  |
| HU-26 | Modificación de pregunta | CRUD banco  |
| HU-27 | Eliminación lógica de pregunta | RB-16  |
| HU-28  | Registro de categoría | CRUD categorías |
| HU-29 | Consulta de categorías | Listado admin  |
| HU-30 | Modificación de categoría | RB-15  |
| HU-31 | Eliminación de categoría | RB-14  |
| HU-40 | Etapa Trivia en la misión | EtapaTrivia \+ categorías RB-33  |
| HU-41 | Usuario administrable (Keycloak \+ BD) | UsuariosPage, RF-31, RB-37  |
| HU-42 | Cuentas Participante e inscripción a sesión | Administración de cuentas demo; juego con código RB-02  |

**RNF Cumplidos:**

| RNF | Descripcion |
| :---: | ----- |
| RNF-01 | React \+ .NET  |
| RNF-02 | PostgreSQL \+ Entity Framework Core  |
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
| 1 | Todos | Login Keycloak: administrador, operador, participante (web)  |
| 2 | Administrador | Misión polimórfica: etapas BT \+ Trivia, pistas y banco trivia  |
| 3 | Operador | Crear sesión con nombre, abrir inscripción, unir participantes, iniciar, evidencia/ranking, penalizar, finalizar  |
| 4 | Participante | Unirse con código, ver sesión (trivia en modo lectura)  |
| 5 | Repositorio | `dotnet test`, reporte cobertura, pipeline CI  |

