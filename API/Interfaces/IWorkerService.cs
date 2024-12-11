using CompanyDirectory.Common;
using CompanyDirectory.Models.Entities;

namespace CompanyDirectory.API.Interfaces
{
    /// <summary>
    /// Interface pour les services liés à la gestion des employés.
    /// Fournit des opérations spécifiques aux employés telles que le filtrage, la récupération détaillée et le transfert.
    /// </summary>
    public interface IWorkerService : ICrudService<Worker>
    {
        /// <summary>
        /// Récupère une liste paginée d'employés avec des champs dynamiques et des critères de recherche.
        /// Permet également de filtrer les employés par localisation et/ou service.
        /// </summary>
        /// <param name="searchTerm">Terme de recherche utilisé pour filtrer les employés.</param>
        /// <param name="fields">Champs spécifiques à inclure dans la réponse, séparés par des virgules.</param>
        /// <param name="pageNumber">Numéro de la page à récupérer.</param>
        /// <param name="pageSize">Nombre d'éléments par page.</param>
        /// <param name="locationId">Identifiant optionnel de la localisation pour filtrer les employés.</param>
        /// <param name="serviceId">Identifiant optionnel du service pour filtrer les employés.</param>
        /// <returns>
        /// Un tuple contenant :
        /// - Une liste paginée d'employés avec les champs spécifiés.
        /// - Le nombre total d'employés correspondant aux critères.
        /// </returns>
        Task<(IEnumerable<object>? Items, int TotalCount)> GetFilteredAsync(
            string? searchTerm = null,
            string? fields = null,
            int pageNumber = 1,
            int pageSize = Constants.MAX_PAGES,
            int? locationId = null,
            int? serviceId = null);

        /// <summary>
        /// Récupère un employé spécifique par son identifiant avec des champs dynamiques et des filtres optionnels.
        /// Permet également de restreindre la recherche par localisation et/ou service.
        /// </summary>
        /// <param name="id">Identifiant unique de l'employé.</param>
        /// <param name="fields">Champs spécifiques à inclure dans la réponse, séparés par des virgules.</param>
        /// <param name="locationId">Identifiant optionnel de la localisation pour restreindre la recherche.</param>
        /// <param name="serviceId">Identifiant optionnel du service pour restreindre la recherche.</param>
        /// <returns>
        /// L'employé correspondant aux critères sous forme d'un objet contenant les champs demandés.
        /// Retourne null si aucun employé ne correspond.
        /// </returns>
        Task<object?> GetAsync(
            int id,
            string? fields,
            int? locationId = null,
            int? serviceId = null);

        /// <summary>
        /// Transfère un ou plusieurs employés vers une nouvelle localisation et/ou un nouveau service.
        /// Permet de définir si les transferts partiels sont autorisés en cas d'erreurs pour certains employés.
        /// </summary>
        /// <param name="workerIds">Liste des identifiants des employés à transférer.</param>
        /// <param name="newLocationId">Identifiant de la nouvelle localisation (optionnel).</param>
        /// <param name="newServiceId">Identifiant du nouveau service (optionnel).</param>
        /// <param name="allowPartialTransfer">Indique si les transferts partiels sont autorisés.</param>
        /// <returns>
        /// Un tuple contenant :
        /// - Le nombre total d'employés ciblés.
        /// - Le nombre d'employés transférés avec succès.
        /// - Une liste des erreurs rencontrées pour chaque employé (ID et message d'erreur).
        /// </returns>
        Task<(int TotalWorkers, int SuccessCount, List<(int WorkerId, string ErrorMessage)> Errors)> TransferWorkersAsync(
            List<int> workerIds, int? newLocationId, int? newServiceId, bool allowPartialTransfer);
    }
}