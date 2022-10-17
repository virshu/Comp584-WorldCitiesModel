using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WorldCitiesModel.Models
{
    public partial class Country
    {
        public Country()
        {
            Cities = new HashSet<City>();
        }

        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        [Unicode(false)]
        public string Name { get; set; }
        [Required]
        [Column("iso2")]
        [StringLength(2)]
        public string Iso2 { get; set; }
        [Required]
        [Column("iso3")]
        [StringLength(3)]
        public string Iso3 { get; set; }

        [InverseProperty("Country")]
        public virtual ICollection<City> Cities { get; set; }
    }
}
