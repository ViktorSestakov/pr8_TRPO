namespace pr8_TRPO.Models;

// Таблица Users в MS SQL Server (см. Database-First в README).
public partial class User
{
    public int Id { get; set; }
    public string Login { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
}
