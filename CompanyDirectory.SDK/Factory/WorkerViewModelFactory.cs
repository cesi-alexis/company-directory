using CompanyDirectory.Models.ViewsModels.Requests;

namespace CompanyDirectory.SDK.Factories
{
    /// <summary>
    /// Factory pour créer des ViewModels liés aux employés.
    /// </summary>
    public static class WorkerViewModelFactory
    {
        /// <summary>
        /// Crée un ViewModel pour ajouter ou mettre à jour un employé.
        /// </summary>
        /// <param name="name">Nom de l'employé.</param>
        /// <param name="email">Email de l'employé.</param>
        /// <param name="phone">Téléphone de l'employé.</param>
        /// <param name="serviceId">Identifiant du service.</param>
        /// <param name="locationId">Identifiant de la localisation.</param>
        /// <returns>Un ViewModel prêt à être utilisé.</returns>
        public static WorkerUpsertRequestViewModel CreateWorkerUpsertViewModel(string firstName, string lastName, string email, string phoneFixed, string phoneMobile, int serviceId, int locationId)
        {
            return new WorkerUpsertRequestViewModel
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PhoneFixed = phoneFixed,
                PhoneMobile = phoneMobile,
                ServiceId = serviceId,
                LocationId = locationId
            };
        }

        /// <summary>
        /// Crée un ViewModel pour transférer des employés.
        /// </summary>
        /// <param name="workerIds">Liste des identifiants des employés.</param>
        /// <param name="newServiceId">Identifiant du nouveau service.</param>
        /// <param name="newLocationId">Identifiant de la nouvelle localisation.</param>
        /// <returns>Un ViewModel prêt à être utilisé.</returns>
        public static WorkerTransferViewModel CreateWorkerTransferViewModel(IEnumerable<int> workerIds, int? newServiceId, int? newLocationId)
        {
            return new WorkerTransferViewModel
            {
                WorkerIds = workerIds.ToList(),
                NewServiceId = newServiceId,
                NewLocationId = newLocationId
            };
        }
    }
}