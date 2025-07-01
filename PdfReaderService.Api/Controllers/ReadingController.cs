using Microsoft.AspNetCore.Mvc;
using PdfReaderService.Api.Services;

namespace PdfReaderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReadingController : ControllerBase
    {
        private readonly IReadingService _readingService;
        private readonly ILogger<ReadingController> _logger;

        public ReadingController(IReadingService readingService, ILogger<ReadingController> logger)
        {
            _readingService = readingService;
            _logger = logger;
        }

        [HttpPost("send-daily-page")]
        public async Task<IActionResult> SendDailyPage()
        {
            try
            {
                var result = await _readingService.SendDailyPageAsync();
                if (result)
                {
                    return Ok(new { success = true, message = "Página enviada correctamente" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "No se pudo enviar la página" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar página diaria");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        [HttpPost("send-next-page")]
        public async Task<IActionResult> SendNextPage()
        {
            try
            {
                var result = await _readingService.SendNextPageAsync();
                if (result)
                {
                    return Ok(new { success = true, message = "Siguiente página enviada correctamente" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "No se pudo enviar la siguiente página" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar siguiente página");
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }
    }
}