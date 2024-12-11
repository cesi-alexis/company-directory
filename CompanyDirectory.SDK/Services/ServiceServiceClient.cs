using System.Net.Http.Json;
using CompanyDirectory.Common;
using CompanyDirectory.Models.Entities;
using CompanyDirectory.Models.ViewsModels.Requests;
using CompanyDirectory.Models.ViewsModels.Responses;
using CompanyDirectory.SDK.Interfaces;

namespace CompanyDirectory.SDK.Services
{
    /// <summary>
    /// Client API pour interagir avec les endpoints du contrôleur Service.
    /// </summary>
    public class ServiceServiceClient : IServiceClient<Service, ServiceUpsertRequestViewModel>
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initialise une nouvelle instance de <see cref="ServiceServiceClient"/>.
        /// </summary>
        /// <param name="httpClient">Instance de <see cref="HttpClient"/> configurée avec l'URL de base.</param>
        public ServiceServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Crée un nouveau service.
        /// </summary>
        public async Task<Service> CreateAsync(ServiceUpsertRequestViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Service", model);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ResponseViewModel<Service>>();
            return result?.Data ?? throw new HttpRequestException("Failed to create service.");
        }

        /// <summary>
        /// Récupère une liste paginée de services.
        /// </summary>
        public async Task<GetAllResponseViewModel<Service>> GetAsync(GetAllRequestViewModel query)
        {
            var response = await _httpClient.GetAsync($"api/Service?searchTerm={query.SearchTerm}&fields={query.Fields}&pageNumber={query.PageNumber}&pageSize={query.PageSize}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ResponseViewModel<GetAllResponseViewModel<Service>>>();
            return result?.Data ?? throw new HttpRequestException("Failed to retrieve services.");
        }

        /// <summary>
        /// Récupère un service spécifique par son identifiant.
        /// </summary>
        public async Task<Service?> GetAsync(GetRequestViewModel query)
        {
            if (query.Id <= 0)
                throw new ArgumentException(Messages.InvalidId, nameof(query.Id));

            var response = await _httpClient.GetAsync($"api/Service/{query.Id}?fields={query.Fields}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ResponseViewModel<Service>>();
            return result?.Data ?? throw new HttpRequestException("Failed to retrieve service by ID.");
        }

        /// <summary>
        /// Vérifie si un service existe par son ID.
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/Service/exists-by-id/{id}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ResponseViewModel<ExistsResponseViewModel>>();
            return result?.Data?.Exists ?? false;
        }

        /// <summary>
        /// Vérifie si un service existe par son nom.
        /// </summary>
        public async Task<bool> ExistsAsync(string name)
        {
            var response = await _httpClient.GetAsync($"api/Service/exists/{name}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ResponseViewModel<ExistsResponseViewModel>>();
            return result?.Data?.Exists ?? false;
        }

        /// <summary>
        /// Met à jour un service existant.
        /// </summary>
        public async Task UpdateAsync(int id, ServiceUpsertRequestViewModel model)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Service/{id}", model);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Supprime un service existant.
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Service/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}