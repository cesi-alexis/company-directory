namespace CompanyDirectory.Models.ViewsModels.Responses
{
    /// <summary>
    /// Modèle de réponse pour représenter les informations d'un service.
    /// Ce modèle est utilisé pour fournir des champs dynamiques selon les besoins.
    /// </summary>
    public class ServiceGetResponseViewModel
    {
        /// <summary>
        /// Identifiant unique du service.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Nom du service.
        /// </summary>
        public string? Name { get; set; }
    }
}