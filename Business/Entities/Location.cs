using System.ComponentModel.DataAnnotations;

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