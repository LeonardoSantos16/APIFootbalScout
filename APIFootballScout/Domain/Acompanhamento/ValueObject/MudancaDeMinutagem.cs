namespace APIFootballScout.Domain.Acompanhamento.ValueObject
{
    public sealed record MudancaDeMinutagem(Minutagem Anterior, Minutagem Atual): MudancaQuantitativa
    {
        public int VariacaoAbsoluta()
        {
            return Math.Abs(Atual.Minutos - Anterior.Minutos);
        }
    }
}
