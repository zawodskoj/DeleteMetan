using System;
using System.Net.Http;

namespace ZaborPokraste.API.Client
{
    public class ApiClient
    {
        private const string ApiPath = "http://51.15.100.12:5000";
        private readonly HttpClient _httpClient = new HttpClient();


        public TRes Fetch<T, TRes>(T model)
        {
            
        }
    }
}