using System.ComponentModel.DataAnnotations;

namespace GestaoDeCinema.Models
{
    public class Reserva
    {
        public int Id { get; set; }
        public DateTime DataCompra { get; set; }
        public int QuantidadeBilhetes { get; set; }
        public int UtilizadorId { get; set; }
        public virtual Utilizador Utilizador { get; set; }
        public int SessaoId { get; set; }
        public virtual Sessao Sessao { get; set; }
    }
}
