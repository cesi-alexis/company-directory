using CompanyDirectory.Models.Entities;
using CompanyDirectory.Models.ViewsModels.Requests;
using CompanyDirectory.SDK.Interfaces;
using CompanyDirectory.SDK.Services;

namespace CompanyDirectory.Tests.SDK
{
    /// <summary>
    /// Tests pour le service SDK de gestion des services.
    /// </summary>
    public class ServiceSdkTests : BaseSdkTests<IServiceClient<Service, ServiceUpsertRequestViewModel>, Service, ServiceUpsertRequestViewModel>
    {
        protected override IServiceClient<Service, ServiceUpsertRequestViewModel> ServiceClient { get; }

        public ServiceSdkTests()
        {
            ServiceClient = new ServiceServiceClient(new HttpClient { BaseAddress = new Uri("http://localhost:7055") });
        }

        protected override ServiceUpsertRequestViewModel CreateValidViewModel()
        {
            return new ServiceUpsertRequestViewModel
            {
                Name = $"Service-{Guid.NewGuid():N}"
            };
        }

        protected override int GetId(Service service)
        {
            return service.Id;
        }

        protected override ServiceUpsertRequestViewModel CreateInvalidViewModel()
        {
            return new ServiceUpsertRequestViewModel
            {
                Name = string.Empty // Nom vide (invalide)
            };
        }

        protected override string GetUniqueIdentifier(Service service)
        {
            return service.Name;
        }
    }
}