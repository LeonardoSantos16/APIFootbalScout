using APIFootballScout.Domain.Analise.Services;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Application.Analise
{
    public class ComparacaoDiretaUseCase
    {
        private readonly ICatalogoDeJogador _catalogoDeJogador;
        private readonly ComparadorDeJogadores _comparador;

        public ComparacaoDiretaUseCase(
            ICatalogoDeJogador catalogoDeJogador,
            ComparadorDeJogadores comparador)
        {
            _catalogoDeJogador = catalogoDeJogador;
            _comparador = comparador;
        }

        public Task<ComparacaoDiretaResult> Comparar(
            ComparacaoDiretaRequest request,
            CancellationToken cancellationToken)
            => throw new NotImplementedException();
    }
}
