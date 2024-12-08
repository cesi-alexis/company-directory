using System.ComponentModel.DataAnnotations;

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
        public ICollection<Worker> Workers { get; set; } = new List<Worker>();
    }

}
