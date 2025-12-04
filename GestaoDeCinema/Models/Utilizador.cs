using System.ComponentModel.DataAnnotations;

namespace GestaoDeCinema.Models
{
    public class Utilizador
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        public string Nome { get; set; }  // <--- Confirma que tens isto aqui!

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string Funcao { get; set; }

        // Se tiveres reservas, adiciona isto, senão pode dar erro noutros lados
        // public virtual ICollection<Reserva> Reservas { get; set; }
    }
}