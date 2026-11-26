using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using HotelListingApi.Contracts;
using HotelListingApi.Data;
using HotelListingApi.DTOs.Auth;
using HotelListingApi.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace HotelListingApi.Services;

public class UserService(UserManager<ApplicationUser> userManager, IMapper mapper,
    IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : IUserService
{
    public string UserId => httpContextAccessor?
            .HttpContext?
            .User?
            .FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
        ?? httpContextAccessor?
            .HttpContext?
            .User?
            .FindFirst(ClaimTypes.NameIdentifier)?.Value 
        ?? string.Empty;
    public async Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto registerUserDto)
    {
        var user = mapper.Map<ApplicationUser>(registerUserDto);

        var result = await userManager.CreateAsync(user, registerUserDto.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => new Error { Code = e.Code, Description = e.Description }).ToArray();
            return Result<RegisteredUserDto>.BadRequest(errors);
        }

        await userManager.AddToRoleAsync(user, registerUserDto.Role);

        var registeredUserDto = mapper.Map<RegisteredUserDto>(user);

        return Result<RegisteredUserDto>.Success(registeredUserDto);
    }

    public async Task<Result<string>> LoginAsync(LoginUserDto loginUserDto)
    {
        var user = await userManager.FindByEmailAsync(loginUserDto.Email);
        if (user == null)
        {
            return Result<string>.BadRequest(new Error { Code = "InvalidCredentials", Description = "Invalid credentials" });
        }

        var isPasswordValid = await userManager.CheckPasswordAsync(user, loginUserDto.Password);
        if (!isPasswordValid)
        {
            return Result<string>.BadRequest(new Error { Code = "InvalidCredentials", Description = "Invalid credentials" });
        }

        var token = await GenerateToken(user);

        return Result<string>.Success(token);
    }

    private async Task<string> GenerateToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.FullName),
        };

        var roles = await userManager.GetRolesAsync(user);
        var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();

        claims = claims.Union(roleClaims).ToList();

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                configuration["JwtSettings:Key"]));

        var credentials = new SigningCredentials(securityKey,
            SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: configuration["JwtSettings:Issuer"],
            audience: configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(
                configuration["JwtSettings:DurationInMinutes"]
            )),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}