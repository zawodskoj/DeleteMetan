using ZaborPokraste.API.Models.Auth;

namespace ZaborPokraste.API.Actions.Auth
{
    public class RegisterPost : APIAction<RegisterDto>
    {
        public RegisterPost(RegisterDto model) : base("/raceapi/Auth/Register", model)
        {
        }
    }
}