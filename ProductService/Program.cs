using Microsoft.EntityFrameworkCore;
using ProductService.Data;

var builder = WebApplication.CreateBuilder(args);

// Configuration des services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Product Service API",
        Version = "v1",
        Description = "Microservice de gestion des produits"
    });
});

// Configuration de la base de données en mémoire
builder.Services.AddDbContext<ProductContext>(options =>
    options.UseInMemoryDatabase("ProductDb"));

// Configuration CORS
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
builder.WebHost.UseUrls("http://0.0.0.0:5002");

var app = builder.Build();

// Initialiser la base de données
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ProductContext>();
    context.Database.EnsureCreated();
}

// Configuration du pipeline HTTP
if (app.Environment.IsDevelopment() || true) // Toujours activer Swagger pour la démo
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Service API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("🚀 Product Service démarré sur le port 5002");
Console.WriteLine("📚 Swagger disponible sur: http://localhost:5002/swagger");

app.Run();
