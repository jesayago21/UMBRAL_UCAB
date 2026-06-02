using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbral.Domain.IdentidadYAccesos;
using Umbral.Domain.IdentidadYAccesos.Enums;
using Umbral.Infrastructure.Persistence.ValueConverters;

namespace Umbral.Infrastructure.Persistence.Configurations;

public sealed class UsuarioAdministrableConfiguration : IEntityTypeConfiguration<UsuarioAdministrable>
{
    public void Configure(EntityTypeBuilder<UsuarioAdministrable> builder)
    {
        builder.ToTable("usuarios_administrables");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Valor,
                g => new UsuarioAdministrableId(g));

        builder.Property(x => x.KeycloakUserId)
            .HasColumnName("keycloak_user_id")
            .HasConversion(new KeycloakUserIdValueConverter())
            .IsRequired();

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasConversion(new EmailAddressValueConverter())
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.Username)
            .HasColumnName("username")
            .HasMaxLength(120)
            .IsRequired();

        builder.HasIndex(x => x.Username).IsUnique();

        builder.Property(x => x.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.Apellido)
            .HasColumnName("apellido")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.Estado)
            .HasColumnName("estado")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property<List<RolSistema>>("_roles")
            .HasColumnName("roles_json")
            .HasConversion(
                roles => string.Join(',', roles.Select(r => r.ToString())),
                s => string.IsNullOrWhiteSpace(s)
                    ? new List<RolSistema>()
                    : s.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(Enum.Parse<RolSistema>)
                        .ToList())
            .HasField("_roles");

        builder.Ignore(x => x.Roles);
        builder.Ignore(x => x.DomainEvents);
    }
}
