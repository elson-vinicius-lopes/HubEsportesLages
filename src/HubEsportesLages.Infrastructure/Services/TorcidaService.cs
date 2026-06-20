using HubEsportesLages.Application.Common;
using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Application.Interfaces;
using HubEsportesLages.Application.Mapping;
using HubEsportesLages.Domain.Entities;
using HubEsportesLages.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HubEsportesLages.Infrastructure.Services;

/// <summary>
/// Implementa a interação da torcida persistida via REST. Votos são idempotentes
/// (1 por torcedor, garantidos por índice único), o mural tem limite de tamanho e
/// rate limit, e as escritas só são aceitas com o evento <c>AoVivo</c>.
/// </summary>
public class TorcidaService(HubDbContext db, ITorcedorContexto torcedor) : ITorcidaService
{
    private static readonly TimeSpan IntervaloMinimoMensagem = TimeSpan.FromSeconds(3);
    private const int LimiteMensagens = 50;

    public async Task<TorcidaEstadoDto?> ObterEstadoAsync(string slug, CancellationToken ct = default)
    {
        var evento = await db.Eventos.AsNoTracking().FirstOrDefaultAsync(e => e.Slug == slug, ct);
        return evento is null ? null : await MontarEstadoAsync(evento, ct);
    }

    public async Task<ResultadoInteracao<TorcidaEstadoDto>> VotarMvpAsync(string slug, VotarMvpDto dto, CancellationToken ct = default)
    {
        var evento = await db.Eventos.AsNoTracking().FirstOrDefaultAsync(e => e.Slug == slug, ct);
        if (evento is null)
            return ResultadoInteracao<TorcidaEstadoDto>.Falha(StatusInteracao.NaoEncontrado);
        if (!evento.AceitaInteracao)
            return ResultadoInteracao<TorcidaEstadoDto>.Falha(StatusInteracao.NaoAoVivo);

        var torcedorId = torcedor.TorcedorId;
        if (string.IsNullOrEmpty(torcedorId))
            return ResultadoInteracao<TorcidaEstadoDto>.Falha(StatusInteracao.SemTorcedor);

        var jogadorValido = await db.JogadoresEvento
            .AnyAsync(j => j.Id == dto.JogadorEventoId && j.EventoId == evento.Id, ct);
        if (!jogadorValido)
            return ResultadoInteracao<TorcidaEstadoDto>.Falha(StatusInteracao.Invalido);

        var jaVotou = await db.VotosMvp
            .AnyAsync(v => v.EventoId == evento.Id && v.TorcedorId == torcedorId, ct);
        if (!jaVotou)
        {
            db.VotosMvp.Add(new VotoMvp
            {
                EventoId = evento.Id,
                JogadorEventoId = dto.JogadorEventoId,
                TorcedorId = torcedorId,
                CriadoEm = DateTime.UtcNow
            });
            await SalvarIdempotenteAsync(ct);
        }

        return ResultadoInteracao<TorcidaEstadoDto>.Sucesso(await MontarEstadoAsync(evento, ct));
    }

    public async Task<ResultadoInteracao<TorcidaEstadoDto>> VotarEnqueteAsync(string slug, int enqueteId, VotarEnqueteDto dto, CancellationToken ct = default)
    {
        var evento = await db.Eventos.AsNoTracking().FirstOrDefaultAsync(e => e.Slug == slug, ct);
        if (evento is null)
            return ResultadoInteracao<TorcidaEstadoDto>.Falha(StatusInteracao.NaoEncontrado);
        if (!evento.AceitaInteracao)
            return ResultadoInteracao<TorcidaEstadoDto>.Falha(StatusInteracao.NaoAoVivo);

        var torcedorId = torcedor.TorcedorId;
        if (string.IsNullOrEmpty(torcedorId))
            return ResultadoInteracao<TorcidaEstadoDto>.Falha(StatusInteracao.SemTorcedor);

        var opcaoValida = await db.OpcoesEnquete
            .AnyAsync(o => o.Id == dto.OpcaoId && o.EnqueteId == enqueteId && o.Enquete!.EventoId == evento.Id, ct);
        if (!opcaoValida)
            return ResultadoInteracao<TorcidaEstadoDto>.Falha(StatusInteracao.Invalido);

        var jaVotou = await db.VotosEnquete
            .AnyAsync(v => v.EnqueteId == enqueteId && v.TorcedorId == torcedorId, ct);
        if (!jaVotou)
        {
            db.VotosEnquete.Add(new VotoEnquete
            {
                EnqueteId = enqueteId,
                OpcaoEnqueteId = dto.OpcaoId,
                TorcedorId = torcedorId,
                CriadoEm = DateTime.UtcNow
            });
            await SalvarIdempotenteAsync(ct);
        }

        return ResultadoInteracao<TorcidaEstadoDto>.Sucesso(await MontarEstadoAsync(evento, ct));
    }

    public async Task<IReadOnlyList<MensagemDto>?> ListarMensagensAsync(string slug, DateTime? desde, CancellationToken ct = default)
    {
        var eventoId = await db.Eventos.AsNoTracking()
            .Where(e => e.Slug == slug)
            .Select(e => (int?)e.Id)
            .FirstOrDefaultAsync(ct);

        return eventoId is null ? null : await ConsultarMensagensAsync(eventoId.Value, desde, ct);
    }

    public async Task<ResultadoInteracao<MensagemDto>> EnviarMensagemAsync(string slug, EnviarMensagemDto dto, CancellationToken ct = default)
    {
        var evento = await db.Eventos.AsNoTracking().FirstOrDefaultAsync(e => e.Slug == slug, ct);
        if (evento is null)
            return ResultadoInteracao<MensagemDto>.Falha(StatusInteracao.NaoEncontrado);
        if (!evento.AceitaInteracao)
            return ResultadoInteracao<MensagemDto>.Falha(StatusInteracao.NaoAoVivo);

        var torcedorId = torcedor.TorcedorId;
        if (string.IsNullOrEmpty(torcedorId))
            return ResultadoInteracao<MensagemDto>.Falha(StatusInteracao.SemTorcedor);

        var texto = dto.Texto.Trim();
        if (texto.Length is 0 or > 140)
            return ResultadoInteracao<MensagemDto>.Falha(StatusInteracao.Invalido);

        var ultima = await db.MensagensTorcida
            .Where(m => m.EventoId == evento.Id && m.TorcedorId == torcedorId)
            .OrderByDescending(m => m.CriadoEm)
            .Select(m => (DateTime?)m.CriadoEm)
            .FirstOrDefaultAsync(ct);
        if (ultima is DateTime quando && DateTime.UtcNow - quando < IntervaloMinimoMensagem)
            return ResultadoInteracao<MensagemDto>.Falha(StatusInteracao.LimiteExcedido);

        var mensagem = new MensagemTorcida
        {
            EventoId = evento.Id,
            TorcedorId = torcedorId,
            Autor = DerivarApelido(torcedorId),
            Texto = texto,
            CriadoEm = DateTime.UtcNow
        };
        db.MensagensTorcida.Add(mensagem);
        await db.SaveChangesAsync(ct);

        return ResultadoInteracao<MensagemDto>.Sucesso(mensagem.ParaDto());
    }

    public async Task<StatusInteracao> FavoritarEquipeAsync(int equipeId, CancellationToken ct = default)
    {
        var torcedorId = torcedor.TorcedorId;
        if (string.IsNullOrEmpty(torcedorId))
            return StatusInteracao.SemTorcedor;

        if (!await db.Equipes.AnyAsync(e => e.Id == equipeId, ct))
            return StatusInteracao.NaoEncontrado;

        var jaFavoritou = await db.EquipesFavoritas
            .AnyAsync(f => f.TorcedorId == torcedorId && f.EquipeId == equipeId, ct);
        if (!jaFavoritou)
        {
            db.EquipesFavoritas.Add(new EquipeFavorita
            {
                TorcedorId = torcedorId,
                EquipeId = equipeId,
                CriadoEm = DateTime.UtcNow
            });
            await SalvarIdempotenteAsync(ct);
        }
        return StatusInteracao.Ok;
    }

    public async Task<StatusInteracao> DesfavoritarEquipeAsync(int equipeId, CancellationToken ct = default)
    {
        var torcedorId = torcedor.TorcedorId;
        if (string.IsNullOrEmpty(torcedorId))
            return StatusInteracao.SemTorcedor;

        var favorito = await db.EquipesFavoritas
            .FirstOrDefaultAsync(f => f.TorcedorId == torcedorId && f.EquipeId == equipeId, ct);
        if (favorito is not null)
        {
            db.EquipesFavoritas.Remove(favorito);
            await db.SaveChangesAsync(ct);
        }
        return StatusInteracao.Ok;
    }

    // ───────────────────────────────────────────────────────────────── helpers

    private async Task<TorcidaEstadoDto> MontarEstadoAsync(Evento evento, CancellationToken ct)
    {
        var torcedorId = torcedor.TorcedorId;
        return new TorcidaEstadoDto(
            evento.Status,
            evento.AceitaInteracao,
            await MontarMvpAsync(evento.Id, torcedorId, ct),
            await MontarEnqueteAsync(evento.Id, torcedorId, ct),
            await ConsultarMensagensAsync(evento.Id, null, ct),
            await ConsultarFavoritadoAsync(evento, torcedorId, ct));
    }

    private async Task<MvpDto> MontarMvpAsync(int eventoId, string? torcedorId, CancellationToken ct)
    {
        var jogadores = await db.JogadoresEvento.AsNoTracking()
            .Where(j => j.EventoId == eventoId)
            .Select(j => new { j.Id, j.Nome, Equipe = j.Equipe!.Nome })
            .ToListAsync(ct);

        var votos = await db.VotosMvp.AsNoTracking()
            .Where(v => v.EventoId == eventoId)
            .GroupBy(v => v.JogadorEventoId)
            .Select(g => new { JogadorId = g.Key, Total = g.Count() })
            .ToListAsync(ct);
        var mapaVotos = votos.ToDictionary(v => v.JogadorId, v => v.Total);

        var candidatos = jogadores
            .Select(j => new MvpCandidatoDto(
                j.Id, j.Nome, j.Equipe,
                mapaVotos.TryGetValue(j.Id, out var total) ? total : 0))
            .OrderByDescending(c => c.Votos)
            .ThenBy(c => c.Nome)
            .ToList();

        int? meuVoto = null;
        if (!string.IsNullOrEmpty(torcedorId))
        {
            meuVoto = await db.VotosMvp.AsNoTracking()
                .Where(v => v.EventoId == eventoId && v.TorcedorId == torcedorId)
                .Select(v => (int?)v.JogadorEventoId)
                .FirstOrDefaultAsync(ct);
        }

        return new MvpDto(candidatos, meuVoto);
    }

    private async Task<EnqueteDto?> MontarEnqueteAsync(int eventoId, string? torcedorId, CancellationToken ct)
    {
        var enquete = await db.Enquetes.AsNoTracking()
            .Where(e => e.EventoId == eventoId && e.Ativa)
            .OrderByDescending(e => e.CriadoEm)
            .Select(e => new { e.Id, e.Pergunta })
            .FirstOrDefaultAsync(ct);
        if (enquete is null)
            return null;

        var opcoes = await db.OpcoesEnquete.AsNoTracking()
            .Where(o => o.EnqueteId == enquete.Id)
            .Select(o => new { o.Id, o.Texto })
            .ToListAsync(ct);

        var votos = await db.VotosEnquete.AsNoTracking()
            .Where(v => v.EnqueteId == enquete.Id)
            .GroupBy(v => v.OpcaoEnqueteId)
            .Select(g => new { OpcaoId = g.Key, Total = g.Count() })
            .ToListAsync(ct);
        var mapaVotos = votos.ToDictionary(v => v.OpcaoId, v => v.Total);
        var totalVotos = votos.Sum(v => v.Total);

        var opcoesDto = opcoes.Select(o =>
        {
            var qtd = mapaVotos.TryGetValue(o.Id, out var t) ? t : 0;
            var pct = totalVotos == 0 ? 0 : (int)Math.Round(qtd * 100.0 / totalVotos);
            return new OpcaoEnqueteDto(o.Id, o.Texto, qtd, pct);
        }).ToList();

        int? minhaOpcao = null;
        if (!string.IsNullOrEmpty(torcedorId))
        {
            minhaOpcao = await db.VotosEnquete.AsNoTracking()
                .Where(v => v.EnqueteId == enquete.Id && v.TorcedorId == torcedorId)
                .Select(v => (int?)v.OpcaoEnqueteId)
                .FirstOrDefaultAsync(ct);
        }

        return new EnqueteDto(enquete.Id, enquete.Pergunta, opcoesDto, minhaOpcao);
    }

    private async Task<IReadOnlyList<MensagemDto>> ConsultarMensagensAsync(int eventoId, DateTime? desde, CancellationToken ct)
    {
        var query = db.MensagensTorcida.AsNoTracking()
            .Where(m => m.EventoId == eventoId && !m.Removida);
        if (desde is DateTime d)
            query = query.Where(m => m.CriadoEm > d);

        var mensagens = await query
            .OrderByDescending(m => m.CriadoEm)
            .Take(LimiteMensagens)
            .ToListAsync(ct);

        return mensagens.Select(m => m.ParaDto()).ToList();
    }

    private async Task<bool> ConsultarFavoritadoAsync(Evento evento, string? torcedorId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(torcedorId))
            return false;

        var ids = new List<int>(2);
        if (evento.EquipeCasaId is int casa) ids.Add(casa);
        if (evento.EquipeVisitanteId is int visitante) ids.Add(visitante);
        if (ids.Count == 0)
            return false;

        return await db.EquipesFavoritas.AsNoTracking()
            .AnyAsync(f => f.TorcedorId == torcedorId && ids.Contains(f.EquipeId), ct);
    }

    /// <summary>Salva tratando a violação do índice único (corrida de duplo voto) como idempotência.</summary>
    private async Task SalvarIdempotenteAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is Microsoft.Data.Sqlite.SqliteException sqlite && sqlite.SqliteErrorCode == 19)
        {
            // 19 = SQLITE_CONSTRAINT (índice único): outro voto do mesmo torcedor já entrou.
            // Trata como idempotência; demais falhas de persistência propagam.
            db.ChangeTracker.Clear();
        }
    }

    private static string DerivarApelido(string torcedorId)
    {
        var limpo = torcedorId.Replace("-", string.Empty);
        if (limpo.Length == 0)
            return "Torcedor";
        var sufixo = (limpo.Length >= 4 ? limpo[^4..] : limpo).ToUpperInvariant();
        return $"Torcedor {sufixo}";
    }
}
