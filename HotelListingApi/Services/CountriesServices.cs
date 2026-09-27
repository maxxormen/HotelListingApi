using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListingApi.Data;
using HotelListingApi.DTOs.Country;
using HotelListingApi.DTOs.Hotel;
using HotelListingApi.Contracts;

namespace HotelListingApi.Services.CountriesServices;

public class CountriesServices(HotelListingDbContext context) : ICountriesServices
{
    public async Task<IEnumerable<GetCountriesDto>> GetCountriesAsync()
    {
        return await context.Countries
        .Select(c => new GetCountriesDto(c.CountryId, c.Name, c.ShortName))
        .ToListAsync();
    }

    public async Task<GetCountryDto?> GetCountryAsync(int id)
    {
        var country = await context.Countries
            .Where(c => c.CountryId == id)
            .Select(c => new GetCountryDto(
                c.CountryId,
                c.Name,
                c.ShortName,
                c.Hotels.Select(h => new GetHotelSlimDto(
                    h.Id,
                    h.Name,
                    h.Address,
                    h.Rating
                )).ToList()
            ))
            .FirstOrDefaultAsync();
        return country ?? null;
    }

    public async Task UpdateCountryAsync(int id, UpdateCountryDto countryDto)
    {
        var country = await context.Countries.FindAsync(id);
        if (country == null)
        {
            throw new Exception($"Country with ID {id} not found.");
        }

        country.Name = countryDto.Name;
        country.ShortName = countryDto.ShortName;
        context.Countries.Update(country);

        await context.SaveChangesAsync();
    }

    public async Task<GetCountryDto> CreateCountryAsync(CreateCountryDto countryDto)
    {
        var country = new Country
        {
            Name = countryDto.Name,
            ShortName = countryDto.ShortName
        };

        context.Countries.Add(country);
        await context.SaveChangesAsync();

        var resultCountry = new GetCountryDto(
            country.CountryId,
            country.Name,
            country.ShortName,
            []
        );

        return resultCountry;
    }

    public async Task DeleteCountryAsync(int id)
    {
        var country = await context.Countries.Where(c => c.CountryId == id).ExecuteDeleteAsync();
        await context.SaveChangesAsync();
    }

    public async Task<bool> CountryExistsAsync(int id)
    {
        return await context.Countries.AnyAsync(e => e.CountryId == id);
    }

    public async Task<bool> CountryExistsAsync(string name)
    {
        return await context.Countries.AnyAsync(e => e.Name == name);
    }
}