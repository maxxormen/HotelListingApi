using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListingApi.Data;
using HotelListingApi.DTOs.Hotel;
using HotelListingApi.Contracts;

namespace HotelListingApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HotelsController : ControllerBase
{
    private readonly IHotelsServices hotelsServices;

    public HotelsController(IHotelsServices hotelsServices)
    {
        this.hotelsServices = hotelsServices;
    }

    // GET: api/Hotels
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetHotelsDto>>> GetHotels()
    {
        var hotels = await hotelsServices.GetHotelsAsync();
        return Ok(hotels);
    }

    // GET: api/Hotels/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GetHotelDto>> GetHotel(int id)
    {
        var hotel = await hotelsServices.GetHotelAsync(id);
        if (hotel == null)
        {
            return NotFound();
        }

        return hotel;
    }

    // PUT: api/Hotels/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutHotel(int id, UpdateHotelDto hotelDto)
    {
        if (id != hotelDto.Id)
        {
            return BadRequest();
        }

        var hotel = await hotelsServices.GetHotelAsync(id);
        if (hotel == null)
        {
            return NotFound();
        }

        try
        {
            await hotelsServices.UpdateHotelAsync(id, hotelDto);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await hotelsServices.HotelExistsAsync(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Hotels
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Hotel>> PostHotel(CreateHotelDto hotelDto)
    {
        var hotel = await hotelsServices.CreateHotelAsync(hotelDto);

        return CreatedAtAction(nameof(GetHotel), new { id = hotel.Id }, hotel);
    }

    // DELETE: api/Hotels/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHotel(int id)
    {
        var hotel = await hotelsServices.GetHotelAsync(id);
        if (hotel == null)
        {
            return NotFound();
        }

        await hotelsServices.DeleteHotelAsync(id);

        return NoContent();
    }
}
