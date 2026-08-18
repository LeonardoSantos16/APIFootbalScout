namespace APIFootballScout.Infrastructure.SofascoreExternalAdapter.Tournament
{
    public class SofaTournamentFullDTO
    {
        public SofaUniqueTournament Details { get; set; }
        public SofaTopPlayers TopPlayers { get; set; }
        public List<SofaStandingGroup> Stading { get; set; }
        public string Image { get; set; }
    }
}
