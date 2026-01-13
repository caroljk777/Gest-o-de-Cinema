using System.ComponentModel.DataAnnotations;

namespace GestaoDeCinema.Models
{
    public class Utilizador
    {
        [Key]
        public int Id { get; set; }

        public string Nome { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string? Funcao { get; set; }
    }
}