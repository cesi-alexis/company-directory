using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    // Modèle représentant un service de l'entreprise
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        // Relation avec les employés
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }

}
