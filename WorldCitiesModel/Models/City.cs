using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WorldCitiesModel.Models;

public sealed class City
{
    [Key]
    public int Id { get; set; }
    [Required]
    [StringLength(50)]
    [Unicode(false)]
    public string Name { get; set; } = string.Empty;
    [StringLength(50)]
    public string NameAscii { get; set; } = string.Empty;
    [Column(TypeName = "decimal(18, 5)")]
    public decimal Lattitude { get; set; }
    [Column(TypeName = "decimal(18, 5)")]
    public decimal Longitude { get; set; }

    public int Population { get; set; }
    public int CountryId { get; set; }

    [ForeignKey("CountryId")]
    [InverseProperty("Cities")]
    public Country Country { get; set; } = new();
}