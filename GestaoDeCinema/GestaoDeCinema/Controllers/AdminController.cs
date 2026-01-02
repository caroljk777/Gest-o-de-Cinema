using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GestaoDeCinema.BD;
using GestaoDeCinema.Models;
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
            var viewModel = new AdminDashboardViewModel
            {
                TotalFilmes = await _context.Filmes.CountAsync(),
                TotalUtilizadores = await _context.Utilizadores.CountAsync(),
                TotalSessoes = await _context.Sessoes.CountAsync(),
                FilmesRecentes = await _context.Filmes.ToListAsync()
            };
            
            return View(viewModel);
        }
    }
}