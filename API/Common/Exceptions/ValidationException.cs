﻿namespace CompanyDirectory.API.Common.Exceptions
{
    public class ValidationException(string message) : Exception(message)
    {
    }
}