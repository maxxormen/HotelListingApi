using HotelListingApi.Contracts;
using HotelListingApi.Data;
using HotelListingApi.DTOs.Booking;
using HotelListingApi.Results;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace HotelListingApi.Services;

public class BookingService(HotelListingDbContext context, IUserService userService) : IBookingService
{
    public async Task<Result<IEnumerable<GetBookingDto>>> GetBookingsForHotelAsync(int id)
    {
        var userId = userService.UserId;

        var hotelAdmin = await context.HotelAdmins
            .FirstOrDefaultAsync(ha => ha.HotelId == id && ha.UserId == userId);

        if (hotelAdmin is null)
        {
            return Result<IEnumerable<GetBookingDto>>.Failure(new Error
            {
                Code = ErrorCodes.Forbid,
                Description = "You are not an admin for this hotel."
            });
        }

        var hotelExists = await context.Hotels.AnyAsync(h => h.Id == id);

        if (!hotelExists)
        {
            return Result<IEnumerable<GetBookingDto>>.Failure(new Error
            {
                Code = ErrorCodes.NotFound,
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

    public async Task<Result<IEnumerable<GetBookingDto>>> GetUserBookingsForHotelAsync(int hotelId)
    {
        var userId = userService.UserId;

        var hotelExists = await context.Hotels.AnyAsync(h => h.Id == hotelId);

        if (!hotelExists)
        {
            return Result<IEnumerable<GetBookingDto>>.Failure(new Error
            {
                Code = ErrorCodes.NotFound,
                Description = $"Hotel with id {hotelId} not found."
            });
        }
    
        var bookings = await context.Bookings
            .Where(b => b.HotelId == hotelId && b.UserId == userId)
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
        var userId = userService.UserId;

        if (userId == null || string.IsNullOrWhiteSpace(userId))
        {
            return Result<GetBookingDto>.Failure(new Error
            {
                Code = ErrorCodes.Validation,
                Description = "User is required."
            });
        }

        var hotel = await context.Hotels.Where(h => h.Id == bookingDto.HotelId).FirstOrDefaultAsync();

        if (hotel is null)
        {
            return Result<GetBookingDto>.Failure(new Error
            {
                Code = ErrorCodes.NotFound,
                Description = $"Hotel with id {bookingDto.HotelId} not found."
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
                Code = ErrorCodes.Validation,
                Description = "Booking overlaps with an existing booking."
            });
        }

        var nights = bookingDto.CheckOut.DayNumber - bookingDto.CheckIn.DayNumber;

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
        var userId = userService.UserId;

        if (userId == null || string.IsNullOrWhiteSpace(userId))
        {
            return Result<GetBookingDto>.Failure(new Error
            {
                Code = ErrorCodes.Validation,
                Description = "User is required."
            });
        }

        if (bookingDto.Guests <= 0)
        {
            return Result<GetBookingDto>.Failure(new Error
            {
                Code = ErrorCodes.Validation,
                Description = "Number of guests must be greater than zero."
            });
        }

        var overlaps = await context.Bookings.AnyAsync(b =>
            b.HotelId == bookingDto.HotelId &&
            b.Status != BookingStatus.Cancelled &&
            b.CheckIn < bookingDto.CheckOut &&
            b.CheckOut > bookingDto.CheckIn &&
            b.UserId == userId
        );

        if (overlaps)
        {
            return Result<GetBookingDto>.Failure(new Error
            {
                Code = ErrorCodes.Validation,
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
                Code = ErrorCodes.NotFound,
                Description = $"Booking with id {bookingId} for hotel {hotelId} not found."
            });
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return Result<GetBookingDto>.Failure(new Error
            {
                Code = ErrorCodes.Validation,
                Description = "Cannot update a cancelled booking."
            });
        }

        var nights = bookingDto.CheckOut.DayNumber - bookingDto.CheckIn.DayNumber;

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
        var userId = userService.UserId;

        var booking = await context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId 
                && b.HotelId == hotelId 
                && b.UserId == userId);

        if (booking is null)
        {
            return Result.Failure(new Error
            {
                Code = ErrorCodes.NotFound,
                Description = $"Booking with id {bookingId} for hotel {hotelId} not found."
            });
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return Result.Failure(new Error
            {
                Code = ErrorCodes.Validation,
                Description = "Booking is already cancelled."
            });
        }

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> AdminCancelBookingAsync(int hotelId, int bookingId)
    {
        var userId = userService.UserId;

        var hotelUserAdmin = await context.HotelAdmins
            .FirstOrDefaultAsync(ha => ha.HotelId == hotelId && ha.UserId == userId);

        if (hotelUserAdmin is null)
        {
            return Result.Failure(new Error
            {
                Code = ErrorCodes.Forbid,
                Description = "You are not an admin for this hotel."
            });
        }

        var booking = await context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.HotelId == hotelId);

        if (booking is null)
        {
            return Result.Failure(new Error
            {
                Code = ErrorCodes.NotFound,
                Description = $"Booking with id {bookingId} for hotel {hotelId} not found."
            });
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return Result.Failure(new Error
            {
                Code = ErrorCodes.Validation,
                Description = "Booking is already cancelled."
            });
        }

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> AdminConfirmBookingAsync(int hotelId, int bookingId)
    {
        var userId = userService.UserId;

        var hotelUserAdmin = await context.HotelAdmins
            .FirstOrDefaultAsync(ha => ha.HotelId == hotelId && ha.UserId == userId);

        if (hotelUserAdmin is null)
        {
            return Result.Failure(new Error
            {
                Code = ErrorCodes.Forbid,
                Description = "You are not an admin for this hotel."
            });
        }

        var booking = await context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.HotelId == hotelId);

        if (booking is null)
        {
            return Result.Failure(new Error
            {
                Code = ErrorCodes.NotFound,
                Description = $"Booking with id {bookingId} for hotel {hotelId} not found."
            });
        }

        if (booking.Status == BookingStatus.Confirmed)
        {
            return Result.Failure(new Error
            {
                Code = ErrorCodes.Validation,
                Description = "Booking is already confirmed."
            });
        }

        booking.Status = BookingStatus.Confirmed;
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return Result.Success();
    }
}