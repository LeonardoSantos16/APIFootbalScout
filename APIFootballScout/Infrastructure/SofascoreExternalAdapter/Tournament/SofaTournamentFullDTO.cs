namespace APIFootballScout.Infrastructure.SofascoreExternalAdapter.Tournament
{
    public class SofaTournamentFullDTO
    {
        public required SofaUniqueTournament? Details { get; set; }
        public required SofaTopPlayers? TopPlayers { get; set; }
        public required List<SofaStandingGroup>? Stading { get; set; }
        public required string Image { get; set; }
    }
}
