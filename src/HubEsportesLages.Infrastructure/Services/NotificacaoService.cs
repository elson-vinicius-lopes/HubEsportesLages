using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Application.Interfaces;
using HubEsportesLages.Application.Mapping;
using HubEsportesLages.Domain.Entities;
using HubEsportesLages.Domain.Enums;
using HubEsportesLages.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HubEsportesLages.Infrastructure.Services;

public class NotificacaoService(HubDbContext db) : INotificacaoService
{
    public async Task<IReadOnlyList<NotificacaoDto>> ListarRecentesAsync(int quantidade = 20, CancellationToken ct = default)
    {
        var itens = await db.Notificacoes
            .Include(n => n.Evento)
            .Include(n => n.Modalidade)
            .AsNoTracking()
            .OrderByDescending(n => n.CriadoEm)
            .Take(quantidade)
            .ToListAsync(ct);

        return itens.Select(n => n.ParaDto()).ToList();
    }

    public Task<int> ContarNaoLidasAsync(CancellationToken ct = default) =>
        db.Notificacoes.CountAsync(n => !n.Lida, ct);

    public async Task<NotificacaoDto> PublicarAsync(
        string titulo, string mensagem, TipoNotificacao tipo,
        int? eventoId = null, int? modalidadeId = null, bool importante = false,
        CancellationToken ct = default)
    {
        var notificacao = new Notificacao
        {
            Titulo = titulo,
            Mensagem = mensagem,
            Tipo = tipo,
            EventoId = eventoId,
            ModalidadeId = modalidadeId,
            Importante = importante,
            CriadoEm = DateTime.Now
        };

        db.Notificacoes.Add(notificacao);
        await db.SaveChangesAsync(ct);

        // recarrega navegações para devolver um DTO completo
        if (eventoId is not null)
            notificacao.Evento = await db.Eventos.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventoId, ct);
        if (modalidadeId is not null)
            notificacao.Modalidade = await db.Modalidades.AsNoTracking().FirstOrDefaultAsync(m => m.Id == modalidadeId, ct);

        return notificacao.ParaDto();
    }

    public async Task MarcarTodasComoLidasAsync(CancellationToken ct = default)
    {
        await db.Notificacoes
            .Where(n => !n.Lida)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.Lida, true), ct);
    }

    public async Task<int> GerarLembretesAsync(CancellationToken ct = default)
    {
        var agora = DateTime.Now;
        var limite = agora.AddHours(24);

        // Eventos que começam nas próximas 24h e ainda não têm lembrete publicado.
        var candidatos = await db.Eventos
            .Include(e => e.Local)
            .Where(e => (e.Status == StatusEvento.Agendado || e.Status == StatusEvento.AoVivo)
                        && e.Inicio > agora
                        && e.Inicio <= limite
                        && !db.Notificacoes.Any(n => n.EventoId == e.Id && n.Tipo == TipoNotificacao.Lembrete))
            .ToListAsync(ct);

        if (candidatos.Count == 0)
            return 0;

        foreach (var ev in candidatos)
        {
            db.Notificacoes.Add(new Notificacao
            {
                Titulo = $"Hoje: {ev.Titulo} ⏰",
                Mensagem = $"Começa às {ev.Inicio:HH'h'mm} no {ev.Local?.Nome}. Prepare-se para torcer!",
                Tipo = TipoNotificacao.Lembrete,
                EventoId = ev.Id,
                ModalidadeId = ev.ModalidadeId,
                Importante = true,
                CriadoEm = agora
            });
        }

        await db.SaveChangesAsync(ct);
        return candidatos.Count;
    }
}
