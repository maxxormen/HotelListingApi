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
    private readonly IUserService userService;
    public AuthController(IUserService userService)
    {
        this.userService = userService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisteredUserDto>> Register(RegisterUserDto registerUserDto)
    {
        var result = await userService.RegisterAsync(registerUserDto);
        return ToActionResult(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(LoginUserDto loginUserDto)
    {
       var result = await userService.LoginAsync(loginUserDto);
       return ToActionResult(result);
    }
}