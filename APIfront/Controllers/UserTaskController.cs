using APIfront.Dtos;
using APIfront.Enums;
using APIfront.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

using TaskModel = APIfront.Models.Task;


namespace APIfront.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles="User")]
    public class UserTaskController : ControllerBase
    {

        private readonly MyContext _context;

        public UserTaskController(MyContext context)
        {
            _context = context;
        }

        [HttpGet("tasks")]
        public async Task<IActionResult> GetAllTasks()
        {
            // Récupérer l'ID de l'utilisateur connecté
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized("Utilisateur non authentifié ou ID utilisateur invalide.");
            }

            // Filtrer les tâches de l'utilisateur connecté
            var userTasks = await _context.Tasks
                .Where(t => t.UserId == userId)
                .ToListAsync();

            // Retourner les tâches
            return Ok(userTasks);
        }

        //*************** Create nouveau Task 

        [HttpPost("CreateTask")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto taskDto)
        {
            // Récupérer l'utilisateur connecté
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized("Utilisateur non authentifié ou ID utilisateur invalide.");
            }

            // Valider les données d'entrée
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Mapper le DTO en modèle
            var task = new TaskModel
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                Etat = EtatTask.Todo, // Par défaut, une nouvelle tâche est en "Todo"
                UserId = userId
            };

            // Ajouter la tâche à la base de données
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            // Retourner une réponse
            return Ok(new { Message = "Tâche créée avec succès.", TaskId = task.Id });
        }


        //************** GetTaskParId 

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            // Vérifier si l'utilisateur est autorisé à accéder à cette tâche
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Récupérer l'ID de l'utilisateur depuis le token

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Message = "Utilisateur non authentifié." });
            }

            // Rechercher la tâche par ID et vérifier si elle existe
            var task = await _context.Tasks
                                     .Where(t => t.Id == id && t.UserId.ToString() == userId)
                                     .FirstOrDefaultAsync();

            if (task == null)
            {
                return NotFound(new { Message = "Tâche non trouvée." });
            }

            // Mapper la tâche en DTO pour ne pas exposer directement les données internes
            var taskDto = new TaskDtoForUser
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DateCreation = task.DateCreation,
                Etat = task.Etat
            };

            return Ok(taskDto);
        }

        //************ UpdateTask *********************

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTask taskDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Vérification de l'utilisateur authentifié
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Message = "Utilisateur non authentifié." });
            }

            // Récupérer la tâche à mettre à jour
            var task = await _context.Tasks
                                     .Where(t => t.Id == id && t.UserId.ToString() == userId)
                                     .FirstOrDefaultAsync();

            // Si la tâche n'est pas trouvée ou l'utilisateur n'a pas les droits
            if (task == null)
            {
                return NotFound(new { Message = "Tâche non trouvée ou vous n'avez pas l'autorisation de la modifier." });
            }

            // Mise à jour des informations de la tâche
            task.Title = taskDto.Title;
            task.Description = taskDto.Description;

            // Mise à jour de l'état
            if (Enum.IsDefined(typeof(EtatTask), taskDto.Etat)) // Optionnel si vous êtes certain que la valeur est valide
            {
                task.Etat = taskDto.Etat;  // Affectation directe de l'énumération
            }
            else
            {
                return BadRequest(new { Message = "L'état spécifié est invalide." });
            }

            // Sauvegarder les changements dans la base de données
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();

            // Retourner la tâche mise à jour
            return Ok(task);
        }

        //********** Suprimer une Tache 

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            // Récupérer l'ID de l'utilisateur authentifié
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Message = "Utilisateur non authentifié." });
            }

            // Rechercher la tâche à supprimer
            var task = await _context.Tasks
                                     .Where(t => t.Id == id && t.UserId.ToString() == userId)
                                     .FirstOrDefaultAsync();

            // Vérifier si la tâche existe et appartient à l'utilisateur
            if (task == null)
            {
                return NotFound(new { Message = "Tâche non trouvée ou vous n'avez pas l'autorisation de la supprimer." });
            }

            // Supprimer la tâche
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Tâche supprimée avec succès." });
        }
    }
}
