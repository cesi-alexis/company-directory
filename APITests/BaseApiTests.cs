using RestSharp;

namespace APITests
{
    public abstract class BaseApiTests : TestBase
    {
        protected readonly RestClient Client;

        protected BaseApiTests()
        {
            Client = new RestClient(new RestClientOptions
            {
                BaseUrl = new Uri("http://localhost:7055/api")
            });
            TestLogger.InitializeLog();
        }
    }
}