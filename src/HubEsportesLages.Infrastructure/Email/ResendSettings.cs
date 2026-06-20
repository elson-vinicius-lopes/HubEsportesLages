namespace HubEsportesLages.Infrastructure.Email;

/// <summary>Configurações do Resend lidas da seção "Resend" do appsettings.</summary>
public class ResendSettings
{
    public const string SectionName = "Resend";

    public string ApiKey { get; set; } = string.Empty;
    public string RemetenteEmail { get; set; } = "onboarding@resend.dev";
    public string RemetenteNome { get; set; } = "Bora pro Jogo";
    public string DestinatarioPadrao { get; set; } = string.Empty;
}
