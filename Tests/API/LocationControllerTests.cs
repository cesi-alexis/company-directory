using CompanyDirectory.API.Controllers;
using CompanyDirectory.Models.Entities;

namespace CompanyDirectory.Tests.API
{
    public class LocationControllerTests : BaseControllerTests<LocationController, Location>
    {
        protected override string Endpoint => "api/Location";
        protected override string[] Fields => [nameof(Location.City)];

        protected override Location CreateValidModel()
        {
            return new Location
            {
                City = $"{Guid.NewGuid():N}"
            };
        }

        protected override Location CreateInvalidModel()
        {
            return new Location
            {
                City = "" // Champ invalide
            };
        }

        protected override Location CreateModelWithIdentifier(string identifier)
        {
            return new Location
            {
                City = $"{identifier}"
            };
        }

        protected override int GetIdFromModel(Location model) => model.Id;

        protected override string GetIdentifier(string identifier)
        {
            return identifier;
        }

        protected override bool Equals(Location model, Location expected)
        {
            return model.City == expected.City;
        }
    }
}