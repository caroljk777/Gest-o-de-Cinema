using GestaoDeCinema.BD; 
using GestaoDeCinema.Models; 
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços ao contentor.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<CinemaContext>(options =>
    options.UseSqlite("Data Source=Cinema.db"));

// Configuração de Login
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Home/Index";
    });

var app = builder.Build();

// ===== CRIAR UTILIZADOR ADMIN AUTOMÁTICO =====
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CinemaContext>();
    
    // Garantir que a base de dados está criada
    context.Database.EnsureCreated();
    
    // Verificar se já existe admin
    if (!context.Utilizadores.Any(u => u.Email == "admin@cinema.com"))
    {
        var admin = new Utilizador
        {
            Nome = "Administrador",
            Email = "admin@cinema.com",
            Password = "Admin123",
            Funcao = "Administrador"
        };
        
        context.Utilizadores.Add(admin);
        context.SaveChanges();
        
        Console.WriteLine(" Admin criado: admin@cinema.com / Admin123");
    }
}

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