using Domain.Entity;

namespace Domain.Repositories
{
    public interface IPlayerRepository
    {
        Task<Player?> GetByIdAsync(int playerId);
        Task<Player?> GetByUsernameAsync(string username);
        Task<IEnumerable<Player>> GetAllAsync();
        Task<Player> CreateAsync(Player player);
        Task<Player> UpdateAsync(Player player);
        Task DeleteAsync(int playerId);
        Task<bool> ExistsByUsernameAsync(string username);

        // Additional methods for better functionality
        Task<IEnumerable<Player>> GetPlayersWithGameSessionsAsync();
        Task<Player?> GetPlayerWithStatisticsAsync(int playerId);
        Task<int> GetTotalPlayersCountAsync();
    }
}