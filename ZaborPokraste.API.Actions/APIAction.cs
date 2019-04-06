using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ZaborPokraste.API.Models.Service;

namespace ZaborPokraste.API.Actions
{
    public abstract class APIAction<T, TRes>
    {
        //private const string ApiPath = "http://51.15.100.12:5000";
        private const string ApiPath = "http://51.158.109.80:5000";
        protected string UrlEndpoint;
        protected T Model;

        protected APIAction(string urlEndpoint, T model)
        {
            UrlEndpoint = urlEndpoint;
            Model = model;
        }

        
        protected abstract HttpMethod Method { get; }

        public async Task<TRes> Dispatch(HttpClient client)
        {
            var httpResponse = await client.SendAsync(
                new HttpRequestMessage(Method, $"{ApiPath}{UrlEndpoint}")
                {
                    Content = Method == HttpMethod.Get ? null : new StringContent(JsonConvert.SerializeObject(Model), Encoding.UTF8, "application/json")
                }
            );
            
            var result = await httpResponse
                .Content
                .ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<TRes>(result);

            Console.WriteLine(result);
            if (response != null)
            {
                return response;
            }
            
            var apiError = JsonConvert.DeserializeObject<ErrorMessage>(result);
            throw new ApiErrorException(apiError);
        }
    }
    
    public abstract class APIActionPut<T, TRes>
    {
        private const string ApiPath = "http://51.158.109.80:5000";
        protected string UrlEndpoint;
        protected T Model;

        protected APIActionPut(string urlEndpoint, T model)
        {
            UrlEndpoint = urlEndpoint;
            Model = model;
        }


        public async Task<TRes> Dispatch(HttpClient client)
        {
            var httpResponse = await client.PutAsync(
                $"{ApiPath}{UrlEndpoint}",
                new StringContent(JsonConvert.SerializeObject(Model), Encoding.UTF8, "application/json")
            );
            
            var result = await httpResponse
                .Content
                .ReadAsStringAsync();
            
            Console.WriteLine(result);
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
        private const string ApiPath = "http://51.158.109.80:5000";
        protected string UrlEndpoint;
        protected T Model;

        
        protected abstract HttpMethod Method { get; }
        
        protected APIAction(string urlEndpoint, T model)
        {
            UrlEndpoint = urlEndpoint;
            Model = model;
        }

        public async Task Dispatch(HttpClient client)
        {
            var httpResponse = await client.SendAsync(
                new HttpRequestMessage(Method, $"{ApiPath}{UrlEndpoint}")
                {
                    Content = new StringContent(JsonConvert.SerializeObject(Model), Encoding.UTF8, "application/json")
                }
            );
            
            var result = await httpResponse
                .Content
                .ReadAsStringAsync();
            Console.WriteLine(result);
            var apiError = JsonConvert.DeserializeObject<ErrorMessage>(result);

            if (apiError != null)
            {
                throw new ApiErrorException(apiError);

            }
        }
    }
}