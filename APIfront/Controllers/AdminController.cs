using APIfront.Dtos;
using APIfront.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIfront.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles="Admin")]
    public class AdminController : ControllerBase
    {

        private readonly MyContext _context;

        public AdminController(MyContext context)
        {
            _context= context;
        }


        //*************** Get All Users *************************

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            // Récupérer tous les utilisateurs
            var users = await _context.Users
                                      .Select(u => new
                                      {
                                          u.Id,
                                          u.Username,
                                          u.Role
                                      }).ToListAsync();
                                     

            return Ok(users);
        }

        //******* SUPRIMER UN UTILISATEUR ET SES TACHES 

        [Authorize(Roles = "Admin")]
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // Rechercher l'utilisateur dans la base de données
            var user = await _context.Users
                                     .Include(u => u.Tasks) // Inclure les tâches associées
                                     .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new { Message = "Utilisateur non trouvé." });
            }

            // Supprimer les tâches associées
            _context.Tasks.RemoveRange(user.Tasks);

            // Supprimer l'utilisateur
            _context.Users.Remove(user);

            // Sauvegarder les modifications
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Utilisateur et ses tâches supprimés avec succès." });
        }


        //************* VOIRE LES TACHES DE CHAQUE UTILISATEUR 

        [HttpGet("user/{userId}/tasks")]
        public async Task<IActionResult> GetTasksUser(int userId)
        {
            // Vérifier si l'utilisateur existe
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { Message = "Utilisateur introuvable." });
            }

            // Récupérer les tâches de l'utilisateur
            var tasks = await _context.Tasks
                                      .Where(t => t.UserId == userId)
                                      .Select(t => new TaskDtoForAdmin
                                      {
                                          Id = t.Id,
                                          Title = t.Title,
                                          Description = t.Description,
                                          DateCreation = t.DateCreation,
                                          Etat = t.Etat.ToString() // Convertir l'enum EtatTask en string
                                      })
                                      .ToListAsync();

            return Ok(tasks);
        }



    }
}
