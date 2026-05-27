_____________________________________________

Jesus Sayago CI: 30035363

Guillermo Mendez CI:

UMBRAL

ERS

UCAB - 2026

> **Actualización 2026-05-27:** Los códigos **RB**, **RNF** y el alcance mobile están alineados con `.cursor/specs/` y `docs/TRAZABILIDAD.md`. Usar esa tabla como referencia si hay discrepancia con el texto narrativo de este PDF exportado.

1. Nombre del proyecto: UMBRAL.

Descripción  general:  UMBRAL  es  una  plataforma  web  para  diseñar  misiones  de  investigación

inmersiva,   crear  sesiones  en  vivo,  registrar  equipos  participantes,  liberar  pistas,  recibir  evidencias,

aplicar  reglas  de   validación  y  puntaje,  y  supervisar  toda  la  operación  desde  una  consola central. El

sistema  debe  permitir   que  los participantes y los operadores observan cambios de estado en tiempo

real y que determinados  procesos sean desacoplados mediante colas de mensajería.

2. Objetivo general

Desarrollar  una  aplicación  web  para  la  operación  en  tiempo  real  de  experiencias  de  investigación

inmersiva,  aplicando  principios  de  arquitectura limpia y hexagonal, modelado del dominio, separación

CQRS,  persistencia  relacional,  comunicación  en tiempo real y mensajería asíncrona, de forma que la

solución  permita  evidenciar  los  contenidos  teóricos y prácticos del curso dentro de un único proyecto

integrador.

3. Objetivos específicos

●

 Diseñar una arquitectura del sistema que separe con claridad dominio, aplicación,

infraestructura e  interfaces externas.

●  Modelar el problema mediante entidades, agregados, repositorios, value objects y servicios de

dominio.

●

Implementar una interfaz web para administración, operación y participación en sesiones.

●  Construir casos de uso mediante MediatR y patrón CQRS, distinguiendo comandos y consultas.

●

●

 Persistir la información del sistema en PostgreSQL mediante Entity Framework Core.

Incorporar comunicación en tiempo real basada en WebSockets para reflejar cambios

instantáneos  en la operación.

●  Desacoplar procesos secundarios por medio de colas RabbitMQ.

●

 Aplicar pruebas unitarias, de integración y end-to-end con criterios de cobertura y calidad.  

Demostrar el uso de patrones de diseño y principios SOLID en componentes concretos de la

solución.

●  Empaquetar la solución para su ejecución en contenedores y automatizar validaciones mínimas

mediante integración continua.

UCAB - 2026

4. Alcance del proyecto

El alcance mínimo esperado para la solución comprende los siguientes módulos y capacidades

funcionales.

1.

  Gestión  de  misiones:  creación,  edición,  consulta y activación de misiones. Cada misión debe

incluir

nombre,  descripción,  nivel  de  dificultad,  tiempo  máximo,  etapas  o  nodos  de

investigación, pistas y  reglas básicas de avance.

2.  Gestión de sesiones en vivo: creación de sesiones a partir de una misión, cambio de estado de

la  sesión y control del tiempo de ejecución.

3.  Gestión de equipos participantes: registro de equipos, asignación a una sesión y consulta de su

progreso.

4.  Panel del operador: visualización del estado global de la sesión, liberación de pistas, aplicación

de  penalizaciones y seguimiento del ranking.

5.  Panel  del  equipo:  visualización  de  pistas asignadas, temporizador, puntaje acumulado y envío

de  respuestas o evidencias.

6.  Supervisión en tiempo real: actualización instantánea de ranking, cambios de estado, liberación

de  pistas y eventos relevantes.

7.  Procesamiento  asíncrono:  publicación  y  consumo  de  eventos  para  auditoría,  recálculo  de

puntajes,  alertas y consolidación del historial.

8.  Seguridad  básica:  diferenciación  de  permisos  por  rol  para  administrador,  operador  y  equipo

participante.

Quedan expresamente fuera del alcance funcionalidades avanzadas como cobros en línea, integración

con  dispositivos  físicos,  módulos  complejos  de  analítica  histórica,  inteligencia  artificial  aplicada  al

contenido  de  las  misiones,  localización  precisa  de  participantes  o  aplicaciones  móviles  nativas  puras  (Swift/Kotlin  sin  Expo).  La

propuesta  incluye  **web**  (administración  y  operación  en  React)  y  **cliente  equipo**  en  **React  Native  +  Expo**  (`umbral-mobile`),  coherente  y  técnicamente  defendible.

5. Actores del sistema

Actor

Resposabilidades

Permisos minimos

Administrador

Configura misiones, consulta

Crear/editar

misión,

sesiones

y

mantiene

consultar

sesiones,

UCAB - 2026

catálogos base.

gestionar usuarios.

Operador

Inicia sesiones, libera pistas,

Crear

sesión,

cambiar

aplica

penalizaciones

y

estado,

liberar

pista,

supervisa la ejecución.

observar ranking e historial.

Equipo Participante

Consulta  su  tablero,  recibe

Acceder

a

su

sesión,

pistas  y  envía  respuestas  o

visualizar

progreso

y

evidencias.

registrar evidencias.

6. Requerimientos funcionales

Codigo

RF-01

RF-02

RF-03

RF-04

RF-05

RF-06

RF-07

RF-08

RF-09

RF-10

Requerimiento Funcional

El sistema debe permitir crear, editar, consultar y desactivar misiones.

Cada misión debe permitir registrar etapas o nodos, pistas y tiempo máximo
de ejecución.

El  sistema  debe  permitir  crear  una  sesión  en  vivo  a  partir  de  una  misión
activa (RB-01).

La  sesión  debe  manejar  estados  al  menos de programada, en preparación,
activa, pausada, finalizada y cancelada.

El  sistema  debe  permitir  registrar  equipos  participantes  y  asociarlos  a  una
sesión (RB-02).

Cada equipo debe visualizar su temporizador, puntaje y pistas habilitadas.

El  sistema  debe  permitir  a  los  equipos  enviar  evidencias  vinculadas  a  la
etapa activa mediante códigos QR

El  sistema  debe  restringir  la  recepción  de  evidencias  únicamente  para  la
etapa que la sesión tenga marcada como activa (RB-06).

 Al validar la primera evidencia correcta, el sistema debe asignar el puntaje al
ganador  y  bloquear  la  obtención  de  puntos  para  los  demás  en  esa  etapa
(RB-04).

Una  vez  validada  la  evidencia  que  completa  el  nodo,  el  sistema  debe
cambiar  automáticamente  el  estado  de  la  sesión  hacia  la  siguiente  etapa

UCAB - 2026

RF-11

RF-12

RF-13

RF-14

RF-15

RF-16

RF-17

RF-18

RF-19

RF-20

RF-21

RF-22

RF-23

RF-24

RF-25

RF-26

para todos los participantes (RB-05).

Cada  envío  debe  quedar  guardado  con  fecha,  hora,  equipo,  sesión  y  el
resultado de la validación.

El  sistema  debe  recalcular  automáticamente  el  puntaje  total  del  equipo
cuando una evidencia sea validada o se aplique una penalización.

El operador debe poder aplicar penalizaciones justificadas que resten puntos
al total del equipo.

El sistema debe liberar pistas automáticamente según el tiempo transcurrido
(RB-07) o cuando un equipo gane la etapa anterior.

El  operador  debe  poder  liberar  pistas  de  forma  manual  para  equipos
específicos o para todos.

El sistema debe mostrar un ranking actualizado automáticamente, aplicando
criterios de desempate si es necesario (RB-08).

El sistema debe notificar en tiempo real a los equipos cuando se habilite una
nueva pista o cambie el estado de la sesión.

Debe  reflejar  el  historial  de  eventos,  cambios  de  estado  y  actividad  de  los
equipos en tiempo real.

La  aplicación  debe  publicar  eventos  de  dominio  en  RabbitMQ  al  registrar
evidencias o cambios significativos de estado (RNF-05).

El sistema debe verificar la validez de la evidencia comparando el código QR
escaneado con el código asociado al nodo activo.

La  aplicación  debe  restringir  el  acceso  a  funciones  de  configuración  y
operación según el rol del usuario (Admin, Operador, Equipo).

La  solución  debe  permitir  consultar misiones, sesiones y rankings mediante
modelos de lectura independientes de los comandos.

El  sistema  debe  permitir  la  creación,  edición  y  gestión  de  bancos  de
preguntas categorizadas para trivias.

Cada  pregunta  de  trivia  debe  permitir  registrar  múltiples  opciones  de
respuesta, indicando cuál es la correcta.

El  sistema  debe  permitir  configurar  el  tiempo  máximo  de  respuesta  (timer)
por cada pregunta de forma independiente.

El  sistema  debe  registrar  las  respuestas  de  los  equipos  en  tiempo  real  y
validar su veracidad inmediatamente al expirar el tiempo de la pregunta.

UCAB - 2026

RF-27

RF-28

RF-29

RF-30

El  sistema  debe  realizar el lanzamiento automático de la siguiente pregunta
tras agotarse el tiempo de feedback de la ronda anterior.

La aplicación debe calcular el puntaje de trivia basado en la corrección de la
respuesta y, opcionalmente, la velocidad de respuesta del equipo.

La  aplicación  debe  publicar  eventos  en  RabbitMQ  específicos  para  la  trivia
(Pregunta iniciada, Respuesta recibida, Tiempo agotado).

El frontend debe bloquear la posibilidad de cambiar la respuesta una vez que
el equipo ha confirmado su selección o el tiempo ha expirado.

7. Requerimientos no funcionales

> Tabla canónica **RNF-01 … RNF-14** en `docs/TRAZABILIDAD.md` (RNF-12 = UX; RNF-13 = concurrencia PostgreSQL; RNF-14 = latencia tiempo real).

Codigo

Requerimiento no funcional

RNF-01

La solución debe implementarse con frontend en React y backend en .NET Core.

RNF-02

La persistencia principal debe resolverse con PostgreSQL y Entity Framework Core.

RNF-03

La comunicación en tiempo real debe implementarse sobre WebSockets.

RNF-04

La lógica de aplicación debe estructurarse con MediatR y enfoque CQRS.

RNF-05

Los procesos asíncronos deben desacoplarse mediante RabbitMQ.

RNF-06

La  solución  debe  seguir  arquitectura  hexagonal  o  una  variante  compatible  con
arquitectura limpia.

RNF-07

El dominio no debe depender de infraestructura ni de detalles del framework web.

RNF-08

RNF-09

La  aplicación  debe  incorporar  logging,  manejo  de  excepciones  y  validaciones
consistentes.

El  backend  debe  alcanzar  como  meta  académica  una  cobertura  de  pruebas  de  al
menos 90 %

RNF-10

La solución debe poder ejecutarse localmente mediante Docker Compose.

RNF-11

RNF-12

El  repositorio  debe  incluir  pipeline  de  integración  continua  para  compilación  y
ejecución de pruebas.

La  interfaz  debe  ser  clara,  utilizable  y  coherente  con  los  flujos  principales  del
sistema.

RNF-13

El  sistema  debe  soportar  picos  de  concurrencia  de  escritura  en  la  base  de  datos

UCAB - 2026

(PostgreSQL)
simultáneamente.

cuando

todos

los  equipos  envían

respuestas  de

trivia

8. Reglas de negocio

Ver tabla canónica completa en **`docs/TRAZABILIDAD.md`** (RB-01 … RB-32). Resumen alineado con `.cursor/specs/umbral-product-spec.md`:

| Código | Regla (resumen) | Modo |
|--------|-----------------|------|
| RB-01 | Sesión BT solo desde misión `Activa`. | BT |
| RB-02 | Nombre de equipo único por sesión. | Ambos |
| RB-03 | No registrar equipo en sesión terminal. | Ambos |
| RB-04 | Primer evidencia válida: único que puntúa en la etapa. | BT |
| RB-05 | Al resolver nodo, todos avanzan de etapa. | BT |
| RB-06 | Evidencias solo para etapa activa. | BT |
| RB-07 | Liberación automática de pistas por tiempo (configurable). | BT |
| RB-08 | Ranking y desempate por tiempo acumulado. | Ambos |
| RB-09 | Misión con ≥1 etapa para activarse. | BT |
| RB-10 | Nombre de misión único. | BT |
| RB-11 | `MisionSnapshot` inmutable en sesión. | BT |
| RB-12–17 | Reglas de trivia (timer, categorías, puntaje). | Trivia |
| RB-18 | Sesión no inicia sin ≥1 equipo. | Ambos |
| RB-19 | Sin evidencias si sesión pausada/finalizada/cancelada. | BT |
| RB-20 | Penalización con motivo obligatorio. | Ambos |
| RB-21–23 | Pistas y QR por nodo activo. | BT |
| RB-24 | Puntaje nunca &lt; 0. | Ambos |
| RB-25–27 | Trazabilidad, solo lectura, operador por sesión asignada. | Ambos |
| RB-28–32 | Formato y flujo de trivia. | Trivia |

9. Historias de Usuario:

Nombre

Definicion

Criterios de aceptación

Flujos alternos

HU-01:  Creación
de misión.

Como
quiero
Administrador,
registrar una nueva misión con su
nombre y descripción.

Validar  que  el  nombre
sea  único;  el  estado
inicial
ser
"Inactivo".

debe

Si el nombre ya existe,
el  sistema  impide  el
guardado  y  notifica  al
usuario.

HU-02:  Consulta
de misiones.

quiero
Administrador,
Como
visualizar  el  listado  de  misiones
existentes  para  gestionar  el
catálogo.

Debe  permitir  filtrar  por
estado (Activo/Inactivo) y
nombre.

no

existen
Si
misiones,  el  sistema
muestra  un  mensaje
de  "No  hay  registros
disponibles".

HU-03:

Como Administrador, quiero editar  Solo

se

permiten  Si

la  misión  está

UCAB - 2026

Modificación
misión.

de

los  detalles  de  una  misión
existente.

cambios  si  la  misión  no
tiene sesiones activas en
ese momento.

HU-04:
Eliminación
misión.

de

Como
quiero
Administrador,
eliminar  o  desactivar  misiones
que ya no se utilicen.

la  misión

Si
tiene
historial  de  sesiones,
la
solo
desactivación
lógica
(RB-01).

permite

se

HU-05:
Configuración  de
nodos (tesoros).

Como
quiero
Administrador,
agregar  nodos  a  una  misión
tiempo
definiendo  su  orden  y
máximo (RF-02).

El
tiempo  debe  ser
mayor  a  cero;  el  orden
debe ser correlativo.

HU-06:  Registro
de  pistas  en  el
nodo (tesoro).

Como  Administrador, quiero crear
y  asociar  pistas  (texto/enlace)  a
un  nodo de misión para alimentar
el banco de ayudas.

HU-07:
Modificación
Pistas

Como Administrador, quiero editar
el contenido para corregir errores.

de

Cada  pista  debe  estar
vinculada
obligatoriamente  a  un
nodo.
Debe  permitirse  asignar
un  "Orden  de  Aparición"
(Prioridad).
El  sistema  debe  permitir
marcar si una pista es de
"Liberación por Ganador"
por
"Liberación
o
Tiempo".

No  se  permite  la  edición
la  misión  está  en
si
estado
en
"Activo"
alguna sesión en curso.

vinculada a una sesión
activa,  el  botón  de
editar  se  bloquea y se
muestra
una
advertencia.

Intento  de  eliminar
misión  con  sesiones:
el  sistema  cambia  el
"Inactivo"
estado  a
automáticamente
en
lugar de borrar.

se

Si
ingresa  un
tiempo menor o igual a
cero, el sistema marca
error  de  validación  de
campo.

misión

Si  el  nodo  no  existe  o
la
está
bloqueada  por  una
el
sesión
sistema
la
creación.

activa,
impide

la  pista  ya

Si
entregada
sesión
bloquea
para
inconsistencias
vivo.

fue
una
en
activa,
se
la  edición
evitar
en

HU-08:
Eliminación
Pistas.

de

quiero
Administrador,
Como
eliminar  pistas  del  catálogo  para
corregir errores.

Al  eliminar  una  pista,  el
sistema  debe  reordenar
las  prioridades  restantes
del nodo.

Si  se  intenta  eliminar
la  última  pista  de  un
nodo, el sistema lanza
una  advertencia  sobre
la dificultad del reto.

HU-09:  Lógica  de
Liberación

Como  Sistema, quiero liberar una
pista si transcurre el tiempo límite

Ejecutar
asíncronamente

el

proceso
y

Si ya no quedan pistas
liberar  en  ese
por

UCAB - 2026

Automática.

sin un ganador (RF-25).

notificar a los equipos.

HU-10:  Liberación
por Ganador.

Como  Sistema, quiero liberar una
pista  del  "Siguiente  Nodo"  para
todos
los  equipos  cuando  se
registre  el  primer  ganador  de  la
fase actual.

dispararse
Debe
inmediatamente después
de  la  validación  exitosa
de la evidencia (RF-24).
La  pista  liberada  debe
ser
la  definida  como
"Pista  de  Avance"  en  el
nodo N+1.

nodo,  el  sistema  deja
el
ejecutar
de
temporizador.

Si es el último nodo de
la  misión,  no  se  libera
nada.

HU-11:
Visualización
Pistas
Disponibles.

de

Como  Equipo,  quiero  visualizar
las  pistas  que  me  han  sido
otorgadas en mi panel y consultar
mi historial de pistas.

aparecer
ya

las
otorgadas

Deben
pistas
anteriormente.
No  se  pueden  recibir
pistas duplicadas para el
mismo nodo (RB-04).

HU-12:  Creación
de sesión.

Como Operador, quiero crear una
sesión  a  partir  de  una  misión
activa (RF-03).

Validar  que
la  misión
esté  en  estado  "Activo"
(RB-01).

el

equipo

Si
se
desconecta,  al  volver
una
realizar
debe
Query de recuperación
para obtener las pistas
perdidas.

Si
la  misión  está
inactiva,  no  aparece
de
en
creación de sesiones.

listado

el

HU-13:  Inscripción
de equipos.

Como  Operador,  quiero  registrar
equipos y asociarlos a una sesión
programada

Generar  un código único
de acceso por sesión.

HU-14:  Control  de
inicio de sesión.

Como  Operador,  quiero  cambiar
el  estado  de  la  sesión  a  "Activa"
para que los equipos empiecen.

El  sistema  bloquea  el
inicio  si  hay  0  equipos
registrados (RB-18)

Si  el  equipo  ya  está
en
registrado
la
sesión,
sistema
el
notifica la duplicidad.

Si  falta  configuración
en  la  misión  original
(ej.  nodos  sin  pistas),
el  sistema  impide  el
inicio.

HU-15:  Pausa  y
reanudación
de
sesión.

HU-16:  Aplicación
de penalizaciones.

Como  Operador,  quiero  parar  la
los
sesión
cronómetros
los
equipos.

detener
todos

para

de

Como  Operador,  quiero  restar
puntos
por
un
comportamiento indebido (RF-11).

equipo

a

UCAB - 2026

Los  equipos  deben  ver
un bloqueo en su interfaz
vía
WebSockets
(RNF-03).

la

conexión
falla,  el
debe

Si
WebSocket
frontend
bloquearse
al
preventivamente
detectar  la  pérdida  de
heartbeat.

registrar  el
Obligatorio
motivo
y
publicar  el  evento  en

(RB-20)

Si  el  operador  intenta
restar  más  puntos  de
los  que  el  equipo

HU-17:
Visualización  del
tablero  en  tiempo
real.

Como Equipo, quiero ver mi nodo
actual,  tiempo  restante  para  la
siguiente  pista  y  puntaje  sin
refrescar la pantalla.

RabbitMQ (RF-14).

datos

deben

Los
actualizarse
instantáneamente
mediante  WebSockets
(RNF-03).
Tiempo  para  la siguiente
pista (RB-07).

tiene,
sistema
el
permite saldo negativo
si la regla lo avala.

Si  el  socket  cae,  el
sistema  muestra  un
indicador
de
"Reconectando"  y  el
estima
se
tiempo
localmente.

HU-18:  Envio  de
Evidencias.

Como  Equipo,  quiero  subir  mi
respuesta  para  intentar  ganar  el
nodo actual (RF-08).

El  sistema  debe  validar
que  sea  el  nodo  activo
(RF-19).
valida
El
mediante  un  código  QR
(RF-28).

sistema

Si  se  escanea  un  QR
de  un  nodo  anterior  o
el
futuro,
sistema
la  evidencia
rechaza
con  un  mensaje  de
error.

HU-19:  Validación
de  ganador  único
de etapa.

Como  Sistema,  quiero  identificar
al  primer  equipo  con  evidencia
válida  para  asignar  los  puntos
exclusivos (RF-20, RB-22).

envíos
Bloquear
posteriores
otros
equipos  para  el  mismo
nodo (RB-04).

de

HU-20:  Transición
Automática
de
Fase.

Como  Sistema,  quiero  mover  a
todos
los  equipos  al  siguiente
nodo  cuando  el  actual  sea
resuelto (RF-21).

el

Actualizar
estado
global  de  la  sesión  para
todos
los  clientes  en
tiempo real.

HU-21:  Ranking  y
Trazabilidad.

Como  Operador/Equipo,  quiero
reordenado
ver
el
automáticamente
cada
evento (RF-12).

ranking

tras

Usar menor tiempo como
desempate  en  caso  de
puntos
iguales  (RF-26,
RB-08).

HU-22:  Consulta
de  Historial  de
Auditoría.

Como Administrador, quiero ver el
log  de  eventos  de  una  sesión
la  integridad  del
para  verificar
juego (RF-15).

Listar
mensajes
desde
(RNF-05).

todos

los
procesados
RabbitMQ

el

Si  dos  equipos envían
en
mismo
la  base
milisegundo,
datos
de
(concurrencia)
determina el primero y
rechaza el segundo.

Si  era  el  último  nodo,
dispara
se
automáticamente
el
flujo  de  finalización de
sesión.

servicio  de
Si  el
ranking
(Query)  no
responde,  se  muestra
la
versión
cacheada en el cliente

última

Si  la  auditoría  es  muy
extensa,  el  sistema
implementar
debe
paginación  para  no
colapsar el navegador.

HU-23:  Reporte
final
de
ganadores.

Como  Operador,  quiero  cerrar  la
sesión  y emitir el listado definitivo
de posiciones.

Cambiar  el  estado  a
"Finalizada"  e
invalidar
cualquier  envío  posterior

Si el operador cierra la
sesión
accidentalmente,

el

UCAB - 2026

HU-24:  Registro
de
nueva
pregunta.

Como  Administrador, quiero crear
una  pregunta  de  trivia  con  sus
opciones  para  alimentar  el  banco
de conocimientos.

HU-25:
Visualización
filtrado
preguntas.

y
de

Como  Administrador,  quiero  listar
y  filtrar  las  preguntas  existentes
para
contenido
disponible.

revisar

el

HU-26:
Modificación
contenido.

de

Como Administrador, quiero editar
enunciados  o  corregir  opciones
de
respuestas  en  preguntas
existentes.

(RB-03).

que

Validar

1.
el
enunciado no esté vacío.
2. Debe permitir registrar
al  menos  3  opciones  de
respuesta.  3.  Se  debe
marcar  obligatoriamente
una  sola  como  correcta
(RB-28).

filtrado

1.  Permitir búsqueda por
texto  en el enunciado. 2.
Permitir
por
Categoría  o  Dificultad
(HU-25).  3.  La  consulta
la
debe
respuesta
correcta
marcada.

mostrar

1.  Al  guardar,  se  debe
actualizar  la  versión  de
la  pregunta  en  la  DB.  2.
Restricción: No se puede
editar  una  pregunta  si
pertenece  a  una  sesión
está
de
actualmente  en  estado
"Activa".

trivia

que

HU-27:
Eliminación
preguntas.

de

quiero
Administrador,
Como
remover preguntas del banco que
ya  no  sean  útiles  o  sean
erróneas.

Se  recomienda  aplicar
lógica
eliminación
(IsDeleted)
no
romper  el  historial  de
auditoría.

para

pide

sistema
una
doble
confirmación
para  evitar  pérdida  de
datos.

Si  el  administrador
intenta  guardar  sin
marcar  cuál  es
la
respuesta  correcta,  el
sistema  bloquea  el
comando  y  resalta  el
error.

Si  no  hay  preguntas
que  coincidan  con  los
filtros  aplicados,  se
muestra  un  mensaje
de "No se encontraron
resultados".

Si  la  pregunta  está en
uso  en  una  sesión
activa,  los  campos  de
aparecen
texto
bloqueados
(Read-only)  con  un
aviso informativo.

Si  el  administrador
intenta  eliminar  una
que
pregunta
pertenece
una
a
está
que
sesión
actualmente  en  curso
el
(Activa/Pausada),
la
sistema  bloquea
acción totalmente

HU-28:  Registro
de categoria.

Como  Administrador, quiero crear
para
categorías
organizar  el  banco  de  preguntas
de trivia.

temáticas

1.  El  nombre  de
la
ser
categoría
único.  2.  Debe  permitir
una descripción breve.

debe

Si el nombre ya existe,
el  sistema  impide  el
guardado  y  sugiere
usar uno distinto.

UCAB - 2026

HU-29:  Consulta
de categorias.

Como
quiero
Administrador,
visualizar  el  listado  de  categorías
para  gestionar  la  clasificación  de
trivia.

Debe  mostrar  el  conteo
de  preguntas  asociadas
a cada categoría.

HU-30:
Modificacion
categorias.

de

Como Administrador, quiero editar
el  nombre  o  descripción  de  una
categoría existente.

HU-31:
Eliminación
categorias.

de

Como
quiero
Administrador,
eliminar  categorías  que  ya  no
sean necesarias.

HU-32:  Creación
de
sesión
de
Trivia.

Como Operador, quiero crear una
trivia
sesión
seleccionando
de
preguntas.

categorías

exclusiva

de

cambio

El
debe
reflejarse  en  todas  las
preguntas  asociadas  de
forma automática.

Al
una
eliminar
categoría,  las  preguntas
asociadas  no  se  borran;
se  mueven
una
categoría  por  defecto
("Sin Categoría").

a

Debe  generar  un  código
de  acceso  único  para
que  los  equipos se unan
a la sala de espera.

Si  no  hay  categorías,
el  sistema  muestra
una  invitación  a  crear
la primera.

Si  se  intenta  duplicar
el  nombre  de  otra
categoría  al  editar,  el
la
sistema  bloquea
acción.

solicita

El
sistema
confirmación
indicando
cuántas
preguntas  se  verán
el
afectadas
cambio.

por

en

Si  no  hay  suficientes
las
preguntas
categorías
seleccionadas,
el
sistema advierte antes
de crear.

HU-33:  Control  de
sala de espera.

Como  Operador,  quiero  ver  en
tiempo  real  qué  equipos  se  han
conectado  antes  de
la
trivia.

iniciar

1.  Visualización  de  lista
vía
equipos
de
WebSockets.  2.  Permitir
expulsar  equipos  si  el
nombre es inapropiado.

falla,

la  conexión  del
Si
el
operador
sistema  debe  intentar
reconectar  sin  cerrar
la sala de espera.

HU-34:  Recepción
de
secuencia
automática.

Como  Equipo,  quiero  que
las
preguntas  aparecen  una  tras otra
externa,
sin
manteniendo  el
la
competencia.

intervención

ritmo  de

1. La interfaz muestra un
"Preparando
aviso  de
pregunta"
siguiente
durante  la  transición.  2.
Sincronización
vía
WebSockets.

Si  el  equipo  entra  con
la  secuencia  iniciada,
lo
sistema
el
sincroniza
la
pregunta  que  esté
corriendo
ese
instante.

con

en

HU-35:  Envío  de
respuesta
seleccionada.

Como  Equipo,  quiero  marcar  mi
y  que  se  envíe
respuesta
automáticamente
servidor
antes de que el tiempo expire.

al

1.  Bloqueo  de  opciones
al  confirmar  o  al  llegar a
0s.  2.  Publicación  de
respuesta en RabbitMQ.

Si  hay  fallo  de  red,  el
frontend  reintentar  el
envío
el
"Estado de Transición"
antes  de  la  siguiente
pregunta.

durante

UCAB - 2026

HU-36:
Procesamiento
asíncrono
respuestas.

de

Como  Sistema,  quiero  consumir
respuestas
los  mensajes  de
alojados  en  la  cola  de  RabbitMQ
para
la
respuesta correcta sin bloquear el
hilo principal de la aplicación.

validarlos

contra

El  suscriptor  de  la  cola
debe extraer el EquipoId,
PreguntaId  y  la  opción
seleccionada.
la
Debe
respuesta
el
modelo  de dominio en la
base de datos.

validar

contra

Si  el mensaje tiene un
inválido,  se
formato
mueve  a  una  Dead
Letter  Queue  (DLQ)
sin
auditoría
para
detener  el  motor  de
juego.

HU-37:
dinámico
puntuacion.

Cálculo
de

Como Sistema, quiero asignar los
puntos  correspondientes  a  cada
equipo basándome en la exactitud
de  la  respuesta  y  el  tiempo  de
envío.

HU-38:  Emision
de  ranking  parcial
y feedback.

Como  Sistema,  quiero  notificar  a
todos  los  equipos  los  resultados
ranking
de
ronda  y  el
actualizado  para  mantener
la
competitividad.

la

en

Se  otorgan  los  puntos
configurados
la
pregunta  si  la  respuesta
es correcta (RB-17).
Se registra el TimeStamp
de llegada para el criterio
por
desempate
de
velocidad.

al

Si un equipo envió una
respuesta  pero  esta
servidor
llegó
después  de  que  el
temporizador
cerró
(por  latencia  de  red),
el  sistema  le  asigna  0
puntos.

masivo
la

Envío
de
(Broadcast)
respuesta correcta.
Actualización
del
componente  de  Ranking
en  el  frontend  de  React
vía
SignalR/WebSockets.

Si  el
servicio  de
falla,  el
WebSockets
ranking  se  persiste  en
la  DB  para  que  el
recupere
cliente
mediante  una  Query
de
al
respaldo
refrescar.

lo

HU-39: Gestión de
Transición
y
lanzamiento
automático.

Como  Sistema,  quiero  gestionar
el  tiempo  de  "descanso"  entre
preguntas  y  disparar  la  siguiente
pregunta automáticamente.

(ej.

finalizar,

Iniciar  un  cronómetro  de
transición
10
segundos).
Al
automáticamente
comando
LaunchNextQuestion  si
quedan  preguntas  en  la
sesión.

enviar
el

Si se detecta que es la
última  pregunta  del
sistema
el
banco,
dispara  el  evento  de
finalización  de  sesión
en
de
lugar
transición.

del

HU-40:
tecnico
desempate.

Criterio
de

Como  Sistema, quiero registrar el
momento  preciso  en  que  cada
equipo  confirma  su
respuesta
para  utilizarlo  como  criterio  de
ranking,
desempate
en
premiando
los
participantes.

el
la  agilidad  de

El sistema debe capturar
la  marca  de
tiempo
(Server-side  Timestamp)
en  el  instante  exacto  en
que  el  mensaje  de
respuesta es recibido por
el backend.
En  caso  de  empate  en
puntos  acumulados,  el
ranking  debe  ordenar  a

Si  dos  o  más  equipos
logran  una  marca  de
tiempo  idéntica  en  el
servidor, el sistema les
asignará
la  misma
posición  en  el  ranking
parcial  hasta  que  una
pregunta
nueva
genere una diferencia.

UCAB - 2026

los  equipos  de
forma
ascendente  basándose
la  suma  de  sus
en
tiempos  de
respuesta
respondió  más
(quien
rápido  en  el  total  de  la
sesión).
La  precisión  del  registro
debe  ser  suficiente  para
minimizar  la probabilidad
de  empates
técnicos
absolutos.

Si  una respuesta llega
al servidor después de
que  el  proceso  de
validación  ha  cerrado
(por
nodo
el
problemas  de  red  del
usuario),  el  sistema
ignorará  el  registro  de
tiempo  y  marcará  la
respuesta
como
"Fuera  de  tiempo"  sin
sumar puntos.

UCAB - 2026

Modelo de Dominio

UCAB - 2026

