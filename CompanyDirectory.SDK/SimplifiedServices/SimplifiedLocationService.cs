using CompanyDirectory.Models.Entities;
using CompanyDirectory.SDK.Factories;
using CompanyDirectory.SDK.Factory;
using CompanyDirectory.SDK.Services;

namespace CompanyDirectory.SDK.SimplifiedServices
{
    /// <summary>
    /// Service simplifié pour gérer les localisations sans nécessiter de ViewModels explicites.
    /// </summary>
    public class SimplifiedLocationService
    {
        private readonly LocationServiceClient _locationServiceClient;

        /// <summary>
        /// Initialise une nouvelle instance de <see cref="SimplifiedLocationService"/>.
        /// </summary>
        /// <param name="locationServiceClient">Client du service Location.</param>
        public SimplifiedLocationService(LocationServiceClient locationServiceClient)
        {
            _locationServiceClient = locationServiceClient;
        }

        /// <summary>
        /// Crée une nouvelle localisation.
        /// </summary>
        /// <param name="city">Nom de la ville.</param>
        /// <returns>La localisation créée.</returns>
        public async Task<Location> CreateLocationAsync(string city)
        {
            var model = LocationViewModelFactory.CreateLocationUpsertViewModel(city);
            return await _locationServiceClient.CreateAsync(model);
        }

        /// <summary>
        /// Récupère une liste paginée de localisations.
        /// </summary>
        /// <param name="searchTerm">Terme de recherche.</param>
        /// <param name="fields">Champs spécifiques à inclure dans les réponses.</param>
        /// <param name="pageNumber">Numéro de la page à récupérer.</param>
        /// <param name="pageSize">Nombre d'éléments par page.</param>
        /// <returns>Une liste paginée de localisations.</returns>
        public async Task<IEnumerable<Location>> GetLocationsAsync(string? searchTerm, string? fields, int pageNumber, int pageSize)
        {
            var query = GetViewModelFactory.CreateGetAllRequestViewModel(searchTerm, fields, pageNumber, pageSize);
            var response = await _locationServiceClient.GetAsync(query);
            return response.Items;
        }

        /// <summary>
        /// Récupère une localisation spécifique par son identifiant.
        /// </summary>
        /// <param name="id">Identifiant unique de la localisation.</param>
        /// <param name="fields">Champs spécifiques à inclure dans la réponse (optionnel).</param>
        /// <returns>La localisation correspondante ou <c>null</c> si elle n'existe pas.</returns>
        public async Task<Location?> GetAsync(int id, string? fields = null)
        {
            var query = GetViewModelFactory.CreateGetViewModel(id, fields);
            return await _locationServiceClient.GetAsync(query);
        }

        /// <summary>
        /// Vérifie si une localisation existe par son nom.
        /// </summary>
        /// <param name="city">Nom de la ville.</param>
        /// <returns><c>true</c> si la localisation existe, sinon <c>false</c>.</returns>
        public async Task<bool> LocationExistsAsync(string city)
        {
            return await _locationServiceClient.ExistsAsync(city);
        }

        /// <summary>
        /// Vérifie si une localisation existe par son identifiant.
        /// </summary>
        /// <param name="id">Identifiant de la localisation.</param>
        /// <returns><c>true</c> si la localisation existe, sinon <c>false</c>.</returns>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _locationServiceClient.ExistsAsync(id);
        }

        /// <summary>
        /// Met à jour une localisation existante.
        /// </summary>
        /// <param name="id">Identifiant unique de la localisation.</param>
        /// <param name="city">Nom mis à jour de la ville.</param>
        public async Task UpdateAsync(int id, string city)
        {
            var model = LocationViewModelFactory.CreateLocationUpsertViewModel(city);
            await _locationServiceClient.UpdateAsync(id, model);
        }

        /// <summary>
        /// Supprime une localisation existante par son identifiant.
        /// </summary>
        /// <param name="id">Identifiant unique de la localisation.</param>
        public async Task DeleteLocationAsync(int id)
        {
            await _locationServiceClient.DeleteAsync(id);
        }
    }
}