namespace ZaborPokraste.API.Models.Auth
{
    public class LoginDto
    {
        public LoginDto(string username = null, string password = null)
        {
            Login = username ?? ".NET_Lewd_Community";
            Password = password ?? "dXgPq9";
        }

        public string Login { get; set; }
        public string Password { get; set; }
    }
}