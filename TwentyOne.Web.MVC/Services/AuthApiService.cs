

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
    }
}
