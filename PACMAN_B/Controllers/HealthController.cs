using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;

namespace PACMAN_B.Controllers
{
    /// <summary>
    /// Controlador para verificar el estado de salud de la aplicación
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class HealthController : ControllerBase
    {
        private readonly PacmanDbContext _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(PacmanDbContext context, ILogger<HealthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Verifica el estado de salud de la API
        /// </summary>
        /// <returns>Estado de la aplicación</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<HealthStatus>> GetHealth()
        {
            var health = new HealthStatus
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0"
            };

            try
            {
                // Verificar conectividad a la base de datos
                var canConnect = await _context.Database.CanConnectAsync();
                health.DatabaseStatus = canConnect ? "Connected" : "Disconnected";
                
                if (canConnect)
                {
                    // Obtener estadísticas básicas
                    var playerCount = await _context.Players.CountAsync();
                    var sessionCount = await _context.GameSessions.CountAsync();
                    
                    health.Statistics = new DatabaseStatistics
                    {
                        TotalPlayers = playerCount,
                        TotalGameSessions = sessionCount
                    };
                }
                else
                {
                    health.Status = "Degraded";
                    return StatusCode(503, health);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar el estado de salud de la base de datos");
                health.Status = "Unhealthy";
                health.DatabaseStatus = "Error";
                health.Error = ex.Message;
                return StatusCode(503, health);
            }

            return Ok(health);
        }

        /// <summary>
        /// Verifica solo la conectividad de la base de datos
        /// </summary>
        /// <returns>Estado de la base de datos</returns>
        [HttpGet("database")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<DatabaseHealth>> GetDatabaseHealth()
        {
            var dbHealth = new DatabaseHealth
            {
                Timestamp = DateTime.UtcNow
            };

            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                dbHealth.CanConnect = canConnect;
                dbHealth.Status = canConnect ? "Connected" : "Disconnected";

                if (canConnect)
                {
                    // Información adicional de la base de datos
                    dbHealth.DatabaseName = _context.Database.GetDbConnection().Database;
                    dbHealth.ConnectionString = _context.Database.GetConnectionString()?.Replace("Password=ofima", "Password=****");
                }

                return canConnect ? Ok(dbHealth) : StatusCode(503, dbHealth);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar la conectividad de la base de datos");
                dbHealth.Status = "Error";
                dbHealth.CanConnect = false;
                dbHealth.Error = ex.Message;
                return StatusCode(503, dbHealth);
            }
        }
    }

    /// <summary>
    /// Modelo para el estado de salud de la aplicación
    /// </summary>
    public class HealthStatus
    {
        /// <summary>
        /// Estado general de la aplicación
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Estado de la base de datos
        /// </summary>
        public string DatabaseStatus { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp del chequeo
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Versión de la aplicación
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Estadísticas de la base de datos
        /// </summary>
        public DatabaseStatistics? Statistics { get; set; }

        /// <summary>
        /// Error si existe
        /// </summary>
        public string? Error { get; set; }
    }

    /// <summary>
    /// Estadísticas de la base de datos
    /// </summary>
    public class DatabaseStatistics
    {
        /// <summary>
        /// Total de jugadores
        /// </summary>
        public int TotalPlayers { get; set; }

        /// <summary>
        /// Total de sesiones de juego
        /// </summary>
        public int TotalGameSessions { get; set; }
    }

    /// <summary>
    /// Estado de salud de la base de datos
    /// </summary>
    public class DatabaseHealth
    {
        /// <summary>
        /// Puede conectar a la base de datos
        /// </summary>
        public bool CanConnect { get; set; }

        /// <summary>
        /// Estado de conexión
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de la base de datos
        /// </summary>
        public string? DatabaseName { get; set; }

        /// <summary>
        /// Cadena de conexión (sin password)
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Timestamp del chequeo
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Error si existe
        /// </summary>
        public string? Error { get; set; }
    }
}