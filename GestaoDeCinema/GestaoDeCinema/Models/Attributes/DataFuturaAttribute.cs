using System.ComponentModel.DataAnnotations;

namespace GestaoDeCinema.Models.Attributes
{
    /// <summary>
    /// Valida que a data da sessão não está no passado
    /// Permite uma margem de 5 minutos para evitar problemas de sincronização
    /// </summary>
    public class DataFuturaAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime dateTime)
            {
                // Permite datas a partir de 5 minutos atrás para evitar problemas de sincronização
                var limiteMinimo = DateTime.Now.AddMinutes(-5);
                
                if (dateTime < limiteMinimo)
                {
                    return new ValidationResult("A data da sessão não pode estar no passado. Por favor, selecione uma data e hora futuras.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
