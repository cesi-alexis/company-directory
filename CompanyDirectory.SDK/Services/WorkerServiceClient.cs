using System.Net.Http.Json;
using CompanyDirectory.Common;
using CompanyDirectory.Models.Entities;
using CompanyDirectory.Models.ViewsModels.Requests;
using CompanyDirectory.Models.ViewsModels.Responses;
using CompanyDirectory.SDK.Interfaces;

namespace CompanyDirectory.SDK.Services
{
    /// <summary>
    /// Client API pour interagir avec les endpoints du contrôleur Worker.
    /// </summary>
    public class WorkerServiceClient : IWorkerServiceClient
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initialise une nouvelle instance de <see cref="WorkerServiceClient"/>.
        /// </summary>
        /// <param name="httpClient">Instance de <see cref="HttpClient"/> configurée avec l'URL de base.</param>
        public WorkerServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Crée un nouvel employé.
        /// </summary>
        public async Task<Worker> CreateAsync(WorkerUpsertRequestViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Worker", model);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ResponseViewModel<Worker>>();
            return result?.Data ?? throw new HttpRequestException("Failed to create worker.");
        }

        /// <summary>
        /// Récupère une liste paginée d'employés avec des filtres dynamiques.
        /// </summary>
        /// <param name="query">Modèle contenant les paramètres de recherche, pagination, et filtres.</param>
        /// <returns>Un modèle contenant les employés paginés et des métadonnées.</returns>
        public async Task<GetAllResponseViewModel<Worker>> GetAsync(GetAllRequestViewModel query)
        {
            if (query.PageNumber <= 0 || query.PageSize <= 0)
                throw new ArgumentException(Messages.PaginationInvalid);

            var requestUri = $"api/Worker?searchTerm={query.SearchTerm}&fields={query.Fields}&pageNumber={query.PageNumber}&pageSize={query.PageSize}&locationId={query.LocationId}&serviceId={query.ServiceId}";

            var response = await _httpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ResponseViewModel<GetAllResponseViewModel<Worker>>>();
            return result?.Data ?? throw new HttpRequestException("Failed to retrieve workers.");
        }

        /// <summary>
        /// Récupère un employé spécifique par son identifiant avec des filtres optionnels.
        /// </summary>
        /// <param name="query">Modèle contenant les paramètres de recherche.</param>
        /// <returns>L'employé correspondant ou null s'il n'est pas trouvé.</returns>
        public async Task<Worker?> GetAsync(GetRequestViewModel query)
        {
            if (query.Id <= 0)
                throw new ArgumentException(Messages.InvalidId, nameof(query.Id));

            var requestUri = $"api/Worker/{query.Id}?fields={query.Fields}";
            var response = await _httpClient.GetAsync(requestUri);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ResponseViewModel<Worker>>();
            return result?.Data ?? throw new HttpRequestException("Failed to retrieve worker by ID.");
        }

        /// <summary>
        /// Récupère un employé spécifique par son identifiant avec des filtres optionnels.
        /// </summary>
        /// <param name="query">Modèle contenant les paramètres de recherche.</param>
        /// <returns>L'employé correspondant aux critères ou null s'il n'est pas trouvé.</returns>
        public async Task<Worker?> GetAsync(WorkerGetRequestViewModel query)
        {
            if (query.Id <= 0)
                throw new ArgumentException(Messages.InvalidId, nameof(query.Id));

            // Construction de l'URI avec les paramètres de filtrage optionnels
            var queryString = $"fields={query.Fields}&locationId={query.LocationId}&serviceId={query.ServiceId}";
            var requestUri = $"api/Worker/Localized/{query.Id}?{queryString}";

            // Envoie de la requête HTTP GET
            var response = await _httpClient.GetAsync(requestUri);

            // Vérification du succès de la requête
            response.EnsureSuccessStatusCode();

            // Désérialisation de la réponse
            var result = await response.Content.ReadFromJsonAsync<ResponseViewModel<Worker>>();
            return result?.Data ?? throw new HttpRequestException("Failed to retrieve worker by ID.");
        }

        /// <summary>
        /// Vérifie si un employé existe par son ID.
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/Worker/exists-by-id/{id}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ResponseViewModel<ExistsResponseViewModel>>();
            return result?.Data?.Exists ?? false;
        }

        /// <summary>
        /// Vérifie si un employé existe par son email.
        /// </summary>
        public async Task<bool> ExistsAsync(string email)
        {
            var response = await _httpClient.GetAsync($"api/Worker/exists/{email}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ResponseViewModel<ExistsResponseViewModel>>();
            return result?.Data?.Exists ?? false;
        }

        /// <summary>
        /// Met à jour un employé existant.
        /// </summary>
        public async Task UpdateAsync(int id, WorkerUpsertRequestViewModel model)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Worker/{id}", model);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Supprime un employé existant.
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Worker/{id}");
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Transfère un ou plusieurs employés vers une nouvelle localisation et/ou un nouveau service.
        /// </summary>
        public async Task<TransferResponseViewModel> TransferWorkersAsync(WorkerTransferViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Worker/transfer", model);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ResponseViewModel<TransferResponseViewModel>>();
            return result?.Data ?? throw new HttpRequestException("Failed to transfer workers.");
        }
    }
}