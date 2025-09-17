using Microsoft.AspNetCore.Mvc;
using Application.Services;
using Domain.Entity;

namespace PACMAN_B.Controllers
{
    /// <summary>
    /// Controlador para la gestión de sesiones de juego
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class GameSessionsController : ControllerBase
    {
        private readonly IGameSessionService _gameSessionService;
        private readonly ILogger<GameSessionsController> _logger;

        public GameSessionsController(IGameSessionService gameSessionService, ILogger<GameSessionsController> logger)
        {
            _gameSessionService = gameSessionService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las sesiones de juego
        /// </summary>
        /// <returns>Lista de sesiones de juego</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<GameSession>>> GetAllGameSessions()
        {
            try
            {
                var sessions = await _gameSessionService.GetAllGameSessionsAsync();
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las sesiones de juego");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene una sesión de juego por su ID
        /// </summary>
        /// <param name="id">ID de la sesión de juego</param>
        /// <returns>Datos de la sesión de juego</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GameSession>> GetGameSession(int id)
        {
            try
            {
                var session = await _gameSessionService.GetGameSessionByIdAsync(id);
                if (session == null)
                {
                    return NotFound($"Sesión de juego con ID {id} no encontrada");
                }

                return Ok(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la sesión de juego con ID {SessionId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene todas las sesiones de juego de un jugador
        /// </summary>
        /// <param name="playerId">ID del jugador</param>
        /// <returns>Lista de sesiones del jugador</returns>
        [HttpGet("player/{playerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<GameSession>>> GetGameSessionsByPlayer(int playerId)
        {
            try
            {
                var sessions = await _gameSessionService.GetGameSessionsByPlayerIdAsync(playerId);
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesiones del jugador {PlayerId}", playerId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Crea una nueva sesión de juego
        /// </summary>
        /// <param name="request">Datos de la nueva sesión</param>
        /// <returns>Sesión de juego creada</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GameSession>> CreateGameSession([FromBody] CreateGameSessionRequest request)
        {
            try
            {
                var session = await _gameSessionService.CreateGameSessionAsync(
                    request.PlayerId, 
                    request.Score, 
                    request.MaxLevelReached);

                return CreatedAtAction(nameof(GetGameSession), new { id = session.GameSessionId }, session);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la sesión de juego");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Actualiza una sesión de juego existente
        /// </summary>
        /// <param name="id">ID de la sesión</param>
        /// <param name="session">Datos actualizados</param>
        /// <returns>Sesión actualizada</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GameSession>> UpdateGameSession(int id, [FromBody] GameSession session)
        {
            try
            {
                if (id != session.GameSessionId)
                {
                    return BadRequest("El ID de la URL no coincide con el ID de la sesión");
                }

                var updatedSession = await _gameSessionService.UpdateGameSessionAsync(session);
                return Ok(updatedSession);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la sesión con ID {SessionId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Elimina una sesión de juego
        /// </summary>
        /// <param name="id">ID de la sesión a eliminar</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteGameSession(int id)
        {
            try
            {
                var deleted = await _gameSessionService.DeleteGameSessionAsync(id);
                if (!deleted)
                {
                    return NotFound($"Sesión de juego con ID {id} no encontrada");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la sesión con ID {SessionId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Elimina todas las sesiones de juego (todos los scores)
        /// </summary>
        /// <returns>Resultado de la operación con el número de registros eliminados</returns>
        [HttpDelete("all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DeleteAllScoresResponse>> DeleteAllGameSessions()
        {
            try
            {
                var deletedCount = await _gameSessionService.DeleteAllGameSessionsAsync();
                
                _logger.LogInformation("Se eliminaron {DeletedCount} sesiones de juego", deletedCount);

                var response = new DeleteAllScoresResponse
                {
                    Message = $"Se eliminaron exitosamente {deletedCount} sesiones de juego",
                    DeletedCount = deletedCount,
                    Success = true,
                    Timestamp = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar todas las sesiones de juego");
                
                var errorResponse = new DeleteAllScoresResponse
                {
                    Message = "Error al eliminar las sesiones de juego",
                    DeletedCount = 0,
                    Success = false,
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                };

                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Obtiene las mejores puntuaciones globales
        /// </summary>
        /// <param name="count">Número de puntuaciones a devolver</param>
        /// <returns>Lista de mejores puntuaciones</returns>
        [HttpGet("top-scores/{count:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<GameSession>>> GetTopScores(int count = 10)
        {
            try
            {
                var topScores = await _gameSessionService.GetTopScoresAsync(count);
                return Ok(topScores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las mejores puntuaciones");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene las mejores puntuaciones de un jugador
        /// </summary>
        /// <param name="playerId">ID del jugador</param>
        /// <param name="count">Número de puntuaciones a devolver</param>
        /// <returns>Lista de mejores puntuaciones del jugador</returns>
        [HttpGet("player/{playerId}/top-scores/{count:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<GameSession>>> GetTopScoresByPlayer(int playerId, int count = 10)
        {
            try
            {
                var topScores = await _gameSessionService.GetTopScoresByPlayerAsync(playerId, count);
                return Ok(topScores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las mejores puntuaciones del jugador {PlayerId}", playerId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene las sesiones de juego más recientes
        /// </summary>
        /// <param name="count">Número de sesiones a devolver</param>
        /// <returns>Lista de sesiones recientes</returns>
        [HttpGet("recent/{count:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<GameSession>>> GetRecentGameSessions(int count = 20)
        {
            try
            {
                var recentSessions = await _gameSessionService.GetRecentGameSessionsAsync(count);
                return Ok(recentSessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las sesiones recientes");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene la mejor puntuación de un jugador
        /// </summary>
        /// <param name="playerId">ID del jugador</param>
        /// <returns>Mejor sesión del jugador</returns>
        [HttpGet("player/{playerId}/best-score")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GameSession>> GetBestScoreByPlayer(int playerId)
        {
            try
            {
                var bestScore = await _gameSessionService.GetBestScoreByPlayerIdAsync(playerId);
                if (bestScore == null)
                {
                    return NotFound($"No se encontraron sesiones para el jugador {playerId}");
                }

                return Ok(bestScore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la mejor puntuación del jugador {PlayerId}", playerId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene estadísticas de un jugador
        /// </summary>
        /// <param name="playerId">ID del jugador</param>
        /// <returns>Estadísticas del jugador</returns>
        [HttpGet("player/{playerId}/statistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PlayerStatistics>> GetPlayerStatistics(int playerId)
        {
            try
            {
                var maxLevel = await _gameSessionService.GetMaxLevelReachedByPlayerIdAsync(playerId);
                var averageScore = await _gameSessionService.GetAverageScoreByPlayerIdAsync(playerId);
                var totalSessions = await _gameSessionService.GetGameSessionsCountByPlayerIdAsync(playerId);
                var bestScore = await _gameSessionService.GetBestScoreByPlayerIdAsync(playerId);

                var statistics = new PlayerStatistics
                {
                    PlayerId = playerId,
                    MaxLevelReached = maxLevel,
                    AverageScore = averageScore,
                    TotalGameSessions = totalSessions,
                    BestScore = bestScore?.Score ?? 0
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas del jugador {PlayerId}", playerId);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }

    /// <summary>
    /// Modelo para crear una nueva sesión de juego
    /// </summary>
    public class CreateGameSessionRequest
    {
        /// <summary>
        /// ID del jugador
        /// </summary>
        public int PlayerId { get; set; }

        /// <summary>
        /// Puntuación obtenida
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Nivel máximo alcanzado
        /// </summary>
        public int MaxLevelReached { get; set; }
    }

    /// <summary>
    /// Modelo para estadísticas de jugador
    /// </summary>
    public class PlayerStatistics
    {
        /// <summary>
        /// ID del jugador
        /// </summary>
        public int PlayerId { get; set; }

        /// <summary>
        /// Nivel máximo alcanzado
        /// </summary>
        public int MaxLevelReached { get; set; }

        /// <summary>
        /// Puntuación promedio
        /// </summary>
        public decimal AverageScore { get; set; }

        /// <summary>
        /// Total de sesiones de juego
        /// </summary>
        public int TotalGameSessions { get; set; }

        /// <summary>
        /// Mejor puntuación
        /// </summary>
        public int BestScore { get; set; }
    }

    /// <summary>
    /// Modelo de respuesta para la eliminación de todos los scores
    /// </summary>
    public class DeleteAllScoresResponse
    {
        /// <summary>
        /// Mensaje descriptivo de la operación
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Número de registros eliminados
        /// </summary>
        public int DeletedCount { get; set; }

        /// <summary>
        /// Indica si la operación fue exitosa
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mensaje de error si ocurrió alguno
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Timestamp de la operación
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}