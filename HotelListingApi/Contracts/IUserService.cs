using HotelListingApi.DTOs.Auth;
using HotelListingApi.Results;

namespace HotelListingApi.Contracts;
public interface IUserService
{
    public string UserId { get; }
    Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto registerUserDto);
    Task<Result<string>> LoginAsync(LoginUserDto loginUserDto);

}