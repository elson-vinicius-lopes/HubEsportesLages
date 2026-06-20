using HubEsportesLages.Domain.Enums;

namespace HubEsportesLages.Domain.Entities;

/// <summary>
/// Notificação publicada no feed central do hub (novo evento, lembrete,
/// alteração de horário, resultado ou cancelamento).
/// </summary>
public class Notificacao
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Mensagem { get; set; } = string.Empty;

    public TipoNotificacao Tipo { get; set; } = TipoNotificacao.NovoEvento;

    public int? EventoId { get; set; }
    public Evento? Evento { get; set; }

    public int? ModalidadeId { get; set; }
    public Modalidade? Modalidade { get; set; }

    public bool Importante { get; set; }

    public bool Lida { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
