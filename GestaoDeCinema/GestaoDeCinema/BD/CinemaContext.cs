using Microsoft.EntityFrameworkCore;
using GestaoDeCinema.Models;

namespace GestaoDeCinema.BD
{
    // Apenas UMA classe, a herdar de DbContext
    public class CinemaContext : DbContext
    {
        public CinemaContext(DbContextOptions<CinemaContext> options) : base(options) { }

        public DbSet<Utilizador> Utilizadores { get; set; }
        public DbSet<Filme> Filmes { get; set; }
        public DbSet<Sessao> Sessoes { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
    }
}