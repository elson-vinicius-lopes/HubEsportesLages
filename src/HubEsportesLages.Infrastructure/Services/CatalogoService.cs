using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Application.Interfaces;
using HubEsportesLages.Application.Mapping;
using HubEsportesLages.Domain.Enums;
using HubEsportesLages.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HubEsportesLages.Infrastructure.Services;

public class CatalogoService(HubDbContext db) : ICatalogoService
{
    private static readonly StatusEvento[] StatusAgenda =
        [StatusEvento.Agendado, StatusEvento.AoVivo, StatusEvento.Adiado];

    public async Task<IReadOnlyList<ModalidadeDto>> ListarModalidadesAsync(CancellationToken ct = default)
    {
        var hoje = DateTime.Today;
        return await db.Modalidades
            .AsNoTracking()
            .OrderBy(m => m.Nome)
            .Select(m => new ModalidadeDto(
                m.Id, m.Nome, m.Slug, m.Icone, m.CorHex, m.Descricao,
                m.Eventos.Count(e => StatusAgenda.Contains(e.Status) && e.Inicio >= hoje)))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<LocalDto>> ListarLocaisAsync(CancellationToken ct = default)
    {
        var locais = await db.Locais
            .AsNoTracking()
            .OrderBy(l => l.Nome)
            .ToListAsync(ct);
        return locais.Select(l => l.ParaDto()).ToList();
    }

    public async Task<IReadOnlyList<EquipeDto>> ListarEquipesAsync(int? modalidadeId = null, CancellationToken ct = default)
    {
        var query = db.Equipes
            .Include(e => e.Modalidade)
            .AsNoTracking()
            .AsQueryable();

        if (modalidadeId is int id)
            query = query.Where(e => e.ModalidadeId == id);

        var equipes = await query.OrderBy(e => e.Nome).ToListAsync(ct);
        return equipes.Select(e => e.ParaDto()).ToList();
    }
}
