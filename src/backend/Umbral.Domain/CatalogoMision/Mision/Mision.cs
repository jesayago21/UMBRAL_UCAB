using Umbral.Domain.CatalogoMision.Mision.Events;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.Shared;

namespace Umbral.Domain.CatalogoMision.Mision;

/// <summary>
/// Aggregate Root del BC CatalogoMision.
/// Mision → etapas polimórficas (BusquedaTesoro | Trivia).
/// </summary>
public sealed class Mision : AggregateRoot
{
    public MisionId MisionId { get; private set; } = default!;
    public string Nombre { get; private set; } = default!;
    public EstadoMision Estado { get; private set; }

    private readonly List<Etapa> _etapas = [];
    public IReadOnlyList<Etapa> Etapas => _etapas.AsReadOnly();

    private Mision() { }

    public static Mision Crear(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre de la misión no puede estar vacío.");

        var mision = new Mision
        {
            MisionId = MisionId.Nuevo(),
            Nombre   = nombre.Trim(),
            Estado   = EstadoMision.Borrador
        };
        mision.RaiseDomainEvent(new MisionCreada(mision.MisionId));
        return mision;
    }

    public void AgregarEtapaBusquedaTesoro(
        string descripcion,
        string codigoQRSolucion,
        double? latitud = null,
        double? longitud = null,
        int? radioMetros = null)
    {
        var orden = _etapas.Count + 1;
        _etapas.Add(EtapaBusquedaTesoro.Crear(
            MisionId, orden, descripcion, codigoQRSolucion, latitud, longitud, radioMetros));
    }

    public void AgregarEtapaTrivia(IReadOnlyList<CategoriaId> categoriaIds)
    {
        ValidarCategoriasTriviaSinSolapamiento(categoriaIds);
        var orden = _etapas.Count + 1;
        _etapas.Add(EtapaTrivia.Crear(MisionId, orden, categoriaIds));
    }

    public void Activar()
    {
        if (_etapas.Count == 0)
            throw new DomainException(
                "Una misión necesita al menos una etapa para activarse (RB-09).");

        if (Estado != EstadoMision.Borrador)
            throw new DomainException(
                $"No se puede activar una misión en estado '{Estado}'. " +
                "Solo es posible desde 'Borrador'.");

        Estado = EstadoMision.Activa;
        RaiseDomainEvent(new MisionActivada(MisionId));
    }

    public void Desactivar()
    {
        if (Estado != EstadoMision.Activa)
            throw new DomainException(
                $"No se puede desactivar una misión en estado '{Estado}'.");

        Estado = EstadoMision.Borrador;
    }

    public void Renombrar(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre de la misión no puede estar vacío.");

        Nombre = nombre.Trim();
    }

    public void AgregarPistaAEtapa(
        EtapaId etapaId,
        string contenido,
        TipoLiberacion tipoLiberacion,
        int? segundosLiberacion = null)
    {
        var etapa = _etapas.OfType<EtapaBusquedaTesoro>()
            .FirstOrDefault(e => e.EtapaId == etapaId)
            ?? throw new DomainException("La etapa indicada no es de Búsqueda del Tesoro o no pertenece a esta misión.");

        etapa.AgregarPista(contenido, tipoLiberacion, segundosLiberacion);
    }

    public void EditarPistaEtapa(
        EtapaId etapaId,
        PistaId pistaId,
        string contenido,
        TipoLiberacion tipoLiberacion,
        int? segundosLiberacion = null)
    {
        var etapa = _etapas.OfType<EtapaBusquedaTesoro>()
            .FirstOrDefault(e => e.EtapaId == etapaId)
            ?? throw new DomainException(
                "La etapa indicada no es de Búsqueda del Tesoro o no pertenece a esta misión.");

        etapa.EditarPista(pistaId, contenido, tipoLiberacion, segundosLiberacion);
    }

    public void EliminarPistaEtapa(EtapaId etapaId, PistaId pistaId)
    {
        var etapa = _etapas.OfType<EtapaBusquedaTesoro>()
            .FirstOrDefault(e => e.EtapaId == etapaId)
            ?? throw new DomainException(
                "La etapa indicada no es de Búsqueda del Tesoro o no pertenece a esta misión.");

        etapa.EliminarPista(pistaId);
    }

    public void EditarEtapaBusquedaTesoro(
        EtapaId etapaId,
        string descripcion,
        string codigoQRSolucion,
        double? latitud = null,
        double? longitud = null,
        int? radioMetros = null)
    {
        var etapa = _etapas.OfType<EtapaBusquedaTesoro>()
            .FirstOrDefault(e => e.EtapaId == etapaId)
            ?? throw new DomainException(
                "La etapa indicada no es de Búsqueda del Tesoro o no pertenece a esta misión.");

        etapa.Actualizar(descripcion, codigoQRSolucion, latitud, longitud, radioMetros);
    }

    public void EditarEtapaTrivia(EtapaId etapaId, IReadOnlyList<CategoriaId> categoriaIds)
    {
        var etapa = _etapas.OfType<EtapaTrivia>()
            .FirstOrDefault(e => e.EtapaId == etapaId)
            ?? throw new DomainException(
                "La etapa indicada no es de Trivia o no pertenece a esta misión.");

        ValidarCategoriasTriviaSinSolapamiento(categoriaIds, etapaId);
        etapa.ActualizarCategorias(categoriaIds);
    }

    public void EliminarEtapa(EtapaId etapaId)
    {
        var etapa = _etapas.FirstOrDefault(e => e.EtapaId == etapaId)
            ?? throw new DomainException("La etapa indicada no pertenece a esta misión.");

        if (Estado == EstadoMision.Activa && _etapas.Count <= 1)
            throw new DomainException(
                "Una misión activa necesita al menos una etapa (RB-09).");

        _etapas.Remove(etapa);
        ReordenarEtapas();
    }

    private void ReordenarEtapas()
    {
        for (var i = 0; i < _etapas.Count; i++)
            _etapas[i].AsignarOrden(i + 1);
    }

    /// <summary>
    /// Evita reutilizar categorías entre etapas trivia: RB-13 bloquea una pregunta por sesión,
    /// y preguntas del mismo banco se repetirían si la categoría ya se jugó en otra etapa.
    /// </summary>
    private void ValidarCategoriasTriviaSinSolapamiento(
        IReadOnlyList<CategoriaId> nuevasCategorias,
        EtapaId? etapaExcluida = null)
    {
        var usadas = _etapas
            .OfType<EtapaTrivia>()
            .Where(e => etapaExcluida is null || e.EtapaId != etapaExcluida)
            .SelectMany(e => e.CategoriaIds)
            .Select(c => c.Valor)
            .ToHashSet();

        if (nuevasCategorias.Any(c => usadas.Contains(c.Valor)))
            throw new DomainException(
                "Una categoría de trivia no puede repetirse en otra etapa de la misma misión. " +
                "Usa categorías distintas en cada etapa trivia para evitar preguntas bloqueadas.");
    }

    public bool PuedeUsarseParaSesion() => Estado == EstadoMision.Activa;

    protected override bool IdEquals(Entity other) =>
        other is Mision m && m.MisionId == MisionId;

    protected override int GetIdHashCode() => MisionId.GetHashCode();
}
