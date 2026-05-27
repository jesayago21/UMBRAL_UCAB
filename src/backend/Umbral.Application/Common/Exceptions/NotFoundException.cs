namespace Umbral.Application.Common.Exceptions;

public sealed class NotFoundException : Exception
{
    public string EntityName { get; }
    public object Key { get; }

    public NotFoundException(string entityName, object key)
        : base($"Entidad '{entityName}' ({key}) no fue encontrada.")
    {
        EntityName = entityName;
        Key        = key;
    }
}
