using System.Security.Claims;

namespace GestaoDeCinema.Extensions
{
    public static class ClaimsExtensions
    {
        /// <summary>
        /// Obtém o email do utilizador autenticado a partir dos claims
        /// </summary>
        public static string? GetUserEmail(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value;
        }

        /// <summary>
        /// Obtém o nome do utilizador autenticado a partir dos claims
        /// </summary>
        public static string? GetUserName(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value;
        }

        /// <summary>
        /// Obtém a função/role do utilizador autenticado
        /// </summary>
        public static string? GetUserRole(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value;
        }

        /// <summary>
        /// Verifica se o utilizador tem uma função específica
        /// </summary>
        public static bool IsInRole(this ClaimsPrincipal user, string role)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value == role;
        }
    }
}
