using HotelListingApi.Contracts;
using HotelListingApi.Data;
using HotelListingApi.DTOs.Booking;
using HotelListingApi.Results;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace HotelListingApi.Services;

public class BookingService(HotelListingDbContext context, IUserService userService,
    IMapper mapper) : IBookingService
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
            .ProjectTo<GetBookingDto>(mapper.ConfigurationProvider)
            .ToListAsync();

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
            .ProjectTo<GetBookingDto>(mapper.ConfigurationProvider)
            .ToListAsync();

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

        var overlaps = await IsOverLap(hotel.Id, userId, bookingDto.CheckIn, bookingDto.CheckOut);

        if (overlaps)
        {
            return Result<GetBookingDto>.Failure(new Error
            {
                Code = ErrorCodes.Conflict,
                Description = "Booking overlaps with an existing booking."
            });
        }

        var nights = bookingDto.CheckOut.DayNumber - bookingDto.CheckIn.DayNumber;
        var totalPrice = nights * hotel.PerNightRate * bookingDto.Guests;

        var booking = mapper.Map<Booking>(bookingDto);
        booking.UserId = userId;

        context.Bookings.Add(booking);

        await context.SaveChangesAsync();
        
        var result = mapper.Map<GetBookingDto>(booking);

        return Result<GetBookingDto>.Success(result);
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

        var overlaps = await IsOverLap(hotelId, userId, bookingDto.CheckIn, bookingDto.CheckOut, bookingId);

        if (overlaps)
        {
            return Result<GetBookingDto>.Failure(new Error
            {
                Code = ErrorCodes.Conflict,
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

        mapper.Map(bookingDto, booking);

        var perNightRate = booking.Hotel!.PerNightRate;
        booking.TotalPrice = nights * perNightRate * bookingDto.Guests;
        booking.UpdatedAtUtc = DateTime.UtcNow;
        // not sure if this one is needed => booking.Status = BookingStatus.Pending;

        await context.SaveChangesAsync();

        var result = mapper.Map<GetBookingDto>(booking);

        return Result<GetBookingDto>.Success(result);
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

    public async Task<bool> IsOverLap(int hotelId,string userId,
        DateOnly checkIn, DateOnly checkOut, int? bookingId = null)
    {
        var query = context.Bookings.Where(b =>
            b.HotelId == hotelId &&
            b.Status != BookingStatus.Cancelled &&
            b.CheckIn < checkOut &&
            b.CheckOut > checkIn &&
            b.UserId == userId).AsQueryable();
        if (bookingId.HasValue)
        {
            query = query.Where(b => b.Id != bookingId.Value);
        }

        return await query.AnyAsync();
    }
}