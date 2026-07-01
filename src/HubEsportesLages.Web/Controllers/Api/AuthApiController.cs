using HubEsportesLages.Application.Common;
using HubEsportesLages.Infrastructure.Identidade;
using HubEsportesLages.Web.Identidade;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HubEsportesLages.Web.Controllers.Api;

/// <summary>
/// Autenticação da API REST (app mobile Arena Lages): troca e-mail/senha por um token JWT Bearer.
/// O site MVC continua usando o cookie do ASP.NET Identity; este endpoint só emite tokens.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
[Tags("Autenticação")]
public class AuthApiController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    JwtSettings jwtSettings) : ControllerBase
{
    /// <summary>
    /// Autentica por e-mail e senha e devolve um token JWT Bearer com as roles do usuário.
    /// Respeita o lockout do Identity (bloqueio por tentativas). Use o token no cabeçalho
    /// <c>Authorization: Bearer &lt;token&gt;</c> para acessar os endpoints protegidos.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginRespostaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginRespostaDto>> Login([FromBody] LoginRequisicaoDto dto, CancellationToken ct)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Senha))
            return Unauthorized(new { mensagem = "E-mail ou senha incorretos." });

        // Aceita login por e-mail ou por nome de usuário (paridade com o site).
        var alvo = dto.Email.Trim();
        var usuario = await userManager.FindByEmailAsync(alvo)
                      ?? await userManager.FindByNameAsync(alvo);

        if (usuario is null)
            return Unauthorized(new { mensagem = "E-mail ou senha incorretos." });

        // CheckPasswordSignInAsync respeita o lockout configurado no Identity.
        var resultado = await signInManager.CheckPasswordSignInAsync(usuario, dto.Senha, lockoutOnFailure: true);

        if (resultado.IsLockedOut)
            return Unauthorized(new { mensagem = "Conta temporariamente bloqueada por excesso de tentativas. Tente novamente em alguns minutos." });

        if (!resultado.Succeeded)
            return Unauthorized(new { mensagem = "E-mail ou senha incorretos." });

        var roles = await userManager.GetRolesAsync(usuario);
        var (token, expiraEm) = GerarToken(usuario, roles);

        var nome = string.IsNullOrWhiteSpace(usuario.NomeCompleto)
            ? usuario.UserName ?? usuario.Email ?? alvo
            : usuario.NomeCompleto;

        return Ok(new LoginRespostaDto(token, expiraEm, nome, roles.ToArray()));
    }

    /// <summary>
    /// Registra uma nova conta de torcedor (espelha o registro do site) e já devolve o token.
    /// A política de senha forte do Identity é validada aqui. Exige o aceite da Política de
    /// Privacidade (LGPD): <c>aceitePrivacidade: true</c> — a política está em /privacidade.
    /// </summary>
    [HttpPost("registrar")]
    [ProducesResponseType(typeof(LoginRespostaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginRespostaDto>> Registrar([FromBody] RegistrarRequisicaoDto dto, CancellationToken ct)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Senha))
            return BadRequest(new { mensagem = "Informe e-mail e senha." });

        // LGPD: sem o aceite explícito da Política de Privacidade não há cadastro.
        if (!dto.AceitePrivacidade)
            return BadRequest(new { mensagem = "É preciso aceitar a Política de Privacidade (/privacidade) para criar a conta." });

        var usuario = new ApplicationUser
        {
            UserName = dto.Email.Trim(),
            Email = dto.Email.Trim(),
            NomeCompleto = string.IsNullOrWhiteSpace(dto.Nome) ? null : dto.Nome.Trim(),
            // Registro do consentimento LGPD (momento UTC + versão da política aceita).
            ConsentimentoLgpdEm = DateTime.UtcNow,
            ConsentimentoVersao = LgpdConstantes.VersaoPoliticaAtual
        };

        var criado = await userManager.CreateAsync(usuario, dto.Senha);
        if (!criado.Succeeded)
            return BadRequest(new { mensagem = "Não foi possível criar a conta.", erros = criado.Errors.Select(e => e.Description) });

        // Todo cadastro novo entra como Torcedor.
        await userManager.AddToRoleAsync(usuario, "Torcedor");

        var roles = await userManager.GetRolesAsync(usuario);
        var (token, expiraEm) = GerarToken(usuario, roles);

        var nome = string.IsNullOrWhiteSpace(usuario.NomeCompleto)
            ? usuario.UserName!
            : usuario.NomeCompleto;

        return Ok(new LoginRespostaDto(token, expiraEm, nome, roles.ToArray()));
    }

    /// <summary>Monta e assina o JWT com sub/email/name + as roles do usuário (HMAC-SHA256).</summary>
    private (string token, DateTime expiraEm) GerarToken(ApplicationUser usuario, IList<string> roles)
    {
        var expiraEm = DateTime.UtcNow.AddMinutes(jwtSettings.ExpiraMinutos);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, usuario.Id),
            new(ClaimTypes.Name, usuario.NomeCompleto ?? usuario.UserName ?? usuario.Email ?? string.Empty)
        };

        // As roles alimentam o [Authorize(Roles="Admin")] existente.
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiraEm,
            signingCredentials: credenciais);

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return (token, expiraEm);
    }
}

/// <summary>Credenciais de login da API (JSON camelCase).</summary>
public record LoginRequisicaoDto(string Email, string Senha);

/// <summary>
/// Dados de registro de um novo torcedor pela API (JSON camelCase).
/// <paramref name="AceitePrivacidade"/> deve ser <c>true</c> (aceite da Política de Privacidade — LGPD).
/// </summary>
public record RegistrarRequisicaoDto(string Nome, string Email, string Senha, bool AceitePrivacidade);

/// <summary>Resposta com o token JWT emitido e os dados básicos do usuário.</summary>
public record LoginRespostaDto(string Token, DateTime ExpiraEm, string Nome, IReadOnlyList<string> Roles);
