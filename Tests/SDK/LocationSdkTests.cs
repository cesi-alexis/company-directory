using CompanyDirectory.Models.Entities;
using CompanyDirectory.Models.ViewsModels.Requests;
using CompanyDirectory.SDK.Interfaces;
using CompanyDirectory.SDK.Services;

namespace CompanyDirectory.Tests.SDK
{
    /// <summary>
    /// Tests pour le service SDK de gestion des localisations.
    /// </summary>
    public class LocationSdkTests : BaseSdkTests<IServiceClient<Location, LocationUpsertRequestViewModel>, Location, LocationUpsertRequestViewModel>
    {
        protected override IServiceClient<Location, LocationUpsertRequestViewModel> ServiceClient { get; }

        public LocationSdkTests()
        {
            ServiceClient = new LocationServiceClient(new HttpClient { BaseAddress = new Uri("http://localhost:7055") });
        }

        /// <summary>
        /// Crée un ViewModel valide pour tester la création ou la mise à jour.
        /// </summary>
        /// <returns>Un ViewModel valide.</returns>
        protected override LocationUpsertRequestViewModel CreateValidViewModel()
        {
            return new LocationUpsertRequestViewModel
            {
                City = $"City-{Guid.NewGuid():N}"
            };
        }

        /// <summary>
        /// Crée un ViewModel invalide pour tester les échecs de validation.
        /// </summary>
        /// <returns>Un ViewModel invalide.</returns>
        protected override LocationUpsertRequestViewModel CreateInvalidViewModel()
        {
            return new LocationUpsertRequestViewModel
            {
                City = string.Empty // Ville vide (invalide)
            };
        }

        /// <summary>
        /// Extrait l'identifiant unique d'une localisation.
        /// </summary>
        /// <param name="location">La localisation dont extraire l'identifiant.</param>
        /// <returns>L'identifiant de la localisation.</returns>
        protected override int GetId(Location location)
        {
            return location.Id;
        }

        protected override string GetUniqueIdentifier(Location location)
        {
            return location.City;
        }
    }
}
