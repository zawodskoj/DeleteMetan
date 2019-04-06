using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ZaborPokraste.API.Models.Auth;

namespace ZaborPokraste.API.Actions.Auth
{
    public class LoginPost : APIAction<LoginDto, TokenDto>
    {
        public LoginPost(LoginDto model) : base("/raceapi/Auth/Login", model)
        {
        }

        protected override HttpMethod Method => HttpMethod.Post;
    }
}