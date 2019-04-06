using ZaborPokraste.API.Models.Enums;

namespace ZaborPokraste.API.Models.Auth
{
    public class RegisterDto
    {
        public RegisterDto(string login, string password, Side side)
        {
            Login = login;
            Password = password;
            Side = side;
        }

        public string Login { get; set; }
        public string Password { get; set; }
        
        public Side Side { get; set; }
    }
}