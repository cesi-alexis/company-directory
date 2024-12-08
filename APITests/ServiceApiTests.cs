using System.Net;
using Newtonsoft.Json.Linq;
using RestSharp;
using Xunit;

namespace APITests
{
    public class ServiceApiTests : BaseApiTests
    {
        public ServiceApiTests() : base() { }

        /// <summary>
        /// Crée un nouveau service pour les tests.
        /// </summary>
        /// <returns>ID du service créé.</returns>
        public async Task<int> CreateService(string? name = null)
        {
            name ??= $"TestService-{Guid.NewGuid():N}".Replace("0", "O").Replace("1", "I");

            var request = new RestRequest("Service", Method.Post)
                .AddJsonBody(new { name });

            var response = await Client.ExecuteAsync(request);
            Assert.Contains(response.StatusCode, new[] { HttpStatusCode.OK, HttpStatusCode.Created });

            var content = JObject.Parse(response.Content!);
            var id = content["value"]?["id"];
            Assert.NotNull(id);

            return (int)id!;
        }

        [Fact]
        public async Task GetAllServices_ShouldReturnServices()
        {
            await LogTest("Service", "GetAllServices_ShouldReturnServices", async () =>
            {
                var request = new RestRequest("Service", Method.Get);
                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var content = JObject.Parse(response.Content!);
                Assert.NotNull(content);

                if (content["services"] is JArray services)
                {
                    Assert.True(services.Count >= 0, "The list of services may be empty but should not cause an error.\n" + content);
                }
                else
                {
                    throw new Xunit.Sdk.XunitException("The 'Services' property is not present or is not a valid JSON array.\n" + content);
                }
            });
        }

        [Fact]
        public async Task GetServiceById_ValidId_ShouldReturnService()
        {
            await LogTest("Service/{id}", "GetServiceById_ValidId_ShouldReturnService", async () =>
            {
                var serviceId = await CreateService();
                var request = new RestRequest($"Service/{serviceId}", Method.Get);
                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var content = JObject.Parse(response.Content!);
                Assert.Equal(serviceId, (int)content["id"]!);
            });
        }

        [Fact]
        public async Task GetServiceById_InvalidId_ShouldReturnNotFound()
        {
            await LogTest("Service/{id}", "GetServiceById_InvalidId_ShouldReturnNotFound", async () =>
            {
                var request = new RestRequest($"Service/{int.MaxValue}", Method.Get);
                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            });
        }

        [Fact]
        public async Task CreateService_WithMissingField_ShouldReturnBadRequest()
        {
            await LogTest("Service", "CreateService_WithMissingField_ShouldReturnBadRequest", async () =>
            {
                var request = new RestRequest("Service", Method.Post)
                    .AddJsonBody(new { });

                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            });
        }

        [Fact]
        public async Task CreateService_WithDuplicateName_ShouldReturnConflict()
        {
            var name = $"DuplicateService-{Guid.NewGuid():N}";
            await CreateService(name);

            var request = new RestRequest("Service", Method.Post)
                .AddJsonBody(new { name });

            var response = await Client.ExecuteAsync(request);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task UpdateService_ValidData_ShouldReturnOk()
        {
            await LogTest("Service/{id}", "UpdateService_ValidData_ShouldReturnOk", async () =>
            {
                var serviceId = await CreateService();
                var request = new RestRequest($"Service/{serviceId}", Method.Put)
                    .AddJsonBody(new
                    {
                        id = serviceId,
                        name = $"UpdatedService-{Guid.NewGuid():N}"
                    });

                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            });
        }

        [Fact]
        public async Task UpdateService_InvalidId_ShouldReturnNotFound()
        {
            await LogTest("Service/{id}", "UpdateService_InvalidId_ShouldReturnNotFound", async () =>
            {
                var request = new RestRequest($"Service/{int.MaxValue}", Method.Put)
                    .AddJsonBody(new
                    {
                        id = int.MaxValue,
                        name = $"NonExistentService-{Guid.NewGuid():N}"
                    });

                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            });
        }

        [Fact]
        public async Task DeleteService_ValidId_ShouldReturnOK()
        {
            await LogTest("Service/{id}", "DeleteService_ValidId_ShouldReturnOK", async () =>
            {
                var serviceId = await CreateService();
                var request = new RestRequest($"Service/{serviceId}", Method.Delete);

                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            });
        }

        [Fact]
        public async Task DeleteService_InvalidId_ShouldReturnNotFound()
        {
            await LogTest("Service/{id}", "DeleteService_InvalidId_ShouldReturnNotFound", async () =>
            {
                var request = new RestRequest($"Service/{int.MaxValue}", Method.Delete);
                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            });
        }

        [Fact]
        public async Task DeleteService_WithLinkedEmployees_ShouldReturnConflict()
        {
            // Crée une instance pour les tests d'employés et de sites
            var siteApiTests = new SiteApiTests();
            var employeeApiTests = new EmployeeApiTests();

            // Crée un nouveau service
            var serviceId = await CreateService();

            // Crée un employé associé à ce service
            await employeeApiTests.CreateEmployee(serviceId: serviceId, siteApiTests: siteApiTests, serviceApiTests: this);

            // Tentative de suppression du service
            var request = new RestRequest($"Service/{serviceId}", Method.Delete);
            var response = await Client.ExecuteAsync(request);

            // Vérifie que le statut retourné est 409 Conflict
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task DeleteService_WithoutLinkedEmployees_ShouldReturnOK()
        {
            var serviceId = await CreateService();

            var request = CreateRequest($"Service/{serviceId}", Method.Delete);
            var response = await Client.ExecuteAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetServices_WithPagination_ShouldReturnLimitedResults()
        {
            var serviceApiTests = new ServiceApiTests();

            // Ajouter 5 services
            for (int i = 0; i < 5; i++)
            {
                await serviceApiTests.CreateService(name: $"TestService-{i}");
            }

            // Récupérer uniquement 4 services via la pagination
            var request = new RestRequest("Service", Method.Get)
                .AddQueryParameter("pageSize", "4")
                .AddQueryParameter("pageNumber", "1");

            var response = await Client.ExecuteAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = JObject.Parse(response.Content!);
            var services = content["services"];
            Assert.NotNull(services);
            Assert.Equal(4, services!.Count()); // Vérifie que seulement 4 services sont retournés
        }
    }
}