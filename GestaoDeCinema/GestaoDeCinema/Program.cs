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

// ===== INICIALIZAR BASE DE DADOS =====
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CinemaContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    // Aplicar migrations automaticamente
    try
    {
        context.Database.Migrate();
        logger.LogInformation("✅ Migrations aplicadas com sucesso");
    }
    catch (Exception ex)
    {
        logger.LogWarning("⚠️ Falha ao aplicar migrations (provavelmente tabelas já existem): {Message}", ex.Message);
    }

    // ===== SINCRONIZAÇÃO DEFINITIVA DA BASE DE DADOS =====
    try
    {
        var dbConnection = context.Database.GetDbConnection();
        if (dbConnection.State != System.Data.ConnectionState.Open) await dbConnection.OpenAsync();

        using var cmd = dbConnection.CreateCommand();
        cmd.CommandText = "PRAGMA table_info(Reservas);";
        var existingColumns = new List<string>();
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync()) existingColumns.Add(reader["name"].ToString() ?? "");
        }

        var columnsToAdd = new Dictionary<string, string>
        {
            { "AssentosEscolhidos", "TEXT" },
            { "PrecoTotal", "DECIMAL(18,2) DEFAULT 0" },
            { "NumeroCartao", "TEXT" },
            { "EstadoReserva", "TEXT DEFAULT 'Confirmada'" }
        };

        foreach (var col in columnsToAdd)
        {
            if (!existingColumns.Contains(col.Key))
            {
                try
                {
                    cmd.CommandText = $"ALTER TABLE Reservas ADD COLUMN {col.Key} {col.Value};";
                    await cmd.ExecuteNonQueryAsync();
                    logger.LogInformation("✅ Coluna adicionada com sucesso: {Column}", col.Key);
                }
                catch (Exception ex)
                {
                    logger.LogWarning("⚠️ Falha ao adicionar {Column}: {Message}", col.Key, ex.Message);
                }
            }
            else
            {
                logger.LogInformation("ℹ️  A coluna {Column} já existe.", col.Key);
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Erro crítico na sincronização da Base de Dados");
    }
    
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
        logger.LogInformation("👤 Admin criado: admin@cinema.com / Admin123");
    }
    
    // Criar dados de teste APENAS em Development
    if (app.Environment.IsDevelopment())
    {
        if (!context.Filmes.Any())
        {
            context.Filmes.Add(new Filme 
            { 
                Titulo = "Avatar", 
                Genero = "Ficção Científica", 
                Duracao = 162, 
                Sinopse = "Filme épico sobre Pandora", 
                CapaImagem = null 
            });
            context.Filmes.Add(new Filme 
            { 
                Titulo = "Titanic", 
                Genero = "Romance", 
                Duracao = 195, 
                Sinopse = "Uma história de amor trágica", 
                CapaImagem = null 
            });
            context.SaveChanges();
            logger.LogInformation("🎬 Filmes de teste criados: Avatar, Titanic");
        }
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