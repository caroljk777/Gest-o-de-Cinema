using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestaoDeCinema.BD;
using GestaoDeCinema.Models;
using Microsoft.AspNetCore.Authorization;

namespace GestaoDeCinema.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class SessoesController : Controller
    {
        private readonly CinemaContext _context;
        private readonly ILogger<SessoesController> _logger;

        public SessoesController(CinemaContext context, ILogger<SessoesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Sessoes
        public async Task<IActionResult> Index()
        {
            var sessoes = await _context.Sessoes
                .Include(s => s.Filme)
                .OrderBy(s => s.Hora)
                .ToListAsync();
            return View(sessoes);
        }

        // GET: Sessoes/Create
        public IActionResult Create()
        {
            ViewBag.Filmes = new SelectList(_context.Filmes, "Id", "Titulo");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Sessao sessao)
        {
            // Remover validacao de campos virtuais/navegacao
            ModelState.Remove("Filme");
            
            // Tentar recuperar valores manualmente se o model binder falhar (comum em problemas de cultura/formato)
            if (sessao.Hora == default(DateTime) && !string.IsNullOrEmpty(Request.Form["Hora"]))
            {
                if (DateTime.TryParse(Request.Form["Hora"], out DateTime horaParseada))
                {
                    sessao.Hora = horaParseada;
                    ModelState.Remove("Hora"); // Remove erro de binding
                }
            }

            // Validar FilmeId
            if (sessao.FilmeId <= 0)
            {
                ModelState.AddModelError("FilmeId", "Por favor, selecione um filme");
            }
            
            if (ModelState.IsValid)
            {
                try 
                {
                    _context.Add(sessao);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("✅ Sessão criada com sucesso: ID {SessaoId}", sessao.Id);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erro ao salvar sessão no banco");
                    ModelState.AddModelError("", "Erro ao salvar na base de dados: " + ex.Message);
                }
            }
            
            // Se chegou aqui, houve erro
            ViewBag.Errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => !string.IsNullOrEmpty(e.ErrorMessage) ? e.ErrorMessage : "Valor inválido ou campo obrigatório em falta")
                .ToList();
                
            ViewBag.Filmes = new SelectList(_context.Filmes, "Id", "Titulo", sessao.FilmeId);
            return View(sessao);
        }

        // GET: Sessoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var sessao = await _context.Sessoes.FindAsync(id);
            if (sessao == null) return NotFound();

            ViewBag.Filmes = new SelectList(_context.Filmes, "Id", "Titulo", sessao.FilmeId);
            return View(sessao);
        }

        // POST: Sessoes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Sessao sessao)
        {
            if (id != sessao.Id) return NotFound();

            // Remove validacao de campo virtual
            ModelState.Remove("Filme");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sessao);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SessaoExists(sessao.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Filmes = new SelectList(_context.Filmes, "Id", "Titulo", sessao.FilmeId);
            return View(sessao);
        }

        // GET: Sessoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var sessao = await _context.Sessoes
                .Include(s => s.Filme)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sessao == null) return NotFound();

            return View(sessao);
        }

        // POST: Sessoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sessao = await _context.Sessoes.FindAsync(id);
            if (sessao != null)
            {
                _context.Sessoes.Remove(sessao);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool SessaoExists(int id)
        {
            return _context.Sessoes.Any(e => e.Id == id);
        }
    }
}
