namespace CompanyDirectory.Models.ViewsModels.Responses
{
    /// <summary>
    /// Détail des erreurs rencontrées lors d'un transfert d'employé.
    /// </summary>
    public class TransferErrorDetailResponseViewModel
    {
        /// <summary>
        /// Identifiant de l'employé concerné.
        /// </summary>
        public int WorkerId { get; set; }

        /// <summary>
        /// Message d'erreur décrivant la cause de l'échec.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
