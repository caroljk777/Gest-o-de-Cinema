using System.ComponentModel.DataAnnotations;

namespace GestaoDeCinema.Models
{
    public class Utilizador
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        public string Nome { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string Funcao { get; set; }

        
    }
}