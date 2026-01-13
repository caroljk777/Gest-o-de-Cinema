using System.ComponentModel.DataAnnotations;

namespace GestaoDeCinema.Models
{
    public class Filme
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O título do filme é obrigatório")]
        [Display(Name = "Título")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "O género é obrigatório")]
        [Display(Name = "Género")]
        public string Genero { get; set; }

        [Required(ErrorMessage = "A duração é obrigatória")]
        [Range(1, 500, ErrorMessage = "A duração deve estar entre 1 e 500 minutos")]
        [Display(Name = "Duração (min)")]
        public int Duracao { get; set; }

        [Required(ErrorMessage = "A sinopse é obrigatória")]
        [Display(Name = "Sinopse")]
        public string Sinopse { get; set; }

        [Display(Name = "Capa do Filme")]
        public string? CapaImagem { get; set; }


        public virtual ICollection<Sessao> Sessoes { get; set; } = new List<Sessao>();
    }
}