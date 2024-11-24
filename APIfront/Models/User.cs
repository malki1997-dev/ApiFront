namespace APIfront.Models
{
    public class User
    {

        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // "Admin" ou "User"

        // Relation avec les tâches
        public ICollection<Task> Tasks { get; set; }


    }
}
