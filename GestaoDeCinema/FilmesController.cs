using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestaoDeCinema.BD;
using GestaoDeCinema.Models;
using System.IO; // <--- NOVA BIBLIOTECA IMPORTANTE!

namespace GestaoDeCinema.Controllers // Confirma se o namespace está certo
{
    public class FilmesController : Controller
    {
        private readonly CinemaContext _context;

        public FilmesController(CinemaContext context)
        {
            _context = context;
        }

        // GET: Filmes
        public async Task<IActionResult> Index()
        {
            return View(await _context.Filmes.ToListAsync());
        }

        // GET: Filmes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var filme = await _context.Filmes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (filme == null) return NotFound();

            return View(filme);
        }

        // GET: Filmes/Create
        public IActionResult Create()
        {
            return View();
        }

        // --- AQUI ESTÁ A MUDANÇA PARA O ADMIN CRIAR COM IMAGEM ---
        // POST: Filmes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Filme filme, IFormFile? ficheiroImagem)
        {
            // 1. Verificar se foi enviada uma imagem
            if (ficheiroImagem != null && ficheiroImagem.Length > 0)
            {
                // Gera nome único (ex: "foto_guid123.jpg")
                var nomeFicheiro = Guid.NewGuid().ToString() + Path.GetExtension(ficheiroImagem.FileName);

                // Define a pasta wwwroot/imagens
                var caminhoPasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagens");

                // Cria a pasta se não existir
                if (!Directory.Exists(caminhoPasta))
                {
                    Directory.CreateDirectory(caminhoPasta);
                }

                // Grava o ficheiro
                var caminhoCompleto = Path.Combine(caminhoPasta, nomeFicheiro);
                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await ficheiroImagem.CopyToAsync(stream);
                }

                // Guarda APENAS o nome na BD
                filme.CapaImagem = nomeFicheiro;
            }

            // Remove a validação do ficheiro (porque não vai para a BD)
            ModelState.Remove("FicheiroImagem");

            if (ModelState.IsValid)
            {
                _context.Add(filme);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(filme);
        }

        // GET: Filmes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var filme = await _context.Filmes.FindAsync(id);
            if (filme == null) return NotFound();
            return View(filme);
        }

        // --- MUDANÇA PARA EDITAR A IMAGEM ---
        // POST: Filmes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Filme filme, IFormFile? ficheiroImagem)
        {
            if (id != filme.Id) return NotFound();

            ModelState.Remove("FicheiroImagem");

            if (ModelState.IsValid)
            {
                try
                {
                    // Se o Admin carregou uma NOVA imagem
                    if (ficheiroImagem != null && ficheiroImagem.Length > 0)
                    {
                        var nomeFicheiro = Guid.NewGuid().ToString() + Path.GetExtension(ficheiroImagem.FileName);
                        var caminhoPasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagens");

                        using (var stream = new FileStream(Path.Combine(caminhoPasta, nomeFicheiro), FileMode.Create))
                        {
                            await ficheiroImagem.CopyToAsync(stream);
                        }
                        filme.CapaImagem = nomeFicheiro;
                    }
                    else
                    {
                        // Se NÃO carregou imagem nova, temos de manter a antiga!
                        // Vamos à BD buscar a imagem que lá estava antes
                        var filmeAntigo = await _context.Filmes.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
                        if (filmeAntigo != null)
                        {
                            filme.CapaImagem = filmeAntigo.CapaImagem;
                        }
                    }

                    _context.Update(filme);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FilmeExists(filme.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(filme);
        }

        // GET: Filmes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var filme = await _context.Filmes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (filme == null) return NotFound();

            return View(filme);
        }

        // POST: Filmes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var filme = await _context.Filmes.FindAsync(id);
            if (filme != null) _context.Filmes.Remove(filme);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FilmeExists(int id)
        {
            return _context.Filmes.Any(e => e.Id == id);
        }
    }
}