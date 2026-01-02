using System.ComponentModel.DataAnnotations;

namespace GestaoDeCinema.Models
{
    public class Reserva
    {
        public int Id { get; set; }
        public DateTime DataCompra { get; set; }
        public int QuantidadeBilhetes { get; set; }
        public string Assento { get; set; } = string.Empty; // Ex: "A1", "B5", "C12"
        public decimal PrecoTotal { get; set; }
        public string Status { get; set; } = "Confirmada"; // Confirmada, Cancelada
        
        public int UtilizadorId { get; set; }
        public virtual Utilizador Utilizador { get; set; }
        public int SessaoId { get; set; }
        public virtual Sessao Sessao { get; set; }
    }
}
