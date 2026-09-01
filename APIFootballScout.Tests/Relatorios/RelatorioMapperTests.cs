using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.RelatorioScouting.Agreggate;
using APIFootballScout.Domain.RelatorioScouting.Specifications;
using APIFootballScout.Domain.RelatorioScouting.ValueObject;
using APIFootballScout.Infrastructure.Persistence.Documents;
using APIFootballScout.Infrastructure.Persistence.Mappers;

namespace APIFootballScout.Tests.Relatorios
{
    public class RelatorioMapperTests
    {
        // Fuso deslocado de proposito: o documento guarda DateTime, o agregado guarda
        // DateTimeOffset. A conversao tem de preservar o instante, nao o relogio local.
        private static readonly DateTimeOffset ObservadoEm = new(2026, 8, 20, 12, 0, 0, TimeSpan.FromHours(-3));
        private static readonly DateTimeOffset AbertoEm = new(2026, 8, 25, 12, 0, 0, TimeSpan.Zero);
        private static readonly DateTime FinalizadoEm = new(2026, 8, 25, 14, 0, 0, DateTimeKind.Utc);

        private static readonly RelatorioComConteudoMinimoSpecification SemExigenciaDeConteudo = new(0, 0, 0);

        private static Relatorio NovoRascunho() => Relatorio.AbrirRascunho(
            jogadorId: 42,
            olheiroId: Guid.NewGuid(),
            texto: "Bom posicionamento sem bola.",
            observadoEm: ObservadoEm,
            agora: AbertoEm);

        private static Relatorio Finalizado()
        {
            var relatorio = NovoRascunho();
            relatorio.AtribuirNota(new Nota(8.5m));
            relatorio.AdicionarPontoPositivo("Leitura de jogo");
            relatorio.AdicionarPontoNegativo("Fragilidade defensiva");
            relatorio.DefinirParecer(Parecer.Contratar);
            relatorio.Finalizar(SemExigenciaDeConteudo, FinalizadoEm);

            return relatorio;
        }

        private static Relatorio RoundTrip(Relatorio relatorio)
            => RelatorioMapper.MapToDomain(RelatorioMapper.MapToEntity(relatorio));

        [Fact]
        public void Rascunho_faz_round_trip_pelo_documento()
        {
            // Arrange
            var original = NovoRascunho();
            original.AtribuirNota(new Nota(7.5m));
            original.AdicionarPontoPositivo("Leitura de jogo");
            original.AdicionarPontoNegativo("Fragilidade defensiva");
            original.DefinirParecer(Parecer.Monitorar);

            // Act
            var restaurado = RoundTrip(original);

            // Assert
            Assert.Equal(original.Id, restaurado.Id);
            Assert.Equal(original.JogadorId, restaurado.JogadorId);
            Assert.Equal(original.OlheiroId, restaurado.OlheiroId);
            Assert.Equal(StatusRelatorio.Rascunho, restaurado.Status);
            Assert.Equal(original.Texto, restaurado.Texto);
            Assert.Equal(new Nota(7.5m), restaurado.Nota);
            Assert.Equal(["Leitura de jogo"], restaurado.PontosPositivos);
            Assert.Equal(["Fragilidade defensiva"], restaurado.PontosNegativos);
            Assert.Equal(Parecer.Monitorar, restaurado.Parecer);
            Assert.Null(restaurado.FinalizadoEm);
            Assert.Null(restaurado.CorrigeRelatorioId);
        }

        [Fact]
        public void Rascunho_restaurado_continua_editavel()
        {
            // Arrange
            var restaurado = RoundTrip(NovoRascunho());

            // Act
            restaurado.AlterarTexto("Reavaliado apos o classico.");

            // Assert
            Assert.Equal("Reavaliado apos o classico.", restaurado.Texto);
        }

        [Fact]
        public void Relatorio_finalizado_restaurado_permanece_imutavel()
        {
            // R5.1 na fronteira do banco: o estado que veio do documento e que
            // decide a imutabilidade, nao a transicao ocorrida em memoria.

            // Arrange
            var restaurado = RoundTrip(Finalizado());

            // Act
            var erro = Assert.Throws<ConflitoDeDominioException>(
                () => restaurado.AlterarTexto("Reavaliado apos o classico."));

            // Assert
            Assert.Equal("relatorio.ja_finalizado", erro.Codigo);
            Assert.Equal(StatusRelatorio.Finalizado, restaurado.Status);
            Assert.Equal("Bom posicionamento sem bola.", restaurado.Texto);
            Assert.Equal(new DateTimeOffset(FinalizadoEm), restaurado.FinalizadoEm);
        }

        [Fact]
        public void As_datas_sobrevivem_ao_round_trip_em_UTC()
        {
            // Arrange
            var original = Finalizado();

            // Act
            var documento = RelatorioMapper.MapToEntity(original);
            var restaurado = RelatorioMapper.MapToDomain(documento);

            // Assert — o documento guarda o instante em UTC.
            Assert.Equal(DateTimeKind.Utc, documento.ObservadoEm.Kind);
            Assert.Equal(ObservadoEm.UtcDateTime, documento.ObservadoEm);
            Assert.Equal(FinalizadoEm, documento.FinalizadoEm);

            // Assert — o agregado volta com o mesmo instante.
            Assert.Equal(ObservadoEm, restaurado.ObservadoEm);
            Assert.Equal(TimeSpan.Zero, restaurado.ObservadoEm.Offset);
            Assert.Equal(new DateTimeOffset(FinalizadoEm), restaurado.EscritoEm);
        }

        [Fact]
        public void Rascunho_sem_nota_e_sem_parecer_sobrevive_ao_round_trip()
        {
            // Arrange — nota e parecer so existem a partir da conclusao (R5.7).
            var original = NovoRascunho();

            // Act
            var documento = RelatorioMapper.MapToEntity(original);
            var restaurado = RelatorioMapper.MapToDomain(documento);

            // Assert
            Assert.Null(documento.Nota);
            Assert.Null(documento.Parecer);
            Assert.Null(documento.FinalizadoEm);

            Assert.Null(restaurado.Nota);
            Assert.Null(restaurado.Parecer);
            Assert.Empty(restaurado.PontosPositivos);
            Assert.Empty(restaurado.PontosNegativos);
        }

        [Fact]
        public void O_documento_declara_o_status_de_forma_legivel()
        {
            // O status vai para o banco como texto: uma reordenacao do enum nao
            // pode reinterpretar documentos ja gravados.

            // Arrange & Act
            var rascunho = RelatorioMapper.MapToEntity(NovoRascunho());
            var finalizado = RelatorioMapper.MapToEntity(Finalizado());

            // Assert
            Assert.Equal("Rascunho", rascunho.Status);
            Assert.Equal("Finalizado", finalizado.Status);
        }

        [Theory]
        [InlineData("5")]
        [InlineData("99")]
        [InlineData("Encerrado")]
        [InlineData("")]
        public void Documento_com_status_desconhecido_e_recusado(string status)
        {
            // Um status que nao existe nao pode virar um relatorio: como a
            // edicao so olha para Finalizado, um valor indefinido devolveria um
            // relatorio finalizado editavel (R5.1).

            // Arrange
            var documento = RelatorioMapper.MapToEntity(Finalizado());
            documento.Status = status;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => RelatorioMapper.MapToDomain(documento));
        }

        [Theory]
        [InlineData("7")]
        [InlineData("Vender")]
        public void Documento_com_parecer_desconhecido_e_recusado(string parecer)
        {
            // Arrange
            var documento = RelatorioMapper.MapToEntity(Finalizado());
            documento.Parecer = parecer;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => RelatorioMapper.MapToDomain(documento));
        }
    }
}
