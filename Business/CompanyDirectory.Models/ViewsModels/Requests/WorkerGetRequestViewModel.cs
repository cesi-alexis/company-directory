namespace CompanyDirectory.Models.ViewsModels.Requests
{
    /// <summary>
    /// Modèle de vue pour les requêtes spécifiques avec des champs optionnels.
    /// </summary>
    public class WorkerGetRequestViewModel : GetRequestViewModel
    {
        /// <summary>
        /// Identifiant optionnel pour filtrer par localisation.
        /// </summary>
        public int? LocationId { get; set; }

        /// <summary>
        /// Identifiant optionnel pour filtrer par service.
        /// </summary>
        public int? ServiceId { get; set; }
    }
}