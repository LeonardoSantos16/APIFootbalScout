namespace APIFootballScout.Configuration
{
    public class ScoutConfig
    {
        public List<int> PremiumTornament { get; set; }
        public List<int> PremiumPlayers { get; set; }

        public bool IsPremiumTournament(int tournamentId)
        {
            return PremiumTornament.Contains(tournamentId);
        }

        public bool IsPremiumPlayer(int playerId)
        {
            return PremiumPlayers.Contains(playerId);
        }
    }
}
