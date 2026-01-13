using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GestaoDeCinema.BD;
using GestaoDeCinema.Models;
using GestaoDeCinema.Models.ViewModels;
using GestaoDeCinema.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GestaoDeCinema.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class FilmesController : Controller
    {
        private readonly CinemaContext _context;
        private readonly ILogger<FilmesController> _logger;

        public FilmesController(CinemaContext context, ILogger<FilmesController> logger)
        {
            _context = context;
            _logger = logger;
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

        // POST: Filmes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FilmeCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var filme = new Filme
                {
                    Titulo = viewModel.Titulo,
                    Genero = viewModel.Genero,
                    Duracao = viewModel.Duracao,
                    Sinopse = viewModel.Sinopse
                };

                // Upload de imagem se fornecida
                if (viewModel.FicheiroImagem != null)
                {
                    if (ImageHelper.ValidateImageFile(viewModel.FicheiroImagem, out string errorMessage))
                    {
                        try
                        {
                            filme.CapaImagem = await ImageHelper.UploadImageAsync(viewModel.FicheiroImagem);
                            _logger.LogInformation("Imagem carregada com sucesso: {FileName}", filme.CapaImagem);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Erro ao carregar imagem do filme");
                            ModelState.AddModelError("", "Erro ao carregar a imagem. Por favor, tente novamente.");
                            return View(viewModel);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("FicheiroImagem", errorMessage);
                        return View(viewModel);
                    }
                }

                _context.Add(filme);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Filme criado: {FilmeId} - {Titulo}", filme.Id, filme.Titulo);
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        // GET: Filmes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var filme = await _context.Filmes.FindAsync(id);
            if (filme == null) return NotFound();

            var viewModel = new FilmeEditViewModel
            {
                Id = filme.Id,
                Titulo = filme.Titulo,
                Genero = filme.Genero,
                Duracao = filme.Duracao,
                Sinopse = filme.Sinopse,
                CapaImagemAtual = filme.CapaImagem
            };

            return View(viewModel);
        }

        // POST: Filmes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FilmeEditViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var filme = await _context.Filmes.FindAsync(id);
                    if (filme == null) return NotFound();

                    // Atualizar campos básicos
                    filme.Titulo = viewModel.Titulo;
                    filme.Genero = viewModel.Genero;
                    filme.Duracao = viewModel.Duracao;
                    filme.Sinopse = viewModel.Sinopse;

                    // Processar nova imagem se fornecida
                    if (viewModel.NovaImagem != null)
                    {
                        if (ImageHelper.ValidateImageFile(viewModel.NovaImagem, out string errorMessage))
                        {
                            try
                            {
                                // Eliminar imagem antiga
                                ImageHelper.DeleteImage(filme.CapaImagem);

                                // Upload nova imagem
                                filme.CapaImagem = await ImageHelper.UploadImageAsync(viewModel.NovaImagem);
                                _logger.LogInformation("Imagem do filme atualizada: {FilmeId}", filme.Id);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Erro ao atualizar imagem do filme {FilmeId}", id);
                                ModelState.AddModelError("", "Erro ao carregar a nova imagem.");
                                viewModel.CapaImagemAtual = filme.CapaImagem;
                                return View(viewModel);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("NovaImagem", errorMessage);
                            viewModel.CapaImagemAtual = filme.CapaImagem;
                            return View(viewModel);
                        }
                    }

                    _context.Update(filme);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Filme editado: {FilmeId} - {Titulo}", filme.Id, filme.Titulo);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FilmeExists(viewModel.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
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
            if (filme != null)
            {
                // Eliminar imagem antes de remover o filme
                ImageHelper.DeleteImage(filme.CapaImagem);
                
                _context.Filmes.Remove(filme);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Filme eliminado: {FilmeId} - {Titulo}", filme.Id, filme.Titulo);
            }
            return RedirectToAction(nameof(Index));
        }

        private bool FilmeExists(int id)
        {
            return _context.Filmes.Any(e => e.Id == id);
        }
    }
}