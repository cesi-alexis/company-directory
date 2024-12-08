using Microsoft.AspNetCore.Mvc;

namespace CompanyDirectory.API.Validation
{
    public class ServiceFilterWithLogger<T> : TypeFilterAttribute where T : class
    {
        public ServiceFilterWithLogger() : base(typeof(T))
        {
        }
    }
}