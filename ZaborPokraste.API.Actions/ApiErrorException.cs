using System;
using ZaborPokraste.API.Models.Service;

namespace ZaborPokraste.API.Actions
{
    public class ApiErrorException : Exception
    {
        public ApiErrorException(ErrorMessage serverError)
        {
            ServerError = serverError;
        }

        public ErrorMessage ServerError;
    }
}