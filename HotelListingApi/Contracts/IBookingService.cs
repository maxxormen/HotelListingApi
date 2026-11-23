using HotelListingApi.DTOs.Booking;
using HotelListingApi.Results;

namespace HotelListingApi.Contracts;
public interface IBookingService
{
    Task<Result<IEnumerable<GetBookingDto>>> GetBookingsForHotelAsync(int id);
    Task<Result<GetBookingDto>> CreateBookingAsync(CreateBookingDto bookingDto);
    Task<Result<GetBookingDto>> UpdateBookingAsync(int hotelId, int bookingId, UpdateBookingDto bookingDto);
    Task<Result> CancelBookingAsync(int hotelId, int bookingId);
    Task<Result> AdminCancelBookingAsync(int hotelId, int bookingId);
    Task<Result> AdminConfirmBookingAsync(int hotelId, int bookingId);
    Task<Result<IEnumerable<GetBookingDto>>> GetUserBookingsForHotelAsync(int hotelId);
}