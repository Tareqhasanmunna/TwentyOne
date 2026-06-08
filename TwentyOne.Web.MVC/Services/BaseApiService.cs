using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TwentyOne.Web.MVC.Services
{
    public class BaseApiService
    {
        protected readonly HttpClient _httpClient;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions;

        public BaseApiService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        protected void AttachToken()
        {
            var token = _httpContextAccessor.HttpContext?
                .Session.GetString("JwtToken");

            // Fall back to guest token
            if (string.IsNullOrEmpty(token))
                token = _httpContextAccessor.HttpContext?
                    .Session.GetString("GuestToken");

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        protected async Task<T?> GetAsync<T>(string url)
        {
            AttachToken();
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return default;

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
                return default;

            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }

        protected async Task<T?> GetPublicAsync<T>(string url)
        {
            // Bypasses AttachToken() entirely
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return default;

            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
                return default;

            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }

        protected async Task<T?> PostAsync<T>(string url, object data)
        {
            AttachToken();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(
                json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(responseContent))
                return default;

            return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
        }

        protected async Task<T?> PutAsync<T>(string url, object data)
        {
            AttachToken();
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(
                json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.PutAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(responseContent))
                return default;

            return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
        }

        protected async Task<T?> DeleteAsync<T>(string url)
        {
            AttachToken();
            var response = await _httpClient.DeleteAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(responseContent))
                return default;

            return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
        }

        protected async Task<T?> DeleteWithBodyAsync<T>(
            string url, object data)
        {
            AttachToken();
            var json = System.Text.Json.JsonSerializer.Serialize(data);
            var request = new HttpRequestMessage(
                HttpMethod.Delete, url)
            {
                Content = new StringContent(
                    json,
                    System.Text.Encoding.UTF8,
                    "application/json")
            };

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(
                    "application/json"));

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content
                .ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(responseContent))
                return default;

            return System.Text.Json.JsonSerializer.Deserialize<T>(
                responseContent,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
    }
}