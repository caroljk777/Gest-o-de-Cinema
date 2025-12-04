using GestaoDeCinema.BD; // <--- O TEU CONTEXTO ESTÁ AQUI
using GestaoDeCinema.Models; // Caso precise dos modelos
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços ao contentor.
builder.Services.AddControllersWithViews();

// --- AQUI ESTÁ A CORREÇÃO DO ERRO ---
// Estamos a registar o CinemaContext para que o programa o encontre
builder.Services.AddDbContext<CinemaContext>(options =>
    options.UseSqlite("Data Source=Cinema.db"));
// ------------------------------------

// Configuração de Login
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Home/Index";
    });

var app = builder.Build();

// Configurar o pipeline de pedidos HTTP.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();