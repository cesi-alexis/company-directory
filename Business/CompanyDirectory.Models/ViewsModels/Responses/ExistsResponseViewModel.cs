namespace CompanyDirectory.Models.ViewsModels.Responses
{
    /// <summary>
    /// Modèle de vue utilisé pour indiquer si une entité existe.
    /// Ce modèle standardise la réponse pour les endpoints "exists".
    /// </summary>
    public class ExistsResponseViewModel
    {
        /// <summary>
        /// Indique si l'entité recherchée existe.
        /// </summary>
        public bool Exists { get; set; }
    }
}