namespace CompanyDirectory.Models.ViewsModels.Responses
{
    /// <summary>
    /// Modèle de réponse pour représenter les informations d'une localisation.
    /// Ce modèle est utilisé pour fournir des champs dynamiques selon les besoins.
    /// </summary>
    public class LocationGetResponseViewModel
    {
        /// <summary>
        /// Identifiant unique de la localisation.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Nom de la ville où se trouve la localisation.
        /// </summary>
        public string? City { get; set; }
    }
}