using CompanyDirectory.Common;
using System.Text.Json.Serialization;

namespace CompanyDirectory.Models.ViewsModels.Responses
{
    /// <summary>
    /// Modèle représentant une réponse paginée pour les requêtes GET.
    /// </summary>
    /// <typeparam name="T">Le type des éléments contenus dans la réponse.</typeparam>
    public class GetAllResponseViewModel<T>
    {
        /// <summary>
        /// Le numéro de la page actuelle.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Le nombre d'éléments affichés par page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Le nombre total d'éléments correspondant à la requête.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Le nombre total de pages disponibles.
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>
        /// La liste des éléments correspondant à la page actuelle.
        /// </summary>
        [JsonPropertyName("items")]
        public IEnumerable<T> Items { get; set; } = Array.Empty<T>();
    }
}