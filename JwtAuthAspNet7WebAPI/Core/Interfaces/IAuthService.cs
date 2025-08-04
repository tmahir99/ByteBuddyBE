using JwtAuthAspNet7WebAPI.Core.Dtos;

namespace JwtAuthAspNet7WebAPI.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthServiceResponseDto> SeedRolesAsync();
        Task<AuthServiceResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthServiceResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthServiceResponseDto> MakeAdminAsync(UpdatePermissionDto updatePermissionDto);
        Task<AuthServiceResponseDto> MakeUserAsync(UpdatePermissionDto updatePermissionDto);
        Task<List<ApplicationUserDto>> GetAllUsersAsync();
        Task<List<string>> GetAllUserNamesAsync();
        Task<AuthServiceResponseDto> RemoveAdminAsync(UpdatePermissionDto updatePermissionDto);
        Task<AuthServiceResponseDto> RemoveUserAsync(UpdatePermissionDto updatePermissionDto);
        Task<AuthServiceResponseDto> RemoveGuestAsync(UpdatePermissionDto updatePermissionDto);

        // Email Activation Methods
        Task<AuthServiceResponseDto> ActivateEmailAsync(string token);
        Task<AuthServiceResponseDto> ResendActivationEmailAsync(string email);
    }
}
