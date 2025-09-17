using Domain.Entity;

namespace Application.Services
{
    public interface IGameSessionService
    {
        // Game session management
        Task<GameSession> CreateGameSessionAsync(int playerId, int score, int maxLevelReached);
        Task<GameSession?> GetGameSessionByIdAsync(int gameSessionId);
        Task<IEnumerable<GameSession>> GetGameSessionsByPlayerIdAsync(int playerId);
        Task<IEnumerable<GameSession>> GetAllGameSessionsAsync();
        Task<GameSession> UpdateGameSessionAsync(GameSession gameSession);
        Task<bool> DeleteGameSessionAsync(int gameSessionId);

        // Statistics and leaderboards
        Task<IEnumerable<GameSession>> GetTopScoresAsync(int count = 10);
        Task<IEnumerable<GameSession>> GetTopScoresByPlayerAsync(int playerId, int count = 10);
        Task<IEnumerable<GameSession>> GetRecentGameSessionsAsync(int count = 20);
        Task<GameSession?> GetBestScoreByPlayerIdAsync(int playerId);
        Task<int> GetMaxLevelReachedByPlayerIdAsync(int playerId);
        Task<decimal> GetAverageScoreByPlayerIdAsync(int playerId);
        
        // Advanced queries
        Task<IEnumerable<GameSession>> GetGameSessionsByScoreRangeAsync(int minScore, int maxScore);
        Task<IEnumerable<GameSession>> GetGameSessionsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> GetTotalGameSessionsCountAsync();
        Task<int> GetGameSessionsCountByPlayerIdAsync(int playerId);

        // Validation
        Task<bool> CanCreateGameSessionAsync(int playerId);
    }
}