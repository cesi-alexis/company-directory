namespace CompanyDirectory.Models.ViewsModels.Responses
{
    /// <summary>
    /// Modèle de réponse pour les opérations de transfert d'employés.
    /// </summary>
    public class TransferResponseViewModel
    {
        /// <summary>
        /// Nombre total d'employés impliqués dans le transfert.
        /// </summary>
        public int TotalWorkers { get; set; }

        /// <summary>
        /// Nombre d'employés transférés avec succès.
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// Nombre d'employés pour lesquels le transfert a échoué.
        /// </summary>
        public int FailedCount => TotalWorkers - SuccessCount;

        /// <summary>
        /// Liste des erreurs survenues pour chaque employé en échec.
        /// </summary>
        public IEnumerable<TransferErrorDetailResponseViewModel> Errors { get; set; } = [];

        /// <summary>
        /// Indique si l'opération a été entièrement réussie.
        /// </summary>
        public bool IsCompleteSuccess => FailedCount == 0;
    }
}
