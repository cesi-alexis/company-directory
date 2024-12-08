namespace CompanyDirectory.API.Common.Exceptions
{
    public class NotFoundException(string message) : Exception(message)
    {
    }
}