using HubEsportesLages.Domain.Enums;

namespace HubEsportesLages.Application.DTOs;

/// <summary>Notificação exibida no feed central do hub.</summary>
public record NotificacaoDto(
    int Id,
    string Titulo,
    string Mensagem,
    TipoNotificacao Tipo,
    bool Importante,
    bool Lida,
    DateTime CriadoEm,
    int? EventoId,
    string? EventoSlug,
    string? ModalidadeIcone,
    string? ModalidadeCor);
