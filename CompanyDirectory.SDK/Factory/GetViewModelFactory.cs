using CompanyDirectory.Common;
using CompanyDirectory.Models.ViewsModels.Requests;

namespace CompanyDirectory.SDK.Factory
{
    public class GetViewModelFactory
    {
        /// <summary>
        /// Crée un ViewModel pour une requête de recherche de services.
        /// </summary>
        /// <param name="searchTerm">Terme de recherche.</param>
        /// <param name="fields">Champs à inclure.</param>
        /// <param name="pageNumber">Numéro de la page.</param>
        /// <param name="pageSize">Taille de la page.</param>
        /// <returns>Un ViewModel prêt à être utilisé pour rechercher des services.</returns>
        public static GetAllRequestViewModel CreateGetAllRequestViewModel(string searchTerm, string fields, int pageNumber, int pageSize, int? locationId = null, int? serviceId = null)
        {
            return new GetAllRequestViewModel
            {
                SearchTerm = searchTerm,
                Fields = fields,
                PageNumber = pageNumber,
                PageSize = pageSize,
                LocationId = locationId,
                ServiceId = serviceId
            };
        }

        /// <summary>
        /// Crée un ViewModel pour une requête de récupération par identifiant.
        /// </summary>
        /// <param name="id">Identifiant unique de l'entité à récupérer.</param>
        /// <param name="fields">Champs à inclure dans la réponse (optionnel).</param>
        /// <returns>Un ViewModel prêt à être utilisé pour récupérer une entité par son ID.</returns>
        public static GetRequestViewModel CreateGetViewModel(int id, string? fields = null, int? locationId = null, int? serviceId = null)
        {
            if (id <= 0)
                throw new ArgumentException(Messages.InvalidId, nameof(id));

            return new WorkerGetRequestViewModel
            {
                Id = id,
                Fields = fields,
                LocationId = locationId,
                ServiceId = serviceId
            };
        }
    }
}