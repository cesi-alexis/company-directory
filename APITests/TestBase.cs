using System.Net;
using System.Text.Json;
using RestSharp;
using Xunit;

namespace APITests
{
    public abstract class TestBase
    {
        protected void LogRequest(RestRequest request, Uri baseUrl)
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

        protected void LogResponse(RestResponse response)
        {
            Console.WriteLine("RESPONSE:");
            Console.WriteLine($"Status Code: {response.StatusCode}");
            Console.WriteLine($"Content: {response.Content}");
            Console.WriteLine("------");
        }

        /// <summary>
        /// Exécute une requête avec journalisation, validation des réponses et gestion des exceptions.
        /// </summary>
        protected async Task<T> ExecuteRequestWithValidation<T>(
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
        /// Simplifie la création de requêtes REST.
        /// </summary>
        protected RestRequest CreateRequest(string resource, Method method, object? body = null)
        {
            var request = new RestRequest(resource, method);
            if (body != null)
            {
                request.AddJsonBody(body);
            }
            return request;
        }

        /// <summary>
        /// Enregistre le résultat d'un test avec gestion centralisée des exceptions.
        /// </summary>
        protected async Task LogTest(string endpoint, string testCase, Func<Task> testAction)
        {
            try
            {
                await testAction();
                TestLogger.LogTestResult(endpoint, testCase, "✔️ Success");
            }
            catch (Exception ex)
            {
                TestLogger.LogTestResult(endpoint, testCase, $"❌ Failed - {ex.Message}");
                throw;
            }
        }
    }
}