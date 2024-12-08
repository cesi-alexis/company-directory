using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class ValidateModelAttribute : ActionFilterAttribute
{
    private readonly ILogger<ValidateModelAttribute> _logger;

    public ValidateModelAttribute(ILogger<ValidateModelAttribute> logger)
    {
        _logger = logger;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        try
        {
            foreach (var argument in context.ActionArguments)
            {
                if (argument.Value == null)
                    continue; // Ignore null values

                // Vérifie si le paramètre est un ID nommé "id"
                if (argument.Key.Equals("id", StringComparison.OrdinalIgnoreCase) && argument.Value is int id)
                {
                    if (id <= 0)
                    {
                        _logger.LogWarning("Invalid ID detected for parameter '{ParameterName}': {Id}", argument.Key, id);
                        context.Result = new BadRequestObjectResult(new
                        {
                            Message = $"Invalid ID provided for '{argument.Key}'. ID must be greater than 0.",
                            ProvidedValue = id
                        });
                        return;
                    }
                }

                // Si l'objet est complexe, valider ses propriétés
                if (argument.Value.GetType().IsClass && !(argument.Value is string))
                {
                    ValidateObjectProperties(argument.Value, context);
                }
            }

            // Vérification des erreurs de ModelState
            if (!context.ModelState.IsValid)
            {
                var errorDetails = context.ModelState
                    .Where(x => x.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                _logger.LogWarning("Model validation failed for action {ActionName}. Errors: {Errors}",
                                   context.ActionDescriptor.DisplayName,
                                   errorDetails);

                context.Result = new BadRequestObjectResult(new
                {
                    Message = "Validation failed.",
                    Errors = errorDetails
                });
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during validation for action {ActionName}.",
                             context.ActionDescriptor.DisplayName);

            context.Result = new ObjectResult(new
            {
                Message = "An unexpected error occurred during validation.",
                Details = ex.Message
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    private void ValidateObjectProperties(object obj, ActionExecutingContext context)
    {
        var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            try
            {
                var value = property.GetValue(obj);

                if (value is string strValue)
                {
                    // Validation des formats spécifiques
                    if (property.Name.EndsWith("Email", StringComparison.OrdinalIgnoreCase) && !Formats.IsValidEmail(strValue))
                    {
                        ThrowValidationError(context, property.Name, "Invalid email format.", strValue);
                    }
                    else if ((property.Name.EndsWith("Name", StringComparison.OrdinalIgnoreCase) || property.Name.Equals("City", StringComparison.OrdinalIgnoreCase))
                             && !Formats.IsValidName(strValue))
                    {
                        ThrowValidationError(context, property.Name, "Invalid name format. Name must only contain letters and valid characters.", strValue);
                    }
                    else if (property.Name.EndsWith("Phone", StringComparison.OrdinalIgnoreCase) && !Formats.IsValidPhoneNumber(strValue))
                    {
                        ThrowValidationError(context, property.Name, "Invalid phone number format.", strValue);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error validating property {PropertyName} of object {ObjectName}.", property.Name, obj.GetType().Name);
                // Continue la validation des autres propriétés
            }
        }
    }

    private void ThrowValidationError(ActionExecutingContext context, string propertyName, string message, string providedValue)
    {
        _logger.LogWarning("Validation failed for property '{PropertyName}': {Message}. Provided value: {Value}", propertyName, message, providedValue);

        context.Result = new BadRequestObjectResult(new
        {
            Message = message,
            Field = propertyName,
            ProvidedValue = providedValue
        });
    }
}
