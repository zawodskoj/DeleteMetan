using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ZaborPokraste.API.Actions;
using ZaborPokraste.API.Actions.Auth;
using ZaborPokraste.API.Models.Auth;
using ZaborPokraste.API.Models.Service;

namespace ZaborPokraste.API.Client
{
    public class ApiClient
    {
        // private const string ApiPath = "http://51.15.100.12:5000";
        private const string ApiPath = "http://127.0.0.1:5000";
        private readonly HttpClient _httpClient = new HttpClient();
        private string Token;


    }
}