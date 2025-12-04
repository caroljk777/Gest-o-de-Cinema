using GestaoDeCinema.BD;
using GestaoDeCinema.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GestaoDeCinema.Controllers
{
    public class HomeController : Controller
    {
        private readonly CinemaContext _context;

        public HomeController(CinemaContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Vai buscar os filmes e inclui as sessões (para mostrar os horários)
            var filmes = await _context.Filmes
                                       .Include(f => f.Sessoes)
                                       .ToListAsync();
            return View(filmes);
        }

       
        public IActionResult Privacy() { return View(); }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}