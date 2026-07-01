namespace HubEsportesLages.Application.DTOs;

/// <summary>Dados cadastrais da conta incluídos no export LGPD (direito de acesso).</summary>
public record ContaTitularDto(
    string? Nome,
    string Email,
    DateTime? ConsentimentoLgpdEm,
    string? ConsentimentoVersao);

/// <summary>
/// Pacote completo do export "Meus dados" (LGPD — acesso/portabilidade), baixado em JSON
/// pelo titular em GET /conta/meus-dados. As interações de torcida (votos, mural, favoritos)
/// não entram: usam identificador anônimo do navegador, não vinculável à conta.
/// </summary>
public record MeusDadosDto(
    DateTime GeradoEm,
    ContaTitularDto Conta,
    IReadOnlyList<InscricaoDto> InscricoesAlerta,
    IReadOnlyList<IngressoDto> Ingressos,
    string ObservacaoTorcida);

/// <summary>Resultado da exclusão dos dados vinculados ao titular (LGPD — direito de exclusão).</summary>
public record ExclusaoDadosDto(int InscricoesRemovidas, int IngressosAnonimizados);
