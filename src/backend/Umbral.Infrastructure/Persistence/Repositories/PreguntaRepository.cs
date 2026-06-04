using Microsoft.EntityFrameworkCore;
using Umbral.Domain.CatalogoTrivia.Categoria;
using Umbral.Domain.CatalogoTrivia.Pregunta;

namespace Umbral.Infrastructure.Persistence.Repositories;

public sealed class PreguntaRepository : IPreguntaRepository
{
    private readonly UmbralDbContext _db;

    public PreguntaRepository(UmbralDbContext db) => _db = db;

    public async Task<Pregunta?> FindByIdAsync(PreguntaId id, CancellationToken ct = default)
    {
        return await _db.Preguntas
            .FirstOrDefaultAsync(x => x.PreguntaId == id, ct);
    }

    public async Task<IReadOnlyList<Pregunta>> FindByIdsAsync(
        IReadOnlyList<PreguntaId> ids,
        CancellationToken ct = default)
    {
        if (ids.Count == 0)
            return Array.Empty<Pregunta>();

        var preguntas = new List<Pregunta>(ids.Count);
        foreach (var id in ids)
        {
            var pregunta = await FindByIdAsync(id, ct);
            if (pregunta is not null)
                preguntas.Add(pregunta);
        }

        return preguntas;
    }

    public async Task<IReadOnlyList<Pregunta>> FindAllAsync(CancellationToken ct = default)
    {
        return await _db.Preguntas
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Pregunta>> FindByCategoriaAsync(
        CategoriaId categoriaId,
        CancellationToken ct = default)
    {
        return await _db.Preguntas
            .AsNoTracking()
            .Where(x => x.CategoriaId == categoriaId)
            .ToListAsync(ct);
    }

    public async Task SaveAsync(Pregunta pregunta, CancellationToken ct = default)
    {
        if (_db.Entry(pregunta).State == EntityState.Detached)
        {
            var exists = await _db.Preguntas
                .AnyAsync(x => x.PreguntaId == pregunta.PreguntaId, ct);

            if (exists)
                _db.Preguntas.Update(pregunta);
            else
                await _db.Preguntas.AddAsync(pregunta, ct);
        }

        await _db.SaveChangesAsync(ct);
    }
}
