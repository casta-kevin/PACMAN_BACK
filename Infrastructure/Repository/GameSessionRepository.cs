using Microsoft.EntityFrameworkCore;
using Domain.Entity;
using Domain.Repositories;
using Infrastructure.Persistence;

namespace Infrastructure.Repository
{
    public class GameSessionRepository : IGameSessionRepository
    {
        private readonly PacmanDbContext _context;

        public GameSessionRepository(PacmanDbContext context)
        {
            _context = context;
        }

        public async Task<GameSession?> GetByIdAsync(int gameSessionId)
        {
            return await _context.GameSessions
                .Include(gs => gs.Player)
                .FirstOrDefaultAsync(gs => gs.GameSessionId == gameSessionId);
        }

        public async Task<IEnumerable<GameSession>> GetByPlayerIdAsync(int playerId)
        {
            return await _context.GameSessions
                .Include(gs => gs.Player)
                .Where(gs => gs.PlayerId == playerId)
                .OrderByDescending(gs => gs.PlayedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<GameSession>> GetTopScoresAsync(int count = 10)
        {
            return await _context.GameSessions
                .Include(gs => gs.Player)
                .OrderByDescending(gs => gs.Score)
                .ThenByDescending(gs => gs.MaxLevelReached)
                .ThenByDescending(gs => gs.PlayedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<GameSession>> GetAllAsync()
        {
            return await _context.GameSessions
                .Include(gs => gs.Player)
                .OrderByDescending(gs => gs.PlayedAt)
                .ToListAsync();
        }

        public async Task<GameSession> CreateAsync(GameSession gameSession)
        {
            var entity = await _context.GameSessions.AddAsync(gameSession);
            return entity.Entity;
        }

        public async Task<GameSession> UpdateAsync(GameSession gameSession)
        {
            var existingGameSession = await _context.GameSessions
                .FirstOrDefaultAsync(gs => gs.GameSessionId == gameSession.GameSessionId);

            if (existingGameSession == null)
            {
                throw new InvalidOperationException($"GameSession with ID {gameSession.GameSessionId} not found.");
            }

            // Update properties
            _context.Entry(existingGameSession).CurrentValues.SetValues(gameSession);
            
            return existingGameSession;
        }

        public async Task DeleteAsync(int gameSessionId)
        {
            var gameSession = await _context.GameSessions.FindAsync(gameSessionId);
            if (gameSession != null)
            {
                _context.GameSessions.Remove(gameSession);
            }
        }

        public async Task<GameSession?> GetBestScoreByPlayerIdAsync(int playerId)
        {
            return await _context.GameSessions
                .Include(gs => gs.Player)
                .Where(gs => gs.PlayerId == playerId)
                .OrderByDescending(gs => gs.Score)
                .ThenByDescending(gs => gs.MaxLevelReached)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetMaxLevelReachedByPlayerIdAsync(int playerId)
        {
            var maxLevel = await _context.GameSessions
                .Where(gs => gs.PlayerId == playerId)
                .MaxAsync(gs => (int?)gs.MaxLevelReached);

            return maxLevel ?? 1;
        }

        // Additional methods for better functionality
        public async Task<IEnumerable<GameSession>> GetTopScoresByPlayerAsync(int playerId, int count = 10)
        {
            return await _context.GameSessions
                .Include(gs => gs.Player)
                .Where(gs => gs.PlayerId == playerId)
                .OrderByDescending(gs => gs.Score)
                .ThenByDescending(gs => gs.MaxLevelReached)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<GameSession>> GetRecentGameSessionsAsync(int count = 20)
        {
            return await _context.GameSessions
                .Include(gs => gs.Player)
                .OrderByDescending(gs => gs.PlayedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<decimal> GetAverageScoreByPlayerIdAsync(int playerId)
        {
            var sessions = await _context.GameSessions
                .Where(gs => gs.PlayerId == playerId)
                .ToListAsync();

            return sessions.Any() ? (decimal)sessions.Average(gs => gs.Score) : 0;
        }

        public async Task<int> GetTotalGameSessionsCountAsync()
        {
            return await _context.GameSessions.CountAsync();
        }

        public async Task<int> GetGameSessionsCountByPlayerIdAsync(int playerId)
        {
            return await _context.GameSessions
                .Where(gs => gs.PlayerId == playerId)
                .CountAsync();
        }

        public async Task<IEnumerable<GameSession>> GetGameSessionsByScoreRangeAsync(int minScore, int maxScore)
        {
            return await _context.GameSessions
                .Include(gs => gs.Player)
                .Where(gs => gs.Score >= minScore && gs.Score <= maxScore)
                .OrderByDescending(gs => gs.Score)
                .ToListAsync();
        }

        public async Task<IEnumerable<GameSession>> GetGameSessionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.GameSessions
                .Include(gs => gs.Player)
                .Where(gs => gs.PlayedAt >= startDate && gs.PlayedAt <= endDate)
                .OrderByDescending(gs => gs.PlayedAt)
                .ToListAsync();
        }

        public async Task<int> DeleteAllAsync()
        {
            // Obtener el conteo antes de eliminar
            var totalCount = await _context.GameSessions.CountAsync();
            
            // Usar ExecuteDeleteAsync para una eliminación eficiente en lote (disponible en EF Core 7+)
            var deletedCount = await _context.GameSessions.ExecuteDeleteAsync();
            
            return deletedCount;
        }
    }
}