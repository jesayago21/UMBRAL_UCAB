namespace Umbral.API.Contracts.Sesiones;

/// <summary>RF-15 — pista ad-hoc del operador. <paramref name="ParticipanteId"/> null = todos.</summary>
public sealed record LiberarPistaManualRequest(
    string Contenido,
    Guid? ParticipanteId = null);
