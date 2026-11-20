using HotelListingApi.Contracts;
using HotelListingApi.Data;
using HotelListingApi.DTOs.Booking;
using HotelListingApi.Results;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace HotelListingApi.Services;

public class BookingService(HotelListingDbContext context, IHttpContextAccessor httpClientAccessor) : IBookingService
{
    public async Task<Result<IEnumerable<GetBookingDto>>> GetBookingsForHotelAsync(int id)
    {
        var hotelExists = await context.Hotels.AnyAsync(h => h.Id == id);

        if (!hotelExists)
        {
            return Result<IEnumerable<GetBookingDto>>.Failure(new Error
            {
                Code = "HotelNotFound",
                Description = $"Hotel with id {id} not found."
            });
        }

        var bookings = await context.Bookings
            .Where(b => b.HotelId == id)
            .OrderBy(b => b.CheckIn)
            .Select(b => new GetBookingDto(
                b.Id,
                b.HotelId,
                b.Hotel!.Name,
                b.CheckIn,
                b.CheckOut,
                b.Guests,
                b.TotalPrice,
                b.CreatedAtUtc,
                b.UpdatedAtUtc,
                b.Status.ToString()
            )).ToListAsync();

        return Result<IEnumerable<GetBookingDto>>.Success(bookings);
    }

    public async Task<Result<GetBookingDto>> CreateBookingAsync(CreateBookingDto bookingDto)
    
    {
        var userId = httpClientAccessor?
        .HttpContext?
        .User?
        .FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (userId == null || string.IsNullOrWhiteSpace(userId))
        {
            return Result<GetBookingDto>.Failure(new Error
            {
                Code = "Validation",
                Description = "User is required."
            });
        }

        var hotel = await context.Hotels.Where(h => h.Id == bookingDto.HotelId).FirstOrDefaultAsync();

        if (hotel is null)
        {
            return Result<GetBookingDto>.Failure(new Error
            {
                Code = "NotFound",
                Description = $"Hotel with id {bookingDto.HotelId} not found."
            });
        }

        var nights = bookingDto.CheckOut.DayNumber - bookingDto.CheckIn.DayNumber;

        if (nights <= 0)
        {
            return Result<GetBookingDto>.Failure(new Error
            {
                Code = "Validation",
                Description = "Check-out date must be after check-in date."
            });
        }

        if (bookingDto.Guests <= 0)
        {
            return Result<GetBookingDto>.Failure(new Error
            {
                Code = "Validation",
                Description = "Number of guests must be greater than zero."
            });
        }

        var overlaps = await context.Bookings.AnyAsync(b =>
            b.HotelId == bookingDto.HotelId &&
            b.Status != BookingStatus.Cancelled &&
            b.CheckIn < bookingDto.CheckOut &&
            bookingDto.CheckIn < b.CheckOut &&
            b.UserId == userId
        );

        if (overlaps)
        {
            return Result<GetBookingDto>.Failure(new Error
            {
                Code = "Validation",
                Description = "Booking overlaps with an existing booking."
            });
        }

        var totalPrice = nights * hotel.PerNightRate * bookingDto.Guests;
        var booking = new Booking
        {
            HotelId = bookingDto.HotelId,
            UserId = userId,
            CheckIn = bookingDto.CheckIn,
            CheckOut = bookingDto.CheckOut,
            Guests = bookingDto.Guests,
            TotalPrice = totalPrice,
            Status = BookingStatus.Pending
        };

        context.Bookings.Add(booking);

        await context.SaveChangesAsync();

        return Result<GetBookingDto>.Success(new GetBookingDto(
            booking.Id,
            booking.HotelId,
            hotel.Name,
            booking.CheckIn,
            booking.CheckOut,
            booking.Guests,
            booking.TotalPrice,
            booking.CreatedAtUtc,
            booking.UpdatedAtUtc,
            booking.Status.ToString()
        ));
    }

    public async Task<Result<GetBookingDto>> UpdateBookingAsync(int hotelId, int bookingId, UpdateBookingDto bookingDto)
    {
        var userId = httpClientAccessor?
        .HttpContext?
        .User?
        .FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (userId == null || string.IsNullOrWhiteSpace(userId))
        {
            return Result<GetBookingDto>.Failure(new Error
            {
                Code = "Validation",
                Description = "User is required."
            });
        }

        var nights = bookingDto.CheckOut.DayNumber - bookingDto.CheckIn.DayNumber;

        if (nights <= 0)
        {
            return Result<GetBookingDto>.Failure(new Error
            {
                Code = "Validation",
                Description = "Check-out date must be after check-in date."
            });
        }

        if (bookingDto.Guests <= 0)
        {
            return Result<GetBookingDto>.Failure(new Error
            {
                Code = "Validation",
                Description = "Number of guests must be greater than zero."
            });
        }

        var overlaps = await context.Bookings.AnyAsync(b =>
            b.HotelId == bookingDto.HotelId &&
            b.Status != BookingStatus.Cancelled &&
            b.CheckIn < bookingDto.CheckOut &&
            bookingDto.CheckIn < b.CheckOut &&
            b.UserId == userId
        );

        if (overlaps)
        {
            return Result<GetBookingDto>.Failure(new Error
            {
                Code = "Validation",
                Description = "Booking overlaps with an existing booking."
            });
        }

        var booking = await context.Bookings
            .Include(b => b.Hotel)
            .FirstOrDefaultAsync(b => b.Id == bookingId 
                && b.HotelId == hotelId 
                && b.UserId == userId);

        if (booking is null)
        {
            return Result<GetBookingDto>.Failure(new Error
            {
                Code = "NotFound",
                Description = $"Booking with id {bookingId} for hotel {hotelId} not found."
            });
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return Result<GetBookingDto>.Failure(new Error
            {
                Code = "Validation",
                Description = "Cannot update a cancelled booking."
            });
        }

        var perNightRate = booking.Hotel!.PerNightRate;
        booking.CheckIn = bookingDto.CheckIn;
        booking.CheckOut = bookingDto.CheckOut;
        booking.Guests = bookingDto.Guests;
        booking.TotalPrice = nights * perNightRate * bookingDto.Guests;
        booking.UpdatedAtUtc = DateTime.UtcNow;
        // not sure if this one is needed => booking.Status = BookingStatus.Pending;

        await context.SaveChangesAsync();

        return Result<GetBookingDto>.Success(new GetBookingDto(
            booking.Id,
            booking.HotelId,
            booking.Hotel!.Name,
            booking.CheckIn,
            booking.CheckOut,
            booking.Guests,
            booking.TotalPrice,
            booking.CreatedAtUtc,
            booking.UpdatedAtUtc,
            booking.Status.ToString()
        ));
    }

    public async Task<Result> CancelBookingAsync(int hotelId, int bookingId)
    {
        var userId = httpClientAccessor?
        .HttpContext?
        .User?
        .FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        var booking = await context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId 
                && b.HotelId == hotelId 
                && b.UserId == userId);

        if (booking is null)
        {
            return Result.Failure(new Error
            {
                Code = "NotFound",
                Description = $"Booking with id {bookingId} for hotel {hotelId} not found."
            });
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return Result.Failure(new Error
            {
                Code = "Validation",
                Description = "Booking is already cancelled."
            });
        }

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return Result.Success();
    }
}