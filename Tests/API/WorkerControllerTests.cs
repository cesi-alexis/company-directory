using CompanyDirectory.API.Controllers;
using CompanyDirectory.Models.Entities;
using System.Net;
using System.Net.Http.Json;

namespace CompanyDirectory.Tests.API
{
    public class WorkerControllerTests : BaseControllerTests<WorkerController, Worker>
    {
        protected override string Endpoint => "api/Worker";
        protected override string[] Fields => [nameof(Worker.FirstName), nameof(Worker.LastName), nameof(Worker.Email), nameof(Worker.ServiceId), nameof(Worker.LocationId)];

        protected override Worker CreateValidModel()
        {
            // Dépendances nécessaires pour les services et localisations
            var serviceTests = new ServiceControllerTests();
            var locationTests = new LocationControllerTests();

            var serviceId = serviceTests.CreateAndReturnIdAsync().Result;
            var locationId = locationTests.CreateAndReturnIdAsync().Result;

            return new Worker
            {
                FirstName = "John",
                LastName = "Doe",
                Email = $"john.doe{Guid.NewGuid():N}@example.com",
                PhoneFixed = "0123456789",
                PhoneMobile = "0612345678",
                ServiceId = serviceId,
                LocationId = locationId
            };
        }

        protected override Worker CreateInvalidModel()
        {
            return new Worker
            {
                FirstName = "", // Champ invalide
                LastName = "Doe",
                Email = "invalid-email",
                PhoneFixed = "123",
                PhoneMobile = "456"
            };
        }
        protected override Worker CreateModelWithIdentifier(string identifier)
        {
            var serviceTests = new ServiceControllerTests();
            var locationTests = new LocationControllerTests();

            var serviceId = serviceTests.CreateAndReturnIdAsync().Result;
            var locationId = locationTests.CreateAndReturnIdAsync().Result;

            return new Worker
            {
                FirstName = $"{identifier}",
                LastName = $"{identifier}",
                Email = $"{identifier}",
                PhoneFixed = "0123456789",
                PhoneMobile = "0612345678",
                ServiceId = serviceId,
                LocationId = locationId
            };
        }

        protected override int GetIdFromModel(Worker model) => model.Id;

        protected override string GetIdentifier(string identifier)
        {
            return identifier + "@example.com";
        }

        /// <summary>
        /// Teste le transfert de plusieurs employés avec des données valides.
        /// </summary>
        [Fact]
        public async Task TransferWorkers_ValidData_ShouldSucceed()
        {
            var serviceTests = new ServiceControllerTests();
            var locationTests = new LocationControllerTests();

            await serviceTests.CreateAndReturnIdAsync();
            await locationTests.CreateAndReturnIdAsync();
            var newServiceId = await serviceTests.CreateAndReturnIdAsync();
            var newLocationId = await locationTests.CreateAndReturnIdAsync();

            var worker1Id = await CreateAndReturnIdAsync();
            var worker2Id = await CreateAndReturnIdAsync();

            var transferRequest = new
            {
                WorkerIds = new[] { worker1Id, worker2Id },
                NewServiceId = newServiceId,
                NewLocationId = newLocationId,
                AllowPartialTransfer = true
            };

            var response = await _client.PostAsJsonAsync($"{Endpoint}/transfer", transferRequest);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        protected override bool Equals(Worker model, Worker expected)
        {
            return model.FirstName == expected.FirstName &&
                   model.LastName == expected.LastName &&
                   model.Email == expected.Email &&
                   model.ServiceId == expected.ServiceId &&
                   model.LocationId == expected.LocationId;
        }
    }
}