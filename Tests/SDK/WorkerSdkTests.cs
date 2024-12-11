using System.Net.Http.Json;
using System.Text.Json;
using CompanyDirectory.Models.Entities;
using CompanyDirectory.Models.ViewsModels.Requests;
using CompanyDirectory.Models.ViewsModels.Responses;
using CompanyDirectory.SDK.Factory;
using CompanyDirectory.SDK.Interfaces;
using CompanyDirectory.SDK.Services;

namespace CompanyDirectory.Tests.SDK
{
    /// <summary>
    /// Tests pour le service SDK de gestion des employés.
    /// </summary>
    public class WorkerSdkTests : BaseSdkTests<IWorkerServiceClient, Worker, WorkerUpsertRequestViewModel>
    {
        protected override IWorkerServiceClient ServiceClient { get; }
        private readonly IServiceClient<Service, ServiceUpsertRequestViewModel> _serviceClient;
        private readonly IServiceClient<Location, LocationUpsertRequestViewModel> _locationClient;

        /// <summary>
        /// Initialise une nouvelle instance de WorkerSdkTests avec les services requis.
        /// </summary>
        public WorkerSdkTests()
        {
            var baseAddress = new Uri("http://localhost:7055");
            ServiceClient = new WorkerServiceClient(new HttpClient { BaseAddress = baseAddress });
            _serviceClient = new ServiceServiceClient(new HttpClient { BaseAddress = baseAddress });
            _locationClient = new LocationServiceClient(new HttpClient { BaseAddress = baseAddress });
        }

        /// <summary>
        /// Crée un ViewModel valide pour tester la création ou la mise à jour.
        /// </summary>
        /// <returns>Un ViewModel valide.</returns>
        protected override WorkerUpsertRequestViewModel CreateValidViewModel()
        {
            return new WorkerUpsertRequestViewModel
            {
                FirstName = "John",
                LastName = "Doe",
                Email = $"john.doe{Guid.NewGuid():N}@example.com",
                PhoneFixed = "0123456789",
                PhoneMobile = "0612345678",
                ServiceId = 1, // Remplacer par un ID de service valide
                LocationId = 1 // Remplacer par un ID de localisation valide
            };
        }

        /// <summary>
        /// Crée un ViewModel invalide pour tester les échecs de validation.
        /// </summary>
        /// <returns>Un ViewModel invalide.</returns>
        protected override WorkerUpsertRequestViewModel CreateInvalidViewModel()
        {
            return new WorkerUpsertRequestViewModel
            {
                FirstName = string.Empty, // Champ requis vide
                LastName = string.Empty,
                Email = "invalid-email",
                PhoneFixed = "123", // Format incorrect
                PhoneMobile = "456" // Format incorrect
            };
        }

        /// <summary>
        /// Extrait l'identifiant unique d'un employé.
        /// </summary>
        /// <param name="worker">L'employé dont extraire l'identifiant.</param>
        /// <returns>L'identifiant de l'employé.</returns>
        protected override int GetId(Worker worker)
        {
            return worker.Id;
        }

        /// <summary>
        /// Teste le transfert d'employés avec des données valides.
        /// </summary>
        [Fact]
        public async Task TransferWorkers_ValidData_ShouldSucceed()
        {
            // Garantit qu'une localisation existe
            async Task<int> EnsureLocationExists(int locationId)
            {
                if (!await _locationClient.ExistsAsync(locationId))
                {
                    var location = await _locationClient.CreateAsync(new LocationUpsertRequestViewModel
                    {
                        City = $"City-{Guid.NewGuid():N}"
                    });
                    return location.Id;
                }
                return locationId;
            }

            // Garantit qu'un service existe
            async Task<int> EnsureServiceExists(int serviceId)
            {
                if (!await _serviceClient.ExistsAsync(serviceId))
                {
                    var service = await _serviceClient.CreateAsync(new ServiceUpsertRequestViewModel
                    {
                        Name = $"Service-{Guid.NewGuid():N}"
                    });
                    return service.Id;
                }
                return serviceId;
            }

            // Garantit qu'un employé existe
            async Task<Worker> EnsureWorkerExists(string firstName, string lastName, string email, int serviceId, int locationId)
            {
                if (!await ServiceClient.ExistsAsync(email))
                {
                    await ServiceClient.CreateAsync(new WorkerUpsertRequestViewModel
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Email = email,
                        PhoneFixed = "0123456789",
                        PhoneMobile = "0612345678",
                        ServiceId = serviceId,
                        LocationId = locationId
                    });
                }

                var query = new GetAllRequestViewModel
                {
                    SearchTerm = email,
                    PageNumber = 1,
                    PageSize = 1
                };

                // Appel à l'API en spécifiant le type Worker
                var response = await ServiceClient.GetAsync(query);

                var worker = response.Items.FirstOrDefault();

                if (worker == null)
                {
                    throw new InvalidOperationException("Worker not found after ensuring existence.");
                }

                return worker;
            }

            // Garantir que les localisations et services existent
            var locationId = await EnsureLocationExists(2);
            var serviceId = await EnsureServiceExists(2);

            // Créez ou récupérez les employés
            var worker1 = await EnsureWorkerExists("Alice", "Doe", "alice@example.com", serviceId, locationId);
            var worker2 = await EnsureWorkerExists("Bob", "Doe", "bob@example.com", serviceId, locationId);

            // Transférez-les
            var result = await ServiceClient.TransferWorkersAsync(new WorkerTransferViewModel
            {
                WorkerIds = new List<int> { worker1.Id, worker2.Id },
                NewLocationId = locationId,
                NewServiceId = serviceId,
                AllowPartialTransfer = true
            });

            Assert.NotNull(result);
            Assert.True(result.SuccessCount > 0);

            // Vérifiez que les employés ont bien été transférés
            foreach (var workerId in new[] { worker1.Id, worker2.Id })
            {
                var transferredWorker = await ServiceClient.GetAsync(GetViewModelFactory.CreateGetViewModel(workerId));
                Assert.Equal(locationId, transferredWorker.LocationId);
                Assert.Equal(serviceId, transferredWorker.ServiceId);
            }
        }

        protected override string GetUniqueIdentifier(Worker worker)
        {
            return worker.Email;
        }
    }
}