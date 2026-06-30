using HubEsportesLages.Application.DTOs;

namespace HubEsportesLages.Application.Interfaces;

/// <summary>
/// Orquestra o ciclo de vida do ingresso digital: compra (Pix simulado), confirmação
/// de pagamento (emissão do token + QR), listagem do torcedor e validação na entrada.
/// </summary>
public interface IIngressoService
{
    /// <summary>
    /// Compra um ingresso de um evento pago (cria Pendente + cobrança Pix simulada).
    /// Retorna null se o evento não existe ou é gratuito.
    /// </summary>
    Task<PagamentoPixDto?> ComprarAsync(int eventoId, string compradorId, string compradorNome, CancellationToken ct = default);

    /// <summary>
    /// Confirma o pagamento (simulado) de um ingresso do próprio comprador: vira Pago,
    /// ganha token assinado e QR. Retorna null se não encontrado/não pertence ao comprador.
    /// </summary>
    Task<IngressoEmitidoDto?> ConfirmarPagamentoAsync(int ingressoId, string compradorId, CancellationToken ct = default);

    /// <summary>Lista os ingressos do comprador (mais recentes primeiro).</summary>
    Task<IReadOnlyList<IngressoDto>> ListarMeusAsync(string compradorId, CancellationToken ct = default);

    /// <summary>
    /// Valida um token na entrada (admin): se válido + Pago + não utilizado, marca
    /// Utilizado e registra o admin. Uso único idempotente; sempre retorna o resultado.
    /// </summary>
    Task<ValidacaoResultadoDto> ValidarAsync(string token, string adminId, CancellationToken ct = default);
}
