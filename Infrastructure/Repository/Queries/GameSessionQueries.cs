using Microsoft.EntityFrameworkCore;
using Domain.Entity;
using Infrastructure.Persistence;

namespace Infrastructure.Repository.Queries
{
    /// <summary>
    /// Consultas especializadas para sesiones de juego que evitan referencias circulares
    /// </summary>
    public static class GameSessionQueries
    {
        /// <summary>
        /// Obtiene sesiones de juego con datos básicos del jugador sin referencias circulares
        /// </summary>
        public static async Task<IEnumerable<object>> GetSessionsWithBasicPlayerInfoAsync(PacmanDbContext context)
        {
            return await context.GameSessions
                .AsNoTracking()
                .OrderByDescending(gs => gs.PlayedAt)
                .Select(gs => new
                {
                    gs.GameSessionId,
                    gs.PlayerId,
                    gs.Score,
                    gs.MaxLevelReached,
                    gs.PlayedAt,
                    Player = new
                    {
                        gs.Player.PlayerId,
                        gs.Player.Username,
                        gs.Player.CreatedAt
                    }
                })
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene las mejores puntuaciones sin referencias circulares
        /// </summary>
        public static async Task<IEnumerable<object>> GetTopScoresWithPlayerAsync(PacmanDbContext context, int count = 10)
        {
            return await context.GameSessions
                .AsNoTracking()
                .OrderByDescending(gs => gs.Score)
                .ThenByDescending(gs => gs.MaxLevelReached)
                .Take(count)
                .Select(gs => new
                {
                    gs.GameSessionId,
                    gs.PlayerId,
                    gs.Score,
                    gs.MaxLevelReached,
                    gs.PlayedAt,
                    PlayerUsername = gs.Player.Username
                })
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene sesiones por jugador sin referencias circulares
        /// </summary>
        public static async Task<IEnumerable<object>> GetSessionsByPlayerAsync(PacmanDbContext context, int playerId)
        {
            return await context.GameSessions
                .AsNoTracking()
                .Where(gs => gs.PlayerId == playerId)
                .OrderByDescending(gs => gs.PlayedAt)
                .Select(gs => new
                {
                    gs.GameSessionId,
                    gs.PlayerId,
                    gs.Score,
                    gs.MaxLevelReached,
                    gs.PlayedAt
                })
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene sesiones recientes con información básica del jugador
        /// </summary>
        public static async Task<IEnumerable<object>> GetRecentSessionsWithPlayerAsync(PacmanDbContext context, int count = 20)
        {
            return await context.GameSessions
                .AsNoTracking()
                .OrderByDescending(gs => gs.PlayedAt)
                .Take(count)
                .Select(gs => new
                {
                    gs.GameSessionId,
                    gs.PlayerId,
                    gs.Score,
                    gs.MaxLevelReached,
                    gs.PlayedAt,
                    PlayerUsername = gs.Player.Username
                })
                .ToListAsync();
        }
    }
}