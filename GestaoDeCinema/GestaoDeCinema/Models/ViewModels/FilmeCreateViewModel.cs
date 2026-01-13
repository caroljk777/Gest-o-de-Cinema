using System.ComponentModel.DataAnnotations;

namespace GestaoDeCinema.Models.ViewModels
{
    public class FilmeCreateViewModel
    {
        [Required(ErrorMessage = "O título do filme é obrigatório")]
        [Display(Name = "Título")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O género é obrigatório")]
        [Display(Name = "Género")]
        public string Genero { get; set; } = string.Empty;

        [Required(ErrorMessage = "A duração é obrigatória")]
        [Range(1, 500, ErrorMessage = "A duração deve estar entre 1 e 500 minutos")]
        [Display(Name = "Duração (min)")]
        public int Duracao { get; set; }

        [Required(ErrorMessage = "A sinopse é obrigatória")]
        [Display(Name = "Sinopse")]
        public string Sinopse { get; set; } = string.Empty;

        [Display(Name = "Imagem de Capa")]
        public IFormFile? FicheiroImagem { get; set; }
    }
}
