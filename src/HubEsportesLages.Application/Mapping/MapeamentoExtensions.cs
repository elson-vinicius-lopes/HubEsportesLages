using HubEsportesLages.Application.DTOs;
using HubEsportesLages.Domain.Entities;

namespace HubEsportesLages.Application.Mapping;

/// <summary>
/// Conversões de entidades de domínio para DTOs. Assume que as navegações
/// necessárias (Modalidade, Local, Equipes) já foram carregadas via Include.
/// </summary>
public static class MapeamentoExtensions
{
    public static EventoResumoDto ParaResumo(this Evento e) => new(
        e.Id,
        e.Titulo,
        e.Slug,
        e.Campeonato,
        e.Modalidade?.Nome ?? string.Empty,
        e.Modalidade?.Icone ?? string.Empty,
        e.Modalidade?.CorHex ?? "#1f6feb",
        e.Local?.Nome ?? string.Empty,
        e.Local?.Bairro ?? string.Empty,
        e.Inicio,
        e.Status,
        e.EhConfronto,
        e.EquipeCasa?.Nome,
        e.EquipeCasa?.Escudo,
        e.EquipeVisitante?.Nome,
        e.EquipeVisitante?.Escudo,
        e.Placar,
        e.Gratuito,
        e.PrecoIngresso,
        e.ImagemUrl,
        e.Destaque);

    public static EventoDetalheDto ParaDetalhe(this Evento e) => new(
        e.Id,
        e.Titulo,
        e.Slug,
        e.Descricao,
        e.Campeonato,
        e.ModalidadeId,
        e.Modalidade?.Nome ?? string.Empty,
        e.Modalidade?.Icone ?? string.Empty,
        e.Modalidade?.CorHex ?? "#1f6feb",
        e.Local?.Nome ?? string.Empty,
        e.Local?.Endereco ?? string.Empty,
        e.Local?.Bairro ?? string.Empty,
        e.Local?.Capacidade,
        e.Local?.MapaUrl ?? string.Empty,
        e.Inicio,
        e.Fim,
        e.Status,
        e.EhConfronto,
        e.EquipeCasa?.Nome,
        e.EquipeCasa?.Escudo,
        e.EquipeVisitante?.Nome,
        e.EquipeVisitante?.Escudo,
        e.EquipeCasaId,
        e.EquipeVisitanteId,
        e.PlacarCasa,
        e.PlacarVisitante,
        e.Placar,
        e.Gratuito,
        e.PrecoIngresso,
        e.ImagemUrl);

    public static LocalDto ParaDto(this Local l) => new(
        l.Id, l.Nome, l.Endereco, l.Bairro, l.Cidade, l.Capacidade, l.MapaUrl);

    public static EquipeDto ParaDto(this Equipe eq) => new(
        eq.Id, eq.Nome, eq.Sigla, eq.Escudo, eq.CorPrimaria, eq.ModalidadeId, eq.Modalidade?.Nome ?? string.Empty);

    public static InscricaoDto ParaDto(this Inscricao i) => new(
        i.Id, i.Nome, i.Email, i.Modalidade?.Nome, i.Equipe?.Nome, i.ReceberEmail, i.ReceberPush, i.CriadoEm);

    public static NotificacaoDto ParaDto(this Notificacao n) => new(
        n.Id,
        n.Titulo,
        n.Mensagem,
        n.Tipo,
        n.Importante,
        n.Lida,
        n.CriadoEm,
        n.EventoId,
        n.Evento?.Slug,
        n.Modalidade?.Icone,
        n.Modalidade?.CorHex);

    public static MensagemDto ParaDto(this MensagemTorcida m) => new(
        m.Id, m.Autor, m.Texto, m.CriadoEm);

    public static IngressoDto ParaDto(this Ingresso i) => new(
        i.Id,
        i.Evento?.Titulo ?? string.Empty,
        i.Evento?.Slug ?? string.Empty,
        i.Preco,
        i.Status,
        i.CriadoEm,
        i.PagoEm);
}
