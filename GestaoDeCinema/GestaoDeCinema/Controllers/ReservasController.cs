using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GestaoDeCinema.BD;
using GestaoDeCinema.Models;
using Microsoft.EntityFrameworkCore;

namespace GestaoDeCinema.Controllers
{
    [Authorize] // Qualquer utilizador autenticado pode aceder
    public class ReservasController : Controller
    {
        private readonly CinemaContext _context;

        public ReservasController(CinemaContext context)
        {
            _context = context;
        }

        // GET: Reservas - Ver minhas reservas
        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity.Name;
            var utilizador = await _context.Utilizadores
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (utilizador == null)
                return RedirectToAction("Login", "Account");

            var reservas = await _context.Reservas
                .Include(r => r.Sessao)
                    .ThenInclude(s => s.Filme)
                .Where(r => r.UtilizadorId == utilizador.Id)
                .OrderByDescending(r => r.DataCompra)
                .ToListAsync();

            return View(reservas);
        }

        // GET: Reservas/EscolherFilme - Iniciar processo de reserva
        public async Task<IActionResult> EscolherFilme()
        {
            var filmesDisponiveis = await _context.Filmes
                .Where(f => _context.Sessoes.Any(s => s.FilmeId == f.Id))
                .ToListAsync();

            return View(filmesDisponiveis);
        }

        // GET: Reservas/EscolherSessao/5 - Escolher sessão do filme
        public async Task<IActionResult> EscolherSessao(int? id)
        {
            if (id == null)
                return RedirectToAction(nameof(EscolherFilme));

            var filme = await _context.Filmes.FindAsync(id);
            if (filme == null)
                return NotFound();

            var sessoes = await _context.Sessoes
                .Where(s => s.FilmeId == id)
                .OrderBy(s => s.Hora)
                .ToListAsync();

            ViewBag.Filme = filme;
            return View(sessoes);
        }

        // GET: Reservas/EscolherAssento/5 - Escolher assento da sessão
        public async Task<IActionResult> EscolherAssento(int? id)
        {
            if (id == null)
                return RedirectToAction(nameof(EscolherFilme));

            var sessao = await _context.Sessoes
                .Include(s => s.Filme)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sessao == null)
                return NotFound();

            // Buscar assentos já reservados nesta sessão
            var assentosReservados = await _context.Reservas
                .Where(r => r.SessaoId == id && r.Status == "Confirmada")
                .Select(r => r.Assento)
                .ToListAsync();

            ViewBag.AssentosReservados = assentosReservados;
            return View(sessao);
        }

        // POST: Reservas/Confirmar - Confirmar e guardar reserva
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmar(int sessaoId, string assento)
        {
            var userEmail = User.Identity.Name;
            var utilizador = await _context.Utilizadores
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (utilizador == null)
                return RedirectToAction("Login", "Account");

            var sessao = await _context.Sessoes
                .Include(s => s.Filme)
                .FirstOrDefaultAsync(s => s.Id == sessaoId);

            if (sessao == null)
                return NotFound();

            // Verificar se assento já está reservado
            var assentoJaReservado = await _context.Reservas
                .AnyAsync(r => r.SessaoId == sessaoId && r.Assento == assento && r.Status == "Confirmada");

            if (assentoJaReservado)
            {
                TempData["Erro"] = "Este assento já foi reservado. Por favor escolha outro.";
                return RedirectToAction(nameof(EscolherAssento), new { id = sessaoId });
            }

            // Criar reserva
            var reserva = new Reserva
            {
                UtilizadorId = utilizador.Id,
                SessaoId = sessaoId,
                Assento = assento,
                QuantidadeBilhetes = 1, // Por agora, 1 bilhete por reserva
                PrecoTotal = sessao.Preco,
                DataCompra = DateTime.Now,
                Status = "Confirmada"
            };

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Reserva realizada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Reservas/ Cancelar/5 - Cancelar reserva
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var userEmail = User.Identity.Name;
            var utilizador = await _context.Utilizadores
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (utilizador == null)
                return RedirectToAction("Login", "Account");

            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r => r.Id == id && r.UtilizadorId == utilizador.Id);

            if (reserva == null)
                return NotFound();

            reserva.Status = "Cancelada";
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Reserva cancelada com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}
