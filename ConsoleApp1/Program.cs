using System;
using System.Net.Http;
using ZaborPokraste.API.Actions.Auth;
using ZaborPokraste.API.Models.Auth;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var token = "";
            var HttpClient = new HttpClient();

            var resp = new LoginPost(new LoginDto())
                .Dispatch(HttpClient).GetAwaiter().GetResult();

            token = resp.Token;
            
            Console.WriteLine($"Мама я получил ебучий токен сука: {token}");
        }
    }
}