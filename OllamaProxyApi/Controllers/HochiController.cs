using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;

namespace OllamaProxyApi.Controllers
{
    [ApiController]
    [Route("api/hochi")]
    public class HochiController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HochiController> _logger;

        public HochiController(IHttpClientFactory httpClientFactory, ILogger<HochiController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // ✅ GET: /api/hochi/course-banners
        [HttpGet("course-banners")]
        public async Task<IActionResult> GetCourseBanners()
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(15);

            var url = "https://internal.hochi.org.tw:8082/api/HochiSystem/CourseBanners";

            try
            {
                using var resp = await client.GetAsync(url);
                var body = await resp.Content.ReadAsStringAsync();

                // 透傳狀態碼與內容（成功就直接回 JSON 字串）
                if (resp.IsSuccessStatusCode)
                {
                    return Content(body, "application/json; charset=utf-8");
                }

                // upstream 有回應但非 2xx，把它包成錯誤格式
                return StatusCode((int)resp.StatusCode, new
                {
                    success = false,
                    code = "UPSTREAM_" + ((int)resp.StatusCode),
                    message = $"Upstream returned HTTP {(int)resp.StatusCode}",
                    upstreamBody = body.Length > 300 ? body.Substring(0, 300) : body
                });
            }
            catch (TaskCanceledException ex)
            {
                // timeout 會走到這
                _logger.LogWarning(ex, "Proxy timeout when calling internal course banners.");
                return StatusCode(504, new
                {
                    success = false,
                    code = "PROXY_TIMEOUT",
                    message = "Proxy timeout when calling internal CourseBanners"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Proxy error when calling internal course banners.");
                return StatusCode(502, new
                {
                    success = false,
                    code = "PROXY_ERROR",
                    message = ex.Message
                });
            }
        }

        [HttpGet("course-banners-lite")]
        public async Task<IActionResult> GetCourseBannersLite()
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(15);

            var url = "https://internal.hochi.org.tw:8082/api/HochiSystem/CourseBanners";

            try
            {
                using var resp = await client.GetAsync(url);
                var body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    return StatusCode((int)resp.StatusCode, new
                    {
                        success = false,
                        code = "UPSTREAM_" + ((int)resp.StatusCode),
                        message = "Upstream returned non-2xx"
                    });
                }

                using var doc = JsonDocument.Parse(body);
                if (!doc.RootElement.TryGetProperty("$values", out var values) || values.ValueKind != JsonValueKind.Array)
                {
                    return StatusCode(502, new { success = false, code = "BAD_UPSTREAM_JSON", message = "Missing $values array" });
                }

                static string GetString(JsonElement x, string name)
                    => x.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String ? (p.GetString() ?? "") : "";

                static int? GetInt(JsonElement x, string name)
                    => x.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var v) ? v : (int?)null;

                var list = new List<object>();
                foreach (var x in values.EnumerateArray())
                {
                    list.Add(new
                    {
                        hCourseName = GetString(x, "hCourseName"),
                        hDateRange = GetString(x, "hDateRange"),
                        hContentTitle = GetString(x, "hContentTitle"),
                        hImg = GetString(x, "hImg"),
                        hSerial = GetInt(x, "hSerial")
                    });
                }

                return Ok(list); // ✅ 回乾淨 array（小很多）
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Proxy error in course-banners-lite");
                return StatusCode(502, new { success = false, code = "PROXY_ERROR", message = ex.Message });
            }
        }
    }
}