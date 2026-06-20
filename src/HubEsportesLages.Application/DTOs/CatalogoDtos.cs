namespace HubEsportesLages.Application.DTOs;

/// <summary>Modalidade esportiva com a contagem de eventos futuros (para os filtros).</summary>
public record ModalidadeDto(
    int Id,
    string Nome,
    string Slug,
    string Icone,
    string CorHex,
    string Descricao,
    int EventosFuturos);

/// <summary>Local/equipamento esportivo.</summary>
public record LocalDto(
    int Id,
    string Nome,
    string Endereco,
    string Bairro,
    string Cidade,
    int? Capacidade,
    string MapaUrl);

/// <summary>Equipe/associação esportiva.</summary>
public record EquipeDto(
    int Id,
    string Nome,
    string Sigla,
    string Escudo,
    string CorPrimaria,
    int ModalidadeId,
    string ModalidadeNome);
