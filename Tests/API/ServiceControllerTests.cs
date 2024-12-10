using CompanyDirectory.API.Controllers;
using CompanyDirectory.Models.Entities;

namespace CompanyDirectory.Tests.API
{
    public class ServiceControllerTests : BaseControllerTests<ServiceController, Service>
    {
        protected override string Endpoint => "api/Service";
        protected override string[] Fields => [nameof(Service.Name)];

        protected override Service CreateValidModel()
        {
            return new Service
            {
                Name = $"{Guid.NewGuid():N}"
            };
        }

        protected override Service CreateInvalidModel()
        {
            return new Service
            {
                Name = "" // Champ invalide
            };
        }
        protected override Service CreateModelWithIdentifier(string identifier)
        {
            return new Service
            {
                Name = $"{identifier}"
            };
        }

        protected override int GetIdFromModel(Service model) => model.Id;

        protected override string GetIdentifier(string identifier)
        {
            return identifier;
        }

        protected override bool Equals(Service model, Service expected)
        {
            return model.Name == expected.Name;
        }
    }
}