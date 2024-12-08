using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    // Modèle représentant un site de l'entreprise
    public class Site
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string City { get; set; } = null!;

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}