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
            await _db.Preguntas.AddAsync(pregunta, ct);

        await _db.SaveChangesAsync(ct);
    }
}
