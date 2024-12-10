using CompanyDirectory.Common.Exceptions;
using CompanyDirectory.Models.ViewsModels.Responses;
using Microsoft.AspNetCore.Mvc;
using CompanyDirectory.Common;
using Azure;

namespace CompanyDirectory.API.Utils
{
    public static class ResponseUtils
    {
        /// <summary>
        /// Gère les réponses des contrôleurs de manière uniforme avec gestion des exceptions.
        /// </summary>
        /// <typeparam name="T">Le type des données renvoyées dans la réponse.</typeparam>
        /// <param name="action">L'action à exécuter.</param>
        /// <returns>Une instance de <see cref="IActionResult"/> enveloppant un <see cref="ResponseViewModel{T}"/>.</returns>
        public static async Task<IActionResult> HandleResponseAsync<T>(Func<Task<T?>> action, int successStatusCode = StatusCodes.Status200OK)
        {
            try
            {
                var result = await action.Invoke();
                
                var response = ResponseViewModel<T>.SuccessResponse(result, Messages.Success);

                // Crée une réponse avec le code de statut personnalisé
                return new ObjectResult(response)
                {
                    StatusCode = successStatusCode
                };
            }
            catch (ValidationException ex)
            {
                return new BadRequestObjectResult(ResponseViewModel<T>.FailureResponse(
                    ex.Message, StatusCodes.Status400BadRequest));
            }
            catch (ConflictException ex)
            {
                return new ConflictObjectResult(ResponseViewModel<T>.FailureResponse(
                    ex.Message, StatusCodes.Status409Conflict));
            }
            catch (NotFoundException ex)
            {
                return new NotFoundObjectResult(ResponseViewModel<T>.FailureResponse(
                    ex.Message, StatusCodes.Status404NotFound));
            }
            catch (Exception ex)
            {
                return new ObjectResult(ResponseViewModel<T>.FailureResponse(
                    Messages.UnexpectedError, 500, new Dictionary<string, object>
                    {
                        { "Details", ex.Message }
                    }))
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}