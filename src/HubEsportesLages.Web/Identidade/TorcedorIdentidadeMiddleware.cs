namespace HubEsportesLages.Web.Identidade;

/// <summary>
/// Resolve a identidade anônima do torcedor uma vez por requisição a partir do cabeçalho
/// <c>X-Torcedor-Id</c> (GUID persistido pelo app) e o guarda em <see cref="HttpContext.Items"/>
/// para o <see cref="TorcedorContexto"/> ler. Quando o cabeçalho está ausente, NÃO fabrica
/// identidade: as escritas são recusadas (400) pelos serviços e as leituras seguem sem
/// personalização. Fabricar um id por requisição permitiria voto duplicado contornando o
/// índice único (1 voto por torcedor).
/// </summary>
public class TorcedorIdentidadeMiddleware(RequestDelegate next)
{
    /// <summary>Chave usada em <see cref="HttpContext.Items"/> para guardar o id do torcedor.</summary>
    public const string ItemKey = "TorcedorId";

    public async Task InvokeAsync(HttpContext context)
    {
        var cabecalho = context.Request.Headers[TorcedorContexto.Header].ToString();
        if (!string.IsNullOrWhiteSpace(cabecalho))
        {
            var torcedorId = cabecalho.Trim();
            context.Items[ItemKey] = torcedorId;
            // Eco do id recebido para o app confirmar/reutilizar nas próximas requisições.
            context.Response.Headers[TorcedorContexto.Header] = torcedorId;
        }

        await next(context);
    }
}
