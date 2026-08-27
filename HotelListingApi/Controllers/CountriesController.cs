using HotelListingApi.Data;
using Microsoft.AspNetCore.Mvc;

namespace HotelListingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private static readonly List<Country> Countries = new()
        {
            new Country { Id = 1, Name = "United States", Code = "US" },
            new Country { Id = 2, Name = "Canada", Code = "CA" },
            new Country { Id = 3, Name = "United Kingdom", Code = "GB" }
        };

        [HttpGet]
        public ActionResult<IEnumerable<Country>> GetCountries()
        {
            return Ok(Countries);
        }

        [HttpGet("{id}")]
        public ActionResult<Country> GetCountry(int id)
        {
            var country = Countries.FirstOrDefault(c => c.Id == id);

            if (country == null)
            {
                return NotFound();
            }

            return Ok(country);
        }

        [HttpPost]
        public ActionResult<Country> CreateCountry([FromBody] Country country)
        {
            if (country.Id <= 0)
            {
                return BadRequest(new { message = "Invalid country ID." });
            }

            if (Countries.Any(c => c.Id == country.Id))
            {
                return BadRequest(new { message = $"Country with ID {country.Id} already exists." });
            }

            Countries.Add(country);

            return CreatedAtAction(nameof(GetCountry), new { id = country.Id }, country);
        }

        [HttpPut("{id}")]
        public ActionResult<Country> UpdateCountry(int id, [FromBody] Country country)
        {
            var existingCountry = Countries.FirstOrDefault(c => c.Id == id);

            if (existingCountry == null)
            {
                return NotFound();
            }

            existingCountry.Name = country.Name;
            existingCountry.Code = country.Code;

            return Ok(existingCountry);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCountry(int id)
        {
            var country = Countries.FirstOrDefault(c => c.Id == id);

            if (country == null)
            {
                return NotFound(new { message = $"Country with ID {id} not found." });
            }

            Countries.Remove(country);

            return NoContent();
        }
    }
}
