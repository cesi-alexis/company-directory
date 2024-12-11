using CompanyDirectory.Common;
using CompanyDirectory.Models.Entities;
using CompanyDirectory.Models.ViewsModels.Requests;
using CompanyDirectory.SDK.Factory;
using CompanyDirectory.SDK.Services;

namespace CompanyDirectory.SDK.UtilsClients
{
    /// <summary>
    /// Utilitaire pour récupérer des champs spécifiques ou effectuer des actions sur les employés.
    /// </summary>
    public class WorkerUtilsClient
    {
        private readonly WorkerServiceClient _workerServiceClient;

        /// <summary>
        /// Initialise une nouvelle instance de <see cref="WorkerUtilsClient"/>.
        /// </summary>
        /// <param name="workerServiceClient">Client du service Worker.</param>
        public WorkerUtilsClient(WorkerServiceClient workerServiceClient)
        {
            _workerServiceClient = workerServiceClient;
        }

        /// <summary>
        /// Récupère le prénom d’un employé.
        /// </summary>
        public async Task<string?> GetFirstNameAsync(int workerId)
        {
            var worker = await _workerServiceClient.GetAsync(GetViewModelFactory.CreateGetViewModel(workerId, "FirstName"));
            return worker?.FirstName;
        }

        /// <summary>
        /// Récupère le nom de famille d’un employé.
        /// </summary>
        public async Task<string?> GetLastNameAsync(int workerId)
        {
            var worker = await _workerServiceClient.GetAsync(GetViewModelFactory.CreateGetViewModel(workerId, "LastName"));
            return worker?.LastName;
        }

        /// <summary>
        /// Récupère l’email d’un employé.
        /// </summary>
        public async Task<string?> GetEmailAsync(int workerId)
        {
            var worker = await _workerServiceClient.GetAsync(GetViewModelFactory.CreateGetViewModel(workerId, "Email"));
            return worker?.Email;
        }

        /// <summary>
        /// Récupère le téléphone fixe d’un employé.
        /// </summary>
        public async Task<string?> GetPhoneFixedAsync(int workerId)
        {
            var worker = await _workerServiceClient.GetAsync(GetViewModelFactory.CreateGetViewModel(workerId, "PhoneFixed"));
            return worker?.PhoneFixed;
        }

        /// <summary>
        /// Récupère le téléphone mobile d’un employé.
        /// </summary>
        public async Task<string?> GetPhoneMobileAsync(int workerId)
        {
            var worker = await _workerServiceClient.GetAsync(GetViewModelFactory.CreateGetViewModel(workerId, "PhoneMobile"));
            return worker?.PhoneMobile;
        }

        /// <summary>
        /// Récupère les employés travaillant à une localisation spécifique dans un service donné.
        /// </summary>
        /// <param name="locationId">Identifiant de la localisation.</param>
        /// <param name="serviceId">Identifiant du service.</param>
        /// <param name="fields">Champs spécifiques à inclure dans la réponse (optionnel).</param>
        /// <returns>Liste des employés correspondant aux critères.</returns>
        public async Task<IEnumerable<Worker>> GetWorkersByLocationAndServiceAsync(int? locationId = null, int? serviceId = null, string? fields = "Id,FirstName,LastName,Email")
        {
            try
            {
                var query = GetViewModelFactory.CreateGetAllRequestViewModel(
                    searchTerm: null,
                    fields: fields,
                    pageNumber: 1,
                    pageSize: Constants.MAX_PAGES,
                    locationId: locationId,
                    serviceId: serviceId
                );

                var workersResponse = await _workerServiceClient.GetAsync(query);
                return workersResponse.Items;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erreur lors de la récupération des employés pour la localisation {locationId} et le service {serviceId}.", ex);
            }
        }

        /// <summary>
        /// Récupère la localisation associée à un employé.
        /// </summary>
        public async Task<Location?> GetLocationAsync(int workerId, string? fields = "Location")
        {
            var worker = await _workerServiceClient.GetAsync(GetViewModelFactory.CreateGetViewModel(workerId, fields));
            return worker?.Location;
        }

        /// <summary>
        /// Récupère le service associé à un employé.
        /// </summary>
        public async Task<Service?> GetServiceAsync(int workerId, string? fields = "Service")
        {
            var worker = await _workerServiceClient.GetAsync(GetViewModelFactory.CreateGetViewModel(workerId, fields));
            return worker?.Service;
        }

        /// <summary>
        /// Vérifie si un employé existe par son identifiant.
        /// </summary>
        public async Task<bool> ExistsAsync(int workerId)
        {
            return await _workerServiceClient.ExistsAsync(workerId);
        }

        /// <summary>
        /// Vérifie si un employé existe par son email.
        /// </summary>
        public async Task<bool> ExistsAsync(string email)
        {
            return await _workerServiceClient.ExistsAsync(email);
        }
    }
}