using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GestaoDeCinema.Models.Attributes;

namespace GestaoDeCinema.Models
{
    public class Sessao
    {
        public int Id { get; set; }
       
        [Required]
        [DataFutura]
        [Display(Name = "Data e Hora")]
        public DateTime Hora { get; set; }

        [Required]
        [Display(Name = "Número da Sala")]
        public int Sala { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Preço do Bilhete")]
        public decimal Preco { get; set; }

        [Required]
        [Display(Name = "Capacidade da Sala")]
        public int Capacidade { get; set; } = 30;

        
        [Required(ErrorMessage = "Por favor, selecione um filme")]
        public int FilmeId { get; set; }
        public virtual Filme? Filme { get; set; }
    }
}
