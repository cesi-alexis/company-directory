using Microsoft.AspNetCore.Mvc;

namespace API.Utils
{
    public static class ResponseUtils
    {
        public static IActionResult HandleResponse(Func<object> action)
        {
            try
            {
                var result = action.Invoke();
                return new OkObjectResult(result);
            }
            catch (ValidationException ex)
            {
                return new BadRequestObjectResult(new { Message = ex.Message });
            }
            catch (ConflictException ex)
            {
                return new ConflictObjectResult(new { Message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return new NotFoundObjectResult(new { Message = ex.Message });
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

        public static async Task<IActionResult> HandleResponseAsync(Func<Task<object>> action)
        {
            try
            {
                var result = await action.Invoke();
                return new OkObjectResult(result);
            }
            catch (ValidationException ex)
            {
                return new BadRequestObjectResult(new { Message = ex.Message });
            }
            catch (ConflictException ex)
            {
                return new ConflictObjectResult(new { Message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return new NotFoundObjectResult(new { Message = ex.Message });
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