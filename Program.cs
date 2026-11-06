using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using pr8_TRPO.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// В отличие от учебного примера, по заданию пользователи и задачи в MS SQL Server.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new Exception("Не задана строка подключения DefaultConnection");

builder.Services.AddDbContext<Pr8Context>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

// JWT, как в примере из методички.
var key = builder.Configuration["Jwt:Key"]
    ?? throw new Exception("Не задан секретный ключ JWT");
if (Encoding.UTF8.GetByteCount(key) < 32)
    throw new Exception("Ключ JWT слишком короткий");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "ExampleApi",
            ValidateAudience = true,
            ValidAudience = "ExampleApiClient",
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// По заданию администратор должен существовать в системе.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Pr8Context>();
    if (!await db.Users.AnyAsync(u => u.Login == "admin123"))
    {
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var admin = new User { Login = "admin123", Role = "Admin" };
        admin.PasswordHash = hasher.HashPassword(admin, "P@s$w0rd");
        db.Users.Add(admin);
        await db.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
