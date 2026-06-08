

using TwentyOne.Shared.DTOs.Requests;
using TwentyOne.Shared.Models;

namespace TwentyOne.Web.MVC.Services
{
    public class AuthApiService: BaseApiService
    {
        public AuthApiService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
            : base(httpClient, httpContextAccessor)
        {
        }

        public async Task<ApiResponse<AuthResponseDto>?> LoginAsync(LoginDto dto)
        {
            return await PostAsync<ApiResponse<AuthResponseDto>>(
                "api/auth/login", dto);
        }

        public async Task<ApiResponse<string>?> ChangePasswordAsync(
            ChangePasswordDto dto)
        {
            return await PostAsync<ApiResponse<string>>(
                "api/auth/change-password", dto);
        }

        public async Task<ApiResponse<string>?> RegisterAsync(RegisterDto dto)
        {
            return await PostAsync<ApiResponse<string>>("api/auth/register", dto);
        }

        public async Task<ApiResponse<string>?> CreateAdminAsync(
            CreateAdminDto dto)
        {
            return await PostAsync<ApiResponse<string>>(
                "api/auth/create-admin", dto);
        }

        public async Task<ApiResponse<object>?> GetAdminsAsync()
        {
            return await GetAsync<ApiResponse<object>>("api/auth/admins");
        }

        public async Task<ApiResponse<string>?> ToggleAdminAsync(
            string id, string password)
        {
            return await PostAsync<ApiResponse<string>>(
                $"api/auth/toggle-admin/{id}",
                new { password });
        }

        public async Task<ApiResponse<string>?> DeleteAdminAsync(
            string id, string password)
        {
            return await DeleteWithBodyAsync<ApiResponse<string>>(
                $"api/auth/delete-admin/{id}",
                new { password });
        }
    }
}
