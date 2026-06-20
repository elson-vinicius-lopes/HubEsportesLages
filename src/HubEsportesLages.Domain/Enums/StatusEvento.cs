namespace HubEsportesLages.Domain.Enums;

/// <summary>Ciclo de vida de um evento esportivo dentro do hub.</summary>
public enum StatusEvento
{
    Agendado = 0,
    AoVivo = 1,
    Encerrado = 2,
    Adiado = 3,
    Cancelado = 4
}
