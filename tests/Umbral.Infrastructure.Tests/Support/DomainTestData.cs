using Umbral.Domain.CatalogoBusquedaTesoro.Mision;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;
using Umbral.Domain.Sesion;
using Umbral.Domain.Shared;

namespace Umbral.Infrastructure.Tests.Support;

internal static class DomainTestData
{
    public static MisionSnapshot MisionSnapshotActiva(string nombre = "Misión integración")
    {
        var mision = Mision.Crear(nombre);
        mision.AgregarEtapa("Busca el árbol rojo", "QR-ARBOL-001");
        mision.AgregarEtapa("Encuentra la fuente", "QR-FUENTE-002");
        mision.Activar();
        mision.ClearDomainEvents();
        return MisionSnapshot.Desde(mision);
    }

    public static Sesion SesionBusquedaTesoroActiva(string equipo = "Alpha")
    {
        var sesion = Sesion.CrearBusquedaTesoro(MisionSnapshotActiva(), UsuarioId.Nuevo());
        sesion.ClearDomainEvents();
        sesion.AbrirParaRegistro();
        sesion.UnirseEquipo(UsuarioId.Nuevo(), equipo, sesion.CodigoAcceso.Valor);
        sesion.Iniciar();
        sesion.ClearDomainEvents();
        return sesion;
    }

    public static Mision MisionConEtapasYPistas(string nombre = "Misión persistencia")
    {
        var mision = Mision.Crear(nombre);
        mision.AgregarEtapa("Etapa 1", "QR-001");
        mision.Etapas[0].AgregarPista("Pista por tiempo", TipoLiberacion.PorTiempo, 30);
        mision.AgregarEtapa("Etapa 2", "QR-002");
        mision.Activar();
        mision.ClearDomainEvents();
        return mision;
    }

    public static Categoria CategoriaPersistencia(string nombre = "Categoría persistencia de prueba")
    {
        var categoria = Categoria.Crear(nombre);
        categoria.ClearDomainEvents();
        return categoria;
    }

    public static Pregunta PreguntaPersistenciaSinCategoria(string enunciado = "Enunciado persistencia de prueba")
    {
        var pregunta = Pregunta.Crear(
            enunciado,
            Dificultad.Facil,
            OpcionesPersistencia());
        pregunta.ClearDomainEvents();
        return pregunta;
    }

    public static Pregunta PreguntaPersistenciaConCategoria(CategoriaId categoriaId)
    {
        var pregunta = Pregunta.Crear(
            "Enunciado con categoría persistencia de prueba",
            Dificultad.Media,
            OpcionesPersistencia(),
            categoriaId);
        pregunta.ClearDomainEvents();
        return pregunta;
    }

    private static IReadOnlyList<OpcionRespuesta> OpcionesPersistencia() =>
    [
        OpcionRespuesta.Crear("Opción correcta persistencia", true),
        OpcionRespuesta.Crear("Opción incorrecta 1 persistencia", false),
        OpcionRespuesta.Crear("Opción incorrecta 2 persistencia", false)
    ];
}
