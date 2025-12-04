using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; //serve para usar atributos que controlam como as classes e propriedades

namespace GestaoDeCinema.Models
{
    public class Filme
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O título do filme é obrigatório")]
        public string Titulo { get; set; }

        public string Genero { get; set; }

        [Display(Name = "Duração (min)")]
        public int Duracao { get; set; }

        public string Sinopse { get; set; }

        // --- SISTEMA DE IMAGEM ---

        // 1. Caminho da Imagem (O que vai para a Base de Dados)
        // Exemplo guardado: "\imagens\batman.jpg"
        [Display(Name = "Capa Atual")]
        public string? CapaImagem { get; set; }

        // 2. O Ficheiro em si (O que vem do formulário)
        // O [NotMapped] diz à BD: "Ignora isto, serve só para transportar o ficheiro até à pasta"
        [NotMapped]
        [Display(Name = "Carregar Nova Imagem")]
        public IFormFile? FicheiroImagem { get; set; }

        // -------------------------

        public virtual ICollection<Sessao> Sessoes { get; set; }
    }
}