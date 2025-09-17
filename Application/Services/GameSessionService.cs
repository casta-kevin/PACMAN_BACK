using Domain.Entity;
using Domain.UnitOfWork;

namespace Application.Services
{
    public class GameSessionService : IGameSessionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GameSessionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GameSession> CreateGameSessionAsync(int playerId, int score, int maxLevelReached)
        {
            // Validate input
            if (score < 0)
            {
                throw new ArgumentException("Score cannot be negative.", nameof(score));
            }

            if (maxLevelReached < 1)
            {
                throw new ArgumentException("Max level reached cannot be less than 1.", nameof(maxLevelReached));
            }

            if (!await CanCreateGameSessionAsync(playerId))
            {
                throw new InvalidOperationException($"Cannot create game session for player with ID {playerId}.");
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var gameSession = new GameSession(playerId, score, maxLevelReached);
                var createdGameSession = await _unitOfWork.GameSessionRepository.CreateAsync(gameSession);
                
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return createdGameSession;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<GameSession?> GetGameSessionByIdAsync(int gameSessionId)
        {
            return await _unitOfWork.GameSessionRepository.GetByIdAsync(gameSessionId);
        }

        public async Task<IEnumerable<GameSession>> GetGameSessionsByPlayerIdAsync(int playerId)
        {
            return await _unitOfWork.GameSessionRepository.GetByPlayerIdAsync(playerId);
        }

        public async Task<IEnumerable<GameSession>> GetAllGameSessionsAsync()
        {
            return await _unitOfWork.GameSessionRepository.GetAllAsync();
        }

        public async Task<GameSession> UpdateGameSessionAsync(GameSession gameSession)
        {
            if (gameSession == null)
            {
                throw new ArgumentNullException(nameof(gameSession));
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var updatedGameSession = await _unitOfWork.GameSessionRepository.UpdateAsync(gameSession);
                
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return updatedGameSession;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> DeleteGameSessionAsync(int gameSessionId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var existingGameSession = await _unitOfWork.GameSessionRepository.GetByIdAsync(gameSessionId);
                if (existingGameSession == null)
                {
                    return false;
                }

                await _unitOfWork.GameSessionRepository.DeleteAsync(gameSessionId);
                
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<IEnumerable<GameSession>> GetTopScoresAsync(int count = 10)
        {
            return await _unitOfWork.GameSessionRepository.GetTopScoresAsync(count);
        }

        public async Task<IEnumerable<GameSession>> GetTopScoresByPlayerAsync(int playerId, int count = 10)
        {
            return await _unitOfWork.GameSessionRepository.GetTopScoresByPlayerAsync(playerId, count);
        }

        public async Task<IEnumerable<GameSession>> GetRecentGameSessionsAsync(int count = 20)
        {
            return await _unitOfWork.GameSessionRepository.GetRecentGameSessionsAsync(count);
        }

        public async Task<GameSession?> GetBestScoreByPlayerIdAsync(int playerId)
        {
            return await _unitOfWork.GameSessionRepository.GetBestScoreByPlayerIdAsync(playerId);
        }

        public async Task<int> GetMaxLevelReachedByPlayerIdAsync(int playerId)
        {
            return await _unitOfWork.GameSessionRepository.GetMaxLevelReachedByPlayerIdAsync(playerId);
        }

        public async Task<decimal> GetAverageScoreByPlayerIdAsync(int playerId)
        {
            return await _unitOfWork.GameSessionRepository.GetAverageScoreByPlayerIdAsync(playerId);
        }

        public async Task<IEnumerable<GameSession>> GetGameSessionsByScoreRangeAsync(int minScore, int maxScore)
        {
            if (minScore > maxScore)
            {
                throw new ArgumentException("Minimum score cannot be greater than maximum score.");
            }

            return await _unitOfWork.GameSessionRepository.GetGameSessionsByScoreRangeAsync(minScore, maxScore);
        }

        public async Task<IEnumerable<GameSession>> GetGameSessionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                throw new ArgumentException("Start date cannot be greater than end date.");
            }

            return await _unitOfWork.GameSessionRepository.GetGameSessionsByDateRangeAsync(startDate, endDate);
        }

        public async Task<int> GetTotalGameSessionsCountAsync()
        {
            return await _unitOfWork.GameSessionRepository.GetTotalGameSessionsCountAsync();
        }

        public async Task<int> GetGameSessionsCountByPlayerIdAsync(int playerId)
        {
            return await _unitOfWork.GameSessionRepository.GetGameSessionsCountByPlayerIdAsync(playerId);
        }

        public async Task<int> DeleteAllGameSessionsAsync()
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Usar el método eficiente del repositorio para eliminar todas las sesiones
                var deletedCount = await _unitOfWork.GameSessionRepository.DeleteAllAsync();

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return deletedCount;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> CanCreateGameSessionAsync(int playerId)
        {
            // Check if player exists
            var player = await _unitOfWork.PlayerRepository.GetByIdAsync(playerId);
            return player != null;
        }
    }
}