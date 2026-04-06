using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManager.Api.Models;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<AppUser> userManager,
        IConfiguration config,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _config = config;
        _logger = logger;
    }

    // ╔══════════════════════════════════════════════════════════════╗
    // ║  ETAPA 1 — Implementem os endpoints Register e Login       ║
    // ╚══════════════════════════════════════════════════════════════╝

    /// <summary>
    /// POST /api/auth/register
    /// Cria um novo usuário com role "User".
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // TODO 1.1: Criar uma instância de AppUser com os dados do request.
        //   - UserName = request.Email
        //   - Email    = request.Email
        //   - FullName = request.FullName

        // TODO 1.2: Chamar _userManager.CreateAsync(user, request.Password)
        //   - Se falhar, retornar BadRequest com os erros do Identity:
        //     return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        // TODO 1.3: Atribuir a role "User" ao usuário criado:
        //   await _userManager.AddToRoleAsync(user, "User");

        // TODO 1.4: Logar o registro e retornar Created:
        //   return Created("", new { user.Id, user.Email, user.FullName, role = "User" });

        throw new NotImplementedException("Implementem o Register na Etapa 1");
    }

    /// <summary>
    /// POST /api/auth/login
    /// Valida credenciais e retorna um JWT.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // TODO 1.5: Buscar o usuário pelo email:
        //   var user = await _userManager.FindByEmailAsync(request.Email);

        // TODO 1.6: Validar a senha com Identity:
        //   await _userManager.CheckPasswordAsync(user, request.Password)
        //   Se user == null OU senha inválida, retornar:
        //   return Unauthorized(new { message = "Credenciais inválidas." });

        // TODO 1.7: Buscar as roles do usuário:
        //   var roles = await _userManager.GetRolesAsync(user);

        // TODO 1.8: Gerar o token JWT chamando o método GerarToken (já implementado abaixo):
        //   var token = GerarToken(user, roles);
        //   return Ok(new { token });

        throw new NotImplementedException("Implementem o Login na Etapa 1");
    }

    // ╔══════════════════════════════════════════════════════════════╗
    // ║  ETAPA 3 — Adicionem o endpoint RegisterAdmin              ║
    // ╚══════════════════════════════════════════════════════════════╝

    /// <summary>
    /// POST /api/auth/register-admin
    /// Cria um novo usuário com role "Admin".
    /// Em produção, este endpoint seria protegido com [Authorize(Roles = "Admin")].
    /// </summary>
    [HttpPost("register-admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterRequest request)
    {
        // TODO 3.1: Reaproveitem a lógica do Register, mas atribuam a role "Admin"
        //   em vez de "User". Dica: é quase idêntico ao Register — só muda a role.

        throw new NotImplementedException("Implementem o RegisterAdmin na Etapa 3");
    }

    // ═══════════════════════════════════════════════════════════════
    //  Método auxiliar para gerar JWT — JÁ IMPLEMENTADO
    //  Vocês vão chamar este método no Login (TODO 1.8).
    // ═══════════════════════════════════════════════════════════════

    private string GerarToken(AppUser user, IList<string> roles)
    {
        var key = _config["Jwt:Key"]!;
        var issuer = _config["Jwt:Issuer"];
        var audience = _config["Jwt:Audience"];
        var expiresIn = int.Parse(_config["Jwt:ExpiresInMinutes"] ?? "60");

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new Claim("fullName", user.FullName)
        };

        // Adiciona uma claim por role — isso permite [Authorize(Roles = "...")]
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expiresIn),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
