# PACMAN API - Arquitectura Hexagonal

## Descripci�n

Esta es una API REST para el juego PACMAN implementada con arquitectura hexagonal usando .NET 9, Entity Framework Core y SQL Server.

## Configuraci�n

### Base de Datos

La aplicaci�n est� configurada para usar SQL Server con la siguiente cadena de conexi�n:

```
Server=DESKTOP-D0M4Q9Q\SQLSERVER;Database=PACMAN;User Id=sa;Password=ofima;TrustServerCertificate=True;
```

### Migraciones

Para aplicar las migraciones a la base de datos, ejecuta:

```bash
dotnet ef database update --project Infrastructure --startup-project PACMAN_B
```

## Swagger/OpenAPI

La documentaci�n de la API est� disponible en:
- **Desarrollo**: `https://localhost:5001/swagger`
- **Producci�n**: `https://your-domain/swagger`

## ?? Soluci�n a Referencias Circulares JSON

### Problema Resuelto
Se solucion� el error de referencia circular que ocurr�a al serializar entidades relacionadas (Player ? GameSession):

```
System.Text.Json.JsonException: A possible object cycle was detected...
```

### Soluciones Implementadas

#### 1. **Configuraci�n Global de JSON** (Implementado)
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
- Devuelven objetos an�nimos sin referencias circulares

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
2. **Application**: Servicios de aplicaci�n con l�gica de negocio, DTOs
3. **Infrastructure**: Implementaciones de repositorios, DbContext y configuraciones
4. **Presentation**: Controladores de la API y configuraci�n de Swagger

### Entidades Principales

#### Player (Jugador)
- `PlayerId`: ID �nico del jugador
- `Username`: Nombre de usuario (�nico)
- `CreatedAt`: Fecha de creaci�n
- `GameSessions`: Colecci�n de sesiones de juego (con control de referencias circulares)

#### GameSession (Sesi�n de Juego)
- `GameSessionId`: ID �nico de la sesi�n
- `PlayerId`: ID del jugador
- `Score`: Puntuaci�n obtenida
- `MaxLevelReached`: Nivel m�ximo alcanzado
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
- `GET /api/players/{id}/statistics` - Obtener estad�sticas del jugador (optimizado)
- `GET /api/players/top/{count}` - Obtener mejores jugadores (optimizado)

### GameSessions (Sesiones de Juego)

- `GET /api/gamesessions` - Obtener todas las sesiones
- `GET /api/gamesessions/{id}` - Obtener sesi�n por ID
- `GET /api/gamesessions/player/{playerId}` - Obtener sesiones de un jugador
- `POST /api/gamesessions` - Crear nueva sesi�n
- `PUT /api/gamesessions/{id}` - Actualizar sesi�n
- `DELETE /api/gamesessions/{id}` - Eliminar sesi�n
- `GET /api/gamesessions/top-scores/{count}` - Mejores puntuaciones globales
- `GET /api/gamesessions/player/{playerId}/top-scores/{count}` - Mejores puntuaciones por jugador
- `GET /api/gamesessions/recent/{count}` - Sesiones m�s recientes
- `GET /api/gamesessions/player/{playerId}/best-score` - Mejor puntuaci�n de un jugador
- `GET /api/gamesessions/player/{playerId}/statistics` - Estad�sticas detalladas

### Health (Estado de Salud)

- `GET /api/health` - Estado general de la aplicaci�n
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

### Crear una Sesi�n de Juego

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

### Obtener Estad�sticas de Jugador (Optimizado)

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

## Patrones de Dise�o Implementados

### Repository Pattern
- Abstrae el acceso a datos
- Interfaces en Domain, implementaciones en Infrastructure

### Unit of Work Pattern
- Gestiona transacciones
- Coordina m�ltiples repositorios

### Query Objects Pattern
- Consultas especializadas en `Infrastructure/Repository/Queries/`
- Optimizadas para evitar referencias circulares

### DTO Pattern
- Objetos de transferencia de datos en `Application/DTOs/`
- Evitan exposici�n de entidades de dominio

### Dependency Injection
- Todas las dependencias son inyectadas
- Configuraci�n en `Program.cs` y extensiones

### Clean Architecture / Hexagonal Architecture
- Separaci�n clara de responsabilidades
- Dependencias apuntan hacia el dominio

## Tecnolog�as Utilizadas

- **.NET 9**: Framework principal
- **Entity Framework Core 9**: ORM para acceso a datos
- **SQL Server**: Base de datos
- **Swashbuckle.AspNetCore**: Documentaci�n Swagger
- **System.Text.Json**: Serializaci�n JSON con manejo de referencias circulares
- **Microsoft.Extensions.DependencyInjection**: Inyecci�n de dependencias

## Ejecuci�n

1. Aseg�rate de que SQL Server est� ejecut�ndose
2. Ejecuta las migraciones de la base de datos
3. Ejecuta la aplicaci�n:

```bash
dotnet run --project PACMAN_B
```

4. Navega a `https://localhost:5001/swagger` para ver la documentaci�n

## Mejores Pr�cticas Implementadas

### Para Evitar Referencias Circulares JSON:

1. **Usa `ReferenceHandler.IgnoreCycles`** en la configuraci�n global
2. **Implementa consultas especializadas** con proyecciones
3. **Usa `AsNoTracking()`** en consultas de solo lectura
4. **Crea DTOs** para casos espec�ficos
5. **Limpia referencias manualmente** cuando sea necesario

### Para Optimizaci�n de Consultas:

1. **Proyecciones con `Select()`** para obtener solo los datos necesarios
2. **�ndices apropiados** definidos en las configuraciones de EF Core
3. **Consultas espec�ficas** en lugar de `Include()` gen�ricos
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

## Contribuci�n

1. Sigue los patrones de arquitectura establecidos
2. Mant�n la separaci�n de responsabilidades
3. Evita referencias circulares en las consultas
4. Usa consultas optimizadas para endpoints de alta frecuencia
5. Agrega tests unitarios para nuevas funcionalidades
6. Documenta los nuevos endpoints en Swagger

## Notas de Desarrollo

- ? **Referencias circulares resueltas** con m�ltiples estrategias
- ? **Consultas optimizadas** implementadas
- ? **Transacciones** configuradas correctamente
- ? **Manejo de errores** implementado en todos los endpoints
- ? **Logging** configurado usando `ILogger<T>`
- ? **Documentaci�n XML** generada autom�ticamente para Swagger