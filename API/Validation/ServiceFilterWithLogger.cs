using Microsoft.AspNetCore.Mvc;

public class ServiceFilterWithLogger<T> : TypeFilterAttribute where T : class
{
    public ServiceFilterWithLogger() : base(typeof(T))
    {
    }
}