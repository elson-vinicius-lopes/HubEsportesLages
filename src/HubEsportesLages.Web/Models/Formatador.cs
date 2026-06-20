using System.Globalization;
using HubEsportesLages.Domain.Enums;

namespace HubEsportesLages.Web.Models;

/// <summary>Funções de apresentação reutilizadas pelas views (status, datas, notificações).</summary>
public static class Formatador
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static string StatusTexto(StatusEvento s) => s switch
    {
        StatusEvento.Agendado => "Agendado",
        StatusEvento.AoVivo => "Ao vivo",
        StatusEvento.Encerrado => "Encerrado",
        StatusEvento.Adiado => "Adiado",
        StatusEvento.Cancelado => "Cancelado",
        _ => s.ToString()
    };

    public static string StatusClasse(StatusEvento s) => s switch
    {
        StatusEvento.Agendado => "badge--agendado",
        StatusEvento.AoVivo => "badge--aovivo",
        StatusEvento.Encerrado => "badge--encerrado",
        StatusEvento.Adiado => "badge--adiado",
        StatusEvento.Cancelado => "badge--cancelado",
        _ => "badge--agendado"
    };

    public static string TipoTexto(TipoNotificacao t) => t switch
    {
        TipoNotificacao.NovoEvento => "Novo evento",
        TipoNotificacao.Lembrete => "Lembrete",
        TipoNotificacao.AlteracaoHorario => "Alteração",
        TipoNotificacao.Resultado => "Resultado",
        TipoNotificacao.Cancelamento => "Cancelamento",
        _ => t.ToString()
    };

    public static string TipoIcone(TipoNotificacao t) => t switch
    {
        TipoNotificacao.NovoEvento => "[Evento]",
        TipoNotificacao.Lembrete => "[Lembrete]",
        TipoNotificacao.AlteracaoHorario => "[Alteração]",
        TipoNotificacao.Resultado => "[Resultado]",
        TipoNotificacao.Cancelamento => "[Cancelamento]",
        _ => "[Notif.]"
    };

    public static string TipoCor(TipoNotificacao t) => t switch
    {
        TipoNotificacao.NovoEvento => "#1d4e89",
        TipoNotificacao.Lembrete => "#f59e0b",
        TipoNotificacao.AlteracaoHorario => "#7c3aed",
        TipoNotificacao.Resultado => "#16a34a",
        TipoNotificacao.Cancelamento => "#dc2626",
        _ => "#64748b"
    };

    public static string TipoClasse(TipoNotificacao t) => "feed-item--" + t.ToString().ToLowerInvariant();

    public static string DataLonga(DateTime d) => d.ToString("dddd, dd 'de' MMMM 'às' HH'h'mm", PtBr);

    public static string DataCurta(DateTime d) => d.ToString("ddd, dd/MM", PtBr);

    public static string Hora(DateTime d) => d.ToString("HH'h'mm", PtBr);

    public static string DiaMes(DateTime d) => d.ToString("dd/MM", PtBr);

    public static string Moeda(decimal v) => v.ToString("C", PtBr);

    /// <summary>Tempo relativo amigável ("há 2h", "há 3 dias").</summary>
    public static string TempoRelativo(DateTime d)
    {
        var diff = DateTime.Now - d;
        if (diff.TotalMinutes < 1) return "agora";
        if (diff.TotalMinutes < 60) return $"há {(int)diff.TotalMinutes} min";
        if (diff.TotalHours < 24) return $"há {(int)diff.TotalHours}h";
        if (diff.TotalDays < 30) return $"há {(int)diff.TotalDays}d";
        return d.ToString("dd/MM/yyyy", PtBr);
    }
}
