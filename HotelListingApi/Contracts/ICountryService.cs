
using HotelListingApi.DTOs.Country;
using HotelListingApi.Results;

namespace HotelListingApi.Contracts;
public interface ICountryService
{
    Task<Result<IEnumerable<GetCountriesDto>>> GetCountriesAsync();
    Task<Result<GetCountryDto?>> GetCountryAsync(int id);
    Task<Result> UpdateCountryAsync(int id, UpdateCountryDto countryDto);
    Task<Result<GetCountryDto>> CreateCountryAsync(CreateCountryDto countryDto);
    Task<Result> DeleteCountryAsync(int id);
    Task<bool> CountryExistsAsync(int id);
    Task<bool> CountryExistsAsync(string name);
}