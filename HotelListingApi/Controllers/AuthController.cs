using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HotelListingApi.DTOs.Auth;
using HotelListingApi.Contracts;

namespace HotelListingApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class AuthController : BaseApiController
{
    private readonly IUsersServices usersService;
    public AuthController(IUsersServices usersService)
    {
        this.usersService = usersService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisteredUserDto>> Register(RegisterUserDto registerUserDto)
    {
        var result = await usersService.RegisterAsync(registerUserDto);
        return ToActionResult(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(LoginUserDto loginUserDto)
    {
       var result = await usersService.LoginAsync(loginUserDto);
       return ToActionResult(result);
    }
}