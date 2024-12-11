using CompanyDirectory.Models.ViewsModels.Requests;

namespace CompanyDirectory.SDK.Factories
{
    /// <summary>
    /// Factory pour créer des ViewModels liés aux localisations.
    /// </summary>
    public static class LocationViewModelFactory
    {
        /// <summary>
        /// Crée un ViewModel pour ajouter ou mettre à jour une localisation.
        /// </summary>
        /// <param name="city">Nom de la ville.</param>
        /// <returns>Un ViewModel prêt à être utilisé.</returns>
        public static LocationUpsertRequestViewModel CreateLocationUpsertViewModel(string city)
        {
            return new LocationUpsertRequestViewModel
            {
                City = city
            };
        }
    }
}