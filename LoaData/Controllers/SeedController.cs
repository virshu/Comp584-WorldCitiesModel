using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldCitiesApi.Data;
using WorldCitiesModel.Models;
using Path = System.IO.Path;

namespace WorldCitiesApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Administrator")]
public class SeedController : ControllerBase
{
    private readonly WorldCitiesContext _context;
    private readonly string _pathName;
    private readonly IConfiguration _configuration;

    public SeedController(WorldCitiesContext context, IHostEnvironment environment, 
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _pathName = Path.Combine(environment.ContentRootPath, "Data/worldcities.csv");
    }

    [HttpGet]
    public async Task<IActionResult> ImportCities()
    {
        Dictionary<string, Country> Countries = await _context.Countries.AsNoTracking()
            .ToDictionaryAsync(c => c.Name);

        CsvConfiguration config = new()
        {
            HasHeaderRecord = true
        };
        int cityCount = 0;
        using (StreamReader reader = new(_pathName))
        using (CsvReader csv = new(reader, config))
        {
            IEnumerable<WorldCitiesCsv>? records = csv.GetRecords<WorldCitiesCsv>();
            foreach (WorldCitiesCsv record in records)
            {
                if (!Countries.ContainsKey(record.country))
                {
                    return NotFound(record);
                }

                City city = new()
                {
                    Name = record.city,
                    Lattitude = record.lat,
                    Longitude = record.lng,
                    CountryId = Countries[record.country].Id
                };
                _context.Cities.Add(city);
                cityCount++;
            }
            await _context.SaveChangesAsync();
        }

        return new JsonResult(cityCount);
    }

    [HttpGet("Countries")]
    public async Task<IActionResult> ImportCountries()
    {
        // create a lookup dictionary containing all the countries already existing 
        // into the Database (it will be empty on first run).
        Dictionary<string, Country> countriesByName = _context.Countries
            .AsNoTracking().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

        CsvConfiguration config = new()
        {
            HasHeaderRecord = true
        };
        using (StreamReader reader = new(_pathName))
        using (CsvReader csv = new(reader, config))
        {
            IEnumerable<WorldCitiesCsv>? records = csv.GetRecords<WorldCitiesCsv>();
            foreach (WorldCitiesCsv record in records)
            {
                if (countriesByName.ContainsKey(record.country))
                {
                    continue;
                }

                Country country = new()
                {
                    Name = record.country,
                    Iso2 = record.iso2,
                    Iso3 = record.iso3
                };
                await _context.Countries.AddAsync(country);
                countriesByName.Add(record.country, country);
            }

            await _context.SaveChangesAsync();
        }
        return new JsonResult(countriesByName.Count);
    }

}
