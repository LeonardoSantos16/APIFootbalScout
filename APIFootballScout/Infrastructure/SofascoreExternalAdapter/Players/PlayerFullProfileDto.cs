namespace APIFootballScout.Infrastructure.SofascoreExternalAdapter.Players
{
    public class PlayerFullProfileDto
    {
        public SofaPlayerDetail? Details { get; set; }
        public List<SofaSeasonStatisticItem>? Stats { get; set; }
        public List<SofaTransfer>? HistoryTransfer { get; set; }
        public List<SofaNationalTeamStats>? NationalTeamStats { get; set; }
        public string? PlayerImage { get; set; }

    }
}