using APIfront.Enums;

namespace APIfront.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        // Utilisation de l'énumération EtatTask
        public EtatTask Etat { get; set; }

        // Relation avec User
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
