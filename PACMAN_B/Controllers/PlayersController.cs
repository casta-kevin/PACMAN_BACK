using Microsoft.AspNetCore.Mvc;
using Application.Services;
using Domain.Entity;
using Infrastructure.Persistence;
using Infrastructure.Repository.Queries;

namespace PACMAN_B.Controllers
{
    /// <summary>
    /// Controlador para la gestión de jugadores
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PlayersController : ControllerBase
    {
        private readonly IPlayerService _playerService;
        private readonly PacmanDbContext _context;
        private readonly ILogger<PlayersController> _logger;

        public PlayersController(IPlayerService playerService, PacmanDbContext context, ILogger<PlayersController> logger)
        {
            _playerService = playerService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los jugadores (optimizado sin referencias circulares)
        /// </summary>
        /// <returns>Lista de jugadores</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllPlayers()
        {
            try
            {
                var players = await PlayerQueries.GetPlayersOnlyAsync(_context);
                return Ok(players);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los jugadores");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene un jugador por su ID (básico, sin referencias circulares)
        /// </summary>
        /// <param name="id">ID del jugador</param>
        /// <returns>Datos del jugador</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Player>> GetPlayer(int id)
        {
            try
            {
                var player = await _playerService.GetPlayerByIdAsync(id);
                if (player == null)
                {
                    return NotFound($"Jugador con ID {id} no encontrado");
                }

                // Limpiar referencias circulares manualmente si es necesario
                if (player.GameSessions != null)
                {
                    foreach (var session in player.GameSessions)
                    {
                        session.Player = null!; // Romper referencia circular
                    }
                }

                return Ok(player);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el jugador con ID {PlayerId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene un jugador por su nombre de usuario
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <returns>Datos del jugador</returns>
        [HttpGet("by-username/{username}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Player>> GetPlayerByUsername(string username)
        {
            try
            {
                var player = await _playerService.GetPlayerByUsernameAsync(username);
                if (player == null)
                {
                    return NotFound($"Jugador con nombre de usuario '{username}' no encontrado");
                }

                // Limpiar referencias circulares
                if (player.GameSessions != null)
                {
                    foreach (var session in player.GameSessions)
                    {
                        session.Player = null!;
                    }
                }

                return Ok(player);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el jugador con username {Username}", username);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Crea un nuevo jugador
        /// </summary>
        /// <param name="request">Datos del nuevo jugador</param>
        /// <returns>Jugador creado</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Player>> CreatePlayer([FromBody] CreatePlayerRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username))
                {
                    return BadRequest("El nombre de usuario es requerido");
                }

                var player = await _playerService.CreatePlayerAsync(request.Username);
                return CreatedAtAction(nameof(GetPlayer), new { id = player.PlayerId }, player);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el jugador");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Actualiza un jugador existente
        /// </summary>
        /// <param name="id">ID del jugador</param>
        /// <param name="player">Datos actualizados del jugador</param>
        /// <returns>Jugador actualizado</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Player>> UpdatePlayer(int id, [FromBody] Player player)
        {
            try
            {
                if (id != player.PlayerId)
                {
                    return BadRequest("El ID de la URL no coincide con el ID del jugador");
                }

                var updatedPlayer = await _playerService.UpdatePlayerAsync(player);
                return Ok(updatedPlayer);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el jugador con ID {PlayerId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Elimina un jugador
        /// </summary>
        /// <param name="id">ID del jugador a eliminar</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeletePlayer(int id)
        {
            try
            {
                var deleted = await _playerService.DeletePlayerAsync(id);
                if (!deleted)
                {
                    return NotFound($"Jugador con ID {id} no encontrado");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el jugador con ID {PlayerId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene las estadísticas completas de un jugador (optimizado)
        /// </summary>
        /// <param name="id">ID del jugador</param>
        /// <returns>Jugador con estadísticas completas</returns>
        [HttpGet("{id}/statistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetPlayerStatistics(int id)
        {
            try
            {
                var playerStats = await PlayerQueries.GetPlayerWithBasicStatsAsync(_context, id);
                if (playerStats == null)
                {
                    return NotFound($"Jugador con ID {id} no encontrado");
                }

                return Ok(playerStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas del jugador con ID {PlayerId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene los mejores jugadores (optimizado)
        /// </summary>
        /// <param name="count">Número de jugadores a devolver</param>
        /// <returns>Lista de mejores jugadores</returns>
        [HttpGet("top/{count:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetTopPlayers(int count = 10)
        {
            try
            {
                var topPlayers = await PlayerQueries.GetTopPlayersAsync(_context, count);
                return Ok(topPlayers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los mejores jugadores");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }

    /// <summary>
    /// Modelo para crear un nuevo jugador
    /// </summary>
    public class CreatePlayerRequest
    {
        /// <summary>
        /// Nombre de usuario del jugador
        /// </summary>
        public string Username { get; set; } = string.Empty;
    }
}