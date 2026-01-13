namespace GestaoDeCinema.Helpers
{
    public static class ImageHelper
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private static readonly long MaxFileSize = 5 * 1024 * 1024; // 5MB
        private static readonly string ImageFolder = "wwwroot/imagens";

        /// <summary>
        /// Valida se o ficheiro é uma imagem válida (tipo e tamanho)
        /// </summary>
        public static bool ValidateImageFile(IFormFile file, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (file == null || file.Length == 0)
            {
                errorMessage = "Nenhum ficheiro foi selecionado.";
                return false;
            }

            // Validar tamanho
            if (file.Length > MaxFileSize)
            {
                errorMessage = $"O ficheiro excede o tamanho máximo de {MaxFileSize / 1024 / 1024}MB.";
                return false;
            }

            // Validar extensão
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                errorMessage = $"Apenas são permitidos ficheiros de imagem ({string.Join(", ", AllowedExtensions)}).";
                return false;
            }

            // Validar MIME type
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                errorMessage = "O tipo de ficheiro não é uma imagem válida.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Faz upload de uma imagem e retorna o nome do ficheiro guardado
        /// </summary>
        public static async Task<string> UploadImageAsync(IFormFile file)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), ImageFolder);

            // Criar pasta se não existir
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        /// <summary>
        /// Elimina uma imagem do disco
        /// </summary>
        public static void DeleteImage(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), ImageFolder, fileName);
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                // Log o erro mas não falhar a operação
                Console.WriteLine($"Erro ao eliminar imagem {fileName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém o caminho completo da pasta de imagens
        /// </summary>
        public static string GetImageFolderPath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), ImageFolder);
        }
    }
}
