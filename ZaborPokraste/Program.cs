using System;
using System.Threading.Tasks;
using ZaborPokraste.API.Client;
using ZaborPokraste.Pathfinding;

namespace ZaborPokraste
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var apiKlient = new ApiClient();
            var kar = await Car.CreateClient();
            
            await kar.EventLoop();
        }
    }
}