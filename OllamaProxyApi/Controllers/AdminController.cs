using Microsoft.AspNetCore.Mvc;
using OllamaProxyApi.Models;
using OllamaProxyApi.Services; // 引用 FlaskProxyService 命名空間
using System.Threading.Tasks;

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

        // ✅ GET: /api/admin/list?page=1&pageSize=100
        [HttpGet("list")]
        public async Task<IActionResult> GetList([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
        {
            var result = await _flaskService.GetAdminListAsync(page, pageSize);
            return Ok(result);
        }

        // ✅ GET: /api/admin/health
        [HttpGet("health")]
        public async Task<ActionResult<HealthStatus>> Health()
        {
            var result = await _flaskService.GetHealthAsync();
            return Ok(result);
        }


        // ✅ GET: /api/admin/paragraphs?page=1&pageSize=100
        [HttpGet("paragraphs")]
        public async Task<IActionResult> GetParagraphs([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
        {
            var result = await _flaskService.GetParagraphPageAsync(page, pageSize);
            return Ok(result);
        }


        // ✅ POST: /api/admin/ask
        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] AskRequest req)
        {
            var result = await _flaskService.AskAsync(req.Question, req.TopK);
            return Content(result, "application/json");

        }

        // ✅ DELETE: /api/admin/delete/123
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _flaskService.DeleteParagraphAsync(id);
            return Ok(result);
        }

        // ✅ GET: /api/admin/source-files
        [HttpGet("source-files")]
        public async Task<IActionResult> GetSourceFiles()
        {
            var result = await _flaskService.GetSourceFilesAsync();
            return Content(result, "application/json");
        }

        // ✅ GET: /api/admin/pending-modifications
        [HttpGet("pending-modifications")]
        public async Task<IActionResult> GetPendingModifications()
        {
            var result = await _flaskService.GetPendingModificationsAsync();
            return Content(result, "application/json");
        }

        // ✅ GET: /api/admin/reload-needed
        [HttpGet("reload-needed")]
        public async Task<IActionResult> GetReloadNeeded()
        {
            var result = await _flaskService.GetReloadNeededStatusAsync();
            return Content(result, "application/json");
        }

        // ✅ PUT: /api/admin/update/123
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateParagraph(int id, [FromBody] UpdateRequest request)
        {
            var result = await _flaskService.UpdateParagraphAsync(id, request.Text);
            return Content(result, "application/json");
        }

        // ✅ POST: /api/admin/reload
        [HttpPost("reload")]
        public async Task<IActionResult> Reload()
        {
            var result = await _flaskService.ReloadAsync();
            return Content(result, "application/json");
        }

    }

}
