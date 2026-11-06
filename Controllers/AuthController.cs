using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using pr8_TRPO.Models;

namespace pr8_TRPO.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly Pr8Context _db;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IConfiguration _configuration;

    public AuthController(Pr8Context db, IPasswordHasher<User> hasher,
        IConfiguration configuration)
    {
        _db = db;
        _hasher = hasher;
        _configuration = configuration;
    }

    public record AuthRequest(string Login, string Password);

    [HttpPost("register")]
    public async System.Threading.Tasks.Task<IActionResult> Register(AuthRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Login) ||
            string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Укажите логин и пароль");

        if (await _db.Users.AnyAsync(u => u.Login == request.Login))
            return Conflict("Логин уже существует");

        var user = new User { Login = request.Login, Role = "User" };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok("Пользователь зарегистрирован");
    }

    [HttpPost("login")]
    public async System.Threading.Tasks.Task<IActionResult> Login(AuthRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Login) || request.Password is null)
            return Unauthorized("Неверная пара логин/пароль");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Login == request.Login);
        if (user == null)
            return Unauthorized("Неверная пара логин/пароль");

        var result = _hasher.VerifyHashedPassword(
            user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized("Неверная пара логин/пароль");

        return Ok(new { access_token = GenerateJwtToken(user) });
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "ExampleApi",
            audience: "ExampleApiClient",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
