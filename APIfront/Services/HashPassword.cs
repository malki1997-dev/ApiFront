using System.Security.Cryptography;
using System.Text;
using APIfront.Interfaces;

namespace APIfront.Services
{
    public class HashPassword : IPasswordHasher
    {
        public string HashPassworde(string password) // Implémente l'interface correctement
        {
            using (var sha256 = SHA256.Create()) // Instancie SHA256
            {
                var bytes = Encoding.UTF8.GetBytes(password); // Convertit en tableau d'octets
                var hash = sha256.ComputeHash(bytes); // Hache le tableau

                var stringBuilder = new StringBuilder();
                foreach (var b in hash)
                {
                    stringBuilder.Append(b.ToString("x2")); // Convertit en chaîne hexadécimale
                }

                return stringBuilder.ToString();
            }
        }
    }
}
