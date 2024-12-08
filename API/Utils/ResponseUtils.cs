using CompanyDirectory.API.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CompanyDirectory.API.Utils
{
    public static class ResponseUtils
    {
        public static async Task<IActionResult> HandleResponseAsync(Func<Task<object?>> action)
        {
            try
            {
                var result = await action.Invoke();
                if (result is null)
                {
                    return new NotFoundObjectResult(new { Message = "The requested resource was not found." });
                }
                return new OkObjectResult(result);
            }
            catch (ValidationException ex)
            {
                return new BadRequestObjectResult(new { ex.Message });
            }
            catch (ConflictException ex)
            {
                return new ConflictObjectResult(new { ex.Message });
            }
            catch (NotFoundException ex)
            {
                return new NotFoundObjectResult(new { ex.Message });
            }
            catch (Exception ex)
            {
                // Pour les erreurs inattendues
                return new ObjectResult(new { Message = "An unexpected error occurred.", Details = ex.Message })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}