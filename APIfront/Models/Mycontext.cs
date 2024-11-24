using APIfront.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace APIfront.Models
{
    public class MyContext:DbContext
    {


        public DbSet<User> Users { get; set; }
        public DbSet<Task> Tasks { get; set; }

        public MyContext(DbContextOptions<MyContext> options) : base(options) 
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Convertir l'énumération 'EtatTask' en chaîne
            modelBuilder.Entity<Task>()
                .Property(t => t.Etat)
                .HasConversion(
                    v => v.ToString(), // Convertir l'enum en chaîne pour la base de données
                    v => (EtatTask)Enum.Parse(typeof(EtatTask), v) // Convertir la chaîne en enum en lecture
                );
        }


    }
}
