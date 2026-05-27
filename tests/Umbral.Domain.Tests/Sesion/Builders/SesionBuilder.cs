using Umbral.Domain.CatalogoBusquedaTesoro.Mision;
using Umbral.Domain.Sesion;
using SesionAR = Umbral.Domain.Sesion.Sesion;

namespace Umbral.Domain.Tests.Sesion.Builders;

/// <summary>
/// Builder fluido para crear instancias de Sesion en estados arbitrarios
/// sin pasar por la maquina de estados real (solo para tests).
/// Patron: SesionBuilder.BusquedaTesoro()
///           .ConEstado(EstadoSesion.EnPreparacion)
///           .ConEquipo("Alpha")
///           .Build()
/// </summary>
public sealed class SesionBuilder
{
    private EstadoSesion _estado = EstadoSesion.Programada;
    private readonly List<string> _equipos = [];
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

    public SesionBuilder ConEquipo(string nombre)
    {
        _equipos.Add(nombre);
        return this;
    }

    /// <summary>
    /// Explícito: no pre-registrar equipos al construir (p. ej. Iniciar sin equipos).
    /// </summary>
    public SesionBuilder SinEquipos() => this;

    /// <summary>
    /// Atajo: construye una sesion con estado Activa.
    /// Si no se llama a ConEquipo(), Build() agrega un equipo por defecto.
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
                break;

            case EstadoSesion.EnPreparacion:
                sesion.AbrirParaRegistro();
                foreach (var nombre in _equipos)
                    sesion.RegistrarEquipo(nombre);
                break;

            case EstadoSesion.Activa:
                sesion.AbrirParaRegistro();
                foreach (var nombre in _equipos)
                    sesion.RegistrarEquipo(nombre);
                if (!sesion.Equipos.Any())
                    sesion.RegistrarEquipo("EquipoDefault");
                sesion.Iniciar();
                break;

            case EstadoSesion.Pausada:
                sesion.AbrirParaRegistro();
                if (_equipos.Count == 0)
                    sesion.RegistrarEquipo("EquipoDefault");
                foreach (var nombre in _equipos)
                    sesion.RegistrarEquipo(nombre);
                sesion.Iniciar();
                sesion.Pausar();
                break;

            case EstadoSesion.Finalizada:
                sesion.AbrirParaRegistro();
                if (_equipos.Count == 0)
                    sesion.RegistrarEquipo("EquipoDefault");
                foreach (var nombre in _equipos)
                    sesion.RegistrarEquipo(nombre);
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
        mision.AgregarEtapa("Busca el árbol rojo", "QR-ARBOL-001");
        mision.AgregarEtapa("Encuentra la fuente", "QR-FUENTE-002");
        mision.Activar();
        mision.ClearDomainEvents();
        return MisionSnapshot.Desde(mision);
    }
}
