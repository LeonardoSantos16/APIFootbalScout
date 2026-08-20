namespace APIFootballScout.Domain.Acompanhamento.Aggregate
{
    public struct JogadorDoPerfil
    {
        public string Nome { get; set; }
        public string Posicao { get; set; }
        public string Clube { get; set; }

        public JogadorDoPerfil(string nome, string? posicao, string? clube)
        {
            Nome = nome;
            Posicao = posicao;
            Clube = clube;
        }
    }
}