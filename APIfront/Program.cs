using APIfront.Helpers;
using APIfront.Models;
using APIfront.Services;
using APIfront.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Ajouter la configuration Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });

    // Ajout de la définition de sécurité pour le token JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Entrez 'Bearer' suivi d'un espace et de votre token JWT.\nExemple : Bearer xxxxxx.yyyyyyy.zzzzzz"
    });

    // Ajout de la sécurité globale à Swagger pour toutes les requêtes protégées
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// Ajouter le DbContext "MyContext" avec la configuration SQL Server
builder.Services.AddDbContext<MyContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("default"));
});

// Configuration de l'authentification JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;  // Utilisation de DefaultAuthenticateScheme
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;     // Utilisation de DefaultChallengeScheme
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],  // Définition de l'émetteur
        ValidAudience = builder.Configuration["Jwt:Audience"],  // Définition de l'audience
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))  // Clé de signature
    };
});

//****** pour configurer la class du jwt 
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

//****** pour configurer HashPassword
builder.Services.AddScoped<IPasswordHasher, HashPassword>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin",
        builder =>
        {
            builder.WithOrigins("https://localhost:7184") // Ajoutez ici toutes les autres origines autorisées si nécessaire
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

builder.WebHost.UseUrls("https://localhost:7184");


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseCors(options => {
    options.AllowAnyHeader();
    options.AllowAnyMethod();
    options.AllowAnyOrigin();
});

app.UseHttpsRedirection();

app.UseCors("AllowAnyOrigin");


app.UseAuthentication(); 

app.UseAuthorization();

app.MapControllers();

app.Run();
