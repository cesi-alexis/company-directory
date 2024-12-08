using System.Net;
using Newtonsoft.Json.Linq;
using RestSharp;
using Xunit;

namespace APITests
{
    public class EmployeeApiTests : BaseApiTests
    {
        public EmployeeApiTests() : base() { }

        public async Task<int> CreateEmployee(
            string? firstName = "TestFirstName",
            string? lastName = "TestLastName",
            string? phoneFixed = "0123456789",
            string? phoneMobile = "0612345678",
            string? email = null,
            int? serviceId = null,
            int? siteId = null,
            ServiceApiTests? serviceApiTests = null,
            SiteApiTests? siteApiTests = null)
        {
            if (serviceApiTests == null || siteApiTests == null)
                throw new ArgumentNullException("ServiceApiTests and SiteApiTests instances must be provided.");

            email ??= $"test{Guid.NewGuid()}@example.com";
            serviceId ??= await serviceApiTests.CreateService();
            siteId ??= await siteApiTests.CreateSite();

            var request = new RestRequest("Employees", Method.Post)
                .AddJsonBody(new
                {
                    firstName,
                    lastName,
                    phoneFixed,
                    phoneMobile,
                    email,
                    serviceId,
                    siteId
                });

            var response = await Client.ExecuteAsync(request);
            Assert.Contains(response.StatusCode, new[] { HttpStatusCode.OK, HttpStatusCode.Created });

            var content = JObject.Parse(response.Content!);
            var id = content["value"]?["id"];
            Assert.NotNull(id);

            return (int)id!;
        }

        [Fact]
        public async Task GetAllEmployees_ShouldReturnEmployees()
        {
            await LogTest("Employees", "GetAllEmployees_ShouldReturnEmployees", async () =>
            {
                var request = new RestRequest("employees", Method.Get);
                var response = await Client.ExecuteAsync(request);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var content = JObject.Parse(response.Content!);
                Assert.NotNull(content);

                if (content["employees"] is JArray employees)
                {
                    Assert.True(employees.Count >= 0, "The list of employees may be empty but should not cause an error.\n" + content);
                }
                else
                {
                    throw new Xunit.Sdk.XunitException("The 'Employees' property is not present or is not a valid JSON array.\n" + content);
                }
            });
        }

        [Fact]
        public async Task CreateEmployee_ValidData_ShouldReturnCreatedEmployee()
        {
            var serviceApiTests = new ServiceApiTests();
            var siteApiTests = new SiteApiTests();

            await LogTest("Employees", "CreateEmployee_ValidData_ShouldReturnCreatedEmployee", async () =>
            {
                var employeeId = await CreateEmployee(serviceApiTests: serviceApiTests, siteApiTests: siteApiTests);
                Assert.True(employeeId > 0);
            });
        }

        [Fact]
        public async Task CreateEmployee_MissingRequiredFields_ShouldReturnBadRequest()
        {
            await LogTest("Employees", "CreateEmployee_MissingRequiredFields_ShouldReturnBadRequest", async () =>
            {
                var request = new RestRequest("Employees", Method.Post)
                    .AddJsonBody(new { });

                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            });
        }

        [Fact]
        public async Task CreateEmployee_InvalidEmailFormat_ShouldReturnBadRequest()
        {
            var serviceApiTests = new ServiceApiTests();
            var siteApiTests = new SiteApiTests();

            await LogTest("Employees", "CreateEmployee_InvalidEmailFormat_ShouldReturnBadRequest", async () =>
            {
                var request = new RestRequest("Employees", Method.Post)
                    .AddJsonBody(new
                    {
                        firstName = "John",
                        lastName = "Doe",
                        phoneFixed = "0123456789",
                        phoneMobile = "0612345678",
                        email = "invalid-email",
                        serviceId = await serviceApiTests.CreateService(),
                        siteId = await siteApiTests.CreateSite()
                    });

                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            });
        }

        [Fact]
        public async Task CreateEmployee_DuplicateEmail_ShouldReturnConflict()
        {
            var serviceApiTests = new ServiceApiTests();
            var siteApiTests = new SiteApiTests();
            var email = $"duplicate{Guid.NewGuid()}@example.com";

            await CreateEmployee(email: email, serviceApiTests: serviceApiTests, siteApiTests: siteApiTests);

            var request = new RestRequest("Employees", Method.Post)
                .AddJsonBody(new
                {
                    firstName = "John",
                    lastName = "Doe",
                    phoneFixed = "0123456789",
                    phoneMobile = "0612345678",
                    email,
                    serviceId = await serviceApiTests.CreateService(),
                    siteId = await siteApiTests.CreateSite()
                });

            var response = await Client.ExecuteAsync(request);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task GetEmployeeById_ValidId_ShouldReturnEmployee()
        {
            var serviceApiTests = new ServiceApiTests();
            var siteApiTests = new SiteApiTests();

            await LogTest("Employees/{id}", "GetEmployeeById_ValidId_ShouldReturnEmployee", async () =>
            {
                var employeeId = await CreateEmployee(serviceApiTests: serviceApiTests, siteApiTests: siteApiTests);

                var request = new RestRequest($"Employees/{employeeId}", Method.Get);
                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var content = JObject.Parse(response.Content!);
                Assert.Equal(employeeId, (int)content["id"]!);
            });
        }

        [Fact]
        public async Task DeleteEmployee_ValidId_ShouldReturnSuccess()
        {
            var serviceApiTests = new ServiceApiTests();
            var siteApiTests = new SiteApiTests();

            await LogTest("Employees/{id}", "DeleteEmployee_ValidId_ShouldReturnSuccess", async () =>
            {
                var employeeId = await CreateEmployee(serviceApiTests: serviceApiTests, siteApiTests: siteApiTests);

                var request = new RestRequest($"Employees/{employeeId}", Method.Delete);
                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            });
        }

        [Fact]
        public async Task GetEmployees_WithSelectedFields_ShouldReturnOnlySpecifiedFields()
        {
            var serviceApiTests = new ServiceApiTests();
            var siteApiTests = new SiteApiTests();

            // Vérifier si un employé avec les champs spécifiés existe déjà
            var existsRequest = new RestRequest("Employees", Method.Get)
                .AddQueryParameter("fields", "firstName,email")
                .AddQueryParameter("pageSize", "100"); // Récupérer suffisamment d'éléments pour vérifier

            var existsResponse = await Client.ExecuteAsync(existsRequest);
            Assert.Equal(HttpStatusCode.OK, existsResponse.StatusCode);

            var existingContent = JObject.Parse(existsResponse.Content!);
            var employees = existingContent["employees"]?.ToObject<List<JObject>>();

            if (employees == null || !employees.Any(e => e["firstName"]?.ToString() != null && e["email"]?.ToString() != null))
            {
                // Ajouter un employé si aucun n'existe avec les champs spécifiés
                await CreateEmployee(serviceApiTests: serviceApiTests, siteApiTests: siteApiTests);
            }

            // Requête avec le filtre de champ "firstName" et "email"
            var request = new RestRequest("Employees", Method.Get)
                .AddQueryParameter("fields", "firstName,email");

            var response = await Client.ExecuteAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = JObject.Parse(response.Content!);
            foreach (var employee in content["employees"]!)
            {
                Assert.NotNull(employee["FirstName"]); // Vérifie que "firstName" est présent
                Assert.NotNull(employee["Email"]); // Vérifie que "email" est présent
                Assert.Null(employee["LastName"]); // Vérifie que "lastName" est absent
            }
        }

        [Fact]
        public async Task CreateEmployee_WithoutFirstName_ShouldReturnBadRequest()
        {
            var serviceApiTests = new ServiceApiTests();
            var siteApiTests = new SiteApiTests();

            var request = new RestRequest("Employees", Method.Post)
                .AddJsonBody(new
                {
                    lastName = "Doe",
                    phoneFixed = "0123456789",
                    phoneMobile = "0612345678",
                    email = $"test{Guid.NewGuid()}@example.com",
                    serviceId = await serviceApiTests.CreateService(),
                    siteId = await siteApiTests.CreateSite()
                });

            var response = await Client.ExecuteAsync(request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetEmployee_ByNonExistentId_ShouldReturnNotFound()
        {
            var request = new RestRequest($"Employees/{int.MaxValue}", Method.Get);
            var response = await Client.ExecuteAsync(request);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteEmployee_ByNonExistentId_ShouldReturnNotFound()
        {
            var request = new RestRequest($"Employees/{int.MaxValue}", Method.Delete);
            var response = await Client.ExecuteAsync(request);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateEmployee_WithMaxLengthFields_ShouldSucceed()
        {
            var serviceApiTests = new ServiceApiTests();
            var siteApiTests = new SiteApiTests();

            var firstName = new string('A', 50); // Longueur maximale
            var lastName = new string('B', 50);
            var email = $"test{Guid.NewGuid()}@example.com";

            var employeeId = await new EmployeeApiTests()
                .CreateEmployee(firstName: firstName, lastName: lastName, email: email, serviceApiTests: serviceApiTests, siteApiTests: siteApiTests);

            Assert.True(employeeId > 0);
        }

        [Fact]
        public async Task GetEmployees_WithPagination_ShouldReturnLimitedResults()
        {
            var serviceApiTests = new ServiceApiTests();
            var siteApiTests = new SiteApiTests();

            // Ajouter 5 employés
            for (int i = 0; i < 5; i++)
            {
                await CreateEmployee(serviceApiTests: serviceApiTests, siteApiTests: siteApiTests);
            }

            // Récupérer uniquement 4 employés via la pagination
            var request = new RestRequest("Employees", Method.Get)
                .AddQueryParameter("pageSize", "4")
                .AddQueryParameter("pageNumber", "1");

            var response = await Client.ExecuteAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = JObject.Parse(response.Content!);
            var employees = content["employees"];
            Assert.NotNull(employees);
            Assert.Equal(4, employees!.Count()); // Vérifie que seulement 4 employés sont retournés
        }

    }
}