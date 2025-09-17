using Domain.Entity;
using Domain.UnitOfWork;

namespace Application.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PlayerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Player> CreatePlayerAsync(string username)
        {
            // Generate random username if empty
            if (string.IsNullOrWhiteSpace(username))
            {
                username = await GenerateRandomUsernameAsync();
            }

            if (!await IsValidUsernameAsync(username))
            {
                throw new InvalidOperationException($"Username '{username}' already exists.");
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var player = new Player(username);
                var createdPlayer = await _unitOfWork.PlayerRepository.CreateAsync(player);
                
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return createdPlayer;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<Player?> GetPlayerByIdAsync(int playerId)
        {
            return await _unitOfWork.PlayerRepository.GetByIdAsync(playerId);
        }

        public async Task<Player?> GetPlayerByUsernameAsync(string username)
        {
            return await _unitOfWork.PlayerRepository.GetByUsernameAsync(username);
        }

        public async Task<IEnumerable<Player>> GetAllPlayersAsync()
        {
            return await _unitOfWork.PlayerRepository.GetAllAsync();
        }

        public async Task<Player> UpdatePlayerAsync(Player player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var updatedPlayer = await _unitOfWork.PlayerRepository.UpdateAsync(player);
                
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return updatedPlayer;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> DeletePlayerAsync(int playerId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var existingPlayer = await _unitOfWork.PlayerRepository.GetByIdAsync(playerId);
                if (existingPlayer == null)
                {
                    return false;
                }

                await _unitOfWork.PlayerRepository.DeleteAsync(playerId);
                
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

        public async Task<bool> PlayerExistsByUsernameAsync(string username)
        {
            return await _unitOfWork.PlayerRepository.ExistsByUsernameAsync(username);
        }

        public async Task<Player?> GetPlayerWithStatisticsAsync(int playerId)
        {
            return await _unitOfWork.PlayerRepository.GetPlayerWithStatisticsAsync(playerId);
        }

        public async Task<int> GetTotalPlayersCountAsync()
        {
            return await _unitOfWork.PlayerRepository.GetTotalPlayersCountAsync();
        }

        public async Task<IEnumerable<Player>> GetTopPlayersAsync(int count = 10)
        {
            // Get players with their best scores
            var playersWithStats = await _unitOfWork.PlayerRepository.GetPlayersWithGameSessionsAsync();
            
            return playersWithStats
                .Where(p => p.GameSessions.Any())
                .OrderByDescending(p => p.GameSessions.Max(gs => gs.Score))
                .Take(count);
        }

        public async Task<bool> IsValidUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            // Removed length validations - only check if username already exists
            if (await PlayerExistsByUsernameAsync(username))
                return false;

            return true;
        }

        /// <summary>
        /// Genera un nombre de usuario aleatorio en formato "Player{5 números}"
        /// </summary>
        /// <returns>Nombre de usuario aleatorio único</returns>
        private async Task<string> GenerateRandomUsernameAsync()
        {
            string username;
            int attempts = 0;
            const int maxAttempts = 100;

            do
            {
                var random = new Random();
                var randomNumbers = random.Next(10000, 99999); // Genera 5 dígitos
                username = $"Player{randomNumbers}";
                attempts++;

                if (attempts >= maxAttempts)
                {
                    // Si después de muchos intentos no encuentra uno único, agrega timestamp
                    var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    username = $"Player{timestamp % 100000:D5}";
                    break;
                }
            }
            while (await PlayerExistsByUsernameAsync(username));

            return username;
        }
    }
}