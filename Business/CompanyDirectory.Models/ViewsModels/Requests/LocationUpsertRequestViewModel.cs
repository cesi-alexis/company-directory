using System.ComponentModel.DataAnnotations;

namespace CompanyDirectory.Models.ViewsModels.Requests
{
    /// <summary>
    /// Modèle utilisé pour les opérations de création et de mise à jour (Upsert) d'une localisation.
    /// </summary>
    public class LocationUpsertRequestViewModel
    {
        /// <summary>
        /// Nom de la ville associée à la localisation.
        /// </summary>
        [Required(ErrorMessage = "Le champ 'City' est obligatoire.")]
        [MaxLength(50, ErrorMessage = "Le champ 'City' ne peut pas dépasser 50 caractères.")]
        public string City { get; set; } = null!;
    }
}