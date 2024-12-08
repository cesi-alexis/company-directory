public static class ErrorMessages
{
    public const string InvalidId = "Invalid ID provided.";
    public const string WorkerNotFound = "Worker with ID {0} not found.";
    public const string ServiceNotFound = "Service with ID {0} not found.";
    public const string LocationNotFound = "Location with ID {0} not found.";
    public const string DuplicateName = "An entity with the same name already exists.";
    public const string DuplicateEmail = "An worker with the same email already exists.";
    public const string CannotDeleteLinkedEntity = "Cannot delete a location linked to workers.";
    public const string InvalidEmailFormat = "Invalid email format.";
    public const string InvalidNameFormat = "Invalid name format. Only letters, spaces, and dashes are allowed.";
    public const string InvalidPhoneNumberFormat = "Invalid phone number format.";
    public const string IdMismatch = "The ID in the URL does not match the location ID.";
    public const string WorkerDataRequired = "Location data is required.";
}