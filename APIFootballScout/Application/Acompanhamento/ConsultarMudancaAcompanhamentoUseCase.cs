using APIFootballScout.Domain.Acompanhamento.Aggregate;
using APIFootballScout.Domain.Acompanhamento.Services;
using APIFootballScout.Domain.Acompanhamento.Specifications;
using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.CatalogoDeJogador;
using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Application.Acompanhamento
{
    public class ConsultarMudancaAcompanhamentoUseCase
    {
        private readonly IAcompanhamentoRepository _acompanhamentoRepository;
        private readonly ICatalogoDeJogador _catalogoDeJogador;
        private readonly AferidorDeMudanca _aferidor;
        private readonly JogadorPossuiInformacoesSpecification _jogadorAcompanhavel;

        public ConsultarMudancaAcompanhamentoUseCase(
            IAcompanhamentoRepository acompanhamentoRepository,
            ICatalogoDeJogador catalogoDeJogador,
            AferidorDeMudanca aferidor)
        {
            _acompanhamentoRepository = acompanhamentoRepository;
            _catalogoDeJogador = catalogoDeJogador;
            _aferidor = aferidor;
            _jogadorAcompanhavel = new JogadorPossuiInformacoesSpecification();
        }

        public async Task<ConsultarMudancaAcompanhamentoResult> ConsultarMudancaAcompanhamento(
            ConsultarMudancaAcompanhamentoRequest request,
            CancellationToken cancellationToken)
        {
            var acompanhamentos = await _acompanhamentoRepository.ObterPorIdAsync(request.OlheiroId, request.JogadorId, cancellationToken);

            var acompanhamento = BuscarAcompanhamentoPrincipal(acompanhamentos);
            acompanhamento.ValidarApenasLeitura();

            var perfilDoJogadorAtualmente = GarantirPerfilAcompanhavel(
                await BuscarPerfilDoJogadorAtual(request, acompanhamento, cancellationToken));

            var mudancaClube = new MudancaDeClube(acompanhamento.LinhaDeBase.Clube, perfilDoJogadorAtualmente.Clube!);
            var mudancaMinutagem = new MudancaDeMinutagem(acompanhamento.LinhaDeBase.Minutagem, new Minutagem(perfilDoJogadorAtualmente.MinutosJogados, perfilDoJogadorAtualmente.Recorte));

            var clube = _aferidor.Aferir(mudancaClube);
            var minutagem = _aferidor.Aferir(mudancaMinutagem);
            var valorDeMercado = _aferidor.AferirValorDeMercado(acompanhamento.LinhaDeBase.ValorDeMercado, perfilDoJogadorAtualmente.ValorDeMercado);
            var janelaDeComparacao = new JanelaDaComparacao(acompanhamento.LinhaDeBase.MedidaEm, perfilDoJogadorAtualmente.LidoEm);

            return new ConsultarMudancaAcompanhamentoResult(acompanhamento.Id, perfilDoJogadorAtualmente.JogadorId, janelaDeComparacao, clube, valorDeMercado, minutagem);
        }

        private static Dossie BuscarAcompanhamentoPrincipal(List<Dossie> acompanhamentos) => acompanhamentos
                .OrderByDescending(ac => ac.Status == StatusDossie.Ativo)
                .FirstOrDefault() ?? throw new RecursoNaoEncontradoException("acompanhamento.dossie_nao_encontrado", "olheiro não possui acompanhamento com esse jogador");

        private async Task<PerfilDoJogador?> BuscarPerfilDoJogadorAtual(ConsultarMudancaAcompanhamentoRequest request, Dossie acompanhamento, CancellationToken cancellationToken)
        {
            var linhaDeBaseMinutagem = acompanhamento.LinhaDeBase.Minutagem;
            return await _catalogoDeJogador.ObterPerfilDoJogador(request.JogadorId, new Recorte(linhaDeBaseMinutagem.Recorte.CompeticaoId,
                                                                                                     linhaDeBaseMinutagem.Recorte.TemporadaId,
                                                                                                     linhaDeBaseMinutagem.Recorte.Contexto), cancellationToken);
        }

        private PerfilDoJogador GarantirPerfilAcompanhavel(PerfilDoJogador? perfilDoJogador)
        {
            if (perfilDoJogador is null)
            {
                throw new RecursoNaoEncontradoException(
                    "jogador.perfil_nao_encontrado",
                    "perfil de jogador não encontrado");
            }

            if (!_jogadorAcompanhavel.IsSatisfiedBy(perfilDoJogador))
            {
                throw new RegraDeNegocioException(
                    "jogador.informacoes_insuficientes",
                    "The player does not have the minimum information required to serve as a comparison baseline.");
            }

            return perfilDoJogador;
        }
    }
}
