namespace HubEsportesLages.Domain.Enums;

/// <summary>Classifica a natureza de uma notificação enviada ao torcedor.</summary>
public enum TipoNotificacao
{
    NovoEvento = 0,
    Lembrete = 1,
    AlteracaoHorario = 2,
    Resultado = 3,
    Cancelamento = 4
}
