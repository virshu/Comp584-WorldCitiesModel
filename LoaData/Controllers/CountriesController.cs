using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldCitiesApi.Dtos;
using WorldCitiesModel.Models;

namespace WorldCitiesApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CountriesController : ControllerBase
{
    private readonly WorldCitiesContext _context;

    public CountriesController(WorldCitiesContext context)
    {
        _context = context;
    }

    // GET: api/Countries
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Country>>> GetCountries() => await _context.Countries.OrderBy(c => c.Name).ToListAsync();

    // GET: api/Countries/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetCountry(int id)
    {
        var countryDTO = await _context.Countries
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Iso2,
                c.Iso3
            })
            .SingleOrDefaultAsync(c => c.Id == id);


        return countryDTO == null ? NotFound() : Ok(countryDTO);
    }

    [HttpGet("Population/{id:int}")]
    public async Task<ActionResult> GetCountryWithPopulation(int id)
    {
        CountryPopulation? countryDTO = await _context.Countries
            .Where(c => c.Id == id)
            .Select(c => new CountryPopulation
            {
                Id = c.Id,
                Name = c.Name,
                Population = c.Cities.Select(t => t.Population).Sum()
            }).SingleOrDefaultAsync();
        
        return countryDTO == null ? NotFound() : Ok(countryDTO);
    }

    [HttpGet("Cities/{id:int}")]
    public async Task<ActionResult<CityDto>> GetCities(int id)
    {
        List<CityDto> cityDto = await _context.Cities
            .Where(c => c.CountryId == id)
            .Select(c => new CityDto
            {
                Id = c.Id,
                Name = c.Name,
                Population = c.Population,
                CountryId = c.CountryId,
                CountryName = c.Country.Name
            }).ToListAsync();
        
        return Ok(cityDto);
    }

    // PUT: api/Countries/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutCountry(int id, Country country)
    {
        if (id != country.Id)
        {
            return BadRequest();
        }

        _context.Entry(country).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CountryExists(id))
            {
                return NotFound();
            }

            throw;
        }
        return NoContent();
    }

    // POST: api/Countries
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Country>> PostCountry(Country country)
    {
        _context.Countries.Add(country);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetCountry", new { id = country.Id }, country);
    }

    // DELETE: api/Countries/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCountry(int id)
    {
        Country? country = await _context.Countries.FindAsync(id);
        if (country == null)
        {
            return NotFound();
        }

        _context.Countries.Remove(country);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CountryExists(int id) => _context.Countries.Any(e => e.Id == id);
}