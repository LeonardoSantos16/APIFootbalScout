namespace APIFootballScout.Models.Domain
{
    public class Player
    {
        public Guid Id { get; set; }
        public int ExternalId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
