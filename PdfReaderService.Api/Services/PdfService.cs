using System.Diagnostics;
using SkiaSharp;

namespace PdfReaderService.Api.Services
{
    public class PdfService : IPdfService
    {
        private readonly ILogger<PdfService> _logger;

        public PdfService(ILogger<PdfService> logger)
        {
            _logger = logger;
        }

        public byte[] GetPageImageAsync(string filePath, int pageNumber)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogError("El archivo PDF no existe: {FilePath}", filePath);
                    return null;
                }

                return RenderPageToImageWithGhostscript(filePath, pageNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener imagen de la página {PageNumber} del archivo {FilePath}",
                    pageNumber, filePath);
                return null;
            }
        }

        private byte[] RenderPageToImageWithGhostscript(string filePath, int pageNumber)
        {
            // Crear archivo temporal para la imagen
            var tempImagePath = Path.GetTempFileName() + ".png";
            
            try
            {
                // Comando Ghostscript para convertir página específica del PDF a PNG
                var processInfo = new ProcessStartInfo
                {
                    FileName = "gs",
                    Arguments = $"-dNOPAUSE -dBATCH -dSAFER -sDEVICE=png16m -r300 " +
                               $"-dFirstPage={pageNumber + 1} -dLastPage={pageNumber + 1} " +
                               $"-sOutputFile=\"{tempImagePath}\" \"{filePath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                _logger.LogInformation("Ejecutando Ghostscript: {Command}", processInfo.Arguments);

                using var process = Process.Start(processInfo);
                if (process == null)
                {
                    _logger.LogError("No se pudo iniciar el proceso Ghostscript");
                    return null;
                }

                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    _logger.LogError("Ghostscript falló con código {ExitCode}. Error: {Error}", 
                        process.ExitCode, error);
                    return null;
                }

                if (!File.Exists(tempImagePath))
                {
                    _logger.LogError("Ghostscript no generó el archivo de imagen esperado");
                    return null;
                }

                // Leer el archivo de imagen generado
                var imageBytes = File.ReadAllBytes(tempImagePath);
                _logger.LogInformation("Imagen generada exitosamente, tamaño: {Size} bytes", imageBytes.Length);
                
                return imageBytes;
            }
            finally
            {
                // Limpiar archivo temporal
                if (File.Exists(tempImagePath))
                {
                    try
                    {
                        File.Delete(tempImagePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "No se pudo eliminar archivo temporal: {TempFile}", tempImagePath);
                    }
                }
            }
        }

        public async Task<int> GetTotalPagesAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogError("El archivo PDF no existe: {FilePath}", filePath);
                    return 0;
                }

                return await GetTotalPagesWithGhostscript(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener total de páginas del archivo {FilePath}", filePath);
                return 0;
            }
        }

        private async Task<int> GetTotalPagesWithGhostscript(string filePath)
        {
            return await Task.Run(() =>
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "gs",
                    Arguments = $"-dNOPAUSE -dBATCH -dQUIET -sDEVICE=nullpage \"{filePath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processInfo);
                if (process == null)
                {
                    _logger.LogError("No se pudo iniciar el proceso Ghostscript para contar páginas");
                    return 0;
                }

                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    _logger.LogError("Ghostscript falló al contar páginas. Error: {Error}", error);
                    return 0;
                }

                // Buscar información de páginas en la salida
                // Ghostscript a menudo muestra "Processing pages 1 through N" en stderr
                var lines = error.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.Contains("Processing pages") && line.Contains("through"))
                    {
                        var parts = line.Split(' ');
                        for (int i = 0; i < parts.Length - 1; i++)
                        {
                            if (parts[i] == "through" && int.TryParse(parts[i + 1].TrimEnd('.'), out int pageCount))
                            {
                                return pageCount;
                            }
                        }
                    }
                }

                // Método alternativo: usar otro comando específico para contar páginas
                return GetPageCountAlternative(filePath);
            });
        }

        private int GetPageCountAlternative(string filePath)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "gs",
                Arguments = $"-dNOPAUSE -dBATCH -dQUIET -c \"({filePath}) (r) file runpdfbegin pdfpagecount = quit\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null) return 0;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0 && int.TryParse(output.Trim(), out int pageCount))
            {
                return pageCount;
            }

            return 0;
        }
    }
}