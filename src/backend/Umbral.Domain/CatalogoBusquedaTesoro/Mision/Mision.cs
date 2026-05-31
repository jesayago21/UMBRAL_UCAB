using Umbral.Domain.CatalogoBusquedaTesoro.Mision.Events;
using Umbral.Domain.Shared;

namespace Umbral.Domain.CatalogoBusquedaTesoro.Mision;

/// <summary>
/// Aggregate Root del BC CatalogoBusquedaTesoro.
/// Implementa el patrón Composite: Mision → List&lt;Etapa&gt; → List&lt;Pista&gt;.
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

    public void AgregarEtapa(string descripcion, string codigoQRSolucion)
    {
        var orden = _etapas.Count + 1;
        var etapa = Etapa.Crear(MisionId, orden, descripcion, codigoQRSolucion);
        _etapas.Add(etapa);
    }

    public void Activar()
    {
        if (_etapas.Count == 0)
            throw new DomainException(
                "Una misión necesita al menos una etapa para activarse.");

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
        var etapa = _etapas.FirstOrDefault(e => e.EtapaId == etapaId)
            ?? throw new DomainException("La etapa indicada no pertenece a esta misión.");

        etapa.AgregarPista(contenido, tipoLiberacion, segundosLiberacion);
    }

    /// <summary>
    /// Indica si la misión puede usarse como base de una sesión BusquedaTesoro.
    /// </summary>
    public bool PuedeUsarseParaSesion() => Estado == EstadoMision.Activa;

    protected override bool IdEquals(Entity other) =>
        other is Mision m && m.MisionId == MisionId;

    protected override int GetIdHashCode() => MisionId.GetHashCode();
}
