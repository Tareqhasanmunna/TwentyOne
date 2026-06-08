using TwentyOne.Shared.Models;

namespace TwentyOne.Web.MVC.Services
{
    public class ImageApiService: BaseApiService
    {
        public ImageApiService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
            : base(httpClient, httpContextAccessor)
        {
        }

        public async Task<ApiResponse<string>?> UploadImageAsync(IFormFile file)
        {
            AttachToken();

            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

            content.Add(fileContent, "file", file.FileName);

            var response = await _httpClient.PostAsync("api/images/upload", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(responseContent)) return default;

            return System.Text.Json.JsonSerializer.Deserialize<ApiResponse<string>>(
                responseContent,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        public async Task<ApiResponse<string>?> UploadBrandLogoAsync(IFormFile file)
        {
            AttachToken();

            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

            content.Add(fileContent, "file", file.FileName);

            var response = await _httpClient
                .PostAsync("api/images/upload-logo", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(responseContent)) return default;

            return System.Text.Json.JsonSerializer.Deserialize<ApiResponse<string>>(
                responseContent,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
    }
}
