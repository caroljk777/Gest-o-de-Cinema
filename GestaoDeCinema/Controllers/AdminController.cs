using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GestaoDeCinema.BD;
using Microsoft.EntityFrameworkCore;

namespace GestaoDeCinema.Controllers
{
    // Apenas quem tem a "etiqueta" de Administrador pode entrar aqui
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly CinemaContext _context;

        public AdminController(CinemaContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Buscar todos os filmes para mostrar no dashboard
            var filmes = await _context.Filmes.ToListAsync();
            return View(filmes);
        }
    }
}