using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListingApi.Data;
using HotelListingApi.DTOs.Country;
using HotelListingApi.DTOs.Hotel;
using HotelListingApi.Contracts;
using HotelListingApi.Results;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace HotelListingApi.Services.CountriesServices;

public class CountriesServices(HotelListingDbContext context, IMapper mapper) : ICountriesServices
{
    public async Task<Result<IEnumerable<GetCountriesDto>>> GetCountriesAsync()
    {
        var countries = await context.Countries
        .Select(c => new GetCountriesDto(c.CountryId, c.Name, c.ShortName))
        .ToListAsync();

        return Result<IEnumerable<GetCountriesDto>>.Success(countries);
    }

    public async Task<Result<GetCountryDto?>> GetCountryAsync(int id)
    {
        try
        {
            var exists = await CountryExistsAsync(id);
            if (!exists)
            {
                return Result<GetCountryDto?>.NotFound();
            }

            var country = await context.Countries
                .Where(c => c.CountryId == id)
                .Include(c => c.Hotels)
                .ProjectTo<GetCountryDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            return Result<GetCountryDto?>.Success(country);
        }
        catch (Exception ex)
        {
            return Result<GetCountryDto?>.Failure(new Error("Error", ex.Message));
        }
    }

    public async Task<Result> UpdateCountryAsync(int id, UpdateCountryDto countryDto)
    {
        try
        {
            var country = await context.Countries.FindAsync(id);

            if (country == null)
            {
                return Result.NotFound(new Error("NotFound", $"Country '{id}' could not be found"));
            }

            mapper.Map(countryDto, country);
            await context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Error", ex.Message));
        }
    }

    public async Task<Result<GetCountryDto>> CreateCountryAsync(CreateCountryDto countryDto)
    {
        try
        {
            var exists = await CountryExistsAsync(countryDto.Name);
            if (exists)
            {
                return Result<GetCountryDto>.Failure(new Error("Conflict", "A country with the same name already exists."));
            }

            var country = mapper.Map<Country>(countryDto);

            context.Countries.Add(country);
            await context.SaveChangesAsync();

            var resultCountry = mapper.Map<GetCountryDto>(country);

            return Result<GetCountryDto>.Success(resultCountry);    
        } 
        catch (Exception ex)
        {
            return Result<GetCountryDto>.Failure(new Error("CreateCountryError", ex.Message));
        }
    }

    public async Task<Result> DeleteCountryAsync(int id)
    {
        try
        {
            var country = await context.Countries.FindAsync(id);
            if (country is null)
            {
                return Result.NotFound(new Error("NotFound", $"Country '{id}' could not be found"));
            }
            await context.Countries
                .Where(q => q.CountryId == id)
                .ExecuteDeleteAsync();
                
            return Result.Success();            
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Error", ex.ToString()));
        }
    }

    public async Task<bool> CountryExistsAsync(int id)
    {
        return await context.Countries.AnyAsync(e => e.CountryId == id);
    }

    public async Task<bool> CountryExistsAsync(string name)
    {
        return await context.Countries
            .AnyAsync(e => e.Name.ToLower().Trim() == name.ToLower().Trim());
    }
}