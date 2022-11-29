using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WorldCitiesApi.Dtos;

public class CityDto
{
    public int Id { get; set; }
    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string Name { get; set; } = string.Empty;

    public int Population { get; set; }
    public int CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;

}