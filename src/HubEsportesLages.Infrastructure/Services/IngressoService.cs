using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Application.Interfaces;
using HubEsportesLages.Application.Mapping;
using HubEsportesLages.Domain.Entities;
using HubEsportesLages.Domain.Enums;
using HubEsportesLages.Infrastructure.Common;
using HubEsportesLages.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HubEsportesLages.Infrastructure.Services;

/// <summary>
/// Fluxo do ingresso digital: compra (Pendente + cobrança Pix simulada), confirmação de
/// pagamento (Pago + token assinado + QR), listagem do comprador e validação na entrada
/// (uso único idempotente).
/// </summary>
public class IngressoService(
    HubDbContext db,
    IPagamentoService pagamento,
    ITokenIngresso token) : IIngressoService
{
    public async Task<PagamentoPixDto?> ComprarAsync(int eventoId, string compradorId, string compradorNome, CancellationToken ct = default)
    {
        var evento = await db.Eventos.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventoId, ct);

        // Só evento existente e pago — gratuito não vende ingresso.
        if (evento is null || evento.Gratuito)
            return null;

        var ingresso = new Ingresso
        {
            EventoId = evento.Id,
            CompradorId = compradorId,
            CompradorNome = compradorNome,
            Preco = evento.PrecoIngresso ?? 0m,
            Status = StatusIngresso.Pendente,
            TxidPix = MockPixPagamentoService.GerarTxid(),
            CriadoEm = DateTime.UtcNow
        };

        db.Ingressos.Add(ingresso);
        await db.SaveChangesAsync(ct);

        return pagamento.GerarCobrancaPix(ingresso);
    }

    public async Task<IngressoEmitidoDto?> ConfirmarPagamentoAsync(int ingressoId, string compradorId, CancellationToken ct = default)
    {
        var ingresso = await db.Ingressos
            .Include(i => i.Evento)
            .FirstOrDefaultAsync(i => i.Id == ingressoId && i.CompradorId == compradorId, ct);

        if (ingresso is null)
            return null;

        // Idempotente: se já está pago e emitido, devolve o mesmo ingresso.
        if (ingresso.Status == StatusIngresso.Pendente)
        {
            if (!pagamento.ConfirmarPagamento(ingresso.TxidPix))
                return null;

            ingresso.Status = StatusIngresso.Pago;
            ingresso.PagoEm = DateTime.UtcNow;
            ingresso.Token = token.Gerar(ingresso.Id);
            await db.SaveChangesAsync(ct);
        }

        if (ingresso.Token is null)
            return null;

        var qr = QrCodeGenerator.GerarPngBase64(ingresso.Token);
        return new IngressoEmitidoDto(
            ingresso.Id,
            ingresso.Token,
            qr,
            ingresso.Evento?.Titulo ?? string.Empty);
    }

    public async Task<IReadOnlyList<IngressoDto>> ListarMeusAsync(string compradorId, CancellationToken ct = default)
    {
        var itens = await db.Ingressos
            .Include(i => i.Evento)
            .AsNoTracking()
            .Where(i => i.CompradorId == compradorId)
            .OrderByDescending(i => i.CriadoEm)
            .ToListAsync(ct);

        return itens.Select(i => i.ParaDto()).ToList();
    }

    public async Task<ValidacaoResultadoDto> ValidarAsync(string tokenIngresso, string adminId, CancellationToken ct = default)
    {
        var id = token.Validar(tokenIngresso);
        if (id is null)
            return new ValidacaoResultadoDto(false, null, "Ingresso inválido: assinatura não confere.");

        var ingresso = await db.Ingressos
            .Include(i => i.Evento)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

        if (ingresso is null)
            return new ValidacaoResultadoDto(false, null, "Ingresso não encontrado.");

        return ingresso.Status switch
        {
            StatusIngresso.Utilizado => new ValidacaoResultadoDto(
                false, ingresso.Status,
                $"Ingresso já utilizado em {ingresso.UtilizadoEm?.ToLocalTime():dd/MM HH:mm}.",
                ingresso.Evento?.Titulo, ingresso.CompradorNome),

            StatusIngresso.Pendente => new ValidacaoResultadoDto(
                false, ingresso.Status, "Ingresso não pago.",
                ingresso.Evento?.Titulo, ingresso.CompradorNome),

            StatusIngresso.Cancelado => new ValidacaoResultadoDto(
                false, ingresso.Status, "Ingresso cancelado.",
                ingresso.Evento?.Titulo, ingresso.CompradorNome),

            StatusIngresso.Pago => await MarcarUtilizadoAsync(ingresso, adminId, ct),

            _ => new ValidacaoResultadoDto(false, ingresso.Status, "Ingresso inválido.",
                ingresso.Evento?.Titulo, ingresso.CompradorNome)
        };
    }

    private async Task<ValidacaoResultadoDto> MarcarUtilizadoAsync(Ingresso ingresso, string adminId, CancellationToken ct)
    {
        ingresso.Status = StatusIngresso.Utilizado;
        ingresso.UtilizadoEm = DateTime.UtcNow;
        ingresso.ValidadoPor = adminId;
        await db.SaveChangesAsync(ct);

        return new ValidacaoResultadoDto(
            true, StatusIngresso.Utilizado, "Entrada liberada. Check-in realizado.",
            ingresso.Evento?.Titulo, ingresso.CompradorNome);
    }
}
