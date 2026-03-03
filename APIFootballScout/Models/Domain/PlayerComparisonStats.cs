namespace APIFootballScout.Models.Domain
{
    public class PlayerComparisonStats
    {
        public int ExternalId { get; set; }
        public double GoalsPerGame { get; set; }
        public double PassAccuracy { get; set; }
        public double DribblesSucceeded { get; set; }
        public double Interceptions { get; set; }
       // public List<RecentMatch> MatchHistory { get; set; } = [];
    }
}
