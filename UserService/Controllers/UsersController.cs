using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(UserContext context, ILogger<UsersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtenir tous les utilisateurs
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        _logger.LogInformation("Récupération de tous les utilisateurs");
        return await _context.Users.ToListAsync();
    }

    /// <summary>
    /// Obtenir un utilisateur par son ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        _logger.LogInformation("Récupération de l'utilisateur avec ID: {UserId}", id);

        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            _logger.LogWarning("Utilisateur avec ID {UserId} non trouvé", id);
            return NotFound(new { message = $"Utilisateur avec ID {id} non trouvé" });
        }

        return user;
    }

    /// <summary>
    /// Créer un nouvel utilisateur
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(User user)
    {
        _logger.LogInformation("Création d'un nouvel utilisateur: {UserEmail}", user.Email);

        user.CreatedAt = DateTime.UtcNow;
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    /// <summary>
    /// Mettre à jour un utilisateur existant
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, User user)
    {
        if (id != user.Id)
        {
            return BadRequest(new { message = "L'ID ne correspond pas" });
        }

        _logger.LogInformation("Mise à jour de l'utilisateur avec ID: {UserId}", id);

        _context.Entry(user).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await UserExists(id))
            {
                return NotFound(new { message = $"Utilisateur avec ID {id} non trouvé" });
            }
            throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Supprimer un utilisateur
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        _logger.LogInformation("Suppression de l'utilisateur avec ID: {UserId}", id);

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { message = $"Utilisateur avec ID {id} non trouvé" });
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> UserExists(int id)
    {
        return await _context.Users.AnyAsync(e => e.Id == id);
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "UserService", timestamp = DateTime.UtcNow });
    }
}
