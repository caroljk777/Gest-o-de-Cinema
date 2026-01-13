using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GestaoDeCinema.BD;
using GestaoDeCinema.Models;
using Microsoft.EntityFrameworkCore;
using GestaoDeCinema.Extensions;

namespace GestaoDeCinema.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UtilizadoresController : Controller
    {
        private readonly CinemaContext _context;

        public UtilizadoresController(CinemaContext context)
        {
            _context = context;
        }

        // GET: Utilizadores
        public async Task<IActionResult> Index()
        {
            var utilizadores = await _context.Utilizadores.ToListAsync();
            return View(utilizadores);
        }

        // GET: Utilizadores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var utilizador = await _context.Utilizadores
                .FirstOrDefaultAsync(m => m.Id == id);
            if (utilizador == null)
            {
                return NotFound();
            }

            return View(utilizador);
        }

        // GET: Utilizadores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var utilizador = await _context.Utilizadores.FindAsync(id);
            if (utilizador == null)
            {
                return NotFound();
            }

            // PROTEÇÃO: Não permitir editar conta admin
            if (utilizador.Email.ToLower() == "admin@cinema.com")
            {
                TempData["Erro"] = "A conta de administrador não pode ser editada.";
                return RedirectToAction(nameof(Index));
            }

            return View(utilizador);
        }

        // POST: Utilizadores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Email,Password,Funcao")] Utilizador utilizador)
        {
            if (id != utilizador.Id)
            {
                return NotFound();
            }

            // PROTEÇÃO: Não permitir editar conta admin
            var utilizadorOriginal = await _context.Utilizadores.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (utilizadorOriginal != null && utilizadorOriginal.Email.ToLower() == "admin@cinema.com")
            {
                TempData["Erro"] = "A conta de administrador não pode ser editada.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar se o email já existe em outro utilizador
                    var emailExists = await _context.Utilizadores
                        .AnyAsync(u => u.Email == utilizador.Email && u.Id != id);
                    
                    if (emailExists)
                    {
                        ModelState.AddModelError("Email", "Este email já está em uso por outro utilizador.");
                        return View(utilizador);
                    }

                    _context.Update(utilizador);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UtilizadorExists(utilizador.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(utilizador);
        }

        // GET: Utilizadores/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var utilizador = await _context.Utilizadores
                .FirstOrDefaultAsync(m => m.Id == id);
            if (utilizador == null)
            {
                return NotFound();
            }

            // PROTEÇÃO: Não permitir eliminar conta admin
            if (utilizador.Email.ToLower() == "admin@cinema.com")
            {
                TempData["Erro"] = "A conta de administrador não pode ser eliminada.";
                return RedirectToAction(nameof(Index));
            }

            // Não permitir eliminar o próprio utilizador logado
            var userEmail = User.GetUserEmail();
            if (utilizador.Email == userEmail)
            {
                TempData["Erro"] = "Não pode eliminar a sua própria conta.";
                return RedirectToAction(nameof(Index));
            }

            return View(utilizador);
        }

        // POST: Utilizadores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var utilizador = await _context.Utilizadores.FindAsync(id);
            
            if (utilizador != null)
            {
                // PROTEÇÃO: Não permitir eliminar conta admin
                if (utilizador.Email.ToLower() == "admin@cinema.com")
                {
                    TempData["Erro"] = "A conta de administrador não pode ser eliminada.";
                    return RedirectToAction(nameof(Index));
                }

                // Verificação adicional de segurança
                var userEmail = User.GetUserEmail();
                if (utilizador.Email == userEmail)
                {
                    TempData["Erro"] = "Não pode eliminar a sua própria conta.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Utilizadores.Remove(utilizador);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UtilizadorExists(int id)
        {
            return _context.Utilizadores.Any(e => e.Id == id);
        }
    }
}