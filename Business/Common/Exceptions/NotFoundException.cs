﻿namespace CompanyDirectory.Common.Exceptions
{
    public class NotFoundException(string message) : Exception(message)
    {
    }
}