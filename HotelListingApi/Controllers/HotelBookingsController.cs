using Microsoft.AspNetCore.Mvc;
using HotelListingApi.DTOs.Booking;
using HotelListingApi.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace HotelListingApi.Controllers;

[Authorize]
[ApiController]
[Route("api/hotels/{hotelId:int}/bookings")]
public class HotelBookingsController(IBookingService bookingService) : BaseApiController
{
    [HttpGet]
    [Authorize(Roles = "Hotel Admin, Administrator")]
    public async Task<ActionResult<IEnumerable<GetBookingDto>>> GetBookings([FromRoute] int hotelId)
    {
        var result = await bookingService.GetBookingsForHotelAsync(hotelId);
        return ToActionResult(result);
    }

    [HttpGet]
    [Authorize(Roles = "Hotel Admin, Administrator")]
    public async Task<ActionResult<IEnumerable<GetBookingDto>>> GetUserBooking([FromRoute] int hotelId)
    {
        var result = await bookingService.GetUserBookingsForHotelAsync(hotelId);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<GetBookingDto>> CreateBooking([FromRoute] int hotelId, 
        [FromBody] CreateBookingDto bookingDto)
    {
        var result = await bookingService.CreateBookingAsync(bookingDto);
        return ToActionResult(result);
    }

    [HttpPut("{bookingId:int}")]
    public async Task<ActionResult<GetBookingDto>> UpdateBooking(
        [FromRoute] int hotelId, 
        [FromRoute] int bookingId, 
        UpdateBookingDto bookingDto)
    {
        var result = await bookingService.UpdateBookingAsync(hotelId, bookingId, bookingDto);
        return ToActionResult(result);
    }

    [HttpDelete("{bookingId:int}/cancel")]
    public async Task<IActionResult> CancelBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await bookingService.CancelBookingAsync(hotelId, bookingId);
        return ToActionResult(result);
    }

    [Authorize(Roles = "Hotel Admin, Administrator")]
    [HttpPut("{bookingId:int}/admin/cancel")]
    public async Task<IActionResult> AdminCancelBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await bookingService.AdminCancelBookingAsync(hotelId, bookingId);
        return ToActionResult(result);
    }

    [Authorize(Roles = "Hotel Admin, Administrator")]
    [HttpPut("{bookingId:int}/admin/confirm")]
    public async Task<IActionResult> AdminConfirmBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await bookingService.AdminConfirmBookingAsync(hotelId, bookingId);
        return ToActionResult(result);
    }
}