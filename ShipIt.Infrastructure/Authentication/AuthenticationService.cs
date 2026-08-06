using ShipIt.Core.Enums;
using ShipIt.Core.Interfaces.Repositories;
using ShipIt.Core.Models;
using ShipIt.Core.Services.Authentication;

namespace ShipIt.Infrastructure.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    public AuthenticationService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService, IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _refreshTokenRepository = refreshTokenRepository;
    }
    public async Task RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var username = request.Username.Trim();

        if (await _userRepository.ExistsByEmailAsync(email))
            throw new Exception("Email already exists.");

        if (await _userRepository.ExistsByUsernameAsync(username))
            throw new Exception("Username already exists.");

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = PlatformRole.User,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);

        await _userRepository.SaveChangesAsync();
    }

    public async Task<AuthenticationResponse> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (storedToken == null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        if (storedToken.IsRevoked)
            throw new UnauthorizedAccessException("Refresh token has been revoked.");

        if (storedToken.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token has expired.");

        // Revoke old token
        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;

        // Generate new refresh token
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            UserId = storedToken.UserId,
            Token = newRefreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        });

        await _refreshTokenRepository.SaveChangesAsync();

        return new AuthenticationResponse
        {
            AccessToken = _jwtService.GenerateAccessToken(storedToken.User),
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };
    }
    public async Task<AuthenticationResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(
            request.UsernameOrEmail.ToLowerInvariant());

        user ??= await _userRepository.GetByUsernameAsync(request.UsernameOrEmail);

        if (user == null)
            throw new Exception("Invalid username/email or password.");

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            throw new Exception("Invalid username/email or password.");

        var refreshToken = _jwtService.GenerateRefreshToken();

        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            RefreshTokenId = Guid.NewGuid(),
            UserId = user.UserId,
            Token = refreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        });

        await _refreshTokenRepository.SaveChangesAsync();

        return new AuthenticationResponse
        {
            AccessToken = _jwtService.GenerateAccessToken(user),
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };
    }
    public async Task LogoutAsync(string refreshToken)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (storedToken == null)
            return;

        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;

        await _refreshTokenRepository.SaveChangesAsync();
    }

}