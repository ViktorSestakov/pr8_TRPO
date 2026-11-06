using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pr8_TRPO.Models;
using DbTask = pr8_TRPO.Models.Task;

namespace pr8_TRPO.Controllers;

[ApiController]
[Route("api/v1/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly Pr8Context _db;

    public TasksController(Pr8Context db)
    {
        _db = db;
    }

    public record TaskRequest(string Title, string Description);
    public record UpdateTaskRequest(string Title, string Description, bool IsCompleted);

    // Собственные задачи (для User и Admin).
    [HttpGet]
    public async System.Threading.Tasks.Task<IActionResult> Index()
    {
        var tasks = await _db.Tasks
            .Where(t => t.OwnerLogin == User.Identity!.Name)
            .ToListAsync();
        return Ok(tasks);
    }

    // Все задачи доступны только администратору.
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async System.Threading.Tasks.Task<IActionResult> All()
    {
        return Ok(await _db.Tasks.ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async System.Threading.Tasks.Task<IActionResult> GetById(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null)
            return NotFound();
        if (!CanEdit(task))
            return Forbid();

        return Ok(task);
    }

    [HttpPost]
    public async System.Threading.Tasks.Task<IActionResult> Create(TaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Description == null)
            return BadRequest("Название и описание обязательны");
        if (request.Title.Length > 200 || request.Description.Length > 1000)
            return BadRequest("Превышена длина названия или описания");

        var task = new DbTask
        {
            Title = request.Title,
            Description = request.Description,
            OwnerLogin = User.Identity!.Name!,
            IsCompleted = false
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id:int}")]
    public async System.Threading.Tasks.Task<IActionResult> Update(int id, UpdateTaskRequest request)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null)
            return NotFound();
        if (!CanEdit(task))
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.Title) || request.Description == null)
            return BadRequest("Название и описание обязательны");
        if (request.Title.Length > 200 || request.Description.Length > 1000)
            return BadRequest("Превышена длина названия или описания");

        task.Title = request.Title;
        task.Description = request.Description;
        task.IsCompleted = request.IsCompleted;
        await _db.SaveChangesAsync();
        return Ok(task);
    }

    [HttpDelete("{id:int}")]
    public async System.Threading.Tasks.Task<IActionResult> Delete(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null)
            return NotFound();
        if (!CanEdit(task))
            return Forbid();

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private bool CanEdit(DbTask task)
    {
        return task.OwnerLogin == User.Identity?.Name || User.IsInRole("Admin");
    }
}
