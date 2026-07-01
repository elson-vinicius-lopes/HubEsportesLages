using Microsoft.AspNetCore.Identity;

namespace HubEsportesLages.Infrastructure.Identidade;

/// <summary>
/// Usuário da aplicação persistido pelo ASP.NET Core Identity (tabela AspNetUsers).
/// Estende o <see cref="IdentityUser"/> padrão com os campos extras do Hub.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>Nome completo do torcedor/organizador (opcional). Usado em saudações e e-mails.</summary>
    public string? NomeCompleto { get; set; }

    /// <summary>Quando (UTC) o titular aceitou a Política de Privacidade no cadastro (LGPD).</summary>
    public DateTime? ConsentimentoLgpdEm { get; set; }

    /// <summary>Versão da Política de Privacidade aceita (ex.: "v1"). Ver LgpdConstantes.</summary>
    public string? ConsentimentoVersao { get; set; }
}
