using System.ComponentModel.DataAnnotations;

namespace CompanyDirectory.Models.ViewsModels.Requests
{
    /// <summary>
    /// Modèle utilisé pour les opérations de création et de mise à jour (Upsert) d'un service.
    /// </summary>
    public class ServiceUpsertRequestViewModel
    {
        /// <summary>
        /// Nom du service.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;
    }
}