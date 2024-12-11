namespace CompanyDirectory.Models.ViewsModels.Requests
{
    /// <summary>
    /// Modèle de vue pour les requêtes spécifiques avec des champs optionnels.
    /// </summary>
    public class GetRequestViewModel
    {
        /// <summary>
        /// Identifiant unique utilisé pour effectuer des requêtes spécifiques.
        /// Exemple : obtenir un enregistrement précis en fonction de cet ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Champs spécifiques à inclure dans la réponse, séparés par des virgules.
        /// Exemple : "Name,Email".
        /// </summary>
        public string? Fields { get; set; }
    }
}