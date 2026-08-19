using APIFootballScout.Domain.Base;
using APIFootballScout.Domain.ValueObject;

namespace APIFootballScout.Domain.Aggregate
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
            Status = StatusDossie.Ativo;
        }

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
