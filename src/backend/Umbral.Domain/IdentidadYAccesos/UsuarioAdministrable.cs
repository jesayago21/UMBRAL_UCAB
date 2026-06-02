using Umbral.Domain.IdentidadYAccesos.Enums;
using Umbral.Domain.IdentidadYAccesos.Events;
using Umbral.Domain.IdentidadYAccesos.ValueObjects;
using Umbral.Domain.Shared;

namespace Umbral.Domain.IdentidadYAccesos;

public sealed class UsuarioAdministrable : AggregateRoot
{
    public UsuarioAdministrableId Id { get; private set; } = default!;
    public KeycloakUserId KeycloakUserId { get; private set; } = default!;
    public EmailAddress Email { get; private set; } = default!;
    public string Username { get; private set; } = default!;
    public string Nombre { get; private set; } = default!;
    public string Apellido { get; private set; } = default!;
    public EstadoUsuario Estado { get; private set; }

    private readonly List<RolSistema> _roles = [];
    public IReadOnlyList<RolSistema> Roles => _roles.AsReadOnly();

    private UsuarioAdministrable() { }

    public static UsuarioAdministrable Crear(
        KeycloakUserId keycloakId,
        EmailAddress email,
        string username,
        string nombre,
        string apellido,
        IEnumerable<RolSistema> rolesIniciales)
    {
        ArgumentNullException.ThrowIfNull(keycloakId);
        ArgumentNullException.ThrowIfNull(email);

        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("El username no puede estar vacío.");

        var roles = rolesIniciales.ToList();
        ValidarRolesAsignables(roles);

        var usuario = new UsuarioAdministrable
        {
            Id             = UsuarioAdministrableId.Nuevo(),
            KeycloakUserId = keycloakId,
            Email          = email,
            Username       = username.Trim(),
            Nombre         = nombre.Trim(),
            Apellido       = apellido.Trim(),
            Estado         = EstadoUsuario.Activo
        };
        usuario._roles.AddRange(roles.Distinct());
        usuario.RaiseDomainEvent(new UsuarioCreadoEnDominio(usuario.Id, keycloakId, email));
        return usuario;
    }

    public void AsignarRoles(IEnumerable<RolSistema> roles)
    {
        var lista = roles.ToList();
        ValidarRolesAsignables(lista);
        _roles.Clear();
        _roles.AddRange(lista.Distinct());
        RaiseDomainEvent(new RolesUsuarioModificados(Id, _roles.AsReadOnly()));
    }

    public void RevocarRol(RolSistema rol)
    {
        _roles.Remove(rol);
        RaiseDomainEvent(new RolesUsuarioModificados(Id, _roles.AsReadOnly()));
    }

    public void Bloquear()
    {
        if (Estado == EstadoUsuario.Bloqueado)
            return;

        Estado = EstadoUsuario.Bloqueado;
        RaiseDomainEvent(new UsuarioEstadoCambiado(Id, Estado));
    }

    public void Activar()
    {
        if (Estado == EstadoUsuario.Activo)
            return;

        Estado = EstadoUsuario.Activo;
        RaiseDomainEvent(new UsuarioEstadoCambiado(Id, Estado));
    }

    private static void ValidarRolesAsignables(IReadOnlyList<RolSistema> roles)
    {
        if (roles.Contains(RolSistema.Participante))
            throw new DomainException(
                "El rol Participante no se asigna desde administración (RB-35).");

        if (roles.Count == 0)
            throw new DomainException("Debe asignarse al menos un rol Administrador u Operador.");
    }

    protected override bool IdEquals(Entity other) =>
        other is UsuarioAdministrable u && u.Id == Id;

    protected override int GetIdHashCode() => Id.Valor.GetHashCode();
}
