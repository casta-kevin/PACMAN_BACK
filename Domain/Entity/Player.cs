using System.Text.Json.Serialization;

namespace Domain.Entity
{
    public class Player
    {
        public int PlayerId { get; set; }
        public string Username { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Navigation property for GameSessions
        // JsonIgnore evita la referencia circular en algunos casos específicos
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public virtual ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();

        public Player()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public Player(string username) : this()
        {
            Username = username;
        }
    }
}