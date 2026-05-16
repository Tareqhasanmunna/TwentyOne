using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TwentyOne.BLL.Services.Interfaces;
using TwentyOne.DAL.Entities;
using TwentyOne.Shared.DTOs.Requests;
using TwentyOne.Shared.Models;

namespace TwentyOne.Web.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.FailResponse(
                    "Validation failed",
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()));

            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(ApiResponse<string>.FailResponse(
                    "Email is already registered"));

            // Create the new user
            var user = new ApplicationUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(ApiResponse<string>.FailResponse(
                    "Registration failed",
                    result.Errors.Select(e => e.Description).ToList()));

            // Assign default Customer role
            await _userManager.AddToRoleAsync(user, "Customer");

            return Ok(ApiResponse<string>.SuccessResponse(
                "Account created successfully",
                "Registration successful"));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.FailResponse(
                    "Validation failed",
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()));

            // Find user by email
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized(ApiResponse<string>.FailResponse(
                    "Invalid email or password"));

            // Check if account is active
            if (!user.IsActive)
                return Unauthorized(ApiResponse<string>.FailResponse(
                    "Your account has been deactivated"));

            // Check password
            var result = await _signInManager.CheckPasswordSignInAsync(
                user, dto.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
                return Unauthorized(ApiResponse<string>.FailResponse(
                    "Account locked. Try again after 10 minutes"));

            if (!result.Succeeded)
                return Unauthorized(ApiResponse<string>.FailResponse(
                    "Invalid email or password"));

            // Generate JWT token
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtTokenService.GenerateToken(user, roles);

            var response = new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email!,
                Roles = roles.ToList(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            };

            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(
                response, "Login successful"));
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.FailResponse(
                    "Validation failed",
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()));

            var userId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null)
                return NotFound(ApiResponse<string>.FailResponse("User not found"));

            var result = await _userManager.ChangePasswordAsync(
                user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
                return BadRequest(ApiResponse<string>.FailResponse(
                    "Password change failed",
                    result.Errors.Select(e => e.Description).ToList()));

            return Ok(ApiResponse<string>.SuccessResponse(
                "Password changed successfully"));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.FailResponse(
                    "Validation failed"));

            var user = await _userManager.FindByEmailAsync(dto.Email);

            // Always return success even if email not found
            // This prevents email enumeration attacks
            if (user == null)
                return Ok(ApiResponse<string>.SuccessResponse(
                    "If this email exists you will receive a reset link"));

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Send email with reset link in a later phase
            // For now we return the token directly (development only)
            return Ok(ApiResponse<string>.SuccessResponse(
                token, "Reset token generated"));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.FailResponse(
                    "Validation failed",
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()));

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest(ApiResponse<string>.FailResponse(
                    "Invalid request"));

            var result = await _userManager.ResetPasswordAsync(
                user, dto.Token, dto.NewPassword);

            if (!result.Succeeded)
                return BadRequest(ApiResponse<string>.FailResponse(
                    "Password reset failed",
                    result.Errors.Select(e => e.Description).ToList()));

            return Ok(ApiResponse<string>.SuccessResponse(
                "Password reset successful"));
        }
    }
}
