namespace HubEsportesLages.Application.Common;

/// <summary>Resultado paginado genérico usado pela agenda e pelos resultados.</summary>
public record PagedResult<T>(
    IReadOnlyList<T> Itens,
    int Pagina,
    int TamanhoPagina,
    int Total)
{
    public int TotalPaginas => TamanhoPagina <= 0 ? 0 : (int)Math.Ceiling(Total / (double)TamanhoPagina);
    public bool TemAnterior => Pagina > 1;
    public bool TemProxima => Pagina < TotalPaginas;
}
