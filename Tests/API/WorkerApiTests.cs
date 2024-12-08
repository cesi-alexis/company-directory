using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;

namespace TestsAPI
{
    public class WorkerApiTests : BaseApiTests
    {
        public WorkerApiTests() : base() { }

        public async Task<int> CreateEmployee(
            string? firstName = "TestFirstName",
            string? lastName = "TestLastName",
            string? phoneFixed = "0123456789",
            string? phoneMobile = "0612345678",
            string? email = null,
            int? serviceId = null,
            int? locationId = null,
            ServiceApiTests? serviceApiTests = null,
            LocationApiTests? locationApiTests = null)
        {
            if (serviceApiTests == null || locationApiTests == null)
                throw new ArgumentNullException("ServiceApiTests and LocationApiTests instances must be provided.");

            email ??= $"test{Guid.NewGuid()}@example.com";
            serviceId ??= await serviceApiTests.CreateService();
            locationId ??= await locationApiTests.CreateLocation();

            var request = new RestRequest("Workers", Method.Post)
                .AddJsonBody(new
                {
                    firstName,
                    lastName,
                    phoneFixed,
                    phoneMobile,
                    email,
                    serviceId,
                    locationId
                });

            var response = await Client.ExecuteAsync(request);
            Assert.Contains(response.StatusCode, new[] { HttpStatusCode.OK, HttpStatusCode.Created });

            var content = JObject.Parse(response.Content!);
            var id = content["value"]?["id"];
            Assert.NotNull(id);

            return (int)id!;
        }

        [Fact]
        public async Task GetAllWorkers_ShouldReturnWorkers()
        {
            await LogTest("Workers", "GetAllWorkers_ShouldReturnWorkers", async () =>
            {
                var request = new RestRequest("workers", Method.Get);
                var response = await Client.ExecuteAsync(request);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var content = JObject.Parse(response.Content!);
                Assert.NotNull(content);

                if (content["workers"] is JArray workers)
                {
                    Assert.True(workers.Count >= 0, "The list of workers may be empty but should not cause an error.\n" + content);
                }
                else
                {
                    throw new Xunit.Sdk.XunitException("The 'Workers' property is not present or is not a valid JSON array.\n" + content);
                }
            });
        }

        [Fact]
        public async Task CreateEmployee_ValidData_ShouldReturnCreatedEmployee()
        {
            var serviceApiTests = new ServiceApiTests();
            var locationApiTests = new LocationApiTests();

            await LogTest("Workers", "CreateEmployee_ValidData_ShouldReturnCreatedEmployee", async () =>
            {
                var workerId = await CreateEmployee(serviceApiTests: serviceApiTests, locationApiTests: locationApiTests);
                Assert.True(workerId > 0);
            });
        }

        [Fact]
        public async Task CreateEmployee_MissingRequiredFields_ShouldReturnBadRequest()
        {
            await LogTest("Workers", "CreateEmployee_MissingRequiredFields_ShouldReturnBadRequest", async () =>
            {
                var request = new RestRequest("Workers", Method.Post)
                    .AddJsonBody(new { });

                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            });
        }

        [Fact]
        public async Task CreateEmployee_InvalidEmailFormat_ShouldReturnBadRequest()
        {
            var serviceApiTests = new ServiceApiTests();
            var locationApiTests = new LocationApiTests();

            await LogTest("Workers", "CreateEmployee_InvalidEmailFormat_ShouldReturnBadRequest", async () =>
            {
                var request = new RestRequest("Workers", Method.Post)
                    .AddJsonBody(new
                    {
                        firstName = "John",
                        lastName = "Doe",
                        phoneFixed = "0123456789",
                        phoneMobile = "0612345678",
                        email = "invalid-email",
                        serviceId = await serviceApiTests.CreateService(),
                        locationId = await locationApiTests.CreateLocation()
                    });

                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            });
        }

        [Fact]
        public async Task CreateEmployee_DuplicateEmail_ShouldReturnConflict()
        {
            var serviceApiTests = new ServiceApiTests();
            var locationApiTests = new LocationApiTests();
            var email = $"duplicate{Guid.NewGuid()}@example.com";

            await CreateEmployee(email: email, serviceApiTests: serviceApiTests, locationApiTests: locationApiTests);

            var request = new RestRequest("Workers", Method.Post)
                .AddJsonBody(new
                {
                    firstName = "John",
                    lastName = "Doe",
                    phoneFixed = "0123456789",
                    phoneMobile = "0612345678",
                    email,
                    serviceId = await serviceApiTests.CreateService(),
                    locationId = await locationApiTests.CreateLocation()
                });

            var response = await Client.ExecuteAsync(request);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task GetEmployeeById_ValidId_ShouldReturnEmployee()
        {
            var serviceApiTests = new ServiceApiTests();
            var locationApiTests = new LocationApiTests();

            await LogTest("Workers/{id}", "GetEmployeeById_ValidId_ShouldReturnEmployee", async () =>
            {
                var workerId = await CreateEmployee(serviceApiTests: serviceApiTests, locationApiTests: locationApiTests);

                var request = new RestRequest($"Workers/{workerId}", Method.Get);
                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var content = JObject.Parse(response.Content!);
                Assert.Equal(workerId, (int)content["id"]!);
            });
        }

        [Fact]
        public async Task DeleteEmployee_ValidId_ShouldReturnSuccess()
        {
            var serviceApiTests = new ServiceApiTests();
            var locationApiTests = new LocationApiTests();

            await LogTest("Workers/{id}", "DeleteEmployee_ValidId_ShouldReturnSuccess", async () =>
            {
                var workerId = await CreateEmployee(serviceApiTests: serviceApiTests, locationApiTests: locationApiTests);

                var request = new RestRequest($"Workers/{workerId}", Method.Delete);
                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            });
        }

        [Fact]
        public async Task GetWorkers_WithSelectedFields_ShouldReturnOnlySpecifiedFields()
        {
            var serviceApiTests = new ServiceApiTests();
            var locationApiTests = new LocationApiTests();

            // Vérifier si un employé avec les champs spécifiés existe déjà
            var existsRequest = new RestRequest("Workers", Method.Get)
                .AddQueryParameter("fields", "firstName,email")
                .AddQueryParameter("pageSize", "100"); // Récupérer suffisamment d'éléments pour vérifier

            var existsResponse = await Client.ExecuteAsync(existsRequest);
            Assert.Equal(HttpStatusCode.OK, existsResponse.StatusCode);

            var existingContent = JObject.Parse(existsResponse.Content!);
            var workers = existingContent["workers"]?.ToObject<List<JObject>>();

            if (workers == null || !workers.Any(e => e["firstName"]?.ToString() != null && e["email"]?.ToString() != null))
            {
                // Ajouter un employé si aucun n'existe avec les champs spécifiés
                await CreateEmployee(serviceApiTests: serviceApiTests, locationApiTests: locationApiTests);
            }

            // Requête avec le filtre de champ "firstName" et "email"
            var request = new RestRequest("Workers", Method.Get)
                .AddQueryParameter("fields", "firstName,email");

            var response = await Client.ExecuteAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = JObject.Parse(response.Content!);
            foreach (var worker in content["workers"]!)
            {
                Assert.NotNull(worker["FirstName"]); // Vérifie que "firstName" est présent
                Assert.NotNull(worker["Email"]); // Vérifie que "email" est présent
                Assert.Null(worker["LastName"]); // Vérifie que "lastName" est absent
            }
        }

        [Fact]
        public async Task CreateEmployee_WithoutFirstName_ShouldReturnBadRequest()
        {
            var serviceApiTests = new ServiceApiTests();
            var locationApiTests = new LocationApiTests();

            var request = new RestRequest("Workers", Method.Post)
                .AddJsonBody(new
                {
                    lastName = "Doe",
                    phoneFixed = "0123456789",
                    phoneMobile = "0612345678",
                    email = $"test{Guid.NewGuid()}@example.com",
                    serviceId = await serviceApiTests.CreateService(),
                    locationId = await locationApiTests.CreateLocation()
                });

            var response = await Client.ExecuteAsync(request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetEmployee_ByNonExistentId_ShouldReturnNotFound()
        {
            var request = new RestRequest($"Workers/{int.MaxValue}", Method.Get);
            var response = await Client.ExecuteAsync(request);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteEmployee_ByNonExistentId_ShouldReturnNotFound()
        {
            var request = new RestRequest($"Workers/{int.MaxValue}", Method.Delete);
            var response = await Client.ExecuteAsync(request);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateEmployee_WithMaxLengthFields_ShouldSucceed()
        {
            var serviceApiTests = new ServiceApiTests();
            var locationApiTests = new LocationApiTests();

            var firstName = new string('A', 50); // Longueur maximale
            var lastName = new string('B', 50);
            var email = $"test{Guid.NewGuid()}@example.com";

            var workerId = await new WorkerApiTests()
                .CreateEmployee(firstName: firstName, lastName: lastName, email: email, serviceApiTests: serviceApiTests, locationApiTests: locationApiTests);

            Assert.True(workerId > 0);
        }

        [Fact]
        public async Task GetWorkers_WithPagination_ShouldReturnLimitedResults()
        {
            var serviceApiTests = new ServiceApiTests();
            var locationApiTests = new LocationApiTests();

            // Ajouter 5 employés
            for (int i = 0; i < 5; i++)
            {
                await CreateEmployee(serviceApiTests: serviceApiTests, locationApiTests: locationApiTests);
            }

            // Récupérer uniquement 4 employés via la pagination
            var request = new RestRequest("Workers", Method.Get)
                .AddQueryParameter("pageSize", "4")
                .AddQueryParameter("pageNumber", "1");

            var response = await Client.ExecuteAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = JObject.Parse(response.Content!);
            var workers = content["workers"];
            Assert.NotNull(workers);
            Assert.Equal(4, workers!.Count()); // Vérifie que seulement 4 employés sont retournés
        }

    }
}