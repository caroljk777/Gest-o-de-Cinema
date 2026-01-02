namespace GestaoDeCinema.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalFilmes { get; set; }
        public int TotalUtilizadores { get; set; }
        public int TotalSessoes { get; set; }
        public List<Filme> FilmesRecentes { get; set; } = new List<Filme>();
    }
}
