using System;
using System.Collections.Generic;

namespace HubEsportesLages.Application.DTOs;

public class MetricasDashboardDto
{
    public int TotalEventos { get; set; }
    public int TotalInscricoes { get; set; }
    public int TotalParticipantesAtivos { get; set; }
    public int TotalMensagensMural { get; set; }
    
    public IReadOnlyList<MetricaSazonalDto> EventosPorDia { get; set; } = Array.Empty<MetricaSazonalDto>();
    public IReadOnlyList<MetricaEngajamentoEventoDto> EngajamentoPorEvento { get; set; } = Array.Empty<MetricaEngajamentoEventoDto>();
}

public class MetricaSazonalDto
{
    public DateTime Data { get; set; }
    public int QuantidadeEventos { get; set; }
}

public class MetricaEngajamentoEventoDto
{
    public int EventoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string ModalidadeIcone { get; set; } = string.Empty;
    public DateTime Inicio { get; set; }
    public int ParticipantesUnicos { get; set; }
}
