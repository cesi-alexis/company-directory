using System.Net;
using Newtonsoft.Json.Linq;
using RestSharp;
using Xunit;

namespace APITests
{
    public class SiteApiTests : BaseApiTests
    {
        public SiteApiTests() : base() { }

        /// <summary>
        /// Crée un nouveau site pour les tests.
        /// </summary>
        /// <returns>ID du site créé.</returns>
        public async Task<int> CreateSite(string? city = null)
        {
            city ??= $"TestCity-{Guid.NewGuid():N}".Replace("0", "O").Replace("1", "I");

            var request = CreateRequest("Site", Method.Post, new { city });

            var response = await Client.ExecuteAsync(request);
            Assert.True(response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK);

            var content = JObject.Parse(response.Content!);
            var id = content["value"]?["id"];
            Assert.NotNull(id);

            return (int)id!;
        }

        [Fact]
        public async Task GetAllSites_ShouldReturnSites()
        {
            await LogTest("Sites", "GetAllSites_ShouldReturnSites", async () =>
            {
                var request = CreateRequest("site", Method.Get);
                var response = await Client.ExecuteAsync(request);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var content = JObject.Parse(response.Content!);
                Assert.NotNull(content); // Vérifie que le contenu n'est pas nul

                if (content["sites"] is JArray sites)
                {
                    Assert.True(sites.Count >= 0, "The list of sites may be empty but should not cause an error.\n" + content);
                }
                else
                {
                    throw new Xunit.Sdk.XunitException("The 'Sites' property is not present or is not a valid JSON array.\n" + content);
                }
            });
        }

        [Fact]
        public async Task GetSiteById_ValidId_ShouldReturnSite()
        {
            await LogTest("Site/{id}", "GetSiteById_ValidId_ShouldReturnSite", async () =>
            {
                var siteId = await CreateSite();
                var request = CreateRequest($"Site/{siteId}", Method.Get);
                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var content = JObject.Parse(response.Content!);
                Assert.Equal(siteId, (int)content["id"]!);
            });
        }

        [Fact]
        public async Task GetSiteById_InvalidId_ShouldReturnNotFound()
        {
            await LogTest("Site/{id}", "GetSiteById_InvalidId_ShouldReturnNotFound", async () =>
            {
                var request = CreateRequest($"Site/{int.MaxValue}", Method.Get);
                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            });
        }

        [Fact]
        public async Task CreateSite_WithMissingField_ShouldReturnBadRequest()
        {
            await LogTest("Site", "CreateSite_WithMissingField_ShouldReturnBadRequest", async () =>
            {
                var request = CreateRequest("Site", Method.Post, new { });
                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            });
        }

        [Fact]
        public async Task CreateSite_WithDuplicateCity_ShouldReturnConflict()
        {
            var city = $"DuplicateCity-{Guid.NewGuid():N}";
            await CreateSite(city);

            var request = CreateRequest("Site", Method.Post, new { city });
            var response = await Client.ExecuteAsync(request);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task UpdateSite_ValidData_ShouldReturnOk()
        {
            await LogTest("Site/{id}", "UpdateSite_ValidData_ShouldReturnOk", async () =>
            {
                var siteId = await CreateSite();
                var request = CreateRequest($"Site/{siteId}", Method.Put, new
                {
                    id = siteId,
                    city = $"UpdatedCity-{Guid.NewGuid():N}"
                });

                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            });
        }

        [Fact]
        public async Task UpdateSite_InvalidId_ShouldReturnNotFound()
        {
            await LogTest("Site/{id}", "UpdateSite_InvalidId_ShouldReturnNotFound", async () =>
            {
                var request = CreateRequest($"Site/{int.MaxValue}", Method.Put, new
                {
                    id = int.MaxValue,
                    city = $"NonExistentCity-{Guid.NewGuid():N}"
                });

                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            });
        }

        [Fact]
        public async Task DeleteSite_ValidId_ShouldReturnOK()
        {
            await LogTest("Site/{id}", "DeleteSite_ValidId_ShouldReturnOK", async () =>
            {
                var siteId = await CreateSite();
                var request = CreateRequest($"Site/{siteId}", Method.Delete);

                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            });
        }

        [Fact]
        public async Task DeleteSite_InvalidId_ShouldReturnNotFound()
        {
            await LogTest("Site/{id}", "DeleteSite_InvalidId_ShouldReturnNotFound", async () =>
            {
                var request = CreateRequest($"Site/{int.MaxValue}", Method.Delete);
                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            });
        }

        [Fact]
        public async Task GetSites_WithFieldFilter_ShouldReturnSpecifiedFieldsOnly()
        {
            var siteApiTests = new SiteApiTests();

            // Vérifier si "TestCity" existe déjà
            var existsRequest = new RestRequest("Site/exists/TestCity", Method.Get);
            var existsResponse = await Client.ExecuteAsync(existsRequest);

            if (existsResponse.StatusCode == HttpStatusCode.NotFound)
            {
                // Ajouter "TestCity" si elle n'existe pas
                await siteApiTests.CreateSite(city: "TestCity");
            }

            // Requête avec le filtre de champ "city"
            var request = new RestRequest("Site", Method.Get)
                .AddQueryParameter("fields", "city");

            var response = await Client.ExecuteAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = JObject.Parse(response.Content!);
            Assert.NotNull(content["sites"]);

            foreach (var site in content["sites"]!)
            {
                Assert.NotNull(site["City"]); // Vérifie que la propriété "city" est présente
                Assert.Null(site["Id"]); // Vérifie que "id" n'est pas présent (non demandé)
            }
        }

        [Fact]
        public async Task DeleteSite_WithLinkedEmployees_ShouldReturnConflict()
        {
            var siteId = await CreateSite();
            var serviceApiTests = new ServiceApiTests();
            var employeeApiTests = new EmployeeApiTests();

            // Crée un employé lié au site
            await employeeApiTests.CreateEmployee(siteId: siteId, siteApiTests: this, serviceApiTests: serviceApiTests);

            // Tente de supprimer le site
            var request = new RestRequest($"Site/{siteId}", Method.Delete);
            var response = await Client.ExecuteAsync(request);

            // Vérifie que le statut HTTP est `Conflict`
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task DeleteSite_WithoutLinkedEmployees_ShouldReturnOK()
        {
            var siteId = await CreateSite();

            var request = new RestRequest($"Site/{siteId}", Method.Delete);
            var response = await Client.ExecuteAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetSites_WithPagination_ShouldReturnLimitedResults()
        {
            var siteApiTests = new SiteApiTests();

            // Ajouter 5 sites
            //for (int i = 0; i < 5; i++)
            //{
            //    await siteApiTests.CreateSite(city: $"TestCity-{i}");
            //}

            // Récupérer uniquement 4 sites via la pagination
            var request = new RestRequest("Site", Method.Get)
                .AddQueryParameter("pageSize", "4")
                .AddQueryParameter("pageNumber", "1");

            var response = await Client.ExecuteAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = JObject.Parse(response.Content!);
            var sites = content["sites"];
            Assert.NotNull(sites);
            Assert.Equal(4, sites!.Count()); // Vérifie que seulement 4 sites sont retournés
        }

        [Fact]
        public async Task SiteExists_ShouldReturnTrueIfSiteExists()
        {
            var siteApiTests = new SiteApiTests();
            var city = "TestCity";

            // Assurez-vous que la ville existe
            var request = new RestRequest($"Site/exists/{city}", Method.Get);
            var response = await Client.ExecuteAsync(request);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                await siteApiTests.CreateSite(city);
            }

            request = new RestRequest($"Site/exists/{city}", Method.Get);
            response = await Client.ExecuteAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SiteExists_ShouldReturnFalseIfSiteDoesNotExist()
        {
            var city = "NonExistentCity";

            var request = new RestRequest($"Site/exists/{city}", Method.Get);
            var response = await Client.ExecuteAsync(request);

            Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

    }
}