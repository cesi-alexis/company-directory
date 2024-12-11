using CompanyDirectory.Models.Entities;
using CompanyDirectory.SDK.Factories;
using CompanyDirectory.SDK.Factory;
using CompanyDirectory.SDK.Services;

namespace CompanyDirectory.SDK.SimplifiedServices
{
    /// <summary>
    /// Service simplifié pour gérer les services sans nécessiter de ViewModels explicites.
    /// </summary>
    public class SimplifiedServiceService
    {
        private readonly ServiceServiceClient _serviceServiceClient;

        /// <summary>
        /// Initialise une nouvelle instance de <see cref="SimplifiedServiceService"/>.
        /// </summary>
        /// <param name="serviceServiceClient">Client du service Service.</param>
        public SimplifiedServiceService(ServiceServiceClient serviceServiceClient)
        {
            _serviceServiceClient = serviceServiceClient;
        }

        /// <summary>
        /// Crée un nouveau service.
        /// </summary>
        /// <param name="name">Nom du service.</param>
        /// <returns>Le service créé.</returns>
        public async Task<Service> CreateAsync(string name)
        {
            var model = ServiceViewModelFactory.CreateServiceUpsertViewModel(name);
            return await _serviceServiceClient.CreateAsync(model);
        }

        /// <summary>
        /// Récupère une liste paginée de services.
        /// </summary>
        /// <param name="searchTerm">Terme de recherche.</param>
        /// <param name="fields">Champs spécifiques à inclure dans la réponse.</param>
        /// <param name="pageNumber">Numéro de la page à récupérer.</param>
        /// <param name="pageSize">Nombre d'éléments par page.</param>
        /// <returns>Une liste paginée de services.</returns>
        public async Task<IEnumerable<Service>> GetAsync(string? searchTerm, string? fields, int pageNumber, int pageSize)
        {
            var query = GetViewModelFactory.CreateGetAllRequestViewModel(searchTerm, fields, pageNumber, pageSize);
            var response = await _serviceServiceClient.GetAsync(query);
            return response.Items;
        }

        /// <summary>
        /// Récupère un service spécifique par son identifiant.
        /// </summary>
        /// <param name="id">Identifiant du service.</param>
        /// <param name="fields">Champs spécifiques à inclure dans la réponse (optionnel).</param>
        /// <returns>Le service correspondant ou <c>null</c> s'il n'existe pas.</returns>
        public async Task<Service?> GetAsync(int id, string? fields = null)
        {
            var query = GetViewModelFactory.CreateGetViewModel(id, fields);
            return await _serviceServiceClient.GetAsync(query);
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
        /// Met à jour un service existant.
        /// </summary>
        /// <param name="id">Identifiant du service.</param>
        /// <param name="name">Nom mis à jour du service.</param>
        public async Task UpdateAsync(int id, string name)
        {
            var model = ServiceViewModelFactory.CreateServiceUpsertViewModel(name);
            await _serviceServiceClient.UpdateAsync(id, model);
        }

        /// <summary>
        /// Supprime un service existant par son identifiant.
        /// </summary>
        /// <param name="id">Identifiant unique du service.</param>
        public async Task DeleteAsync(int id)
        {
            await _serviceServiceClient.DeleteAsync(id);
        }
    }
}