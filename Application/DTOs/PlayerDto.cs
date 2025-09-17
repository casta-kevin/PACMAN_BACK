namespace Application.DTOs
{
    /// <summary>
    /// DTO para transferir datos básicos de un jugador
    /// </summary>
    public class PlayerDto
    {
        public int PlayerId { get; set; }
        public string Username { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int TotalGameSessions { get; set; }
        public int? BestScore { get; set; }
        public int? MaxLevelReached { get; set; }
    }

    /// <summary>
    /// DTO para transferir datos completos de un jugador con sus sesiones
    /// </summary>
    public class PlayerWithSessionsDto
    {
        public int PlayerId { get; set; }
        public string Username { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<GameSessionDto> GameSessions { get; set; } = new();
    }

    /// <summary>
    /// DTO para crear un nuevo jugador
    /// </summary>
    public class CreatePlayerDto
    {
        public string Username { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para actualizar un jugador
    /// </summary>
    public class UpdatePlayerDto
    {
        public int PlayerId { get; set; }
        public string Username { get; set; } = string.Empty;
    }
}