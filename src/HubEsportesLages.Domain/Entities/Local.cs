namespace HubEsportesLages.Domain.Entities;

/// <summary>Local/equipamento esportivo onde os eventos acontecem (ginásios, estádios, quadras).</summary>
public class Local
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Endereco { get; set; } = string.Empty;

    public string Bairro { get; set; } = string.Empty;

    public string Cidade { get; set; } = "Lages";

    public string Uf { get; set; } = "SC";

    public int? Capacidade { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public ICollection<Evento> Eventos { get; set; } = new List<Evento>();

    /// <summary>Link pronto para o Google Maps a partir das coordenadas (ou do endereço).</summary>
    public string MapaUrl =>
        Latitude.HasValue && Longitude.HasValue
            ? $"https://www.google.com/maps/search/?api=1&query={Latitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)},{Longitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}"
            : $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString($"{Nome}, {Cidade} - {Uf}")}";
}
