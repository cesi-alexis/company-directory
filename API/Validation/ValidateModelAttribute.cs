using CompanyDirectory.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;

namespace CompanyDirectory.API.Validation
{
    /// <summary>
    /// Attribut pour valider les modèles envoyés dans les requêtes HTTP.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ValidateModelAttribute(ILogger<ValidateModelAttribute> logger) : ActionFilterAttribute
    {
        private readonly ILogger<ValidateModelAttribute> _logger = logger;

        /// <summary>
        /// Méthode exécutée avant l'exécution d'une action pour valider les paramètres de la requête.
        /// </summary>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                // Parcours des arguments envoyés dans la requête
                foreach (var argument in context.ActionArguments)
                {
                    // Ignorer les arguments nuls
                    if (argument.Value == null)
                        continue;

                    // Validation spécifique pour les paramètres d'identifiants (ID)
                    if (argument.Key.Equals("id", StringComparison.OrdinalIgnoreCase) && argument.Value is int id)
                    {
                        if (id <= 0) // Vérifie si l'ID est inférieur ou égal à 0
                        {
                            _logger.LogWarning(Messages.InvalidId); // Journalise l'erreur
                            context.Result = new BadRequestObjectResult(new
                            {
                                Message = Messages.InvalidId,
                                ProvidedValue = id
                            });
                            return;
                        }
                    }

                    // Si l'argument est un objet complexe, valider ses propriétés
                    if (argument.Value.GetType().IsClass && argument.Value is not string)
                    {
                        ValidateObjectProperties(argument.Value, context);
                    }
                }

                // Vérification de l'état global des modèles
                if (!context.ModelState.IsValid)
                {
                    var errorDetails = context.ModelState
                        .Where(x => x.Value?.Errors.Any() == true)
                        .OrderBy(x => x.Key) // Trie les erreurs par clé
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    _logger.LogWarning(Messages.ValidationFailed); // Journalise l'erreur de validation

                    context.Result = new BadRequestObjectResult(new
                    {
                        Message = Messages.ValidationFailed,
                        Errors = errorDetails
                    });
                }
            }
            catch (Exception ex)
            {
                // Gestion des exceptions inattendues
                _logger.LogError(ex, Messages.UnexpectedError);
                context.Result = new ObjectResult(new
                {
                    Message = Messages.UnexpectedError,
                    Details = ex.Message
                })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        /// <summary>
        /// Valide les propriétés d'un objet complexe.
        /// </summary>
        /// <param name="obj">L'objet à valider.</param>
        /// <param name="context">Le contexte de l'exécution de l'action.</param>
        private void ValidateObjectProperties(object obj, ActionExecutingContext context)
        {
            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                try
                {
                    var value = property.GetValue(obj);

                    if (value is string strValue) // Validation des propriétés de type chaîne
                    {
                        if (property.Name.EndsWith("Email", StringComparison.OrdinalIgnoreCase) && !Formats.IsValidEmail(strValue))
                        {
                            ThrowValidationError(context, property.Name, Messages.InvalidEmailFormat, strValue);
                        }
                        else if ((property.Name.EndsWith("Name", StringComparison.OrdinalIgnoreCase) || property.Name.Equals("City", StringComparison.OrdinalIgnoreCase))
                                 && !Formats.IsValidName(strValue))
                        {
                            ThrowValidationError(context, property.Name, Messages.InvalidNameFormat, strValue);
                        }
                        else if (property.Name.EndsWith("Phone", StringComparison.OrdinalIgnoreCase) && !Formats.IsValidPhoneNumber(strValue))
                        {
                            ThrowValidationError(context, property.Name, Messages.InvalidPhoneNumberFormat, strValue);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Journalise les erreurs rencontrées lors de la validation d'une propriété
                    _logger.LogWarning(ex, "Error validating property {PropertyName} of object {ObjectName}.", property.Name, obj.GetType().Name);
                }
            }
        }

        /// <summary>
        /// Génère une erreur de validation pour une propriété spécifique.
        /// </summary>
        /// <param name="context">Le contexte de l'exécution de l'action.</param>
        /// <param name="propertyName">Le nom de la propriété en erreur.</param>
        /// <param name="message">Le message d'erreur.</param>
        /// <param name="providedValue">La valeur fournie qui est invalide.</param>
        private void ThrowValidationError(ActionExecutingContext context, string propertyName, string message, string providedValue)
        {
            // Journalise l'erreur avec un message statique et des paramètres
            _logger.LogWarning("Validation failed for property '{PropertyName}' with value '{ProvidedValue}'. Error: {ErrorMessage}",
                propertyName, providedValue, message);

            context.Result = new BadRequestObjectResult(new
            {
                Message = message,
                Field = propertyName,
                ProvidedValue = providedValue
            });
        }
    }
}
