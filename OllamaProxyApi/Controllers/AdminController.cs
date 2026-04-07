using Microsoft.AspNetCore.Mvc;
using OllamaProxyApi.Models;
using OllamaProxyApi.Services;

namespace OllamaProxyApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly FlaskProxyService _flaskService;

        public AdminController(FlaskProxyService flaskService)
        {
            _flaskService = flaskService;
        }

        private ContentResult ProxyJson(System.Net.HttpStatusCode statusCode, string body)
            => new()
            {
                StatusCode = (int)statusCode,
                ContentType = "application/json; charset=utf-8",
                Content = body,
            };

        [HttpGet("list")]
        public async Task<IActionResult> GetList([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
        {
            var result = await _flaskService.GetAdminListAsync(page, pageSize);
            return ProxyJson(result.StatusCode, result.Body);
        }

        [HttpGet("health")]
        public async Task<ActionResult<HealthStatus>> Health()
        {
            var result = await _flaskService.GetHealthAsync();
            if (result == null)
            {
                return StatusCode(502, new { code = "UPSTREAM_HEALTH_FAILED", message = "Flask health endpoint returned non-200" });
            }

            return Ok(result);
        }

        [HttpGet("paragraphs")]
        public async Task<IActionResult> GetParagraphs([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
        {
            var result = await _flaskService.GetAdminListAsync(page, pageSize);
            return ProxyJson(result.StatusCode, result.Body);
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] AskRequest req)
        {
            var result = await _flaskService.AskAsync(req.Question, req.TopK);
            return ProxyJson(result.StatusCode, result.Body);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _flaskService.DeleteParagraphAsync(id);
            return ProxyJson(result.StatusCode, result.Body);
        }

        [HttpGet("source-files")]
        public async Task<IActionResult> GetSourceFiles()
        {
            var result = await _flaskService.GetSourceFilesAsync();
            return ProxyJson(result.StatusCode, result.Body);
        }

        [HttpGet("pending-modifications")]
        public async Task<IActionResult> GetPendingModifications()
        {
            var result = await _flaskService.GetPendingModificationsAsync();
            return ProxyJson(result.StatusCode, result.Body);
        }

        [HttpGet("reload-needed")]
        public async Task<IActionResult> GetReloadNeeded()
        {
            var result = await _flaskService.GetReloadNeededStatusAsync();
            return ProxyJson(result.StatusCode, result.Body);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateParagraph(int id, [FromBody] UpdateRequest request)
        {
            var result = await _flaskService.UpdateParagraphAsync(id, request.Text);
            return ProxyJson(result.StatusCode, result.Body);
        }

        [HttpPost("reload")]
        public async Task<IActionResult> Reload()
        {
            var result = await _flaskService.ReloadAsync();
            return ProxyJson(result.StatusCode, result.Body);
        }
    }
}
