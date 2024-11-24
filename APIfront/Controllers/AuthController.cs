using APIfront.Dtos;
using APIfront.Helpers;
using APIfront.Models;
using APIfront.Services;
using APIfront.Interfaces;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using APIfront.Enums;

namespace APIfront.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtSettings _jwtSettings;
        private readonly MyContext _context;
        private readonly IPasswordHasher _passwordHasher;

        // Injection des dépendances
        public AuthController(IOptions<JwtSettings> jwtSettings, MyContext context, IPasswordHasher passwordHasher)
        {
            _jwtSettings = jwtSettings.Value;
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // Méthode pour enregistrer un utilisateur
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            // Vérifier si l'utilisateur existe déjà
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                return BadRequest(new { Message = "Cet utilisateur existe déjà." });
            }

            // Valider le rôle en s'assurant qu'il appartient à l'énumération UserRole
            if (!Enum.TryParse(typeof(UserRole), model.Role, true, out var parsedRole))
            {
                return BadRequest(new { Message = "Le rôle spécifié est invalide." });
            }

            // Hacher le mot de passe
            var hashedPassword = _passwordHasher.HashPassworde(model.Password);

            // Créer un nouvel utilisateur
            var user = new User
            {
                Username = model.Username,
                PasswordHash = hashedPassword,
                Role = parsedRole.ToString() // Enregistrer le rôle sous forme de chaîne
            };

            // Ajouter l'utilisateur à la base de données
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Retourner un message de succès
            return Ok(new { Message = "Utilisateur enregistré avec succès !" });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {

            // Rechercher l'utilisateur dans la base de donnée

            var user= await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
           if(user==null)
            {

                return Unauthorized("username or password incorrect.");

            }

            // verifier si le password est correct

            var hashpassword = _passwordHasher.HashPassworde(model.Password);
            if (user.PasswordHash != hashpassword)
            {
                return Unauthorized("username or password incorrect.");

            }

            var token = GenerateJwtToken(user);

            // Retourner le token
            return Ok(new
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(1) // Facultatif : Ajout de l'heure d'expiration
            });

        }


        //*************** creation du fonction pour generer Token

        private string GenerateJwtToken(User user)
        {
            // Créer les claims (informations à inclure dans le token)
            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // Utiliser l'ID utilisateur
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.Role, user.Role)
    };

            // Clé secrète pour signer le token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));

            // Définir l'algorithme de signature
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Définir la durée de validité du token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1), // Token valide pour 1 heure
                SigningCredentials = credentials,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };

            // Générer le token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // Retourner le token sous forme de chaîne
            return tokenHandler.WriteToken(token);
        }





    }
}
