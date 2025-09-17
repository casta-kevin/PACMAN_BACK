using Domain.Entity;

namespace Application.Services
{
    public interface IPlayerService
    {
        // Player management
        Task<Player> CreatePlayerAsync(string username);
        Task<Player?> GetPlayerByIdAsync(int playerId);
        Task<Player?> GetPlayerByUsernameAsync(string username);
        Task<IEnumerable<Player>> GetAllPlayersAsync();
        Task<Player> UpdatePlayerAsync(Player player);
        Task<bool> DeletePlayerAsync(int playerId);
        Task<bool> PlayerExistsByUsernameAsync(string username);

        // Player statistics
        Task<Player?> GetPlayerWithStatisticsAsync(int playerId);
        Task<int> GetTotalPlayersCountAsync();
        Task<IEnumerable<Player>> GetTopPlayersAsync(int count = 10);
        
        // Validation
        Task<bool> IsValidUsernameAsync(string username);
    }
}