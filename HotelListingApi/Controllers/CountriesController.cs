using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListingApi.Data;
using HotelListingApi.DTOs.Country;
using HotelListingApi.Contracts;

namespace HotelListingApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CountriesController : ControllerBase
{
    private readonly ICountriesServices countriesServices;

    public CountriesController(ICountriesServices countriesServices)
    {
        this.countriesServices = countriesServices;
    }
    // GET: api/Countries
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetCountriesDto>>> GetCountries()
    {
        var countries = await countriesServices.GetCountriesAsync();
        return Ok(countries);
    }

    // GET: api/Countries/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GetCountryDto>> GetCountry(int id)
    {
        var country = await countriesServices.GetCountryAsync(id);
        if (country == null)
        {
            return NotFound();
        }

        return country;
    }

    // PUT: api/Countries/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCountry(int id, UpdateCountryDto countryDto)
    {
        if (id != countryDto.CountryId)
        {
            return BadRequest();
        }

        var country = await countriesServices.GetCountryAsync(id);
        
        if (country == null)
        {
            return NotFound();
        }

        try
        {
            await countriesServices.UpdateCountryAsync(id, countryDto);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await countriesServices.CountryExistsAsync(id))
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

    // POST: api/Countries
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Country>> PostCountry(CreateCountryDto countryDto)
    {
        var resultDto = await countriesServices.CreateCountryAsync(countryDto);
        return CreatedAtAction(nameof(GetCountry), new { id = resultDto.CountryId }, resultDto);
    }

    // DELETE: api/Countries/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCountry(int id)
    {
        var country = await countriesServices.GetCountryAsync(id);
        if (country == null)
        {
            return NotFound();
        }

        await countriesServices.DeleteCountryAsync(id);
        return NoContent();
    }
}
