using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    // Modèle représentant une localisation de l'entreprise
    public class Location
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string City { get; set; } = null!;

        public ICollection<Worker> Workers { get; set; } = new List<Worker>();
    }
}