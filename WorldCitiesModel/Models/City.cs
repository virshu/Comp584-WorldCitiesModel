using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WorldCitiesModel.Models
{
    public partial class City
    {

        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        [Unicode(false)]
        public string Name { get; set; }
        [StringLength(50)]
        public string NameAscii { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal Lattitude { get; set; }
        [Column(TypeName = "decimal(18, 5)")]
        public decimal Longitude { get; set; }

        public int Population { get; set; }
        public int CountryId { get; set; }

        [ForeignKey("CountryId")]
        [InverseProperty("Cities")]
        public virtual Country Country { get; set; }
    }
}
