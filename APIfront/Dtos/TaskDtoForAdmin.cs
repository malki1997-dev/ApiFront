namespace APIfront.Dtos
{
    public class TaskDtoForAdmin
    {

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateCreation { get; set; }
        public string Etat { get; set; } // Converti l'énumération en chaîne lisible

    }
}
