using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShipIt.Core.Services.Authentication;

namespace ShipIt.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AuthController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        await _authenticationService.RegisterAsync(request);

        return Ok(new
        {
            message = "User registered successfully."
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResponse>> Login(LoginRequest request)
    {
        var response = await _authenticationService.LoginAsync(request);

        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthenticationResponse>> RefreshToken(RefreshTokenRequest request)
    {
        var response = await _authenticationService.RefreshTokenAsync(request.RefreshToken);

        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequest request)
    {
        await _authenticationService.LogoutAsync(request.RefreshToken);

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            Username = User.Identity?.Name,
            Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
            Role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
        });
    }
}