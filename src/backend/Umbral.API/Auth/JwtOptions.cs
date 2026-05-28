namespace Umbral.API.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "Umbral.API";
    public string Audience { get; init; } = "Umbral.Client";
    public string Key { get; init; } = "THIS_IS_ONLY_FOR_LOCAL_DEV_CHANGE_ME_1234567890";
    public int ExpiresMinutes { get; init; } = 60;
}
