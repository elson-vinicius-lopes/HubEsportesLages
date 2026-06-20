using System.ComponentModel.DataAnnotations;
using HubEsportesLages.Domain.Enums;

namespace HubEsportesLages.Application.DTOs;

/// <summary>Versão enxuta de um evento, usada nos cards da agenda e dos resultados.</summary>
public record EventoResumoDto(
    int Id,
    string Titulo,
    string Slug,
    string Campeonato,
    string ModalidadeNome,
    string ModalidadeIcone,
    string ModalidadeCor,
    string LocalNome,
    string Bairro,
    DateTime Inicio,
    StatusEvento Status,
    bool EhConfronto,
    string? EquipeCasa,
    string? EquipeCasaEscudo,
    string? EquipeVisitante,
    string? EquipeVisitanteEscudo,
    string? Placar,
    bool Gratuito,
    decimal? PrecoIngresso,
    string? ImagemUrl,
    bool Destaque);

/// <summary>Visão completa de um evento (página de detalhe).</summary>
public record EventoDetalheDto(
    int Id,
    string Titulo,
    string Slug,
    string Descricao,
    string Campeonato,
    int ModalidadeId,
    string ModalidadeNome,
    string ModalidadeIcone,
    string ModalidadeCor,
    string LocalNome,
    string LocalEndereco,
    string Bairro,
    int? Capacidade,
    string MapaUrl,
    DateTime Inicio,
    DateTime? Fim,
    StatusEvento Status,
    bool EhConfronto,
    string? EquipeCasa,
    string? EquipeCasaEscudo,
    string? EquipeVisitante,
    string? EquipeVisitanteEscudo,
    int? PlacarCasa,
    int? PlacarVisitante,
    string? Placar,
    bool Gratuito,
    decimal? PrecoIngresso,
    string? ImagemUrl);

/// <summary>Payload para criação/edição de um evento (área administrativa e API).</summary>
public class CriarEventoDto
{
    [Required(ErrorMessage = "Informe o título do evento.")]
    [StringLength(160, MinimumLength = 4)]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Descricao { get; set; } = string.Empty;

    [StringLength(160)]
    public string Campeonato { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Selecione a modalidade.")]
    public int ModalidadeId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecione o local.")]
    public int LocalId { get; set; }

    public int? EquipeCasaId { get; set; }
    public int? EquipeVisitanteId { get; set; }

    [Required(ErrorMessage = "Informe a data e hora de início.")]
    public DateTime Inicio { get; set; }

    public DateTime? Fim { get; set; }

    public bool Gratuito { get; set; } = true;

    [Range(0, 100000)]
    public decimal? PrecoIngresso { get; set; }

    public bool Destaque { get; set; }

    public string? ImagemUrl { get; set; }
}

/// <summary>Atualização do placar/encerramento de um evento.</summary>
public class AtualizarResultadoDto
{
    [Range(0, 999)]
    public int? PlacarCasa { get; set; }

    [Range(0, 999)]
    public int? PlacarVisitante { get; set; }

    public StatusEvento Status { get; set; } = StatusEvento.Encerrado;
}
