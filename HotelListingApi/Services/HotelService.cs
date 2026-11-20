using Microsoft.EntityFrameworkCore;
using AutoMapper;

using HotelListingApi.Data;
using HotelListingApi.Contracts;
using HotelListingApi.DTOs.Country;
using HotelListingApi.DTOs.Hotel;
using AutoMapper.QueryableExtensions;
using HotelListingApi.Results;

namespace HotelListingApi.Services;

public class HotelService(HotelListingDbContext context, IMapper mapper) : IHotelService
{
    public async Task<Result<IEnumerable<GetHotelsDto>>> GetHotelsAsync()
    {
        var hotels = await context.Hotels
            .Include(h => h.Country)
            .Select(h => new GetHotelsDto(
                h.Id,
                h.Name,
                h.Address,
                h.Rating,
                h.CountryId
            ))
            .ToListAsync();

        return Result<IEnumerable<GetHotelsDto>>.Success(hotels);
    }

    public async Task<Result<GetHotelDto?>> GetHotelAsync(int id)
    {
        try
        {
            var hotel = await context.Hotels
                .Where(h => h.Id == id)
                .Include(h => h.Country)
                .ProjectTo<GetHotelDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
            
            if (hotel is null)
            {
                return Result<GetHotelDto?>.NotFound();
            }

            return Result<GetHotelDto?>.Success(hotel);
        }
        catch (Exception ex)
        {
            return Result<GetHotelDto?>.Failure(new Error("Error", ex.Message));
        }
        
    }

    public async Task<Result> UpdateHotelAsync(int id, UpdateHotelDto hotelDto)
    {
        try
        {
            var hotel = await context.Hotels.FindAsync(id);
            if (hotel == null)
            {
                return Result.NotFound(new Error("NotFound", $"Hotel '{id}' not found"));
            }

            // Update the tracked entity in place
            mapper.Map(hotelDto, hotel);
            await context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Error", ex.Message));
        }
    }

    public async Task<Result<GetHotelDto>> CreateHotelAsync(CreateHotelDto hotelDto)
    {
        try
        {
            var exists = await HotelExistsAsync(hotelDto.Name);
            if(exists)
            {
                return Result<GetHotelDto>.BadRequest(new Error("Duplicate", $"Hotel '{hotelDto.Name}' already exists"));
            }
            var hotel = mapper.Map<Hotel>(hotelDto);
            context.Hotels.Add(hotel);

            await context.SaveChangesAsync();

            var returnHotel = mapper.Map<GetHotelDto>(hotel);

            return Result<GetHotelDto>.Success(returnHotel);
        }
        catch (Exception ex)
        {
            return Result<GetHotelDto>.Failure(new Error("Error", ex.Message));
        }
        
    }

    public async Task<Result> DeleteHotelAsync(int id)
    {
        try
        {
            var hotel = await context.Hotels.FindAsync(id);
            if (hotel is null)
            {
                return Result.NotFound(new Error("NotFound", $"Hotel '{id}' could not be found"));
            }
            await context.Hotels
                .Where(h => h.Id == id)
                .ExecuteDeleteAsync();
            await context.SaveChangesAsync();

            return Result.Success();    
        } 
        catch (Exception ex)
        {
            return Result.Failure(new Error("Error", ex.ToString()));
        }
    }

    public async Task<bool> HotelExistsAsync(int id)
    {
        return await context.Hotels.AnyAsync(h => h.Id == id);
    }

    public async Task<bool> HotelExistsAsync(string name)
    {
        return await context.Hotels.AnyAsync(h => h.Name == name);
    }
}