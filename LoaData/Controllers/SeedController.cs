using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using WorldCitiesApi.Data;
using WorldCitiesModel.Models;
using Path = System.IO.Path;

namespace WorldCitiesApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SeedController : ControllerBase
{
    private readonly WorldCitiesContext _context;
    private readonly string _pathName;

    private readonly UserManager<WorldCitiesUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    public SeedController(WorldCitiesContext context, IHostEnvironment environment, 
        UserManager<WorldCitiesUser> userManager, RoleManager<IdentityRole> roleManager, 
        IConfiguration configuration)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _pathName = Path.Combine(environment.ContentRootPath, "Data/worldcities.csv");
    }

    [HttpGet("Cities")]
    public async Task<IActionResult> ImportCities()
    {
        Dictionary<string, Country> Countries = await _context.Countries.AsNoTracking()
            .ToDictionaryAsync(c => c.Name);

        CsvConfiguration config = new(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            HeaderValidated = null
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
                    Console.WriteLine($"Not found country for {record.city}");
                    return NotFound(record);
                }

                if (!record.population.HasValue || string.IsNullOrEmpty(record.city_ascii))
                {
                    Console.WriteLine($"Skipping {record.city}");
                    continue;
                }
                City city = new()
                {
                    Name = record.city,
                    NameAscii = record.city_ascii,
                    Lattitude = record.lat,
                    Longitude = record.lng,
                    Population = (int) record.population.Value,
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

        CsvConfiguration config = new(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            HeaderValidated = null
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

    [HttpGet("Users")]
    public async Task<IActionResult> CreateUsers()
    {
        const string roleUser = "RegisteredUser";
        const string roleAdmin = "Administrator";

        if (await _roleManager.FindByNameAsync(roleUser) is null)
        {
            await _roleManager.CreateAsync(new IdentityRole(roleUser));
        }
        if (await _roleManager.FindByNameAsync(roleAdmin) is null)
        {
            await _roleManager.CreateAsync(new IdentityRole(roleAdmin));
        }

        List<WorldCitiesUser> addedUserList = new();
        (string name, string email) = ("admin", "admin@email.com");

        if (await _userManager.FindByNameAsync(name) is null)
        {
            WorldCitiesUser userAdmin = new()
            {
                UserName = name,
                Email = email,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            await _userManager.CreateAsync(userAdmin, _configuration["DefaultPasswords:Administrator"]!);
            await _userManager.AddToRolesAsync(userAdmin, new[] { roleUser, roleAdmin });
            userAdmin.EmailConfirmed = true;
            userAdmin.LockoutEnabled = false;
            addedUserList.Add(userAdmin);
        }

        (string name, string email) registered = ("user", "user@email.com");

        if (await _userManager.FindByNameAsync(registered.name) is null)
        {
            WorldCitiesUser user = new()
            {
                UserName = registered.name,
                Email = registered.email,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            await _userManager.CreateAsync(user, _configuration["DefaultPasswords:RegisteredUser"]!);
            await _userManager.AddToRoleAsync(user, roleUser);
            user.EmailConfirmed = true;
            user.LockoutEnabled = false;
            addedUserList.Add(user);
        }

        if (addedUserList.Count > 0)
        {
            await _context.SaveChangesAsync();
        }

        return new JsonResult(new
        {
            addedUserList.Count,
            Users = addedUserList
        });

    }
}
