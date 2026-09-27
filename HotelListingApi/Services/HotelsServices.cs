using Microsoft.EntityFrameworkCore;
using HotelListingApi.Data;
using HotelListingApi.Contracts;
using HotelListingApi.DTOs.Country;
using HotelListingApi.DTOs.Hotel;

namespace HotelListingApi.Services.HotelsServices;

public class HotelServices(HotelListingDbContext context) : IHotelsServices
{
    public async Task<IEnumerable<GetHotelsDto>> GetHotelsAsync()
    {
        return await context.Hotels
            .Include(h => h.Country)
            .Select(h => new GetHotelsDto(
                h.Id,
                h.Name,
                h.Address,
                h.Rating,
                h.CountryId
            ))
            .ToListAsync();
    }

    public async Task<GetHotelDto?> GetHotelAsync(int id)
    {
        var hotel = await context.Hotels
            .Where(h => h.Id == id)
            .Select(h => new GetHotelDto(
                h.Id,
                h.Name,
                h.Address,
                h.Rating,
                h.CountryId,
                h.Country!.Name
            ))
            .FirstOrDefaultAsync();
        return hotel ?? null;
    }

    public async Task UpdateHotelAsync(int id, UpdateHotelDto hotelDto)
    {
        var hotel = await context.Hotels.FindAsync(id);
        if (hotel == null)
        {
            throw new Exception($"Hotel with ID {id} not found.");
        }

        hotel.Name = hotelDto.Name;
        hotel.Address = hotelDto.Address;
        hotel.Rating = hotelDto.Rating;
        hotel.CountryId = hotelDto.CountryId;
        context.Hotels.Update(hotel);

        await context.SaveChangesAsync();
    }

    public async Task<GetHotelDto> CreateHotelAsync(CreateHotelDto hotelDto)
    {
        var hotel = new Hotel
        {
            Name = hotelDto.Name,
            Address = hotelDto.Address,
            Rating = hotelDto.Rating,
            CountryId = hotelDto.CountryId
        };

        context.Hotels.Add(hotel);
        await context.SaveChangesAsync();

        return new GetHotelDto(
            hotel.Id,
            hotel.Name,
            hotel.Address,
            hotel.Rating,
            hotel.CountryId,
            (await context.Countries.FindAsync(hotel.CountryId))?.Name ?? string.Empty
        );
    }

    public async Task DeleteHotelAsync(int id)
    {
        var hotel = await context.Hotels.Where(h => h.Id == id).ExecuteDeleteAsync();
        await context.SaveChangesAsync();
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