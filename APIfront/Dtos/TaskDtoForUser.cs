using APIfront.Enums;

namespace APIfront.Dtos
{
    public class TaskDtoForUser
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateCreation { get; set; }
        public EtatTask Etat { get; set; } // Utiliser l'énum EtatTask
    }
}
