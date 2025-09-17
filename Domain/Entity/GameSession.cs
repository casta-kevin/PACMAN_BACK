using System.Text.Json.Serialization;

namespace Domain.Entity
{
    public class GameSession
    {
        public int GameSessionId { get; set; }
        public int PlayerId { get; set; }
        public int Score { get; set; }
        public int MaxLevelReached { get; set; }
        public DateTime PlayedAt { get; set; }

        // Navigation property for Player
        // JsonIgnore evita la referencia circular cuando se serializa desde Player
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public virtual Player Player { get; set; } = null!;

        public GameSession()
        {
            Score = 0;
            MaxLevelReached = 1;
            PlayedAt = DateTime.UtcNow;
        }

        public GameSession(int playerId) : this()
        {
            PlayerId = playerId;
        }

        public GameSession(int playerId, int score, int maxLevelReached) : this(playerId)
        {
            Score = score;
            MaxLevelReached = maxLevelReached;
        }
    }
}