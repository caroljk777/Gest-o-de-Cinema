using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoDeCinema.Models
{
    public class Sessao
    {
        public int Id { get; set; }
       
        [Required]
        [Display(Name = "Data e Hora")]
        public DateTime Hora { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")] // Importante para o preço funcionar
        [Display(Name = "Preço do Bilhete")]
        public decimal Preco { get; set; }

        
        public int FilmeId { get; set; }
        public virtual Filme Filme { get; set; }
    }
}
