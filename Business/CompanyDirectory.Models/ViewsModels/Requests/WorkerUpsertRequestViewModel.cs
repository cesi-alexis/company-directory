using System.ComponentModel.DataAnnotations;

namespace CompanyDirectory.Models.ViewsModels.Requests
{
    /// <summary>
    /// Modèle utilisé pour les opérations de création et de mise à jour (Upsert) d'un employé.
    /// </summary>
    public class WorkerUpsertRequestViewModel
    {
        /// <summary>
        /// Prénom de l'employé.
        /// </summary>
        [Required(ErrorMessage = "Le champ 'FirstName' est obligatoire.")]
        [MaxLength(50, ErrorMessage = "Le champ 'FirstName' ne peut pas dépasser 50 caractères.")]
        public string FirstName { get; set; } = null!;

        /// <summary>
        /// Nom de famille de l'employé.
        /// </summary>
        [Required(ErrorMessage = "Le champ 'LastName' est obligatoire.")]
        [MaxLength(50, ErrorMessage = "Le champ 'LastName' ne peut pas dépasser 50 caractères.")]
        public string LastName { get; set; } = null!;

        /// <summary>
        /// Numéro de téléphone fixe de l'employé.
        /// </summary>
        [Required(ErrorMessage = "Le champ 'PhoneFixed' est obligatoire.")]
        [Phone(ErrorMessage = "Le champ 'PhoneFixed' doit être un numéro de téléphone valide.")]
        [MaxLength(15, ErrorMessage = "Le champ 'PhoneFixed' ne peut pas dépasser 15 caractères.")]
        public string PhoneFixed { get; set; } = null!;

        /// <summary>
        /// Numéro de téléphone mobile de l'employé.
        /// </summary>
        [Required(ErrorMessage = "Le champ 'PhoneMobile' est obligatoire.")]
        [Phone(ErrorMessage = "Le champ 'PhoneMobile' doit être un numéro de téléphone valide.")]
        [MaxLength(15, ErrorMessage = "Le champ 'PhoneMobile' ne peut pas dépasser 15 caractères.")]
        public string PhoneMobile { get; set; } = null!;

        /// <summary>
        /// Adresse e-mail de l'employé.
        /// </summary>
        [Required(ErrorMessage = "Le champ 'Email' est obligatoire.")]
        [EmailAddress(ErrorMessage = "Le champ 'Email' doit être une adresse e-mail valide.")]
        [MaxLength(100, ErrorMessage = "Le champ 'Email' ne peut pas dépasser 100 caractères.")]
        public string Email { get; set; } = null!;

        /// <summary>
        /// Identifiant du service associé à l'employé.
        /// </summary>
        [Required(ErrorMessage = "Le champ 'ServiceId' est obligatoire.")]
        public int ServiceId { get; set; }

        /// <summary>
        /// Identifiant de la localisation associée à l'employé.
        /// </summary>
        [Required(ErrorMessage = "Le champ 'Id' est obligatoire.")]
        public int LocationId { get; set; }
    }
}