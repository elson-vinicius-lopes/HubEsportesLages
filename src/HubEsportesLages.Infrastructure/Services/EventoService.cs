using HubEsportesLages.Application.Common;
using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Application.Interfaces;
using HubEsportesLages.Application.Mapping;
using HubEsportesLages.Domain.Entities;
using HubEsportesLages.Domain.Enums;
using HubEsportesLages.Infrastructure.Common;
using HubEsportesLages.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HubEsportesLages.Infrastructure.Services;

public class EventoService(HubDbContext db, INotificacaoService notificacoes) : IEventoService
{
    private static readonly StatusEvento[] StatusAgenda =
        [StatusEvento.Agendado, StatusEvento.AoVivo, StatusEvento.Adiado];

    private IQueryable<Evento> ComIncludes() => db.Eventos
        .Include(e => e.Modalidade)
        .Include(e => e.Local)
        .Include(e => e.EquipeCasa)
        .Include(e => e.EquipeVisitante)
        .AsNoTracking();

    public async Task<PagedResult<EventoResumoDto>> ListarAgendaAsync(AgendaFiltro filtro, CancellationToken ct = default)
    {
        var query = AplicarFiltrosComuns(ComIncludes(), filtro)
            .Where(e => StatusAgenda.Contains(e.Status));

        query = AplicarPeriodo(query, filtro.Periodo);

        var total = await query.CountAsync(ct);
        var itens = await query
            .OrderBy(e => e.Inicio)
            .Skip((filtro.PaginaNormalizada - 1) * filtro.TamanhoNormalizado)
            .Take(filtro.TamanhoNormalizado)
            .ToListAsync(ct);

        return new PagedResult<EventoResumoDto>(
            itens.Select(e => e.ParaResumo()).ToList(),
            filtro.PaginaNormalizada, filtro.TamanhoNormalizado, total);
    }

    public async Task<PagedResult<EventoResumoDto>> ListarResultadosAsync(AgendaFiltro filtro, CancellationToken ct = default)
    {
        var query = AplicarFiltrosComuns(ComIncludes(), filtro)
            .Where(e => e.Status == StatusEvento.Encerrado);

        var total = await query.CountAsync(ct);
        var itens = await query
            .OrderByDescending(e => e.Inicio)
            .Skip((filtro.PaginaNormalizada - 1) * filtro.TamanhoNormalizado)
            .Take(filtro.TamanhoNormalizado)
            .ToListAsync(ct);

        return new PagedResult<EventoResumoDto>(
            itens.Select(e => e.ParaResumo()).ToList(),
            filtro.PaginaNormalizada, filtro.TamanhoNormalizado, total);
    }

    public async Task<IReadOnlyList<EventoResumoDto>> ListarDestaquesAsync(int quantidade = 5, CancellationToken ct = default)
    {
        var hoje = DateTime.Today;
        var itens = await ComIncludes()
            .Where(e => e.Destaque && StatusAgenda.Contains(e.Status) && e.Inicio >= hoje)
            .OrderBy(e => e.Inicio)
            .Take(quantidade)
            .ToListAsync(ct);

        return itens.Select(e => e.ParaResumo()).ToList();
    }

    public async Task<IReadOnlyList<EventoResumoDto>> ListarProximosAsync(int quantidade = 6, CancellationToken ct = default)
    {
        var hoje = DateTime.Today;
        var itens = await ComIncludes()
            .Where(e => StatusAgenda.Contains(e.Status) && e.Inicio >= hoje)
            .OrderBy(e => e.Inicio)
            .Take(quantidade)
            .ToListAsync(ct);

        return itens.Select(e => e.ParaResumo()).ToList();
    }

    public async Task<EventoDetalheDto?> ObterPorSlugAsync(string slug, CancellationToken ct = default)
    {
        var ev = await ComIncludes().FirstOrDefaultAsync(e => e.Slug == slug, ct);
        return ev?.ParaDetalhe();
    }

    public async Task<EventoDetalheDto?> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var ev = await ComIncludes().FirstOrDefaultAsync(e => e.Id == id, ct);
        return ev?.ParaDetalhe();
    }

    public async Task<int> CriarAsync(CriarEventoDto dto, CancellationToken ct = default)
    {
        var slugBase = SlugGenerator.Gerar(dto.Titulo);
        var slug = slugBase;
        if (await db.Eventos.AnyAsync(e => e.Slug == slug, ct))
            slug = $"{slugBase}-{await db.Eventos.CountAsync(ct) + 1}";

        var evento = new Evento
        {
            Titulo = dto.Titulo.Trim(),
            Slug = slug,
            Descricao = dto.Descricao?.Trim() ?? string.Empty,
            Campeonato = dto.Campeonato?.Trim() ?? string.Empty,
            ModalidadeId = dto.ModalidadeId,
            LocalId = dto.LocalId,
            EquipeCasaId = dto.EquipeCasaId,
            EquipeVisitanteId = dto.EquipeVisitanteId,
            Inicio = dto.Inicio,
            Fim = dto.Fim,
            Gratuito = dto.Gratuito,
            PrecoIngresso = dto.Gratuito ? null : dto.PrecoIngresso,
            Destaque = dto.Destaque,
            ImagemUrl = dto.ImagemUrl,
            Status = StatusEvento.Agendado,
            CriadoEm = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow
        };

        db.Eventos.Add(evento);
        await db.SaveChangesAsync(ct);

        await notificacoes.PublicarAsync(
            $"Novo na agenda: {evento.Titulo}",
            $"{evento.Inicio:dd/MM 'às' HH'h'} — {evento.Campeonato}".TrimEnd(' ', '—'),
            TipoNotificacao.NovoEvento,
            eventoId: evento.Id,
            modalidadeId: evento.ModalidadeId,
            importante: evento.Destaque,
            ct: ct);

        return evento.Id;
    }

    public async Task<bool> AtualizarResultadoAsync(int id, AtualizarResultadoDto dto, CancellationToken ct = default)
    {
        var evento = await db.Eventos
            .Include(e => e.Modalidade)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
        if (evento is null)
            return false;

        evento.PlacarCasa = dto.PlacarCasa;
        evento.PlacarVisitante = dto.PlacarVisitante;
        evento.Status = dto.Status;
        evento.AtualizadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        if (dto.Status == StatusEvento.Encerrado && evento.Placar is not null)
        {
            await notificacoes.PublicarAsync(
                $"Resultado: {evento.Titulo}",
                $"Placar final: {evento.Titulo.Replace(" x ", $" {evento.PlacarCasa} x {evento.PlacarVisitante} ")}.",
                TipoNotificacao.Resultado,
                eventoId: evento.Id,
                modalidadeId: evento.ModalidadeId,
                ct: ct);
        }

        return true;
    }

    // ------------------------------------------------------------------ filtros
    private static IQueryable<Evento> AplicarFiltrosComuns(IQueryable<Evento> query, AgendaFiltro filtro)
    {
        if (!string.IsNullOrWhiteSpace(filtro.Modalidade))
            query = query.Where(e => e.Modalidade!.Slug == filtro.Modalidade);

        if (filtro.EquipeId is int eqId)
            query = query.Where(e => e.EquipeCasaId == eqId || e.EquipeVisitanteId == eqId);

        if (filtro.LocalId is int locId)
            query = query.Where(e => e.LocalId == locId);

        if (filtro.ApenasGratuitos)
            query = query.Where(e => e.Gratuito);

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var termo = filtro.Busca.Trim();
            query = query.Where(e =>
                e.Titulo.Contains(termo) ||
                e.Campeonato.Contains(termo) ||
                (e.EquipeCasa != null && e.EquipeCasa.Nome.Contains(termo)) ||
                (e.EquipeVisitante != null && e.EquipeVisitante.Nome.Contains(termo)));
        }

        return query;
    }

    private static IQueryable<Evento> AplicarPeriodo(IQueryable<Evento> query, PeriodoAgenda periodo)
    {
        var hoje = DateTime.Today;
        return periodo switch
        {
            PeriodoAgenda.Hoje => query.Where(e => e.Inicio >= hoje && e.Inicio < hoje.AddDays(1)),
            PeriodoAgenda.Semana => query.Where(e => e.Inicio >= hoje && e.Inicio < hoje.AddDays(7)),
            PeriodoAgenda.Mes => query.Where(e => e.Inicio >= hoje && e.Inicio < hoje.AddDays(31)),
            PeriodoAgenda.Todos => query,
            _ => query.Where(e => e.Inicio >= hoje) // Proximos
        };
    }

    public async Task<MetricasDashboardDto> ObterMetricasDashboardAsync(CancellationToken ct = default)
    {
        var totalEventos = await db.Eventos.CountAsync(ct);
        var totalInscricoes = await db.Inscricoes.CountAsync(i => i.Ativa, ct);
        var totalMensagensMural = await db.MensagensTorcida.CountAsync(m => !m.Removida, ct);
        
        var totalParticipantesAtivos = await db.MensagensTorcida.Where(m => !m.Removida).Select(m => m.TorcedorId)
            .Union(db.VotosMvp.Select(v => v.TorcedorId))
            .Union(db.VotosEnquete.Select(v => v.TorcedorId))
            .Distinct()
            .CountAsync(ct);
            
        var limiteInicio = DateTime.Today.AddYears(-1);
        var datasEventos = await db.Eventos
            .Where(e => e.Inicio >= limiteInicio)
            .Select(e => e.Inicio)
            .ToListAsync(ct);
            
        var eventosPorDia = datasEventos
            .GroupBy(d => d.Date)
            .Select(g => new MetricaSazonalDto
            {
                Data = g.Key,
                QuantidadeEventos = g.Count()
            })
            .OrderBy(m => m.Data)
            .ToList();
            
        var eventosRecentes = await db.Eventos
            .Include(e => e.Modalidade)
            .OrderByDescending(e => e.Inicio)
            .Take(10)
            .ToListAsync(ct);
            
        var idsEventos = eventosRecentes.Select(e => e.Id).ToList();
        
        var votosMvp = await db.VotosMvp
            .Where(v => idsEventos.Contains(v.EventoId))
            .Select(v => new { v.EventoId, v.TorcedorId })
            .ToListAsync(ct);
            
        var mensagens = await db.MensagensTorcida
            .Where(m => !m.Removida && idsEventos.Contains(m.EventoId))
            .Select(m => new { m.EventoId, m.TorcedorId })
            .ToListAsync(ct);
            
        var votosEnquete = await db.VotosEnquete
            .Where(v => idsEventos.Contains(v.Enquete!.EventoId))
            .Select(v => new { EventoId = v.Enquete!.EventoId, v.TorcedorId })
            .ToListAsync(ct);
            
        var engajamentoPorEvento = new List<MetricaEngajamentoEventoDto>();
        foreach (var ev in eventosRecentes)
        {
            var torcedoresDoEvento = votosMvp.Where(v => v.EventoId == ev.Id).Select(v => v.TorcedorId)
                .Concat(mensagens.Where(m => m.EventoId == ev.Id).Select(m => m.TorcedorId))
                .Concat(votosEnquete.Where(v => v.EventoId == ev.Id).Select(v => v.TorcedorId))
                .Distinct()
                .Count();
                
            engajamentoPorEvento.Add(new MetricaEngajamentoEventoDto
            {
                EventoId = ev.Id,
                Titulo = ev.Titulo,
                ModalidadeIcone = ev.Modalidade?.Icone ?? "⚽",
                Inicio = ev.Inicio,
                ParticipantesUnicos = torcedoresDoEvento
            });
        }
        
        return new MetricasDashboardDto
        {
            TotalEventos = totalEventos,
            TotalInscricoes = totalInscricoes,
            TotalParticipantesAtivos = totalParticipantesAtivos,
            TotalMensagensMural = totalMensagensMural,
            EventosPorDia = eventosPorDia,
            EngajamentoPorEvento = engajamentoPorEvento
        };
    }
}
