using HotelListingApi.DTOs.Country;
using HotelListingApi.DTOs.Auth;
using HotelListingApi.Results;

namespace HotelListingApi.Contracts;
public interface IUsersServices
{
    Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto registerUserDto);
    Task<Result<string>> LoginAsync(LoginUserDto loginUserDto);

}