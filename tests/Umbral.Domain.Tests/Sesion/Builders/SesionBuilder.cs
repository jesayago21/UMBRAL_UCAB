using Umbral.Domain.CatalogoMision.Mision;
using Umbral.Domain.Sesion;
using Umbral.Domain.Tests.Sesion;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Domain.Tests.Sesion.Builders;

/// <summary>
/// Builder fluido para crear instancias de Sesion en estados arbitrarios
/// sin pasar por la maquina de estados real (solo para tests).
/// Patron: SesionBuilder.BusquedaTesoro()
///           .ConEstado(EstadoSesion.EnPreparacion)
///           .ConParticipante("Alpha")
///           .Build()
/// </summary>
public sealed class SesionBuilder
{
    private EstadoSesion _estado = EstadoSesion.Programada;
    private readonly List<string> _participantes = [];
    private MisionSnapshot? _snapshot;
    private UsuarioId? _operadorId;

    private SesionBuilder() { }

    public static SesionBuilder BusquedaTesoro()
    {
        var b = new SesionBuilder();
        b._snapshot = MisionSnapshotFake();
        return b;
    }

    public SesionBuilder ConEstado(EstadoSesion estado)
    {
        _estado = estado;
        return this;
    }

    public SesionBuilder ConOperador(UsuarioId operadorId)
    {
        _operadorId = operadorId;
        return this;
    }

    public SesionBuilder ConParticipante(string nombre)
    {
        _participantes.Add(nombre);
        return this;
    }

    /// <summary>
    /// Explícito: no pre-registrar participantes al construir (p. ej. Iniciar sin participantes).
    /// </summary>
    public SesionBuilder SinParticipantes() => this;

    /// <summary>
    /// Atajo: construye una sesion con estado Activa.
    /// Si no se llama a ConParticipante(), Build() agrega un participante por defecto.
    /// </summary>
    public SesionBuilder Activa()
    {
        _estado = EstadoSesion.Activa;
        return this;
    }

    public SesionAR Build()
    {
        var operadorId = _operadorId ?? UsuarioId.Nuevo();
        var snapshot   = _snapshot ?? MisionSnapshotFake();

        var sesion = SesionAR.CrearBusquedaTesoro(snapshot, operadorId);
        sesion.ClearDomainEvents();

        // Forzar estado usando la maquina real hasta el punto requerido,
        // o para estados avanzados se usan los metodos publicos.
        switch (_estado)
        {
            case EstadoSesion.Programada:
                foreach (var nombre in _participantes)
                    SesionTestHelpers.UnirParticipante(sesion, nombre);
                break;

            case EstadoSesion.EnPreparacion:
                sesion.AbrirParaRegistro();
                foreach (var nombre in _participantes)
                    SesionTestHelpers.UnirParticipante(sesion, nombre);
                break;

            case EstadoSesion.Activa:
                sesion.AbrirParaRegistro();
                foreach (var nombre in _participantes)
                    SesionTestHelpers.UnirParticipante(sesion, nombre);
                if (!sesion.Participantes.Any())
                    SesionTestHelpers.UnirParticipante(sesion, "Alpha");
                sesion.Iniciar();
                break;

            case EstadoSesion.Pausada:
                sesion.AbrirParaRegistro();
                if (_participantes.Count == 0)
                    SesionTestHelpers.UnirParticipante(sesion, "Alpha");
                foreach (var nombre in _participantes)
                    SesionTestHelpers.UnirParticipante(sesion, nombre);
                sesion.Iniciar();
                sesion.Pausar();
                break;

            case EstadoSesion.Finalizada:
                sesion.AbrirParaRegistro();
                if (_participantes.Count == 0)
                    SesionTestHelpers.UnirParticipante(sesion, "Alpha");
                foreach (var nombre in _participantes)
                    SesionTestHelpers.UnirParticipante(sesion, nombre);
                sesion.Iniciar();
                sesion.Finalizar();
                break;

            case EstadoSesion.Cancelada:
                sesion.Cancelar("Cancelado por builder de test");
                break;
        }

        sesion.ClearDomainEvents();
        return sesion;
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    public static MisionSnapshot MisionSnapshotFake(string nombre = "Misión Test")
    {
        var mision = Mision.Crear(nombre);
        mision.AgregarEtapaBusquedaTesoro("Busca el árbol rojo", "QR-ARBOL-001");
        mision.AgregarEtapaBusquedaTesoro("Encuentra la fuente", "QR-FUENTE-002");
        mision.Activar();
        mision.ClearDomainEvents();
        return MisionSnapshot.DesdeSoloBusquedaTesoro(mision);
    }
}
