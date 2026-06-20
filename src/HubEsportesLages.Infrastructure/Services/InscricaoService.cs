using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Application.Interfaces;
using HubEsportesLages.Application.Mapping;
using HubEsportesLages.Domain.Entities;
using HubEsportesLages.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HubEsportesLages.Infrastructure.Services;

public class InscricaoService(HubDbContext db) : IInscricaoService
{
    public async Task<InscricaoDto> InscreverAsync(CriarInscricaoDto dto, CancellationToken ct = default)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        // Evita duplicar a mesma combinação e-mail + modalidade + equipe: reaproveita o registro.
        var inscricao = await db.Inscricoes.FirstOrDefaultAsync(i =>
            i.Email == email &&
            i.ModalidadeId == dto.ModalidadeId &&
            i.EquipeId == dto.EquipeId, ct);

        if (inscricao is null)
        {
            inscricao = new Inscricao
            {
                Email = email,
                ModalidadeId = dto.ModalidadeId,
                EquipeId = dto.EquipeId,
                CriadoEm = DateTime.Now
            };
            db.Inscricoes.Add(inscricao);
        }

        inscricao.Nome = dto.Nome.Trim();
        inscricao.Telefone = string.IsNullOrWhiteSpace(dto.Telefone) ? null : dto.Telefone.Trim();
        inscricao.ReceberEmail = dto.ReceberEmail;
        inscricao.ReceberPush = dto.ReceberPush;
        inscricao.Ativa = true;

        await db.SaveChangesAsync(ct);

        // recarrega navegações para o DTO de retorno
        if (inscricao.ModalidadeId is not null)
            inscricao.Modalidade = await db.Modalidades.AsNoTracking().FirstOrDefaultAsync(m => m.Id == inscricao.ModalidadeId, ct);
        if (inscricao.EquipeId is not null)
            inscricao.Equipe = await db.Equipes.AsNoTracking().FirstOrDefaultAsync(e => e.Id == inscricao.EquipeId, ct);

        return inscricao.ParaDto();
    }

    public Task<int> ContarAtivasAsync(CancellationToken ct = default) =>
        db.Inscricoes.CountAsync(i => i.Ativa, ct);
}
