using APIFootballScout.Domain.Base;
using APIFootballScout.Domain.Acompanhamento.ValueObject;

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
                throw new InvalidOperationException("Não se encerra um dossiê já encerrado.");

            if (encerradoEm <= AbertoEm)
                throw new ArgumentException(
                    "A data de encerramento deve ser posterior à data de abertura.",
                    nameof(encerradoEm));

            Status = StatusDossie.Encerrado;
            EncerradoEm = encerradoEm;
        }

        public void ValidarApenasLeitura(StatusDossie statusDossie)
        {
            if (Status is StatusDossie.Encerrado)
                throw new InvalidOperationException("Não é possivel alterar informações ou criar uma nova comparação de dossies já encerrado.");
        }

    }
}
