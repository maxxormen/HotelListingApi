using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HotelListingApi.Data;
using System.Linq;

namespace HotelListingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private static List<Hotel> hotels = new List<Hotel>
        {
            new Hotel { Id = 1, Name = "Hotel A", Address = "Address A", Rating = 4.5 },
            new Hotel { Id = 2, Name = "Hotel B", Address = "Address B", Rating = 3.8 },
        };

        [HttpGet]
        public ActionResult<IEnumerable<Hotel>> GetHotels()
        {
            return Ok(hotels);
        }

        [HttpGet("{id}")]
        public ActionResult<Hotel> GetHotel(int id)
        {
            var hotel = hotels.FirstOrDefault(h => h.Id == id);
            if (hotel == null)
            {
                return NotFound();
            }
            return Ok(hotel);
        }
   
        [HttpPost]
        public ActionResult<Hotel> CreateHotel([FromBody] Hotel hotel)
        {
            if (hotel.Id <= 0)
            {
                return BadRequest(new { message = $"Invalid hotel ID." });
            }
            if (!hotels.Any(h => h.Id == hotel.Id))
            {
                hotels.Add(hotel);
                return CreatedAtAction(nameof(GetHotel), new { id = hotel.Id }, hotel);
            }
            else
            {
                return BadRequest(new { message = $"Hotel with ID {hotel.Id} already exists." });
            }
            
        }

        [HttpPut("{id}")]
        public ActionResult<Hotel> PutHotel(int id,[FromBody] Hotel hotel)
        {
            var existingHotel = hotels.FirstOrDefault(h => h.Id == id);
            if (existingHotel == null)
            {
                return NotFound();
            }

            existingHotel.Name = hotel.Name;
            existingHotel.Address = hotel.Address;
            existingHotel.Rating = hotel.Rating;

            return Ok(existingHotel);
        }

        [HttpDelete("{id}")]
        public ActionResult<Hotel> DeleteHotel(int id)
        {
            var hotel = hotels.FirstOrDefault(h => h.Id == id);
            if (hotel == null)
            {
                return NotFound(new { message = $"Hotel with ID {id} not found." });
            }
            hotels.Remove(hotel);
            return NoContent();
        }
    }
}
