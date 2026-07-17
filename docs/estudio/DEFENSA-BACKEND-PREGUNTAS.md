# Estudio backend UMBRAL — Defensa (preguntas y respuestas)

Guía de estudio oral + ejercicios mínimos (validaciones, flujo HTTP → dominio).  
Basada en el código real del monolito hexagonal.

**Más allá de evidencia (Domain + Application general):**  
[`DEFENSA-DOMINIO-APPLICATION.md`](DEFENSA-DOMINIO-APPLICATION.md)

**Archivos clave para abrir mientras estudias:**
- `SubmitEvidenciaValidator.cs`
- `SubmitEvidenciaCommand.cs` / `SubmitEvidenciaCommandHandler.cs`
- `ValidationBehavior.cs`
- `ExceptionHandlingMiddleware.cs`
- `GetSesionOperadorQuery.cs` / `GetSesionOperadorQueryHandler.cs`
- `ValidacionEvidenciaService.cs`
- `DomainException.cs` (`Umbral.Domain/Shared`)

---

## 1. Mapa mental rápido

```
UI (web/mobile)
  → HTTP Request JSON
  → Controller (API) arma Command/Query
  → MediatR (_sender.Send)
  → ValidationBehavior + FluentValidation  (si falla → ValidationException → 400)
  → Handler (Application)
      → repositorio carga agregado
      → métodos de dominio
      → Save / Result / DTO
  → Controller traduce a HTTP Response
```

| Objeto | Capa | Rol |
|--------|------|-----|
| Request / Response | API | Hablar HTTP |
| Command / Query | Application | Mensaje del caso de uso |
| Validator | Application | Validar entrada del Command |
| Handler | Application | Orquestar (no reglas de juego) |
| Agregado / Domain Service | Domain | Reglas de negocio |
| DTO (`*Dto`) | Application | Salida de lectura sin exponer dominio |

---

## 2. Validator vs Dominio

| Pedido del profesor | Dónde |
|---------------------|--------|
| vacío / largo / formato del request | **Validator** (FluentValidation) |
| sesión activa / QR correcto / invariantes del juego | **Dominio** (`DomainException` o resultado de validación) |

**Frase de defensa:**  
> “Formato y obligatoriedad del request → FluentValidation. Reglas del juego y estado del agregado → Dominio.”

**Importante:** `ValidationException` **no** está en Dominio. Es de FluentValidation, lanzada por `ValidationBehavior`.  
En Dominio se usa `DomainException`.

Ambas las mapea el middleware de API a **400**, con tipos distintos (`ValidationError` vs `DomainError`).

---

## 3. Cómo sale el HTTP 400 (FluentValidation)

1. `ValidationBehavior` corre **antes** del handler.
2. Si hay errores → `throw new ValidationException(failures)` y **no** llama a `next()` (no entra al handler).
3. `ExceptionHandlingMiddleware` atrapa esa excepción y responde **400 Bad Request**.

**Frase:**  
> “FluentValidation corre en el pipeline de MediatR. Si falla, lanza ValidationException y el middleware de la API la mapea a 400. El Validator no escribe return BadRequest().”

---

## 4. Command / Query / Handler

- **Command:** mensaje para **cambiar** estado (datos de entrada + tipo de respuesta, p. ej. `Result<T>`).
- **Query:** mensaje para **leer** (devuelve DTO).
- **Handler:** recibe el mensaje, hace el trabajo (repo + dominio si aplica + respuesta).

**Frase pulida:**  
> “El Command/Query transporta los datos de entrada del caso de uso. El Handler orquesta: carga el agregado, llama su comportamiento, guarda y devuelve el resultado.”

El controller **no** instancia el Handler por nombre: manda el Command/Query con MediatR (`_sender.Send`).

---

## 5. DTO

Los DTO (y Request/Response/Command) evitan **exponer entidades de dominio** hacia afuera.

- El handler de lectura mapea `Sesion` → `SesionDetalleDto`.
- El controller traduce DTO → Response HTTP.
- El handler **no** le pasa un DTO al dominio: usa Guid/string → Value Objects → métodos del agregado.

---

## 6. Snapshot

La misión del **catálogo** es la plantilla editable.  
El **snapshot** es la **copia congelada** al crear la sesión. La partida juega contra esa copia; editar el catálogo no altera la sesión en curso.

---

## 7. Preguntas tipo test (con respuesta)

### Pregunta 1
“En SubmitEvidencia, el código QR no puede ir vacío.” ¿Dónde?

- A) Controller  
- B) `SubmitEvidenciaValidator`  
- C) `Sesion.cs`  
- D) Solo el record del Command  

**Respuesta: B**

---

### Pregunta 2
¿Qué escribes aproximadamente?

- A) `if` en el controller  
- B) `RuleFor(x => x.CodigoQr).NotEmpty()`  
- C) Un handler nuevo  
- D) Un DTO nuevo  

**Respuesta: B** (completo: `RuleFor` + `.NotEmpty()` + `.WithMessage(...)`)

---

### Pregunta 3
Si falla esa validación, ¿llega al handler?

**Respuesta: No**

---

### Pregunta 4
“Mínimo 3 caracteres en el QR.” ¿Qué agregas?

- A) `.MaxLength(3)`  
- B) `.MinimumLength(3)`  
- C) `if` en el handler  
- D) Nada, va en el controller  

**Respuesta: B**

---

### Pregunta 5
“No se puede validar evidencia si la sesión no está Activa.” ¿Dónde?

- A) Validator  
- B) Dominio (`ValidacionEvidenciaService` / `Sesion`)  
- C) Controller  
- D) Query  

**Respuesta: B**  
*(Porque depende del estado del agregado, no del formato del request. Ya existe en `ValidacionEvidenciaService`.)*

---

### Pregunta 6
¿Para qué sirve el Command y el Handler?

**Respuesta modelo:**  
El Command es el mensaje con los datos del caso de uso. El Handler lo recibe, carga el agregado, llama su comportamiento, guarda y devuelve el resultado.

---

### Pregunta 7
¿Quién llama a `sesion.RegistrarEvidencia(...)`?

- A) Controller  
- B) Validator  
- C) Handler  
- D) Query  

**Respuesta: C**  
*(El repositorio carga/guarda; el método de negocio está en el agregado.)*

---

### Pregunta 8
`GetSesionOperador` es:

- A) Command  
- B) Query  

**Respuesta: B** (solo lee)

---

### Pregunta 9
“Agrega MaximumLength(100) al QR” en el flujo de **enviar evidencia**. ¿Dónde?

**Respuesta: Validator** (`SubmitEvidenciaValidator`).  
Es límite del request.  
*(Sería dominio si la regla fuera del catálogo: QR solución de la etapa al crear/editar misión.)*

---

### Pregunta 10
¿Obligatorio + máximo 100 van en el mismo `RuleFor`?

**Respuesta: Sí**

```csharp
RuleFor(x => x.CodigoQr)
    .NotEmpty()
    .MaximumLength(100);
```

---

### Pregunta 11
Si el Validator falla, status HTTP típico:

- A) 200  
- B) 201  
- C) 400  
- D) 500  

**Respuesta: C**

---

### Pregunta 12
¿Qué hace `GetSesionOperadorQueryHandler` después de encontrar la sesión?

**Respuesta modelo:**  
La convierte a DTO (`ToDetalle()`) y lo **devuelve**.  
El controller es quien lo pone en HTTP (`Ok(...)`) para el UI.  
El handler no piensa en HTTP.

---

### Pregunta 13
Orden del flujo al enviar evidencia:

- A) Handler → Validator → Controller → Dominio  
- B) Controller → Validator → Handler → Dominio  
- C) Dominio → Handler → Controller → Validator  

**Respuesta: B**  
*(Controller arma Command; ValidationBehavior; Handler; Dominio.)*

---

### Pregunta 14
¿El controller conoce `SubmitEvidenciaCommandHandler` por nombre?

- A) Sí, lo instancia  
- B) No; manda el Command por MediatR  

**Respuesta: B**  
*(Se pasa un Command, no “un DTO de Application” en el sentido de `*Dto` de lectura.)*

---

### Pregunta 15
¿Qué es un Snapshot en UMBRAL?

**Respuesta modelo:**  
Copia congelada de la misión al crear la sesión. No es la plantilla del catálogo; es la foto con la que se juega esa partida.

---

### Pregunta 16
Si falla `.NotEmpty()` del QR, ¿entra al handler?

**Respuesta: No**

---

### Pregunta 17
¿Quién “devuelve” el 400?

**Respuesta:** La **API** vía `ExceptionHandlingMiddleware`, después de que `ValidationBehavior` lanzó `ValidationException`. No el Handler.

---

### Pregunta 18
Completa: “El controller es delgado porque ___.”

**Respuesta modelo:**  
…solo traduce HTTP ↔ Command/Query (y Result/DTO ↔ Response), sin lógica de negocio.

---

## 8. Ejercicios prácticos (código mínimo)

### Ejercicio A — Validación mínima
**Pedido:** QR con al menos 3 caracteres.

**Archivo:** `SubmitEvidenciaValidator.cs`

```csharp
RuleFor(x => x.CodigoQr)
    .NotEmpty()
    .WithMessage("El código QR es obligatorio.")
    .MinimumLength(3)
    .WithMessage("El código QR debe tener al menos 3 caracteres.");
```

---

### Ejercicio B — Máximo 100
```csharp
RuleFor(x => x.CodigoQr)
    .NotEmpty()
    .MaximumLength(100);
```

---

### Ejercicio C — Oral: flujo GetSesionOperador
1. UI: GET detalle sesión  
2. `ConsultaSesionesOperadorController` → `GetSesionOperadorQuery`  
3. Handler: `FindById` → `ToDetalle()` → `SesionDetalleDto`  
4. Controller: mapea a Response → 200 JSON  

---

### Ejercicio D — Esbozo Query + Handler (en papel)
Pedido: devolver solo el estado (`string`) de la sesión.

```csharp
public sealed record GetEstadoSesionQuery(Guid SesionId) : IRequest<string>;

internal sealed class GetEstadoSesionQueryHandler
    : IRequestHandler<GetEstadoSesionQuery, string>
{
    private readonly ISesionRepository _sesionRepository;
    public GetEstadoSesionQueryHandler(ISesionRepository repo) => _sesionRepository = repo;

    public async Task<string> Handle(GetEstadoSesionQuery query, CancellationToken ct)
    {
        var sesion = await _sesionRepository.FindByIdAsync(new SesionId(query.SesionId), ct)
            ?? throw new NotFoundException(nameof(Sesion), query.SesionId);
        return sesion.Estado.ToString();
    }
}
```

---

## 9. Frases listas para defensa (30–40 s)

**Flujo evidencia:**  
> “El front hace POST. El controller crea SubmitEvidenciaCommand y lo envía con MediatR. FluentValidation corre en el pipeline; si falla, ValidationException y el middleware responde 400. Si pasa, el handler carga Sesion, llama RegistrarEvidencia, guarda y devuelve Result. La API lo convierte a HTTP.”

**Por qué no lógica en el controller:**  
> “El controller es adaptador de entrada. Las reglas viven en el dominio; el handler solo orquesta. Así es testeable sin HTTP.”

**DTO:**  
> “No devolvemos el agregado Sesion. Devolvemos un DTO de lectura para no acoplar la API al dominio.”

**Snapshot:**  
> “Al crear la sesión congelamos la misión. La partida no depende de ediciones posteriores del catálogo.”

---

## 10. Cheatsheet de una página

1. Validación de campos del request → `*Validator` + `RuleFor`  
2. Regla de negocio → Dominio / `DomainException`  
3. Command/Query = mensaje; Handler = trabajo  
4. Validator falla → no handler → 400 (`ValidationBehavior` + middleware)  
5. Controller delgado → MediatR  
6. Queries → DTO; Commands → dominio + `Result`  
7. Snapshot = foto congelada de la misión en la sesión  

---

## 11. Mini simulacro (sin mirar respuestas)

1. ¿Command o Query para listar misiones?  
2. ¿Dónde validas “nombre obligatorio”?  
3. ¿Quién llama a `RegistrarEvidencia`?  
4. ¿Qué es MediatR en una frase?  
5. ¿`ValidationException` está en Domain?  

**Claves:** 1 Query · 2 Validator · 3 Handler · 4 Bus de mensajes Command/Query→Handler · 5 No (FluentValidation / Application)
