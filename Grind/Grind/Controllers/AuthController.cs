using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Grind.Core.DTOs;
using Grind.Core.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IClientService _clientService;

    public AuthController(IUserService userService, IClientService clientService)
    {
        _userService = userService;
        _clientService = clientService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterClient([FromBody] UserRegistrationDTO registrationDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var errorMessage = await _clientService.RegisterClientAsync(registrationDto);

        if (errorMessage == null)
        {
            return Ok("Client registered successfully.");
        }
        else
        {
            return BadRequest(errorMessage);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDTO loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var loginResponse = await _userService.LoginAsync(loginDto);

        if (loginResponse == null)
        {
            return Unauthorized("Invalid credentials.");
        }

        return Ok(loginResponse);
    }
}