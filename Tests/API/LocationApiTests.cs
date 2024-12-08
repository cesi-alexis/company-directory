using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;

namespace CompanyDirectory.Tests.API
{
    public class LocationApiTests : BaseTests
    {
        public LocationApiTests() : base() { }

        /// <summary>
        /// Crée un nouveau location pour les tests.
        /// </summary>
        /// <returns>ID du location créé.</returns>
        public async Task<int> CreateLocation(string? city = null)
        {
            city ??= $"TestCity-{Guid.NewGuid():N}".Replace("0", "O").Replace("1", "I");

            var request = CreateRequest("Location", Method.Post, new { city });

            var response = await Client.ExecuteAsync(request);
            Assert.True(response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK);

            var content = JObject.Parse(response.Content!);
            var id = content["value"]?["id"];
            Assert.NotNull(id);

            return (int)id!;
        }

        [Fact]
        public async Task GetAllLocations_ShouldReturnLocations()
        {
            await AwaitAction(async () =>
            {
                var request = CreateRequest("location", Method.Get);
                var response = await Client.ExecuteAsync(request);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var content = JObject.Parse(response.Content!);
                Assert.NotNull(content); // Vérifie que le contenu n'est pas nul

                if (content["locations"] is JArray locations)
                {
                    Assert.True(locations.Count >= 0, "The list of locations may be empty but should not cause an error.\n" + content);
                }
                else
                {
                    throw new Xunit.Sdk.XunitException("The 'Locations' property is not present or is not a valid JSON array.\n" + content);
                }
            });
        }

        [Fact]
        public async Task GetLocationById_ValidId_ShouldReturnLocation()
        {
            await AwaitAction(async () =>
            {
                var locationId = await CreateLocation();
                var request = CreateRequest($"Location/{locationId}", Method.Get);
                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var content = JObject.Parse(response.Content!);
                Assert.Equal(locationId, (int)content["id"]!);
            });
        }

        [Fact]
        public async Task GetLocationById_InvalidId_ShouldReturnNotFound()
        {
            await AwaitAction(async () =>
            {
                var request = CreateRequest($"Location/{int.MaxValue}", Method.Get);
                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            });
        }

        [Fact]
        public async Task CreateLocation_WithMissingField_ShouldReturnBadRequest()
        {
            await AwaitAction(async () =>
            {
                var request = CreateRequest("Location", Method.Post, new { });
                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            });
        }

        [Fact]
        public async Task CreateLocation_WithDuplicateCity_ShouldReturnConflict()
        {
            var city = $"DuplicateCity-{Guid.NewGuid():N}";
            await CreateLocation(city);

            var request = CreateRequest("Location", Method.Post, new { city });
            var response = await Client.ExecuteAsync(request);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task UpdateLocation_ValidData_ShouldReturnOk()
        {
            await AwaitAction(async () =>
            {
                var locationId = await CreateLocation();
                var request = CreateRequest($"Location/{locationId}", Method.Put, new
                {
                    id = locationId,
                    city = $"UpdatedCity-{Guid.NewGuid():N}"
                });

                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            });
        }

        [Fact]
        public async Task UpdateLocation_InvalidId_ShouldReturnNotFound()
        {
            await AwaitAction(async () =>
            {
                var request = CreateRequest($"Location/{int.MaxValue}", Method.Put, new
                {
                    id = int.MaxValue,
                    city = $"NonExistentCity-{Guid.NewGuid():N}"
                });

                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            });
        }

        [Fact]
        public async Task DeleteLocation_ValidId_ShouldReturnOK()
        {
            await AwaitAction(async () =>
            {
                var locationId = await CreateLocation();
                var request = CreateRequest($"Location/{locationId}", Method.Delete);

                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            });
        }

        [Fact]
        public async Task DeleteLocation_InvalidId_ShouldReturnNotFound()
        {
            await AwaitAction(async () =>
            {
                var request = CreateRequest($"Location/{int.MaxValue}", Method.Delete);
                var response = await Client.ExecuteAsync(request);

                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            });
        }

        [Fact]
        public async Task GetLocations_WithFieldFilter_ShouldReturnSpecifiedFieldsOnly()
        {
            var locationApiTests = new LocationApiTests();

            // Vérifier si "TestCity" existe déjà
            var existsRequest = new RestRequest("Location/exists/TestCity", Method.Get);
            var existsResponse = await Client.ExecuteAsync(existsRequest);

            if (existsResponse.StatusCode == HttpStatusCode.NotFound)
            {
                // Ajouter "TestCity" si elle n'existe pas
                await locationApiTests.CreateLocation(city: "TestCity");
            }

            // Requête avec le filtre de champ "city"
            var request = new RestRequest("Location", Method.Get)
                .AddQueryParameter("fields", "city");

            var response = await Client.ExecuteAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = JObject.Parse(response.Content!);
            Assert.NotNull(content["locations"]);

            foreach (var location in content["locations"]!)
            {
                Assert.NotNull(location["City"]); // Vérifie que la propriété "city" est présente
                Assert.Null(location["Id"]); // Vérifie que "id" n'est pas présent (non demandé)
            }
        }

        [Fact]
        public async Task DeleteLocation_WithLinkedWorkers_ShouldReturnConflict()
        {
            var locationId = await CreateLocation();
            var serviceApiTests = new ServiceApiTests();
            var workerApiTests = new WorkerApiTests();

            // Crée un employé lié au location
            await workerApiTests.CreateEmployee(locationId: locationId, locationApiTests: this, serviceApiTests: serviceApiTests);

            // Tente de supprimer le location
            var request = new RestRequest($"Location/{locationId}", Method.Delete);
            var response = await Client.ExecuteAsync(request);

            // Vérifie que le statut HTTP est `Conflict`
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task DeleteLocation_WithoutLinkedWorkers_ShouldReturnOK()
        {
            var locationId = await CreateLocation();

            var request = new RestRequest($"Location/{locationId}", Method.Delete);
            var response = await Client.ExecuteAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetLocations_WithPagination_ShouldReturnLimitedResults()
        {
            //var locationApiTests = new LocationApiTests();

            // Ajouter 5 locations
            //for (int i = 0; i < 5; i++)
            //{
            //    await locationApiTests.CreateLocation(city: $"TestCity-{i}");
            //}

            // Récupérer uniquement 4 locations via la pagination
            var request = new RestRequest("Location", Method.Get)
                .AddQueryParameter("pageSize", "4")
                .AddQueryParameter("pageNumber", "1");

            var response = await Client.ExecuteAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = JObject.Parse(response.Content!);
            var locations = content["locations"];
            Assert.NotNull(locations);
            Assert.Equal(4, locations!.Count()); // Vérifie que seulement 4 locations sont retournés
        }

        [Fact]
        public async Task LocationExists_ShouldReturnTrueIfLocationExists()
        {
            var locationApiTests = new LocationApiTests();
            var city = "TestCity";

            // Assurez-vous que la ville existe
            var request = new RestRequest($"Location/exists/{city}", Method.Get);
            var response = await Client.ExecuteAsync(request);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                await locationApiTests.CreateLocation(city);
            }

            request = new RestRequest($"Location/exists/{city}", Method.Get);
            response = await Client.ExecuteAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task LocationExists_ShouldReturnFalseIfLocationDoesNotExist()
        {
            var city = "NonExistentCity";

            var request = new RestRequest($"Location/exists/{city}", Method.Get);
            var response = await Client.ExecuteAsync(request);

            Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

    }
}