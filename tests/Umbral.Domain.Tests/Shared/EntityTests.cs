using FluentAssertions;
using Umbral.Domain.CatalogoBusquedaTesoro.Mision;
using Xunit;

namespace Umbral.Domain.Tests.Shared;

/// <summary>
/// Tests de la clase base <c>Entity</c> (igualdad por identidad).
/// Usa <see cref="Mision"/> (AggregateRoot : Entity) y su entidad hija <c>Etapa</c>.
/// </summary>
public sealed class EntityTests
{
    private static Mision MisionConEtapa()
    {
        var mision = Mision.Crear("Misión entidad");
        mision.AgregarEtapa("Etapa 1", "QR-ENT-001");
        return mision;
    }

    [Fact]
    public void Equals_CuandoMismaReferencia_DevuelveTrue()
    {
        var mision = MisionConEtapa();

        mision.Equals(mision).Should().BeTrue();
        mision.GetHashCode().Should().Be(mision.GetHashCode());
    }

    [Fact]
    public void Equals_CuandoDistintaIdentidad_DevuelveFalse()
    {
        var a = Mision.Crear("Misión A");
        var b = Mision.Crear("Misión B");

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_CuandoNoEsEntity_DevuelveFalse()
    {
        var mision = Mision.Crear("Misión X");

        mision.Equals("texto").Should().BeFalse();
        mision.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_CuandoTiposDistintos_DevuelveFalse()
    {
        var mision = MisionConEtapa();
        var etapa = mision.Etapas[0];

        mision.Equals(etapa).Should().BeFalse();
        etapa.Equals(mision).Should().BeFalse();
    }
}
