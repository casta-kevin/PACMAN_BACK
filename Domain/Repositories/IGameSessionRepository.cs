using Domain.Entity;

namespace Domain.Repositories
{
    public interface IGameSessionRepository
    {
        // Basic CRUD operations
        Task<GameSession?> GetByIdAsync(int gameSessionId);
        Task<IEnumerable<GameSession>> GetByPlayerIdAsync(int playerId);
        Task<IEnumerable<GameSession>> GetTopScoresAsync(int count = 10);
        Task<IEnumerable<GameSession>> GetAllAsync();
        Task<GameSession> CreateAsync(GameSession gameSession);
        Task<GameSession> UpdateAsync(GameSession gameSession);
        Task DeleteAsync(int gameSessionId);

        // Statistics and specialized queries
        Task<GameSession?> GetBestScoreByPlayerIdAsync(int playerId);
        Task<int> GetMaxLevelReachedByPlayerIdAsync(int playerId);
        Task<IEnumerable<GameSession>> GetTopScoresByPlayerAsync(int playerId, int count = 10);
        Task<IEnumerable<GameSession>> GetRecentGameSessionsAsync(int count = 20);
        Task<decimal> GetAverageScoreByPlayerIdAsync(int playerId);
        Task<int> GetTotalGameSessionsCountAsync();
        Task<int> GetGameSessionsCountByPlayerIdAsync(int playerId);

        // Advanced filtering
        Task<IEnumerable<GameSession>> GetGameSessionsByScoreRangeAsync(int minScore, int maxScore);
        Task<IEnumerable<GameSession>> GetGameSessionsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}