using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoDeCinema.Models
{
    public class Reserva
    {
        public int Id { get; set; }
        
        [Required]
        public int UtilizadorId { get; set; }
        public virtual Utilizador? Utilizador { get; set; }
        
        [Required]
        public int SessaoId { get; set; }
        public virtual Sessao? Sessao { get; set; }
        [Column("DataCompra")]
        public DateTime DataReserva { get; set; } = DateTime.Now;
        
        [Required(ErrorMessage = "Número de lugares é obrigatório")]
        [Range(1, 10, ErrorMessage = "Número de bilhetes deve estar entre 1 e 10")]
        [Column("QuantidadeBilhetes")]
        public int NumeroLugares { get; set; }
        
        // Colunas novas (podem não existir na BD antiga)
        public string? AssentosEscolhidos { get; set; }
        public decimal PrecoTotal { get; set; }
        
        [Required(ErrorMessage = "Número do cartão é obrigatório")]
        public string NumeroCartao { get; set; } = string.Empty;
        
        public string EstadoReserva { get; set; } = "Confirmada"; 
    }
}
