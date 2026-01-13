// Script para adicionar coluna AssentosEscolhidos na tabela Reservas
// Execute este código temporariamente no Program.cs ou crie uma migration manualmente

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService\u003cCinemaContext\u003e();
    
    try
    {
        // Tentar adicionar a coluna se não existir
        context.Database.ExecuteSqlRaw("ALTER TABLE Reservas ADD COLUMN AssentosEscolhidos TEXT");
        Console.WriteLine("Coluna AssentosEscolhidos adicionada com sucesso!");
    }
    catch (Exception ex)
    {
        // Se a coluna já existir, vai dar erro - ignorar
        Console.WriteLine($"Aviso: {ex.Message}");
    }
}
