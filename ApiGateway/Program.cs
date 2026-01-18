var builder = WebApplication.CreateBuilder(args);

// Configuration des services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "API Gateway",
        Version = "v1",
        Description = "Point d'entrée unique pour tous les microservices"
    });
});

// Configuration de HttpClient pour les appels aux microservices
builder.Services.AddHttpClient();

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
builder.WebHost.UseUrls("http://0.0.0.0:5000");

var app = builder.Build();

// Configuration du pipeline HTTP
if (app.Environment.IsDevelopment() || true) // Toujours activer Swagger pour la démo
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("╔════════════════════════════════════════════════════════╗");
Console.WriteLine("║           🚀 API GATEWAY DÉMARRÉ                       ║");
Console.WriteLine("╚════════════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine("📍 Port: 5000");
Console.WriteLine("📚 Swagger UI: http://localhost:5000/swagger");
Console.WriteLine("🔗 Health Check: http://localhost:5000/api/health");
Console.WriteLine();
Console.WriteLine("📡 Services routés:");
Console.WriteLine("   → UserService    (Port 5001)");
Console.WriteLine("   → ProductService (Port 5002)");
Console.WriteLine("   → OrderService   (Port 5003)");
Console.WriteLine();
Console.WriteLine("✨ Tous les appels passent par ce gateway!");
Console.WriteLine();

app.Run();
