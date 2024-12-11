using CompanyDirectory.Models.Entities;
using CompanyDirectory.Models.ViewsModels.Requests;
using CompanyDirectory.Models.ViewsModels.Responses;

namespace CompanyDirectory.SDK.Interfaces
{
    public interface IWorkerServiceClient : IServiceClient<Worker, WorkerUpsertRequestViewModel>
    {
        /// <summary>
        /// Récupère un élément spécifique par son identifiant.
        /// </summary>
        /// <param name="query">Modèle contenant l'identifiant de l'élément et les champs à inclure.</param>
        /// <returns>L'élément correspondant ou <c>null</c> s'il n'existe pas.</returns>
        Task<Worker?> GetAsync(WorkerGetRequestViewModel query);

        /// <summary>
        /// Transfère un ou plusieurs employés vers une nouvelle localisation et/ou un nouveau service.
        /// </summary>
        /// <param name="model">Données pour effectuer le transfert.</param>
        /// <returns>Résumé des résultats du transfert.</returns>
        Task<TransferResponseViewModel> TransferWorkersAsync(WorkerTransferViewModel model);
    }
}