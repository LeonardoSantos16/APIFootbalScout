using APIFootballScout.Domain.Acompanhamento.Services;
using APIFootballScout.Domain.CatalogoDeJogador;
using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Application.Acompanhamento
{
    public class ConsultarMudancaAcompanhamentoUseCase
    {
        private readonly IAcompanhamentoRepository _acompanhamentoRepository;
        private readonly ICatalogoDeJogador _catalogoDeJogador;
        private readonly AferidorDeMudanca _aferidor;

        public ConsultarMudancaAcompanhamentoUseCase(
            IAcompanhamentoRepository acompanhamentoRepository,
            ICatalogoDeJogador catalogoDeJogador,
            AferidorDeMudanca aferidor)
        {
            _acompanhamentoRepository = acompanhamentoRepository;
            _catalogoDeJogador = catalogoDeJogador;
            _aferidor = aferidor;
        }

        public Task<ConsultarMudancaAcompanhamentoResult> ConsultarMudancaAcompanhamento(
            ConsultarMudancaAcompanhamentoRequest request,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
