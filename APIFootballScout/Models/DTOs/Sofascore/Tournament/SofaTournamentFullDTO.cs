namespace APIFootballScout.Models.DTOs.Sofascore.Tournament
{
    public class SofaTournamentFullDTO
    {
        public SofaUniqueTournament Details { get; set; }
        public SofaTopPlayers TopPlayers { get; set; }
        public List<SofaStandingGroup> Stading { get; set; }
    }
}
