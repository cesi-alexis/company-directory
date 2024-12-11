using CompanyDirectory.Models.ViewsModels.Requests;
using CompanyDirectory.Models.ViewsModels.Responses;

namespace CompanyDirectory.SDK.Factories
{
    /// <summary>
    /// Factory pour créer des ViewModels liés aux services.
    /// </summary>
    public static class ServiceViewModelFactory
    {
        /// <summary>
        /// Crée un ViewModel pour ajouter ou mettre à jour un service.
        /// </summary>
        /// <param name="name">Nom du service.</param>
        /// <returns>Un ViewModel prêt à être utilisé pour un service.</returns>
        public static ServiceUpsertRequestViewModel CreateServiceUpsertViewModel(string name)
        {
            return new ServiceUpsertRequestViewModel
            {
                Name = name
            };
        }
    }
}