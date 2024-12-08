using RestSharp;
using System.Net;
using System.Text.Json;

namespace CompanyDirectory.Tests.API
{
    public abstract class BaseTests
    {
        protected readonly RestClient Client;

        protected BaseTests()
        {
            Client = new RestClient(new RestClientOptions
            {
                BaseUrl = new Uri("http://localhost:7055/api")
            });
        }

        /// <summary>
        /// Log the request details.
        /// </summary>
        protected static void LogRequest(RestRequest request, Uri baseUrl)
        {
            Console.WriteLine("REQUEST:");
            Console.WriteLine($"Method: {request.Method}");
            Console.WriteLine($"Endpoint: {baseUrl}{request.Resource}");
            foreach (var header in request.Parameters.Where(p => p.Type == ParameterType.HttpHeader))
            {
                Console.WriteLine($"Header: {header.Name}: {header.Value}");
            }
            foreach (var body in request.Parameters.Where(p => p.Type == ParameterType.RequestBody))
            {
                Console.WriteLine($"Body: {body.Value}");
            }
            Console.WriteLine("------");
        }

        /// <summary>
        /// Log the response details.
        /// </summary>
        protected static void LogResponse(RestResponse response)
        {
            Console.WriteLine("RESPONSE:");
            Console.WriteLine($"Status Code: {response.StatusCode}");
            Console.WriteLine($"Content: {response.Content}");
            Console.WriteLine("------");
        }

        /// <summary>
        /// Execute a REST request with validation and logging.
        /// </summary>
        protected static async Task<T> ExecuteRequestWithValidation<T>(
            RestClient client,
            RestRequest request,
            params HttpStatusCode[] expectedStatusCodes)
        {
            try
            {
                LogRequest(request, client.Options.BaseUrl!);
                var response = await client.ExecuteAsync(request);
                LogResponse(response);

                if (!expectedStatusCodes.Contains(response.StatusCode))
                {
                    throw new HttpRequestException(
                        $"Unexpected status code: {response.StatusCode}. Content: {response.Content}");
                }

                Assert.NotNull(response.Content);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(response.Content!)!;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to deserialize response content: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Simplifies the creation of REST requests.
        /// </summary>
        protected static RestRequest CreateRequest(string resource, Method method, object? body = null)
        {
            var request = new RestRequest(resource, method);
            if (body != null)
            {
                request.AddJsonBody(body);
            }
            return request;
        }

        /// <summary>
        /// Handles actions with centralized exception management.
        /// </summary>
        protected static async Task AwaitAction(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR during action execution: {ex.Message}");
                throw;
            }
        }
    }
}