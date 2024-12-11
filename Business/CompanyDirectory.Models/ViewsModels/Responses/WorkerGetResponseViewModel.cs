namespace CompanyDirectory.Models.ViewsModels.Responses
{
    /// <summary>
    /// Modèle de réponse pour les employés, incluant les champs nécessaires.
    /// </summary>
    public class WorkerGetResponseViewModel
    {
        /// <summary>
        /// Identifiant unique de l'employé.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Prénom de l'employé.
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Nom de famille de l'employé.
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// Numéro de téléphone fixe de l'employé.
        /// </summary>
        public string? PhoneFixed { get; set; }

        /// <summary>
        /// Numéro de téléphone mobile de l'employé.
        /// </summary>
        public string? PhoneMobile { get; set; }

        /// <summary>
        /// Adresse email de l'employé.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Identifiant du service auquel l'employé est associé.
        /// </summary>
        public int? ServiceId { get; set; }

        /// <summary>
        /// Identifiant de la localisation à laquelle l'employé est associé.
        /// </summary>
        public int? LocationId { get; set; }
    }
}