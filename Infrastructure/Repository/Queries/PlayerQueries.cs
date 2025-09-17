using Microsoft.EntityFrameworkCore;
using Domain.Entity;
using Infrastructure.Persistence;

namespace Infrastructure.Repository.Queries
{
    /// <summary>
    /// Consultas especializadas para jugadores que evitan referencias circulares
    /// </summary>
    public static class PlayerQueries
    {
        /// <summary>
        /// Obtiene jugadores sin las sesiones de juego para evitar referencias circulares
        /// </summary>
        public static async Task<IEnumerable<Player>> GetPlayersOnlyAsync(PacmanDbContext context)
        {
            return await context.Players
                .AsNoTracking()
                .OrderBy(p => p.Username)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene un jugador con datos básicos de estadísticas sin referencias circulares
        /// </summary>
        public static async Task<object?> GetPlayerWithBasicStatsAsync(PacmanDbContext context, int playerId)
        {
            return await context.Players
                .AsNoTracking()
                .Where(p => p.PlayerId == playerId)
                .Select(p => new
                {
                    p.PlayerId,
                    p.Username,
                    p.CreatedAt,
                    TotalSessions = p.GameSessions.Count(),
                    BestScore = p.GameSessions.Max(gs => (int?)gs.Score),
                    MaxLevel = p.GameSessions.Max(gs => (int?)gs.MaxLevelReached),
                    AverageScore = p.GameSessions.Any() 
                        ? p.GameSessions.Average(gs => (double)gs.Score) 
                        : 0,
                    RecentSessions = p.GameSessions
                        .OrderByDescending(gs => gs.PlayedAt)
                        .Take(5)
                        .Select(gs => new
                        {
                            gs.GameSessionId,
                            gs.Score,
                            gs.MaxLevelReached,
                            gs.PlayedAt
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Obtiene los mejores jugadores sin referencias circulares
        /// </summary>
        public static async Task<IEnumerable<object>> GetTopPlayersAsync(PacmanDbContext context, int count = 10)
        {
            return await context.Players
                .AsNoTracking()
                .Where(p => p.GameSessions.Any())
                .OrderByDescending(p => p.GameSessions.Max(gs => gs.Score))
                .Take(count)
                .Select(p => new
                {
                    p.PlayerId,
                    p.Username,
                    p.CreatedAt,
                    BestScore = p.GameSessions.Max(gs => gs.Score),
                    TotalSessions = p.GameSessions.Count(),
                    MaxLevel = p.GameSessions.Max(gs => gs.MaxLevelReached)
                })
                .ToListAsync();
        }
    }
}