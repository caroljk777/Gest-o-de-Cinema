using Microsoft.AspNetCore.Mvc;
using GestaoDeCinema.BD;
using GestaoDeCinema.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace GestaoDeCinema.Controllers
{
    public class AccountController : Controller
    {
        private readonly CinemaContext _context;

        public AccountController(CinemaContext context)
        {
            _context = context;
        }

        // --- PÁGINA DE REGISTO (GET) ---
        public IActionResult Register()
        {
            return View();
        }

        // --- AÇÃO DE REGISTAR (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Utilizador utilizador)
        {
            // 1. Verificar se o email já existe
            if (await _context.Utilizadores.AnyAsync(u => u.Email == utilizador.Email))
            {
                ModelState.AddModelError("Email", "Este email já está registado.");
                return View(utilizador);
            }

            // 2. Definir função baseado no email
            if (utilizador.Email.ToLower() == "admin@cinema.com")
            {
                utilizador.Funcao = "Administrador";
            }
            else
            {
                utilizador.Funcao = "Cliente";
            }

            // 3. Guardar na Base de Dados
            _context.Utilizadores.Add(utilizador);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Login");
        }

        // --- PÁGINA DE LOGIN (GET) ---
        public IActionResult Login()
        {
            return View();
        }

        // --- AÇÃO DE LOGIN (POST) ---
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // 1. Procurar o utilizador
            var user = await _context.Utilizadores.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                // 2. Criar a "Identidade" (O Cartão de Cidadão digital)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Nome),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Funcao)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // 3. Criar o Cookie de Login
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                // 4. Redirecionar baseado na função
                if (user.Funcao == "Administrador")
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Erro = "Email ou Password incorretos";
            return View();
        }

        // --- PERFIL (GET) ---
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Perfil()
        {
            var userEmail = User.Identity.Name;
            var utilizador = await _context.Utilizadores
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (utilizador == null)
                return RedirectToAction("Login");

            return View(utilizador);
        }

        // --- PERFIL (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Perfil(string nome, string email, string? password)
        {
            var userEmail = User.Identity.Name;
            var utilizador = await _context.Utilizadores
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (utilizador == null)
                return RedirectToAction("Login");

            // Atualizar Nome
            utilizador.Nome = nome;

            // Atualizar Email (se diferente)
            if (utilizador.Email != email)
            {
                // Verificar se novo email já existe
                var emailExists = await _context.Utilizadores
                    .AnyAsync(u => u.Email == email && u.Id != utilizador.Id);

                if (emailExists)
                {
                    TempData["Erro"] = "Este email já está em uso.";
                    return View(utilizador);
                }

                utilizador.Email = email;
            }

            // Atualizar Password (se fornecida)
            if (!string.IsNullOrWhiteSpace(password))
            {
                utilizador.Password = password;
            }

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Perfil atualizado com sucesso!";
            return RedirectToAction("Perfil");
        }

        // --- LOGOUT ---
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}