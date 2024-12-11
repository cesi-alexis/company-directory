using CompanyDirectory.Common;
using CompanyDirectory.Models.Entities;
using CompanyDirectory.Models.ViewsModels.Requests;
using CompanyDirectory.SDK.Factories;
using CompanyDirectory.SDK.Factory;
using CompanyDirectory.SDK.Interfaces;
using CompanyDirectory.SDK.Services;

namespace CompanyDirectory.SDK.UtilsClients
{
    /// <summary>
    /// Utilitaire pour récupérer des champs spécifiques ou effectuer des actions sur les services.
    /// </summary>
    public class ServiceUtilsClient
    {
        private readonly ServiceServiceClient _serviceServiceClient;
        private readonly WorkerServiceClient _workerServiceClient;

        /// <summary>
        /// Initialise une nouvelle instance de <see cref="ServiceUtilsClient"/>.
        /// </summary>
        /// <param name="serviceServiceClient">Client du service Service.</param>
        public ServiceUtilsClient(ServiceServiceClient serviceServiceClient)
        {
            _serviceServiceClient = serviceServiceClient;
        }

        /// <summary>
        /// Récupère le nom d'un service.
        /// </summary>
        /// <param name="id">Identifiant du service.</param>
        /// <returns>Nom du service ou <c>null</c> si non trouvé.</returns>
        public async Task<string?> GetNameAsync(int id)
        {
            var query = GetViewModelFactory.CreateGetViewModel(id, "Name");
            var service = await _serviceServiceClient.GetAsync(query);
            return service?.Name;
        }

        /// <summary>
        /// Récupère une liste paginée de services.
        /// </summary>
        /// <param name="searchTerm">Terme de recherche.</param>
        /// <param name="fields">Champs spécifiques à inclure dans la réponse.</param>
        /// <param name="pageNumber">Numéro de la page.</param>
        /// <param name="pageSize">Taille de la page.</param>
        /// <returns>Liste paginée des services correspondant aux critères.</returns>
        public async Task<IEnumerable<Service>> GetServicesAsync(string? searchTerm = null, string? fields = "Id,Name", int pageNumber = 1, int pageSize = 10)
        {
            var query = GetViewModelFactory.CreateGetAllRequestViewModel(searchTerm, fields, pageNumber, pageSize);
            var response = await _serviceServiceClient.GetAsync(query);
            return response.Items;
        }

        /// <summary>
        /// Vérifie si un service existe par son identifiant.
        /// </summary>
        /// <param name="id">Identifiant du service.</param>
        /// <returns><c>true</c> si le service existe, sinon <c>false</c>.</returns>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _serviceServiceClient.ExistsAsync(id);
        }

        /// <summary>
        /// Vérifie si un service existe par son nom.
        /// </summary>
        /// <param name="name">Nom du service.</param>
        /// <returns><c>true</c> si le service existe, sinon <c>false</c>.</returns>
        public async Task<bool> ExistsAsync(string name)
        {
            return await _serviceServiceClient.ExistsAsync(name);
        }

        /// <summary>
        /// Récupère tous les employés associés à un service donné.
        /// </summary>
        /// <param name="serviceId">Identifiant du service.</param>
        /// <param name="fields">Champs spécifiques à inclure dans la réponse (optionnel).</param>
        /// <returns>Liste des employés associés au service.</returns>
        public async Task<IEnumerable<Worker>> GetWorkersByServiceAsync(int serviceId, string? fields = "Id,FirstName,LastName,Email")
        {
            // Prépare la requête pour récupérer les employés par service
            var query = GetViewModelFactory.CreateGetAllRequestViewModel(
                searchTerm: null,
                fields: fields,
                pageNumber: 1,
                pageSize: Constants.MAX_PAGES,
                locationId: null,
                serviceId: serviceId
            );

            // Appelle le client pour récupérer les employés
            var workersResponse = await _workerServiceClient.GetAsync(query);

            // Effectue une conversion explicite en IEnumerable<Worker> si nécessaire
            return workersResponse.Items.Cast<Worker>();
        }

        /// <summary>
        /// Met à jour le nom d'un service.
        /// </summary>
        /// <param name="id">Identifiant du service.</param>
        /// <param name="name">Nouveau nom du service.</param>
        public async Task UpdateServiceNameAsync(int id, string name)
        {
            var model = ServiceViewModelFactory.CreateServiceUpsertViewModel(name);
            await _serviceServiceClient.UpdateAsync(id, model);
        }

        /// <summary>
        /// Supprime un service par son identifiant.
        /// </summary>
        /// <param name="id">Identifiant du service.</param>
        public async Task DeleteAsync(int id)
        {
            await _serviceServiceClient.DeleteAsync(id);
        }
    }
}