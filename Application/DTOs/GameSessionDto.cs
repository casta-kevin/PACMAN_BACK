namespace Application.DTOs
{
    /// <summary>
    /// DTO para transferir datos básicos de una sesión de juego
    /// </summary>
    public class GameSessionDto
    {
        public int GameSessionId { get; set; }
        public int PlayerId { get; set; }
        public string PlayerUsername { get; set; } = string.Empty;
        public int Score { get; set; }
        public int MaxLevelReached { get; set; }
        public DateTime PlayedAt { get; set; }
    }

    /// <summary>
    /// DTO para transferir datos completos de una sesión con datos del jugador
    /// </summary>
    public class GameSessionWithPlayerDto
    {
        public int GameSessionId { get; set; }
        public int PlayerId { get; set; }
        public int Score { get; set; }
        public int MaxLevelReached { get; set; }
        public DateTime PlayedAt { get; set; }
        public PlayerDto Player { get; set; } = null!;
    }

    /// <summary>
    /// DTO para crear una nueva sesión de juego
    /// </summary>
    public class CreateGameSessionDto
    {
        public int PlayerId { get; set; }
        public int Score { get; set; }
        public int MaxLevelReached { get; set; }
    }

    /// <summary>
    /// DTO para actualizar una sesión de juego
    /// </summary>
    public class UpdateGameSessionDto
    {
        public int GameSessionId { get; set; }
        public int PlayerId { get; set; }
        public int Score { get; set; }
        public int MaxLevelReached { get; set; }
        public DateTime PlayedAt { get; set; }
    }
}