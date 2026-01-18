using Microsoft.EntityFrameworkCore;
using UserService.Data;

var builder = WebApplication.CreateBuilder(args);

// Configuration des services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "User Service API",
        Version = "v1",
        Description = "Microservice de gestion des utilisateurs"
    });
});

// Configuration de la base de données en mémoire
builder.Services.AddDbContext<UserContext>(options =>
    options.UseInMemoryDatabase("UserDb"));

// Configuration CORS pour permettre les appels depuis d'autres services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configuration du port
builder.WebHost.UseUrls("http://0.0.0.0:5001");

var app = builder.Build();

// Initialiser la base de données
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UserContext>();
    context.Database.EnsureCreated();
}

// Configuration du pipeline HTTP
if (app.Environment.IsDevelopment() || true) // Toujours activer Swagger pour la démo
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Service API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("🚀 User Service démarré sur le port 5001");
Console.WriteLine("📚 Swagger disponible sur: http://localhost:5001/swagger");

app.Run();
