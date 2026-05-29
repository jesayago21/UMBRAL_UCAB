using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta.Events;
using Umbral.Domain.Shared;

namespace Umbral.Domain.CatalogoTrivia.Pregunta;

/// <summary>
/// Aggregate Root del BC CatalogoTrivia — pregunta del banco (HU-24..27).
/// Invariantes: enunciado no vacío, ≥3 opciones, exactamente una correcta (RB-28).
/// </summary>
public sealed class Pregunta : AggregateRoot
{
    private const int MinOpciones = 3;

    public PreguntaId PreguntaId { get; private set; } = default!;
    public string Enunciado { get; private set; } = default!;
    public Dificultad Dificultad { get; private set; }
    public CategoriaId? CategoriaId { get; private set; }
    public bool Eliminada { get; private set; }

    private readonly List<OpcionRespuesta> _opciones = [];
    public IReadOnlyList<OpcionRespuesta> Opciones => _opciones.AsReadOnly();

    private Pregunta() { }

    public static Pregunta Crear(
        string enunciado,
        Dificultad dificultad,
        IEnumerable<OpcionRespuesta> opciones,
        CategoriaId? categoriaId = null)
    {
        var listaOpciones = ValidarContenido(enunciado, opciones);

        var pregunta = new Pregunta
        {
            PreguntaId  = PreguntaId.Nuevo(),
            Enunciado   = enunciado.Trim(),
            Dificultad  = dificultad,
            CategoriaId = categoriaId,
            Eliminada   = false
        };
        pregunta._opciones.AddRange(listaOpciones);
        pregunta.RaiseDomainEvent(new PreguntaCreada(pregunta.PreguntaId));
        return pregunta;
    }

    public void ModificarContenido(
        string enunciado,
        Dificultad dificultad,
        IEnumerable<OpcionRespuesta> opciones)
    {
        var listaOpciones = ValidarContenido(enunciado, opciones);

        Enunciado  = enunciado.Trim();
        Dificultad = dificultad;
        _opciones.Clear();
        _opciones.AddRange(listaOpciones);
    }

    public void AsignarCategoria(CategoriaId categoriaId) =>
        CategoriaId = categoriaId ?? throw new DomainException(
            "La categoría asignada no puede ser nula.");

    public void QuitarCategoria() => CategoriaId = null;

    public void Eliminar() => Eliminada = true;

    private static List<OpcionRespuesta> ValidarContenido(
        string enunciado,
        IEnumerable<OpcionRespuesta> opciones)
    {
        if (string.IsNullOrWhiteSpace(enunciado))
            throw new DomainException("El enunciado de la pregunta no puede estar vacío.");

        var lista = opciones?.ToList() ?? [];

        if (lista.Count < MinOpciones)
            throw new DomainException(
                $"La pregunta debe tener al menos {MinOpciones} opciones de respuesta.");

        if (lista.Count(o => o.EsCorrecta) != 1)
            throw new DomainException(
                "La pregunta debe tener exactamente una opción correcta.");

        return lista;
    }

    protected override bool IdEquals(Entity other) =>
        other is Pregunta p && p.PreguntaId == PreguntaId;

    protected override int GetIdHashCode() => PreguntaId.GetHashCode();
}
