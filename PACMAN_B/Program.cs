using Infrastructure.Services;
using Application;
using System.Text.Json.Serialization;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()     // Permite cualquier origen
              .AllowAnyHeader()     // Permite cualquier header
              .AllowAnyMethod();    // Permite cualquier método HTTP (GET, POST, PUT, DELETE, etc.)
    });

    // Política más específica para desarrollo local (opcional)
    options.AddPolicy("DevelopmentPolicy", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",    // React default
            "http://localhost:4200",    // Angular default
            "http://localhost:8080",    // Vue.js default
            "http://localhost:5173",    // Vite default
            "http://localhost:8000",    // Otros puertos comunes
            "https://localhost:3000",
            "https://localhost:4200",
            "https://localhost:8080",
            "https://localhost:5173"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials(); // Permite cookies/credenciales
    });
});

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configurar manejo de referencias circulares
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        
        // Opcional: Configuraciones adicionales de JSON
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Add Infrastructure layer services (DbContext, Repositories, UnitOfWork)
builder.Services.AddInfrastructure(builder.Configuration);

// Add Application layer services (Business Logic)
builder.Services.AddApplicationServices();

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "PACMAN API",
        Version = "v1",
        Description = "API para el juego PACMAN con arquitectura hexagonal - Con soporte CORS completo",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Equipo de Desarrollo",
            Email = "desarrollo@ofima.com"
        }
    });
    
    // Incluir comentarios XML si están disponibles
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure CORS middleware - IMPORTANTE: debe ir ANTES de UseAuthorization
app.UseCors("AllowAllOrigins");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PACMAN API v1");
        c.RoutePrefix = "swagger"; // Swagger UI estará disponible en /swagger
        c.DocumentTitle = "PACMAN API - Documentación";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
