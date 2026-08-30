using APIFootballScout.Domain.Base;
using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Base.Exceptions;

namespace APIFootballScout.Domain.Acompanhamento.Aggregate
{
    public sealed class Dossie : AggregateRootBase<Guid>
    {
        public int JogadorId { get; private set; }
        public Guid OlheiroId { get; private set; }
        public DateTime AbertoEm { get; private set; }
        public DateTime? EncerradoEm { get; private set; }
        public StatusDossie Status { get; private set; }
        public LinhaDeBase LinhaDeBase { get; private set; }

        public Dossie(int jogadorId, Guid olheiroId, DateTime abertoEm, LinhaDeBase linhaDeBase)
            : base(Guid.NewGuid())
        {
            JogadorId = jogadorId;
            OlheiroId = olheiroId;
            AbertoEm = abertoEm;
            LinhaDeBase = linhaDeBase;
            EncerradoEm = null;
            Status = StatusDossie.Ativo;
        }

        private Dossie(Guid id, int jogadorId, Guid olheiroId, DateTime abertoEm, StatusDossie status, LinhaDeBase linhaDeBase, DateTime? encerradoEm)
            : base(id)
        {
            JogadorId = jogadorId;
            OlheiroId = olheiroId;
            AbertoEm = abertoEm;
            Status = status;
            LinhaDeBase = linhaDeBase;
            EncerradoEm = encerradoEm;
        }

        public static Dossie Restaurar(Guid id, int jogadorId, Guid olheiroId, DateTime abertoEm, StatusDossie status, LinhaDeBase linhaDeBase, DateTime? encerradoEm)
            => new(id, jogadorId, olheiroId, abertoEm, status, linhaDeBase, encerradoEm);

        public void Encerrar(DateTime encerradoEm)
        {
            if (Status is StatusDossie.Encerrado)
                throw new ConflitoDeDominioException(
                    "dossie.ja_encerrado",
                    "The dossier has already been closed.");

            if (encerradoEm <= AbertoEm)
                throw new ValorInvalidoException(
                    "dossie.data_de_encerramento_invalida",
                    "The closing date must be later than the opening date.");

            Status = StatusDossie.Encerrado;
            EncerradoEm = encerradoEm;
        }

        public void ValidarApenasLeitura()
        {
            if (Status is StatusDossie.Encerrado)
                throw new ConflitoDeDominioException(
                    "dossie.encerrado_somente_leitura",
                    "A closed dossier cannot be modified or used as the basis for a new comparison.");
        }

    }
}
