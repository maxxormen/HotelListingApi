using Microsoft.AspNetCore.Mvc;
using HotelListingApi.DTOs.Hotel;
using HotelListingApi.Contracts;
using HotelListingApi.Results;

namespace HotelListingApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HotelsController : BaseApiController
{
    private readonly IHotelService hotelService;

    public HotelsController(IHotelService hotelService)
    {
        this.hotelService = hotelService;
    }

    // GET: api/Hotels
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetHotelsDto>>> GetHotels()
    {
        var result = await hotelService.GetHotelsAsync();
        return ToActionResult(result);
    }

    // GET: api/Hotels/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GetHotelDto?>> GetHotel(int id)
    {
        var result = await hotelService.GetHotelAsync(id);
        return ToActionResult(result);
    }

    // PUT: api/Hotels/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutHotel(int id, UpdateHotelDto hotelDto)
    {
        var result = await hotelService.UpdateHotelAsync(id, hotelDto);
        return ToActionResult(result);
    }

    // POST: api/Hotels
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<GetHotelDto>> PostHotel(CreateHotelDto hotelDto)
    {
        var result = await hotelService.CreateHotelAsync(hotelDto);
        if (!result.IsSuccess) return MapErrorsToResponse(result.Errors);

        return CreatedAtAction(nameof(GetHotel), new { id = result.Value!.Id }, result.Value);
    }

    // DELETE: api/Hotels/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHotel(int id)
    {
        var result = await hotelService.DeleteHotelAsync(id);
        return ToActionResult(result);
    }
}
