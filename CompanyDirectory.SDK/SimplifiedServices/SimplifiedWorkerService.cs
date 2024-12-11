using CompanyDirectory.Models.Entities;
using CompanyDirectory.Models.ViewsModels.Responses;
using CompanyDirectory.SDK.Factories;
using CompanyDirectory.SDK.Factory;
using CompanyDirectory.SDK.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompanyDirectory.SDK.SimplifiedServices
{
    /// <summary>
    /// Service simplifié pour gérer les employés sans nécessiter de ViewModels explicites.
    /// </summary>
    public class SimplifiedWorkerService
    {
        private readonly WorkerServiceClient _workerServiceClient;

        /// <summary>
        /// Initialise une nouvelle instance de <see cref="SimplifiedWorkerService"/>.
        /// </summary>
        /// <param name="workerServiceClient">Client du service Worker.</param>
        public SimplifiedWorkerService(WorkerServiceClient workerServiceClient)
        {
            _workerServiceClient = workerServiceClient;
        }

        /// <summary>
        /// Crée un nouvel employé.
        /// </summary>
        /// <param name="firstName">Prénom de l'employé.</param>
        /// <param name="lastName">Nom de famille de l'employé.</param>
        /// <param name="email">Adresse email de l'employé.</param>
        /// <param name="phoneFixed">Téléphone fixe de l'employé.</param>
        /// <param name="phoneMobile">Téléphone mobile de l'employé.</param>
        /// <param name="serviceId">Identifiant du service associé.</param>
        /// <param name="locationId">Identifiant de la localisation associée.</param>
        /// <returns>L'employé créé.</returns>
        public async Task<Worker> CreateWorkerAsync(string firstName, string lastName, string email, string phoneFixed, string phoneMobile, int serviceId, int locationId)
        {
            var model = WorkerViewModelFactory.CreateWorkerUpsertViewModel(firstName, lastName, email, phoneFixed, phoneMobile, serviceId, locationId);
            return await _workerServiceClient.CreateAsync(model);
        }

        /// <summary>
        /// Récupère une liste paginée d'employés.
        /// </summary>
        /// <param name="searchTerm">Terme de recherche pour filtrer les employés.</param>
        /// <param name="fields">Champs spécifiques à inclure dans la réponse.</param>
        /// <param name="pageNumber">Numéro de la page.</param>
        /// <param name="pageSize">Nombre d'éléments par page.</param>
        /// <returns>Une liste paginée d'employés.</returns>
        public async Task<IEnumerable<Worker>> GetAsync(string? searchTerm, string? fields, int pageNumber, int pageSize)
        {
            var query = GetViewModelFactory.CreateGetAllRequestViewModel(searchTerm, fields, pageNumber, pageSize);
            var response = await _workerServiceClient.GetAsync(query);
            return response.Items;
        }

        /// <summary>
        /// Récupère un employé spécifique par son identifiant avec des filtres optionnels.
        /// </summary>
        /// <param name="id">Identifiant unique de l'employé.</param>
        /// <param name="fields">Champs spécifiques à inclure dans la réponse.</param>
        /// <param name="locationId">Identifiant optionnel de la localisation.</param>
        /// <param name="serviceId">Identifiant optionnel du service.</param>
        /// <returns>L'employé correspondant ou <c>null</c> s'il n'existe pas.</returns>
        public async Task<Worker?> GetAsync(int id, string? fields = null, int? locationId = null, int? serviceId = null)
        {
            var query = GetViewModelFactory.CreateGetViewModel(id, fields, locationId, serviceId);
            return await _workerServiceClient.GetAsync(query);
        }

        /// <summary>
        /// Vérifie si un employé existe par son identifiant.
        /// </summary>
        /// <param name="id">Identifiant unique de l'employé.</param>
        /// <returns><c>true</c> si l'employé existe, sinon <c>false</c>.</returns>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _workerServiceClient.ExistsAsync(id);
        }

        /// <summary>
        /// Vérifie si un employé existe par son email.
        /// </summary>
        /// <param name="email">Adresse email de l'employé.</param>
        /// <returns><c>true</c> si l'employé existe, sinon <c>false</c>.</returns>
        public async Task<bool> ExistsAsync(string email)
        {
            return await _workerServiceClient.ExistsAsync(email);
        }

        /// <summary>
        /// Met à jour un employé existant.
        /// </summary>
        /// <param name="id">Identifiant unique de l'employé.</param>
        /// <param name="firstName">Prénom de l'employé.</param>
        /// <param name="lastName">Nom de famille de l'employé.</param>
        /// <param name="email">Adresse email de l'employé.</param>
        /// <param name="phoneFixed">Téléphone fixe de l'employé.</param>
        /// <param name="phoneMobile">Téléphone mobile de l'employé.</param>
        /// <param name="serviceId">Identifiant du service associé.</param>
        /// <param name="locationId">Identifiant de la localisation associée.</param>
        public async Task UpdateAsync(int id, string firstName, string lastName, string email, string phoneFixed, string phoneMobile, int serviceId, int locationId)
        {
            var model = WorkerViewModelFactory.CreateWorkerUpsertViewModel(firstName, lastName, email, phoneFixed, phoneMobile, serviceId, locationId);
            await _workerServiceClient.UpdateAsync(id, model);
        }

        /// <summary>
        /// Supprime un employé par son identifiant.
        /// </summary>
        /// <param name="id">Identifiant unique de l'employé.</param>
        public async Task DeleteAsync(int id)
        {
            await _workerServiceClient.DeleteAsync(id);
        }

        /// <summary>
        /// Transfère des employés vers une nouvelle localisation et/ou un nouveau service.
        /// </summary>
        /// <param name="workerIds">Liste des identifiants des employés à transférer.</param>
        /// <param name="newServiceId">Identifiant du nouveau service (facultatif).</param>
        /// <param name="newLocationId">Identifiant de la nouvelle localisation (facultatif).</param>
        /// <returns>Le résultat du transfert.</returns>
        public async Task<TransferResponseViewModel> TransferWorkersAsync(IEnumerable<int> workerIds, int? newServiceId = null, int? newLocationId = null)
        {
            var model = WorkerViewModelFactory.CreateWorkerTransferViewModel(workerIds, newServiceId, newLocationId);
            return await _workerServiceClient.TransferWorkersAsync(model);
        }
    }
}