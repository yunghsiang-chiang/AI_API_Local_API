using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using OllamaProxyApi.Models;

namespace OllamaProxyApi.Services
{
    public class FlaskProxyService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FlaskProxyService> _logger;
        private const int MAX_TOP_K = 30;
        private const string AdminApiKeyHeader = "X-Admin-API-Key";

        public FlaskProxyService(HttpClient httpClient, IConfiguration configuration, ILogger<FlaskProxyService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        private string BaseUrl => _configuration["FlaskApi:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5000";

        private string ConfiguredAdminApiKey =>
            _configuration["FlaskApi:AdminApiKey"] ??
            Environment.GetEnvironmentVariable("ADMIN_API_KEY") ??
            string.Empty;

        private async Task<(HttpStatusCode StatusCode, string Body)> SendAsync(
            HttpRequestMessage request,
            bool requireAdminApiKey = false,
            string? adminApiKey = null)
        {
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var apiKeyToUse = string.IsNullOrWhiteSpace(adminApiKey) ? ConfiguredAdminApiKey : adminApiKey;

            if (requireAdminApiKey && !string.IsNullOrWhiteSpace(apiKeyToUse))
            {
                request.Headers.TryAddWithoutValidation(AdminApiKeyHeader, apiKeyToUse);
            }

            using var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            return (response.StatusCode, body);
        }

        public async Task<HealthStatus?> GetHealthAsync()
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/health");
            var (statusCode, body) = await SendAsync(request);
            if (statusCode != HttpStatusCode.OK)
            {
                _logger.LogWarning("Flask health check returned {StatusCode}: {Body}", (int)statusCode, body);
                return null;
            }

            return JsonConvert.DeserializeObject<HealthStatus>(body);
        }

        public async Task<(HttpStatusCode StatusCode, string Body)> AskAsync(string question, int topK = 5)
        {
            topK = Math.Clamp(topK, 1, MAX_TOP_K);

            var postData = new
            {
                question,
                top_k = topK,
            };

            var json = JsonConvert.SerializeObject(postData);
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/ask")
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
            };

            return await SendAsync(request);
        }

        public async Task<(HttpStatusCode StatusCode, string Body)> GetAdminListAsync(int page, int pageSize, string? adminApiKey = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/admin/list?page={page}&pageSize={pageSize}");
            return await SendAsync(request, requireAdminApiKey: true, adminApiKey: adminApiKey);
        }

        public async Task<(HttpStatusCode StatusCode, string Body)> GetParagraphsByFileAsync(string fileName)
        {
            var encodedFileName = Uri.EscapeDataString(fileName);
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/paragraphs-by-file?file_name={encodedFileName}");
            return await SendAsync(request);
        }

        public async Task<(HttpStatusCode StatusCode, string Body)> DeleteParagraphAsync(int id, string? adminApiKey = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/admin/delete/{id}");
            return await SendAsync(request, requireAdminApiKey: true, adminApiKey: adminApiKey);
        }

        public async Task<(HttpStatusCode StatusCode, string Body)> GetSourceFilesAsync(string? adminApiKey = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/admin/source-files");
            return await SendAsync(request, requireAdminApiKey: true, adminApiKey: adminApiKey);
        }


        public async Task<(HttpStatusCode StatusCode, string Body)> GetSampledFilesAsync(string? adminApiKey = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/admin/sampled-files");
            return await SendAsync(request, requireAdminApiKey: true, adminApiKey: adminApiKey);
        }

        public async Task<(HttpStatusCode StatusCode, string Body)> AddSampledFileAsync(string fileName, string? adminApiKey = null)
        {
            var payload = new { file_name = fileName };
            var json = JsonConvert.SerializeObject(payload);
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/admin/sampled-files")
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
            };

            return await SendAsync(request, requireAdminApiKey: true, adminApiKey: adminApiKey);
        }

        public async Task<(HttpStatusCode StatusCode, string Body)> RemoveSampledFileAsync(string fileName, string? adminApiKey = null)
        {
            var encodedFileName = Uri.EscapeDataString(fileName);
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/admin/sampled-files/{encodedFileName}");
            return await SendAsync(request, requireAdminApiKey: true, adminApiKey: adminApiKey);
        }

        public async Task<(HttpStatusCode StatusCode, string Body)> GetPendingModificationsAsync(string? adminApiKey = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/admin/pending-modifications");
            return await SendAsync(request, requireAdminApiKey: true, adminApiKey: adminApiKey);
        }

        public async Task<(HttpStatusCode StatusCode, string Body)> GetReloadNeededStatusAsync(string? adminApiKey = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/admin/reload-needed");
            return await SendAsync(request, requireAdminApiKey: true, adminApiKey: adminApiKey);
        }

        public async Task<(HttpStatusCode StatusCode, string Body)> UpdateParagraphAsync(int id, string newText, string? adminApiKey = null)
        {
            var payload = new { text = newText };
            var json = JsonConvert.SerializeObject(payload);
            using var request = new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/admin/update/{id}")
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
            };

            return await SendAsync(request, requireAdminApiKey: true, adminApiKey: adminApiKey);
        }

        public async Task<(HttpStatusCode StatusCode, string Body)> ReloadAsync(string? adminApiKey = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/admin/reload");
            return await SendAsync(request, requireAdminApiKey: true, adminApiKey: adminApiKey);
        }
    }
}
