using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbral.Infrastructure.Persistence.Entities;

namespace Umbral.Infrastructure.Persistence.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Rol)
            .HasColumnName("rol")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Activo)
            .HasColumnName("activo")
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique();
    }
}
