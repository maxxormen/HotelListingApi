using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListingApi.Data;
using HotelListingApi.DTOs.Country;
using HotelListingApi.Contracts;
using HotelListingApi.Results;
using Microsoft.AspNetCore.Authorization;

namespace HotelListingApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CountriesController : BaseApiController
{
    private readonly ICountriesServices countriesServices;

    public CountriesController(ICountriesServices countriesServices)
    {
        this.countriesServices = countriesServices;
    }
    // GET: api/Countries
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<GetCountriesDto>>> GetCountries()
    {
        var result = await countriesServices.GetCountriesAsync();
        return ToActionResult(result);
    }

    // GET: api/Countries/5
    [HttpGet("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<GetCountryDto?>> GetCountry(int id)
    {
        var result = await countriesServices.GetCountryAsync(id);
        return ToActionResult(result);
    }

    // PUT: api/Countries/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult> PutCountry(int id, UpdateCountryDto countryDto)
    {
        var result = await countriesServices.UpdateCountryAsync(id, countryDto);
        return ToActionResult(result);
    }

    // POST: api/Countries
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<Country>> PostCountry(CreateCountryDto countryDto)
    {
        var result = await countriesServices.CreateCountryAsync(countryDto);
        if (!result.IsSuccess) return MapErrorsToResponse(result.Errors);
        return CreatedAtAction(nameof(GetCountry), new { id = result.Value!.CountryId }, result.Value);
    }

    // DELETE: api/Countries/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteCountry(int id)
    {
        var result = await countriesServices.DeleteCountryAsync(id);
        return ToActionResult(result);
    }
}
