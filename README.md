# PACMAN API - Arquitectura Hexagonal

## Descripción

Esta es una API REST para el juego PACMAN implementada con arquitectura hexagonal usando .NET 9, Entity Framework Core y SQL Server.

## Configuración

### Base de Datos

La aplicación está configurada para usar SQL Server con la siguiente cadena de conexión:

```
Server=DESKTOP-D0M4Q9Q\SQLSERVER;Database=PACMAN;User Id=sa;Password=ofima;TrustServerCertificate=True;
```

### Migraciones

Para aplicar las migraciones a la base de datos, ejecuta:

```bash
dotnet ef database update --project Infrastructure --startup-project PACMAN_B
```

## Swagger/OpenAPI

La documentación de la API está disponible en:
- **Desarrollo**: `https://localhost:5001/swagger`
- **Producción**: `https://your-domain/swagger`

## ?? Solución a Referencias Circulares JSON

### Problema Resuelto
Se solucionó el error de referencia circular que ocurría al serializar entidades relacionadas (Player ? GameSession):

```
System.Text.Json.JsonException: A possible object cycle was detected...
```

### Soluciones Implementadas

#### 1. **Configuración Global de JSON** (Implementado)
```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configurar manejo de referencias circulares
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
```

#### 2. **Atributos JSON en Entidades** (Implementado)
```csharp
public class Player
{
    // ...otras propiedades...
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual ICollection<GameSession> GameSessions { get; set; }
}
```

#### 3. **Consultas Optimizadas** (Implementado)
Se crearon consultas especializadas en `Infrastructure/Repository/Queries/` que:
- Usan proyecciones para evitar cargar navegaciones innecesarias
- Implementan `AsNoTracking()` para mejor rendimiento
- Devuelven objetos anónimos sin referencias circulares

#### 4. **Limpieza Manual de Referencias** (Implementado)
En los controladores se limpian las referencias circulares manualmente cuando es necesario:
```csharp
if (player.GameSessions != null)
{
    foreach (var session in player.GameSessions)
    {
        session.Player = null!; // Romper referencia circular
    }
}
```

## Arquitectura

### Capas del Proyecto

1. **Domain**: Entidades de dominio, interfaces de repositorios y Unit of Work
2. **Application**: Servicios de aplicación con lógica de negocio, DTOs
3. **Infrastructure**: Implementaciones de repositorios, DbContext y configuraciones
4. **Presentation**: Controladores de la API y configuración de Swagger

### Entidades Principales

#### Player (Jugador)
- `PlayerId`: ID único del jugador
- `Username`: Nombre de usuario (único)
- `CreatedAt`: Fecha de creación
- `GameSessions`: Colección de sesiones de juego (con control de referencias circulares)

#### GameSession (Sesión de Juego)
- `GameSessionId`: ID único de la sesión
- `PlayerId`: ID del jugador
- `Score`: Puntuación obtenida
- `MaxLevelReached`: Nivel máximo alcanzado
- `PlayedAt`: Fecha y hora de la partida
- `Player`: Referencia al jugador (con control de referencias circulares)

## Endpoints Principales

### Players (Jugadores)

- `GET /api/players` - Obtener todos los jugadores (optimizado)
- `GET /api/players/{id}` - Obtener jugador por ID
- `GET /api/players/by-username/{username}` - Obtener jugador por username
- `POST /api/players` - Crear nuevo jugador
- `PUT /api/players/{id}` - Actualizar jugador
- `DELETE /api/players/{id}` - Eliminar jugador
- `GET /api/players/{id}/statistics` - Obtener estadísticas del jugador (optimizado)
- `GET /api/players/top/{count}` - Obtener mejores jugadores (optimizado)

### GameSessions (Sesiones de Juego)

- `GET /api/gamesessions` - Obtener todas las sesiones
- `GET /api/gamesessions/{id}` - Obtener sesión por ID
- `GET /api/gamesessions/player/{playerId}` - Obtener sesiones de un jugador
- `POST /api/gamesessions` - Crear nueva sesión
- `PUT /api/gamesessions/{id}` - Actualizar sesión
- `DELETE /api/gamesessions/{id}` - Eliminar sesión
- `GET /api/gamesessions/top-scores/{count}` - Mejores puntuaciones globales
- `GET /api/gamesessions/player/{playerId}/top-scores/{count}` - Mejores puntuaciones por jugador
- `GET /api/gamesessions/recent/{count}` - Sesiones más recientes
- `GET /api/gamesessions/player/{playerId}/best-score` - Mejor puntuación de un jugador
- `GET /api/gamesessions/player/{playerId}/statistics` - Estadísticas detalladas

### Health (Estado de Salud)

- `GET /api/health` - Estado general de la aplicación
- `GET /api/health/database` - Estado de conectividad de la base de datos

## Ejemplos de Uso

### Crear un Jugador

```bash
POST /api/players
Content-Type: application/json

{
  "username": "jugador1"
}
```

### Crear una Sesión de Juego

```bash
POST /api/gamesessions
Content-Type: application/json

{
  "playerId": 1,
  "score": 15000,
  "maxLevelReached": 5
}
```

### Obtener Mejores Puntuaciones

```bash
GET /api/gamesessions/top-scores/10
```

### Obtener Estadísticas de Jugador (Optimizado)

```bash
GET /api/players/1/statistics
```

Respuesta optimizada sin referencias circulares:
```json
{
  "playerId": 1,
  "username": "jugador1",
  "createdAt": "2025-01-17T13:00:00Z",
  "totalSessions": 5,
  "bestScore": 25000,
  "maxLevel": 8,
  "averageScore": 18500.5,
  "recentSessions": [
    {
      "gameSessionId": 10,
      "score": 25000,
      "maxLevelReached": 8,
      "playedAt": "2025-01-17T12:30:00Z"
    }
  ]
}
```

## Patrones de Diseño Implementados

### Repository Pattern
- Abstrae el acceso a datos
- Interfaces en Domain, implementaciones en Infrastructure

### Unit of Work Pattern
- Gestiona transacciones
- Coordina múltiples repositorios

### Query Objects Pattern
- Consultas especializadas en `Infrastructure/Repository/Queries/`
- Optimizadas para evitar referencias circulares

### DTO Pattern
- Objetos de transferencia de datos en `Application/DTOs/`
- Evitan exposición de entidades de dominio

### Dependency Injection
- Todas las dependencias son inyectadas
- Configuración en `Program.cs` y extensiones

### Clean Architecture / Hexagonal Architecture
- Separación clara de responsabilidades
- Dependencias apuntan hacia el dominio

## Tecnologías Utilizadas

- **.NET 9**: Framework principal
- **Entity Framework Core 9**: ORM para acceso a datos
- **SQL Server**: Base de datos
- **Swashbuckle.AspNetCore**: Documentación Swagger
- **System.Text.Json**: Serialización JSON con manejo de referencias circulares
- **Microsoft.Extensions.DependencyInjection**: Inyección de dependencias

## Ejecución

1. Asegúrate de que SQL Server esté ejecutándose
2. Ejecuta las migraciones de la base de datos
3. Ejecuta la aplicación:

```bash
dotnet run --project PACMAN_B
```

4. Navega a `https://localhost:5001/swagger` para ver la documentación

## Mejores Prácticas Implementadas

### Para Evitar Referencias Circulares JSON:

1. **Usa `ReferenceHandler.IgnoreCycles`** en la configuración global
2. **Implementa consultas especializadas** con proyecciones
3. **Usa `AsNoTracking()`** en consultas de solo lectura
4. **Crea DTOs** para casos específicos
5. **Limpia referencias manualmente** cuando sea necesario

### Para Optimización de Consultas:

1. **Proyecciones con `Select()`** para obtener solo los datos necesarios
2. **Índices apropiados** definidos en las configuraciones de EF Core
3. **Consultas específicas** en lugar de `Include()` genéricos
4. **Lazy loading deshabilitado** para evitar consultas N+1

## Estructura de Base de Datos

### Tabla Players
```sql
CREATE TABLE Players (
    PlayerId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME()
);
```

### Tabla GameSessions
```sql
CREATE TABLE GameSessions (
    GameSessionId INT IDENTITY(1,1) PRIMARY KEY,
    PlayerId INT NOT NULL,
    Score INT NOT NULL DEFAULT 0,
    MaxLevelReached INT NOT NULL DEFAULT 1,
    PlayedAt DATETIME2 DEFAULT SYSDATETIME(),
    FOREIGN KEY (PlayerId) REFERENCES Players(PlayerId)
);
```

## Contribución

1. Sigue los patrones de arquitectura establecidos
2. Mantén la separación de responsabilidades
3. Evita referencias circulares en las consultas
4. Usa consultas optimizadas para endpoints de alta frecuencia
5. Agrega tests unitarios para nuevas funcionalidades
6. Documenta los nuevos endpoints en Swagger

## Notas de Desarrollo

- ? **Referencias circulares resueltas** con múltiples estrategias
- ? **Consultas optimizadas** implementadas
- ? **Transacciones** configuradas correctamente
- ? **Manejo de errores** implementado en todos los endpoints
- ? **Logging** configurado usando `ILogger<T>`
- ? **Documentación XML** generada automáticamente para Swagger