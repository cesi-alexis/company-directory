using CompanyDirectory.Common;

namespace CompanyDirectory.Models.ViewsModels.Requests
{
    /// <summary>
    /// Modèle de vue pour les requêtes paginées avec des champs spécifiques à inclure dans les réponses.
    /// </summary>
    public class GetAllRequestViewModel
    {
        /// <summary>
        /// Terme de recherche utilisé pour filtrer les résultats.
        /// Exemple : "John" pour rechercher des employés dont le nom ou l'email contient "John".
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Champs spécifiques à inclure dans la réponse, séparés par des virgules.
        /// Exemple : "Name,Email".
        /// </summary>
        public string? Fields { get; set; }

        /// <summary>
        /// Numéro de la page à récupérer.
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Taille de la page.
        /// </summary>
        public int PageSize { get; set; } = Constants.MAX_PAGES;

        /// <summary>
        /// Filtre facultatif pour la localisation.
        /// </summary>
        public int? LocationId { get; set; }

        /// <summary>
        /// Filtre facultatif pour le service.
        /// </summary>
        public int? ServiceId { get; set; }
    }
}