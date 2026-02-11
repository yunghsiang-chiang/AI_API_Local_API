using Newtonsoft.Json;
using OllamaProxyApi.Models;

namespace OllamaProxyApi.Services
{
    public class FlaskProxyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://localhost:5000";
        private const int MAX_TOP_K = 10;

        public FlaskProxyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HealthStatus> GetHealthAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/health");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<HealthStatus>(content);
        }



        public async Task<ParagraphPageResult> GetParagraphPageAsync(int page, int pageSize)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/admin/list?page={page}&pageSize={pageSize}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ParagraphPageResult>(content);
        }


        public async Task<dynamic> AskAsync(string question, int topK = 5)
        {
            topK = Math.Min(topK, MAX_TOP_K);

            var postData = new
            {
                question = question,
                top_k = topK // <-- 確保這裡是 "top_k" 而不是 "topK"
            };

            var json = JsonConvert.SerializeObject(postData);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/ask", content);

            var result = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[ERROR] HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");
            }
            Console.WriteLine("[FlaskProxy Response Raw] " + result);


            return result; // 回傳 JSON 原始字串試看看

        }


        public async Task<string> GetAdminListAsync(int page, int pageSize)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/admin/list?page={page}&pageSize={pageSize}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> DeleteParagraphAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/admin/delete/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetSourceFilesAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/admin/source-files");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetPendingModificationsAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/admin/pending-modifications");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetReloadNeededStatusAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/admin/reload-needed");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> UpdateParagraphAsync(int id, string newText)
        {
            var payload = new { text = newText };
            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}/admin/update/{id}", content);
            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                throw new Exception($"更新失敗: {response.StatusCode} - {errorMsg}");
            }
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> ReloadAsync()
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/admin/reload", null);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

    }
}
