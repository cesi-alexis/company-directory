using CompanyDirectory.Common;
using CompanyDirectory.Models.Entities;
using CompanyDirectory.Models.ViewsModels.Requests;
using CompanyDirectory.SDK.Factories;
using CompanyDirectory.SDK.Factory;
using CompanyDirectory.SDK.Services;

namespace CompanyDirectory.SDK.UtilsClients
{
    /// <summary>
    /// Utilitaire pour récupérer des champs spécifiques ou effectuer des actions sur les localisations.
    /// </summary>
    public class LocationUtilsClient
    {
        private readonly LocationServiceClient _locationServiceClient;
        private readonly WorkerServiceClient _workerServiceClient;

        /// <summary>
        /// Initialise une nouvelle instance de <see cref="LocationUtilsClient"/>.
        /// </summary>
        /// <param name="locationServiceClient">Client du service Location.</param>
        /// <param name="workerServiceClient">Client du service Worker.</param>
        public LocationUtilsClient(LocationServiceClient locationServiceClient, WorkerServiceClient workerServiceClient)
        {
            _locationServiceClient = locationServiceClient;
            _workerServiceClient = workerServiceClient;
        }

        /// <summary>
        /// Récupère le nom de la ville d’une localisation.
        /// </summary>
        /// <param name="locationId">Identifiant de la localisation.</param>
        /// <param name="fields">Champs spécifiques à récupérer (optionnel).</param>
        /// <returns>Nom de la ville ou <c>null</c> si non trouvé.</returns>
        public async Task<string?> GetCityAsync(int locationId, string? fields = "City")
        {
            try
            {
                var query = GetViewModelFactory.CreateGetViewModel(locationId, fields);
                var location = await _locationServiceClient.GetAsync(query);

                return location?.City;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erreur lors de la récupération de la ville pour la localisation ID {locationId}.", ex);
            }
        }

        /// <summary>
        /// Récupère tous les employés associés à une localisation.
        /// </summary>
        /// <param name="locationId">Identifiant de la localisation.</param>
        /// <param name="fields">Champs spécifiques à inclure dans la réponse (optionnel).</param>
        /// <returns>Liste des employés associés à la localisation.</returns>
        public async Task<IEnumerable<Worker>> GetWorkersAsync(int locationId, string? fields = "Id,FirstName,LastName,Email")
        {
            try
            {
                var query = GetViewModelFactory.CreateGetAllRequestViewModel(
                    searchTerm: null,
                    fields: fields,
                    pageNumber: 1,
                    pageSize: Constants.MAX_PAGES,
                    locationId: locationId
                );

                var workersResponse = await _workerServiceClient.GetAsync(query);
                return workersResponse.Items;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erreur lors de la récupération des employés pour la localisation ID {locationId}.", ex);
            }
        }

        /// <summary>
        /// Transfère tous les employés d’une localisation source vers une localisation cible.
        /// </summary>
        /// <param name="sourceLocationId">Identifiant de la localisation source.</param>
        /// <param name="targetLocationId">Identifiant de la localisation cible.</param>
        /// <returns>Le nombre d'employés transférés.</returns>
        public async Task<int> TransferAllWorkersToLocationAsync(int sourceLocationId, int targetLocationId)
        {
            try
            {
                var workers = await GetWorkersAsync(sourceLocationId);
                var workerIds = workers.Select(w => w.Id).ToList();

                if (!workerIds.Any())
                    return 0;

                var transferRequest = WorkerViewModelFactory.CreateWorkerTransferViewModel(workerIds, null, targetLocationId);

                await _workerServiceClient.TransferWorkersAsync(transferRequest);

                return workerIds.Count;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erreur lors du transfert des employés de la localisation {sourceLocationId} vers {targetLocationId}.", ex);
            }
        }

        /// <summary>
        /// Récupère le nombre d’employés associés à une localisation.
        /// </summary>
        /// <param name="locationId">Identifiant de la localisation.</param>
        /// <returns>Nombre d'employés associés.</returns>
        public async Task<int> GetWorkerCountAsync(int locationId)
        {
            try
            {
                var workers = await GetWorkersAsync(locationId, fields: "Id");
                return workers.Count();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erreur lors de la récupération du nombre d'employés pour la localisation ID {locationId}.", ex);
            }
        }

        /// <summary>
        /// Vérifie si une localisation existe.
        /// </summary>
        /// <param name="locationId">Identifiant de la localisation.</param>
        /// <returns><c>true</c> si la localisation existe, sinon <c>false</c>.</returns>
        public async Task<bool> LocationExistsAsync(int locationId)
        {
            try
            {
                return await _locationServiceClient.ExistsAsync(locationId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erreur lors de la vérification de l'existence de la localisation ID {locationId}.", ex);
            }
        }
    }
}