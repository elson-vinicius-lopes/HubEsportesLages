using System.ComponentModel.DataAnnotations;
using HubEsportesLages.Domain.Enums;

namespace HubEsportesLages.Application.DTOs;

/// <summary>Pedido de compra de um ingresso (torcedor logado escolhe o evento).</summary>
public class CriarIngressoDto
{
    [Required(ErrorMessage = "Informe o evento.")]
    public int EventoId { get; set; }
}

/// <summary>Representação de um ingresso já comprado (lista "Meus ingressos").</summary>
public record IngressoDto(
    int Id,
    string EventoTitulo,
    string EventoSlug,
    decimal Preco,
    StatusIngresso Status,
    DateTime CriadoEm,
    DateTime? PagoEm);

/// <summary>Cobrança Pix (simulada) gerada ao comprar — copia-e-cola + QR de pagamento.</summary>
public record PagamentoPixDto(
    int IngressoId,
    string PixCopiaECola,
    string QrPagamentoBase64,
    decimal Valor);

/// <summary>Ingresso emitido após o pagamento — token assinado + QR do ingresso.</summary>
public record IngressoEmitidoDto(
    int Id,
    string Token,
    string QrIngressoBase64,
    string EventoTitulo);

/// <summary>Token apresentado pelo admin para validar/realizar o check-in.</summary>
public class ValidarIngressoDto
{
    [Required(ErrorMessage = "Informe o token do ingresso.")]
    public string Token { get; set; } = string.Empty;
}

/// <summary>Resultado da validação de um ingresso na entrada (✅/❌ + motivo).</summary>
public record ValidacaoResultadoDto(
    bool Valido,
    StatusIngresso? Status,
    string Mensagem,
    string? EventoTitulo = null,
    string? CompradorNome = null);
