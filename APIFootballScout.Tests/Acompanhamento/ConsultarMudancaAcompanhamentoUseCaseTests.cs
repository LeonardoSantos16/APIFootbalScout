using APIFootballScout.Domain.Acompanhamento.Aggregate;
using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.SharedKernel;

namespace APIFootballScout.Tests.Acompanhamento
{
    public class ConsultarMudancaAcompanhamentoUseCaseTests
    {
        [Fact]
        public async Task Confronta_a_linha_de_base_com_a_leitura_atual_e_emite_uma_afericao_por_tipo()
        {
            // Arrange
            var ctx = new AcompanhamentoTestContext();
            var linhaDeBase = ctx.PerfilValido(clube: "Santos") with
            {
                ValorDeMercado = new Dinheiro(50_000_000_00, "EUR"),
                MinutosJogados = 900
            };
            var leituraAtual = linhaDeBase with
            {
                Clube = "Al-Hilal",
                ValorDeMercado = new Dinheiro(80_000_000_00, "EUR"),
                MinutosJogados = 1500,
                LidoEm = ctx.Agora
            };

            await ctx.ComLinhaDeBaseELeituraAtual(linhaDeBase, leituraAtual);

            // Act
            var resultado = await ctx.ConsultarMudanca()
                .ConsultarMudancaAcompanhamento(ctx.Consulta(), CancellationToken.None);

            // Assert
            var dossie = Assert.Single(ctx.Dossies.Todos);
            Assert.Equal(dossie.Id, resultado.DossieId);
            Assert.Equal(AcompanhamentoTestContext.JogadorId, resultado.JogadorId);

            var clube = Assert.IsType<MudancaDeClube>(resultado.Clube);
            Assert.Equal("Santos", clube.Anterior);
            Assert.Equal("Al-Hilal", clube.Atual);

            var valor = Assert.IsType<MudancaDeValorDeMercado>(resultado.ValorDeMercado);
            Assert.Equal(linhaDeBase.ValorDeMercado, valor.Anterior);
            Assert.Equal(leituraAtual.ValorDeMercado, valor.Atual);

            var minutagem = Assert.IsType<MudancaDeMinutagem>(resultado.Minutagem);
            Assert.Equal(900, minutagem.Anterior.Minutos);
            Assert.Equal(1500, minutagem.Atual.Minutos);
        }

        [Fact]
        public async Task A_janela_vai_da_medicao_da_linha_de_base_ate_a_leitura_atual()
        {
            // R2.7 - o intervalo decorrido integra o resultado da comparacao.

            // Arrange
            var ctx = new AcompanhamentoTestContext();
            var linhaDeBase = ctx.PerfilValido(lidoEm: ctx.Agora.AddDays(-30));
            var leituraAtual = linhaDeBase with { Clube = "Al-Hilal", LidoEm = ctx.Agora };

            await ctx.ComLinhaDeBaseELeituraAtual(linhaDeBase, leituraAtual);

            // Act
            var resultado = await ctx.ConsultarMudanca()
                .ConsultarMudancaAcompanhamento(ctx.Consulta(), CancellationToken.None);

            // Assert
            Assert.Equal(linhaDeBase.LidoEm, resultado.Janela.De);
            Assert.Equal(leituraAtual.LidoEm, resultado.Janela.Ate);
            Assert.Equal(TimeSpan.FromDays(30), resultado.Janela.Duracao());
        }

        [Fact]
        public async Task Leitura_anterior_a_linha_de_base_recusa_a_consulta()
        {
            // R2.7 - sem intervalo decorrido nao ha janela, e sem janela nao ha resultado.

            // Arrange
            var ctx = new AcompanhamentoTestContext();
            var linhaDeBase = ctx.PerfilValido(lidoEm: ctx.Agora);
            var leituraAtual = linhaDeBase with { LidoEm = ctx.Agora.AddDays(-1) };

            await ctx.ComLinhaDeBaseELeituraAtual(linhaDeBase, leituraAtual);

            // Act
            var excecao = await Assert.ThrowsAsync<ValorInvalidoException>(
                () => ctx.ConsultarMudanca()
                    .ConsultarMudancaAcompanhamento(ctx.Consulta(), CancellationToken.None));

            // Assert
            Assert.Equal("janela_da_comparacao.intervalo_invalido", excecao.Codigo);
        }

        [Fact]
        public async Task Mudancas_aquem_do_limiar_saem_como_sem_mudanca_relevante()
        {
            // R2.1 e R2.2 - limiares de 10% e 180 min, aplicados por tipo.

            // Arrange
            var ctx = new AcompanhamentoTestContext();
            var linhaDeBase = ctx.PerfilValido(clube: "Santos") with
            {
                ValorDeMercado = new Dinheiro(50_000_000_00, "EUR"),
                MinutosJogados = 900
            };
            var leituraAtual = linhaDeBase with
            {
                ValorDeMercado = new Dinheiro(52_500_000_00, "EUR"),  // +5%
                MinutosJogados = 1000,                                // +100 min
                LidoEm = ctx.Agora
            };

            await ctx.ComLinhaDeBaseELeituraAtual(linhaDeBase, leituraAtual);

            // Act
            var resultado = await ctx.ConsultarMudanca()
                .ConsultarMudancaAcompanhamento(ctx.Consulta(), CancellationToken.None);

            // Assert
            Assert.IsType<SemMudancaRelevante>(resultado.ValorDeMercado);
            Assert.IsType<SemMudancaRelevante>(resultado.Minutagem);
            Assert.IsType<SemMudancaRelevante>(resultado.Clube);
        }

        [Fact]
        public async Task Temporada_virada_torna_a_minutagem_indisponivel_sem_afetar_os_demais_tipos()
        {
            // R2.5 e R2.6 - a incomparabilidade e por tipo, nao da consulta inteira.

            // Arrange
            var ctx = new AcompanhamentoTestContext();
            var outraTemporada = AcompanhamentoTestContext.RecortePadrao with { TemporadaId = 71234 };

            var linhaDeBase = ctx.PerfilValido(clube: "Santos") with { MinutosJogados = 2400 };
            var leituraAtual = linhaDeBase with
            {
                Clube = "Al-Hilal",
                ValorDeMercado = new Dinheiro(80_000_000_00, "EUR"),
                MinutosJogados = 200,
                Recorte = outraTemporada,
                LidoEm = ctx.Agora
            };

            await ctx.ComLinhaDeBaseELeituraAtual(linhaDeBase, leituraAtual);

            // Act
            var resultado = await ctx.ConsultarMudanca()
                .ConsultarMudancaAcompanhamento(ctx.Consulta(), CancellationToken.None);

            // Assert
            var indisponivel = Assert.IsType<Indisponivel>(resultado.Minutagem);
            Assert.Equal(MotivoDeIndisponibilidade.TemporadaVirada, indisponivel.Motivo);

            Assert.IsType<MudancaDeClube>(resultado.Clube);
            Assert.IsType<MudancaDeValorDeMercado>(resultado.ValorDeMercado);
        }

        [Fact]
        public async Task Moeda_distinta_torna_o_valor_de_mercado_indisponivel_sem_derrubar_a_consulta()
        {
            // R2.4 e R2.6 - a recusa de comparar moedas vira afericao indisponivel,
            // nunca excecao que interrompe os demais tipos.

            // Arrange
            var ctx = new AcompanhamentoTestContext();
            var linhaDeBase = ctx.PerfilValido(clube: "Santos") with
            {
                ValorDeMercado = new Dinheiro(50_000_000_00, "EUR")
            };
            var leituraAtual = linhaDeBase with
            {
                Clube = "Al-Hilal",
                ValorDeMercado = new Dinheiro(80_000_000_00, "USD"),
                LidoEm = ctx.Agora
            };

            await ctx.ComLinhaDeBaseELeituraAtual(linhaDeBase, leituraAtual);

            // Act
            var resultado = await ctx.ConsultarMudanca()
                .ConsultarMudancaAcompanhamento(ctx.Consulta(), CancellationToken.None);

            // Assert
            var indisponivel = Assert.IsType<Indisponivel>(resultado.ValorDeMercado);
            Assert.Equal(MotivoDeIndisponibilidade.MoedaInesperada, indisponivel.Motivo);

            Assert.IsType<MudancaDeClube>(resultado.Clube);
        }

        [Fact]
        public async Task A_leitura_atual_e_pedida_no_recorte_da_linha_de_base()
        {
            // A comparabilidade depende de pedir a fonte o mesmo recorte que originou
            // a linha de base, e nao um recorte escolhido na consulta.

            // Arrange
            var ctx = new AcompanhamentoTestContext();
            ctx.SeedPerfil();
            await ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(), CancellationToken.None);

            ctx.Catalogo.Perfil = ctx.PerfilValido(clube: "Al-Hilal", lidoEm: ctx.Agora);

            // Act
            await ctx.ConsultarMudanca()
                .ConsultarMudancaAcompanhamento(ctx.Consulta(), CancellationToken.None);

            // Assert
            Assert.Equal(AcompanhamentoTestContext.RecortePadrao, ctx.Catalogo.UltimoRecorte);
        }

        [Fact]
        public async Task Sem_dossie_para_o_olheiro_recusa_a_consulta()
        {
            // Arrange
            var ctx = new AcompanhamentoTestContext();
            ctx.SeedPerfil();

            // Act
            var excecao = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
                () => ctx.ConsultarMudanca()
                    .ConsultarMudancaAcompanhamento(ctx.Consulta(), CancellationToken.None));

            // Assert
            Assert.Equal("acompanhamento.dossie_nao_encontrado", excecao.Codigo);
            Assert.Equal(0, ctx.Catalogo.Chamadas);
        }

        [Fact]
        public async Task O_dossie_de_outro_olheiro_nao_e_visivel_na_consulta()
        {
            // Arrange
            var ctx = new AcompanhamentoTestContext();
            ctx.SeedPerfil();
            await ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(), CancellationToken.None);

            // Act
            var excecao = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
                () => ctx.ConsultarMudanca().ConsultarMudancaAcompanhamento(
                    ctx.Consulta(olheiroId: Guid.NewGuid()), CancellationToken.None));

            // Assert
            Assert.Equal("acompanhamento.dossie_nao_encontrado", excecao.Codigo);
        }

        [Fact]
        public async Task Dossie_encerrado_nao_sustenta_nova_afericao()
        {
            // R1 - o dossie encerrado permanece legivel, mas nao serve de base
            // para nova comparacao.

            // Arrange
            var ctx = new AcompanhamentoTestContext();
            ctx.SeedPerfil();
            await ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(), CancellationToken.None);

            var dossie = Assert.Single(ctx.Dossies.Todos);
            dossie.Encerrar(dossie.AbertoEm.AddDays(30));

            ctx.Catalogo.Perfil = ctx.PerfilValido(clube: "Al-Hilal", lidoEm: ctx.Agora);

            // Act
            var excecao = await Assert.ThrowsAsync<ConflitoDeDominioException>(
                () => ctx.ConsultarMudanca()
                    .ConsultarMudancaAcompanhamento(ctx.Consulta(), CancellationToken.None));

            // Assert
            Assert.Equal("dossie.encerrado_somente_leitura", excecao.Codigo);
        }

        [Fact]
        public async Task Apos_reacompanhar_a_consulta_usa_a_linha_de_base_do_dossie_ativo()
        {
            // O reacompanhamento deixa um dossie encerrado e um ativo para o mesmo par
            // olheiro/jogador. A consulta afere contra o ativo.

            // Arrange
            var ctx = new AcompanhamentoTestContext();
            ctx.SeedPerfil(clube: "Santos");
            await ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(), CancellationToken.None);

            var primeiro = Assert.Single(ctx.Dossies.Todos);
            primeiro.Encerrar(primeiro.AbertoEm.AddDays(30));

            ctx.SeedPerfil(clube: "Al-Hilal", lidoEm: ctx.Agora.AddDays(-10));
            await ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(), CancellationToken.None);

            var ativo = Assert.Single(ctx.Dossies.Todos, d => d.Status is StatusDossie.Ativo);
            ctx.Catalogo.Perfil = ctx.PerfilValido(clube: "Real Madrid", lidoEm: ctx.Agora);

            // Act
            var resultado = await ctx.ConsultarMudanca()
                .ConsultarMudancaAcompanhamento(ctx.Consulta(), CancellationToken.None);

            // Assert
            Assert.Equal(ativo.Id, resultado.DossieId);

            var clube = Assert.IsType<MudancaDeClube>(resultado.Clube);
            Assert.Equal("Al-Hilal", clube.Anterior);
            Assert.Equal("Real Madrid", clube.Atual);
        }

        [Fact]
        public async Task Perfil_ausente_no_catalogo_recusa_a_consulta()
        {
            // A falta do perfil na fonte e erro da operacao, nao afericao indisponivel.

            // Arrange
            var ctx = new AcompanhamentoTestContext();
            ctx.SeedPerfil();
            await ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(), CancellationToken.None);

            ctx.Catalogo.Perfil = null;

            // Act
            var excecao = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
                () => ctx.ConsultarMudanca()
                    .ConsultarMudancaAcompanhamento(ctx.Consulta(), CancellationToken.None));

            // Assert
            Assert.Equal("jogador.perfil_nao_encontrado", excecao.Codigo);
        }

        [Fact]
        public async Task Perfil_sem_informacoes_minimas_recusa_a_consulta()
        {
            // Arrange
            var ctx = new AcompanhamentoTestContext();
            ctx.SeedPerfil();
            await ctx.AbrirDossie().AbrirAcompanhamento(ctx.Pedido(), CancellationToken.None);

            ctx.Catalogo.Perfil = ctx.PerfilValido(lidoEm: ctx.Agora) with { Clube = "   " };

            // Act
            var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(
                () => ctx.ConsultarMudanca()
                    .ConsultarMudancaAcompanhamento(ctx.Consulta(), CancellationToken.None));

            // Assert
            Assert.Equal("jogador.informacoes_insuficientes", excecao.Codigo);
        }

        [Fact]
        public async Task A_consulta_nao_altera_o_dossie()
        {
            // F2 - a comparacao e calculada na leitura e nada persiste.

            // Arrange
            var ctx = new AcompanhamentoTestContext();
            var linhaDeBase = ctx.PerfilValido(clube: "Santos");
            var leituraAtual = linhaDeBase with { Clube = "Al-Hilal", LidoEm = ctx.Agora };

            await ctx.ComLinhaDeBaseELeituraAtual(linhaDeBase, leituraAtual);

            var linhaDeBaseAntes = Assert.Single(ctx.Dossies.Todos).LinhaDeBase;

            // Act
            await ctx.ConsultarMudanca()
                .ConsultarMudancaAcompanhamento(ctx.Consulta(), CancellationToken.None);

            // Assert
            var depois = Assert.Single(ctx.Dossies.Todos);
            Assert.Equal(linhaDeBaseAntes, depois.LinhaDeBase);
            Assert.Equal(StatusDossie.Ativo, depois.Status);
            Assert.Equal(0, ctx.Dossies.Atualizacoes);
        }
    }
}
