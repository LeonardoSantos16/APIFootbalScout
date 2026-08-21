using APIFootballScout.Domain.Acompanhamento.Aggregate;
using APIFootballScout.Domain.Acompanhamento.Services;
using APIFootballScout.Domain.Acompanhamento.Specifications;
using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.CatalogoDeJogador;
using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Application.Acompanhamento
{
    public class AbrirAcompanhamentoUseCase
    {
        private readonly IAcompanhamentoRepository _acompanhamentoRepository;
        private readonly IAcompanhamentoService _acompanhamentoService;
        private readonly ICatalogoDeJogador _catalogoDeJogador;
        private readonly JogadorPossuiInformacoesSpecification _jogadorAcompanhavel;
        private readonly int _limiteObservacoes;

        public AbrirAcompanhamentoUseCase(
            IAcompanhamentoRepository acompanhamentoRepository,
            int limiteObservacoes,
            IAcompanhamentoService acompanhamentoService,
            ICatalogoDeJogador catalogoDeJogador)
        {
            _acompanhamentoRepository = acompanhamentoRepository;
            _limiteObservacoes = limiteObservacoes;
            _acompanhamentoService = acompanhamentoService;
            _catalogoDeJogador = catalogoDeJogador;
            _jogadorAcompanhavel = new JogadorPossuiInformacoesSpecification();
        }

        public async Task<AbrirAcompanhamentoResult> AbrirAcompanhamento(AbrirAcompanhamentoRequest request, CancellationToken cancellationToken)
        {
            var jogadorAcompanhado = await _acompanhamentoService.VerificarAcompanhamentoJogadorAsync(
                request.OlheiroId, request.JogadorId, cancellationToken);

            if (jogadorAcompanhado)
            {
                throw new InvalidOperationException("O jogador já está sendo acompanhado pelo olheiro.");
            }

            var limiteAtingido = await _acompanhamentoService.VerificarLimiteDeAcompanhamentosAsync(
                request.OlheiroId, _limiteObservacoes, cancellationToken);

            if (limiteAtingido)
            {
                throw new InvalidOperationException("Limite de jogadores em observação atingido.");
            }

            var recorte = new Recorte(request.CompeticaoId, request.TemporadaId, request.Contexto);

            var perfilDoJogador = await _catalogoDeJogador.ObterPerfilDoJogador(request.JogadorId, recorte, cancellationToken);

            if (perfilDoJogador is null)
            {
                throw new InvalidOperationException("Perfil do jogador não encontrado.");
            }

            if (!_jogadorAcompanhavel.IsSatisfiedBy(perfilDoJogador))
            {
                throw new InvalidOperationException("O jogador não possui as informações mínimas para servir de base comparável.");
            }

            var dossie = CriarDossie(request, perfilDoJogador);

            await _acompanhamentoRepository.AdicionarAsync(dossie, cancellationToken);

            return new AbrirAcompanhamentoResult(dossie.Id, dossie.AbertoEm, dossie.LinhaDeBase.MedidaEm);
        }

        private static Dossie CriarDossie(AbrirAcompanhamentoRequest request, PerfilDoJogador perfilDoJogador)
        {
            return new Dossie(
                jogadorId: request.JogadorId,
                olheiroId: request.OlheiroId,
                abertoEm: DateTime.UtcNow,
                new LinhaDeBase(
                    MedidaEm: perfilDoJogador.LidoEm,
                    Clube: perfilDoJogador.Clube!,
                    ValorDeMercado: perfilDoJogador.ValorDeMercado,
                    Minutagem: new Minutagem(perfilDoJogador.MinutosJogados, perfilDoJogador.Recorte)));
        }
    }
}
