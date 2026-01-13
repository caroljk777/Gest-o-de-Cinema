using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GestaoDeCinema.BD;
using GestaoDeCinema.Models;
using Microsoft.EntityFrameworkCore;
using GestaoDeCinema.Extensions;

namespace GestaoDeCinema.Controllers
{
    [Authorize] // Qualquer utilizador autenticado pode aceder
    public class ReservasController : Controller
    {
        private readonly CinemaContext _context;
        private readonly ILogger<ReservasController> _logger;

        public ReservasController(CinemaContext context, ILogger<ReservasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Reservas - Ver minhas reservas
        public async Task<IActionResult> Index()
        {
            var userEmail = User.GetUserEmail();
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Login", "Account");

            var utilizador = await _context.Utilizadores
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (utilizador == null)
                return RedirectToAction("Login", "Account");

            var reservas = await _context.Reservas
                .Include(r => r.Sessao)
                    .ThenInclude(s => s.Filme)
                .Where(r => r.UtilizadorId == utilizador.Id)
                .OrderByDescending(r => r.DataReserva)
                .ToListAsync();

            return View(reservas);
        }

        // GET: Reservas/EscolherFilme - Iniciar processo de reserva
        public async Task<IActionResult> EscolherFilme()
        {
            // Admin não pode comprar bilhetes
            if (User.IsInRole("Administrador"))
            {
                TempData["Erro"] = "Administradores não podem comprar bilhetes. Use uma conta de utilizador normal.";
                return RedirectToAction("Index", "Admin");
            }

            var filmesDisponiveis = await _context.Filmes
                .Where(f => _context.Sessoes.Any(s => s.FilmeId == f.Id))
                .ToListAsync();

            return View(filmesDisponiveis);
        }

        // GET: Reservas/EscolherSessao/5 - Escolher sessão do filme
        public async Task<IActionResult> EscolherSessao(int? id)
        {
            // Admin não pode comprar bilhetes
            if (User.IsInRole("Administrador"))
            {
                TempData["Erro"] = "Administradores não podem comprar bilhetes.";
                return RedirectToAction("Index", "Admin");
            }

            if (id == null)
                return RedirectToAction(nameof(EscolherFilme));

            var filme = await _context.Filmes.FindAsync(id);
            if (filme == null)
                return NotFound();

            var sessoes = await _context.Sessoes
                .Where(s => s.FilmeId == id)
                .OrderBy(s => s.Hora)
                .ToListAsync();

            _logger.LogInformation("Visualizando sessões para o filme {FilmeId}", id);
            ViewBag.Filme = filme;
            return View(sessoes);
        }

        // GET: Reservas/EscolherAssento/5 - Escolher assento da sessão
    public async Task<IActionResult> EscolherAssento(int? id)
    {
        // Admin não pode comprar bilhetes
        if (User.IsInRole("Administrador"))
        {
            _logger.LogWarning("Tentativa de compra de bilhete bloqueada para o Administrador: {User}", User.Identity?.Name);
            TempData["Erro"] = "Administradores não podem comprar bilhetes. Use uma conta de utilizador normal para testar o fluxo de compra.";
            return RedirectToAction("Index", "Admin");
        }

        if (id == null)
            return RedirectToAction(nameof(EscolherFilme));

        var sessao = await _context.Sessoes
            .Include(s => s.Filme)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sessao == null)
            return NotFound();

        // Buscar lugares já reservados nesta sessão (apenas contagem)
        var lugaresReservados = await _context.Reservas
            .Where(r => r.SessaoId == id && r.EstadoReserva == "Confirmada")
            .SumAsync(r => r.NumeroLugares);

        ViewBag.LugaresDisponiveis = sessao.Capacidade - lugaresReservados;
        ViewBag.AssentosReservados = new List<string>(); // Lista vazia por enquanto
        
        return View(sessao);
    }    

        // POST: Reservas/Confirmar - Confirmar e guardar reserva
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirmar(int sessaoId, int numeroLugares, string numeroCartao, string? assentosEscolhidos = null)
    {
        var userEmail = User.GetUserEmail();
        if (string.IsNullOrEmpty(userEmail))
            return RedirectToAction("Login", "Account");

        var utilizador = await _context.Utilizadores
            .FirstOrDefaultAsync(u => u.Email == userEmail);

        if (utilizador == null)
            return RedirectToAction("Login", "Account");

        // Admin não pode comprar bilhetes
        if (User.IsInRole("Administrador"))
        {
            TempData["Erro"] = "Administradores não podem comprar bilhetes.";
            return RedirectToAction("Index", "Admin");
        }

        var sessao = await _context.Sessoes
            .Include(s => s.Filme)
            .FirstOrDefaultAsync(s => s.Id == sessaoId);

        if (sessao == null)
            return NotFound();

            // Iniciar transação para controlo de concorrência
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Verificar capacidade disponível dentro da transação
                var lugaresReservados = await _context.Reservas
                    .Where(r => r.SessaoId == sessaoId && r.EstadoReserva == "Confirmada")
                    .SumAsync(r => r.NumeroLugares);

                if (lugaresReservados + numeroLugares > sessao.Capacidade)
                {
                    TempData["Erro"] = $"Apenas {sessao.Capacidade - lugaresReservados} lugares disponíveis.";
                    return RedirectToAction(nameof(EscolherAssento), new { id = sessaoId });
                }

                // Criar reserva
        var reserva = new Reserva
        {
            UtilizadorId = utilizador.Id,
            SessaoId = sessaoId,
            NumeroLugares = numeroLugares,
            PrecoTotal = sessao.Preco * numeroLugares,
            NumeroCartao = numeroCartao,
            AssentosEscolhidos = assentosEscolhidos,
            DataReserva = DateTime.Now,
            EstadoReserva = "Confirmada"
        };

                _context.Reservas.Add(reserva);
                await _context.SaveChangesAsync();
                
                // Confirmar transação
                await transaction.CommitAsync();
                
                _logger.LogInformation("Reserva criada: {ReservaId} para sessão {SessaoId} por utilizador {Email}", 
                    reserva.Id, sessaoId, userEmail);

                TempData["Sucesso"] = "Reserva realizada com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erro ao criar reserva para sessão {SessaoId}", sessaoId);
                TempData["Erro"] = "Ocorreu um erro ao processar a reserva. Por favor, tente novamente.";
                return RedirectToAction(nameof(EscolherAssento), new { id = sessaoId });
            }
        }

        // POST: Reservas/ Cancelar/5 - Cancelar reserva
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var userEmail = User.GetUserEmail();
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Login", "Account");

            var utilizador = await _context.Utilizadores
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (utilizador == null)
                return RedirectToAction("Login", "Account");

            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r => r.Id == id && r.UtilizadorId == utilizador.Id);

            if (reserva == null)
                return NotFound();

            reserva.EstadoReserva = "Cancelada";
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Reserva cancelada com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}
