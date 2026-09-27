using HotelListingApi.Data;
using HotelListingApi.DTOs.Country;
using HotelListingApi.DTOs.Hotel;

namespace HotelListingApi.Contracts;
public interface ICountriesServices
{
    Task<IEnumerable<GetCountriesDto>> GetCountriesAsync();
    Task<GetCountryDto?> GetCountryAsync(int id);
    Task UpdateCountryAsync(int id, UpdateCountryDto countryDto);
    Task<GetCountryDto> CreateCountryAsync(CreateCountryDto countryDto);
    Task DeleteCountryAsync(int id);
    Task<bool> CountryExistsAsync(int id);
    Task<bool> CountryExistsAsync(string name);
}

public interface IHotelsServices
{
    Task<IEnumerable<GetHotelsDto>> GetHotelsAsync();
    Task<GetHotelDto?> GetHotelAsync(int id);
    Task UpdateHotelAsync(int id, UpdateHotelDto hotelDto);
    Task<GetHotelDto> CreateHotelAsync(CreateHotelDto hotelDto);
    Task DeleteHotelAsync(int id);
    Task<bool> HotelExistsAsync(int id);
    Task<bool> HotelExistsAsync(string name);
}