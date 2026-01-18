using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration des services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Order Service API",
        Version = "v1",
        Description = "Microservice de gestion des commandes avec communication inter-services"
    });
});

// Configuration de la base de données en mémoire
builder.Services.AddDbContext<OrderContext>(options =>
    options.UseInMemoryDatabase("OrderDb"));

// Configuration de HttpClient pour les appels inter-services
builder.Services.AddHttpClient<OrderOrchestrationService>();

// Enregistrement du service d'orchestration
builder.Services.AddScoped<OrderOrchestrationService>();

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
builder.WebHost.UseUrls("http://0.0.0.0:5003");

var app = builder.Build();

// Initialiser la base de données
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<OrderContext>();
    context.Database.EnsureCreated();
}

// Configuration du pipeline HTTP
if (app.Environment.IsDevelopment() || true) // Toujours activer Swagger pour la démo
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Service API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("🚀 Order Service démarré sur le port 5003");
Console.WriteLine("📚 Swagger disponible sur: http://localhost:5003/swagger");
Console.WriteLine("🔗 Ce service communique avec UserService et ProductService");

app.Run();
