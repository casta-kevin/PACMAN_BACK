using Microsoft.EntityFrameworkCore;
using Domain.Entity;
using Domain.Repositories;
using Infrastructure.Persistence;

namespace Infrastructure.Repository
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly PacmanDbContext _context;

        public PlayerRepository(PacmanDbContext context)
        {
            _context = context;
        }

        public async Task<Player?> GetByIdAsync(int playerId)
        {
            return await _context.Players
                .Include(p => p.GameSessions)
                .FirstOrDefaultAsync(p => p.PlayerId == playerId);
        }

        public async Task<Player?> GetByUsernameAsync(string username)
        {
            return await _context.Players
                .Include(p => p.GameSessions)
                .FirstOrDefaultAsync(p => p.Username == username);
        }

        public async Task<IEnumerable<Player>> GetAllAsync()
        {
            return await _context.Players
                .Include(p => p.GameSessions)
                .OrderBy(p => p.Username)
                .ToListAsync();
        }

        public async Task<Player> CreateAsync(Player player)
        {
            var entity = await _context.Players.AddAsync(player);
            return entity.Entity;
        }

        public async Task<Player> UpdateAsync(Player player)
        {
            var existingPlayer = await _context.Players
                .Include(p => p.GameSessions)
                .FirstOrDefaultAsync(p => p.PlayerId == player.PlayerId);

            if (existingPlayer == null)
            {
                throw new InvalidOperationException($"Player with ID {player.PlayerId} not found.");
            }

            // Update properties
            _context.Entry(existingPlayer).CurrentValues.SetValues(player);
            
            return existingPlayer;
        }

        public async Task DeleteAsync(int playerId)
        {
            var player = await _context.Players.FindAsync(playerId);
            if (player != null)
            {
                _context.Players.Remove(player);
            }
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            return await _context.Players.AnyAsync(p => p.Username == username);
        }

        // Additional methods for better functionality
        public async Task<IEnumerable<Player>> GetPlayersWithGameSessionsAsync()
        {
            return await _context.Players
                .Include(p => p.GameSessions.OrderByDescending(gs => gs.PlayedAt))
                .ToListAsync();
        }

        public async Task<Player?> GetPlayerWithStatisticsAsync(int playerId)
        {
            return await _context.Players
                .Include(p => p.GameSessions)
                .Where(p => p.PlayerId == playerId)
                .Select(p => new Player
                {
                    PlayerId = p.PlayerId,
                    Username = p.Username,
                    CreatedAt = p.CreatedAt,
                    GameSessions = p.GameSessions.OrderByDescending(gs => gs.PlayedAt).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetTotalPlayersCountAsync()
        {
            return await _context.Players.CountAsync();
        }
    }
}