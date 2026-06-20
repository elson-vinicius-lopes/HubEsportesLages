using HubEsportesLages.Application.Interfaces;

namespace HubEsportesLages.Web.Identidade;

/// <summary>
/// Identidade anônima do torcedor por dispositivo. O id é resolvido por
/// <see cref="TorcedorIdentidadeMiddleware"/> (cabeçalho <c>X-Torcedor-Id</c> ou GUID
/// efêmero) e guardado em <see cref="HttpContext.Items"/>. É o fallback até existir
/// autenticação real (lacuna #3 do design).
/// </summary>
public class TorcedorContexto(IHttpContextAccessor accessor) : ITorcedorContexto
{
    public const string Header = "X-Torcedor-Id";

    public string? TorcedorId
    {
        get
        {
            var contexto = accessor.HttpContext;
            if (contexto is null)
                return null;

            // Preenchido pelo middleware; recai sobre o cabeçalho se ele não tiver rodado.
            if (contexto.Items.TryGetValue(TorcedorIdentidadeMiddleware.ItemKey, out var valor)
                && valor is string id && !string.IsNullOrWhiteSpace(id))
                return id;

            var cabecalho = contexto.Request.Headers[Header].ToString();
            return string.IsNullOrWhiteSpace(cabecalho) ? null : cabecalho.Trim();
        }
    }
}
