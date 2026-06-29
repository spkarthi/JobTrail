using JobTrail.DTOs;
namespace JobTrail.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
    Task<AuthResponseDto> RefreshAsync(RefreshRequestDto dto);
    Task RevokeAsync(Guid userId);
}