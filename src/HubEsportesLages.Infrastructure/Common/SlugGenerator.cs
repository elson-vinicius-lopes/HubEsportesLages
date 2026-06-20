using System.Globalization;
using System.Text;

namespace HubEsportesLages.Infrastructure.Common;

/// <summary>Gera slugs amigáveis para URL a partir de um texto (remove acentos e símbolos).</summary>
public static class SlugGenerator
{
    public static string Gerar(string texto, int? sufixoUnico = null)
    {
        if (string.IsNullOrWhiteSpace(texto))
            texto = "evento";

        var normalizado = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalizado.Length);

        foreach (var c in normalizado)
        {
            var categoria = CharUnicodeInfo.GetUnicodeCategory(c);
            if (categoria == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(c))
                sb.Append(char.ToLowerInvariant(c));
            else if (char.IsWhiteSpace(c) || c is '-' or '_' or '/')
                sb.Append('-');
        }

        var slug = sb.ToString();
        while (slug.Contains("--"))
            slug = slug.Replace("--", "-");
        slug = slug.Trim('-');

        if (string.IsNullOrEmpty(slug))
            slug = "evento";

        return sufixoUnico.HasValue ? $"{slug}-{sufixoUnico}" : slug;
    }
}
