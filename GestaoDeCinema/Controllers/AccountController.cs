using Microsoft.AspNetCore.Mvc;
using GestaoDeCinema.BD;      // <--- Confirma se a tua pasta se chama BD ou Data
using GestaoDeCinema.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

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
        public async Task<IActionResult> Register(Utilizador utilizador)
        {
            
            if (_context.Utilizadores.Any(u => u.Email == utilizador.Email))
            {
                ModelState.AddModelError("Email", "Este email já está registado.");
                return View(utilizador);
            }

            // Se o email for este, ganha poderes de chefe
            if (utilizador.Email.ToLower() == "admin@cinema.com")
            {
                utilizador.Funcao = "Administrador";
            }
            else
            {
                utilizador.Funcao = "Cliente";
            }

            // 3. Guardar na Base de Dados
            if (ModelState.IsValid)
            {
                _context.Utilizadores.Add(utilizador);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login");
            }
            return View(utilizador);
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
            var user = _context.Utilizadores.FirstOrDefault(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                // 2. Criar a "Identidade" (O Cartão de Cidadão digital)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Nome),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Funcao) // Importante: Guarda se é Admin ou Cliente
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // 3. Criar o Cookie de Login (Entrada no sistema)
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Erro = "Email ou Password incorretos";
            return View();
        }

        // --- LOGOUT ---
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}