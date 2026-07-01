using HubEsportesLages.Application.Common;
using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Application.Interfaces;
using HubEsportesLages.Application.Mapping;
using HubEsportesLages.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HubEsportesLages.Infrastructure.Services;

/// <summary>
/// Direitos do titular (LGPD): consulta e exclusão dos dados de domínio vinculados
/// à conta do torcedor. As interações de torcida (votos, mural, favoritos) usam o
/// TorcedorId anônimo do navegador e não são vinculáveis à conta — ficam de fora.
/// Spec: docs/specs/lgpd/.
/// </summary>
public class LgpdService(HubDbContext db) : ILgpdService
{
    public async Task<IReadOnlyList<InscricaoDto>> ListarInscricoesDoTitularAsync(string email, CancellationToken ct = default)
    {
        var alvo = Normalizar(email);

        var inscricoes = await db.Inscricoes
            .AsNoTracking()
            .Include(i => i.Modalidade)
            .Include(i => i.Equipe)
            .Where(i => i.Email == alvo)
            .OrderByDescending(i => i.CriadoEm)
            .ToListAsync(ct);

        return inscricoes.Select(i => i.ParaDto()).ToList();
    }

    public async Task<IReadOnlyList<IngressoDto>> ListarIngressosDoTitularAsync(string email, CancellationToken ct = default)
    {
        var alvo = email.Trim();

        var ingressos = await db.Ingressos
            .AsNoTracking()
            .Include(i => i.Evento)
            .Where(i => i.CompradorId == alvo)
            .OrderByDescending(i => i.CriadoEm)
            .ToListAsync(ct);

        return ingressos.Select(i => i.ParaDto()).ToList();
    }

    public async Task<ExclusaoDadosDto> ApagarDadosVinculadosAsync(string email, CancellationToken ct = default)
    {
        var alvoInscricao = Normalizar(email);
        var alvoIngresso = email.Trim();

        // Inscrições de alerta: dados pessoais sem valor contábil — apagadas de vez.
        var inscricoesRemovidas = await db.Inscricoes
            .Where(i => i.Email == alvoInscricao)
            .ExecuteDeleteAsync(ct);

        // Ingressos: registro contábil da venda é mantido (valor/status/datas), mas o
        // comprador é anonimizado. O CompradorId guarda o e-mail (dado pessoal), então
        // também é trocado por um marcador — assim um futuro cadastro com o mesmo e-mail
        // não herda os ingressos antigos.
        var marcador = $"removido:{Guid.NewGuid():N}";
        var ingressosAnonimizados = await db.Ingressos
            .Where(i => i.CompradorId == alvoIngresso)
            .ExecuteUpdateAsync(s => s
                .SetProperty(i => i.CompradorNome, LgpdConstantes.CompradorNomeAnonimizado)
                .SetProperty(i => i.CompradorId, marcador), ct);

        return new ExclusaoDadosDto(inscricoesRemovidas, ingressosAnonimizados);
    }

    /// <summary>Mesma normalização usada pelo InscricaoService ao gravar (trim + lower).</summary>
    private static string Normalizar(string email) => email.Trim().ToLowerInvariant();
}
