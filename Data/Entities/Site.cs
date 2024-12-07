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
        [Key] // Clé primaire
        public int Id { get; set; }

        [Required] // Champ obligatoire
        [MaxLength(50)] // Limite de caractères
        public string City { get; set; } = null!; // Nom de la ville

        // Relation avec les employés
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}