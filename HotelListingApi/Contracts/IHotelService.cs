using HotelListingApi.DTOs.Hotel;
using HotelListingApi.Results;

namespace HotelListingApi.Contracts;

public interface IHotelService
{
    Task<Result<IEnumerable<GetHotelsDto>>> GetHotelsAsync();
    Task<Result<GetHotelDto?>> GetHotelAsync(int id);
    Task<Result> UpdateHotelAsync(int id, UpdateHotelDto hotelDto);
    Task<Result<GetHotelDto>> CreateHotelAsync(CreateHotelDto hotelDto);
    Task<Result> DeleteHotelAsync(int id);
    Task<bool> HotelExistsAsync(int id);
    Task<bool> HotelExistsAsync(string name);
}