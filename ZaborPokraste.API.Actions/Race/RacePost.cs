using System.Net.Http;
using System.Threading.Tasks;
using ZaborPokraste.API.Models.Game;

namespace ZaborPokraste.API.Actions.Race
{
    /// <summary>
    /// Creates or restart race
    /// </summary>
    public class RacePost : APIAction<CreateRaceDto, PlayerSessionInfo>
    {
        public RacePost(CreateRaceDto model) : base("/raceapi/race", model)
        {
        }
    }
}