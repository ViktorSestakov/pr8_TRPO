namespace pr8_TRPO.Models;

// Таблица Tasks в MS SQL Server (см. Database-First в README).
public partial class Task
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string OwnerLogin { get; set; } = null!;
    public bool IsCompleted { get; set; }
}
