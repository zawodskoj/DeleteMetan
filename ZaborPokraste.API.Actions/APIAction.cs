using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ZaborPokraste.API.Models.Service;

namespace ZaborPokraste.API.Actions
{
    public abstract class APIAction<T, TRes>
    {
        private const string ApiPath = "http://51.15.100.12:5000";
        protected string UrlEndpoint;
        protected T Model;

        protected APIAction(string urlEndpoint, T model)
        {
            UrlEndpoint = urlEndpoint;
            Model = model;
        }


        public async Task<TRes> Dispatch(HttpClient client)
        {
            var httpResponse = await client.PostAsync(
                $"{ApiPath}{UrlEndpoint}",
                new StringContent(JsonConvert.SerializeObject(Model), Encoding.UTF8, "application/json")
            );
            
            var result = await httpResponse
                .Content
                .ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<TRes>(result);

            if (response != null)
            {
                return response;
            }
            
            var apiError = JsonConvert.DeserializeObject<ErrorMessage>(result);
            throw new ApiErrorException(apiError);
        }
    }
    
    public abstract class APIAction<T>
    {
        private const string ApiPath = "http://51.15.100.12:5000";
        protected string UrlEndpoint;
        protected T Model;

        protected APIAction(string urlEndpoint, T model)
        {
            UrlEndpoint = urlEndpoint;
            Model = model;
        }

        public async Task Dispatch(HttpClient client)
        {
            var httpResponse = await client.PostAsync(
                $"{ApiPath}{UrlEndpoint}",
                new StringContent(JsonConvert.SerializeObject(Model), Encoding.UTF8, "application/json")
            );
            
            var result = await httpResponse
                .Content
                .ReadAsStringAsync();
            var apiError = JsonConvert.DeserializeObject<ErrorMessage>(result);

            if (apiError != null)
            {
                throw new ApiErrorException(apiError);

            }
        }
    }
}