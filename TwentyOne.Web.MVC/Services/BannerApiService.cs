using TwentyOne.Shared.DTOs.Responses;
using TwentyOne.Shared.Models;

namespace TwentyOne.Web.MVC.Services
{
    public class BannerApiService : BaseApiService
    {
        public BannerApiService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
            : base(httpClient, httpContextAccessor)
        {
        }

        public async Task<ApiResponse<List<BannerResponseDto>>?> GetActiveAsync()
        {
            return await GetAsync<ApiResponse<List<BannerResponseDto>>>(
                "api/banners/active");
        }

        public async Task<ApiResponse<List<BannerResponseDto>>?> GetAllAsync()
        {
            return await GetAsync<ApiResponse<List<BannerResponseDto>>>(
                "api/banners");
        }

        public async Task<ApiResponse<string>?> UploadLogoAsync(IFormFile file)
        {
            AttachToken();
            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(
                    file.ContentType);
            content.Add(fileContent, "file", file.FileName);
            var response = await _httpClient
                .PostAsync("api/banners/logo", content);
            var responseContent = await response.Content
                .ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseContent))
                return default;
            return System.Text.Json.JsonSerializer
                .Deserialize<ApiResponse<string>>(responseContent,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        public async Task<ApiResponse<BannerResponseDto>?> CreateBannerAsync(
    string? title, string? subtitle,
    int displayOrder, IFormFile image)
        {
            AttachToken();
            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(image.OpenReadStream());
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(
                    image.ContentType);
            content.Add(fileContent, "image", image.FileName);
            content.Add(new StringContent(title ?? ""), "title");
            content.Add(new StringContent(subtitle ?? ""), "subtitle");
            content.Add(new StringContent(displayOrder.ToString()),
                "displayOrder");
            var response = await _httpClient
                .PostAsync("api/banners", content);
            var responseContent = await response.Content
                .ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseContent))
                return default;
            return System.Text.Json.JsonSerializer
                .Deserialize<ApiResponse<BannerResponseDto>>(responseContent,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        public async Task<ApiResponse<string>?> DeleteAsync(int id)
        {
            return await DeleteAsync<ApiResponse<string>>(
                $"api/banners/{id}");
        }

        public async Task<ApiResponse<string>?> ToggleAsync(int id)
        {
            return await PostAsync<ApiResponse<string>>(
                $"api/banners/{id}/toggle", new { });
        }

        public async Task<string?> GetLogoUrlAsync()
        {
            try
            {
                var result = await GetAsync<ApiResponse<string>>(
                    "api/banners/logo");
                return result?.Data;
            }
            catch
            {
                return null;
            }
        }

        public async Task<(int inside, int outside)> GetDeliveryChargesAsync()
        {
            try
            {
                var result = await GetAsync < ApiResponse
                    <DeliveryChargesResponse >> (
            "api/banners/delivery-charges");
                return (
                    result?.Data?.InsideDhaka ?? 80,
                    result?.Data?.OutsideDhaka ?? 130
                );
            }
            catch
            {
                return (80, 130);
            }
        }

        public async Task<ApiResponse<string>?> UpdateDeliveryChargesAsync(
            int insideDhaka, int outsideDhaka)
        {
            return await PostAsync<ApiResponse<string>>(
                "api/banners/delivery-charges",
                new { insideDhaka, outsideDhaka });
        }

        // Helper class
        public class DeliveryChargesResponse
        {
            public int InsideDhaka { get; set; }
            public int OutsideDhaka { get; set; }
        }

        public async Task<string> GetBkashNumberAsync()
        {
            try
            {
                var result = await GetAsync<ApiResponse<string>>(
                    "api/banners/bkash-number");
                return result?.Data ?? "01749028100";
            }
            catch
            {
                return "01749028100";
            }
        }
        public async Task<ApiResponse<string>?> UpdateBkashNumberAsync(
            string number)
        {
            return await PostAsync<ApiResponse<string>>(
                "api/banners/bkash", number);
        }

    }
}
