namespace ShipIt.Core.Services.Authentication;
public interface IAuthenticationService
{
    Task RegisterAsync(RegisterRequest request);

    Task<AuthenticationResponse> LoginAsync(LoginRequest request);

    Task<AuthenticationResponse> RefreshTokenAsync(string refreshToken);

    Task LogoutAsync(string refreshToken);
}