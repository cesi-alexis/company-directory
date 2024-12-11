using System.Net.Http.Json;
using CompanyDirectory.Common;
using CompanyDirectory.Models.Entities;
using CompanyDirectory.Models.ViewsModels.Requests;
using CompanyDirectory.Models.ViewsModels.Responses;
using CompanyDirectory.SDK.Interfaces;

namespace CompanyDirectory.SDK.Services
{
    /// <summary>
    /// Client API pour interagir avec les endpoints du contrôleur Location.
    /// </summary>
    public class LocationServiceClient : IServiceClient<Location, LocationUpsertRequestViewModel>
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initialise une nouvelle instance de <see cref="LocationServiceClient"/>.
        /// </summary>
        /// <param name="httpClient">Instance de <see cref="HttpClient"/> configurée avec l'URL de base.</param>
        public LocationServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Crée une nouvelle localisation.
        /// </summary>
        public async Task<Location> CreateAsync(LocationUpsertRequestViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Location", model);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ResponseViewModel<Location>>();
            return result?.Data ?? throw new HttpRequestException("Failed to create location.");
        }

        /// <summary>
        /// Récupère une liste paginée de localisations.
        /// </summary>
        public async Task<GetAllResponseViewModel<Location>> GetAsync(GetAllRequestViewModel query)
        {
            var response = await _httpClient.GetAsync($"api/Location?searchTerm={query.SearchTerm}&fields={query.Fields}&pageNumber={query.PageNumber}&pageSize={query.PageSize}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ResponseViewModel<GetAllResponseViewModel<Location>>>();
            return result?.Data ?? throw new HttpRequestException("Failed to retrieve locations.");
        }

        /// <summary>
        /// Récupère une localisation spécifique par son identifiant.
        /// </summary>
        public async Task<Location?> GetAsync(GetRequestViewModel query)
        {
            if (query.Id <= 0)
                throw new ArgumentException(Messages.InvalidId, nameof(query.Id));

            var response = await _httpClient.GetAsync($"api/Location/{query.Id}?fields={query.Fields}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ResponseViewModel<Location>>();
            return result?.Data ?? throw new HttpRequestException("Failed to retrieve location by ID.");
        }

        /// <summary>
        /// Vérifie si une localisation existe par son ID.
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/Location/exists-by-id/{id}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ResponseViewModel<ExistsResponseViewModel>>();
            return result?.Data?.Exists ?? false;
        }

        /// <summary>
        /// Vérifie si une localisation existe par son nom.
        /// </summary>
        public async Task<bool> ExistsAsync(string city)
        {
            var response = await _httpClient.GetAsync($"api/Location/exists/{city}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ResponseViewModel<ExistsResponseViewModel>>();
            return result?.Data?.Exists ?? false;
        }

        /// <summary>
        /// Met à jour une localisation existante.
        /// </summary>
        public async Task UpdateAsync(int id, LocationUpsertRequestViewModel model)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Location/{id}", model);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Supprime une localisation existante.
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Location/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}