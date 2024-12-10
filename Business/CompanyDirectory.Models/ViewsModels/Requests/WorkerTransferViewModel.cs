using System.ComponentModel.DataAnnotations;

namespace CompanyDirectory.Models.ViewsModels.Requests
{
    /// <summary>
    /// Modèle utilisé pour transférer un ou plusieurs employés vers une nouvelle localisation et/ou un nouveau service.
    /// </summary>
    public class WorkerTransferViewModel
    {
        /// <summary>
        /// Liste des identifiants des employés à transférer.
        /// </summary>
        [Required(ErrorMessage = "La liste des employés est obligatoire.")]
        [MinLength(1, ErrorMessage = "Au moins un employé doit être sélectionné pour le transfert.")]
        public List<int> WorkerIds { get; set; } = [];

        /// <summary>
        /// Identifiant de la nouvelle localisation (facultatif).
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "L'ID de la localisation doit être un entier positif.")]
        public int? NewLocationId { get; set; }

        /// <summary>
        /// Identifiant du nouveau service (facultatif).
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "L'ID du service doit être un entier positif.")]
        public int? NewServiceId { get; set; }

        /// <summary>
        /// Indique si un transfert partiel est autorisé en cas d'erreurs.
        /// </summary>
        public bool AllowPartialTransfer { get; set; } = false;
    }
}