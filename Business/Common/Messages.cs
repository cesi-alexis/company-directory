namespace CompanyDirectory.Common
{
    /// <summary>
    /// Classe centralisée pour les messages d'erreur exposés par l'API.
    /// </summary>
    public static class Messages
    {
        // Général
        public const string InvalidId                   = "Invalid ID provided. ID must be greater than 0.";
        public const string IdMismatch                  = "The ID in the URL does not match the provided entity ID.";
        public const string ValidationFailed            = "Validation failed.";
        public const string UnexpectedError             = "An unexpected error occurred.";
        public const string PaginationInvalid           = "Page number and page size must be greater than 0.";

        // Non trouvé
        public const string WorkerNotFound              = "Worker with ID {0} not found.";
        public const string ServiceNotFound             = "Service with ID {0} not found.";
        public const string LocationNotFound            = "Location with ID {0} not found.";
        public const string ResourceNotFound            = "The requested resource was not found.";
        public const string NotFoundFromQuery           = "No results found for the specified query. SearchTerm: '{0}', Fields: '{1}', PageNumber: {2}, PageSize: {3}.";


        // Conflits (Doublons ou relations)
        public const string DuplicateName               = "An entity with the name '{0}' already exists.";
        public const string DuplicateEmail              = "A worker with the email '{0}' already exists.";
        public const string CannotDeleteLinkedEntity    = "Cannot delete a {0} linked to other entities.";
        public const string LinkedWorkersConflict       = "Cannot delete a location linked to workers.";

        // Validation des formats
        public const string InvalidEmailFormat          = "Invalid email format '{0}'.";
        public const string InvalidNameFormat           = "Invalid name format '{0}'. Only letters, spaces, and dashes are allowed.";
        public const string InvalidPhoneNumberFormat    = "Invalid phone number format '{0}'.";
        public const string InvalidFieldName            = "The following field(s) are invalid for the specified model: {0}.";
        public const string InvalidSearchTerms          = "The following search term(s) are invalid for the specified model: {0}.";

        // Success
        public const string Success                     = "Operation completed successfully.";
    }
}