using System.Net.Http;
using System.Threading.Tasks;
using ZaborPokraste.API.Models.Game;

namespace ZaborPokraste.API.Actions.Race
{
    /// <summary>
    /// Info about current race
    /// </summary>
    public class RaceGet : APIAction<CreateRaceDto, PlayerSessionInfo>
    {
        public RaceGet(string sessionId, CreateRaceDto model) : 
            base($"/raceapi/race?sessionId={sessionId}", model)
        {
        }
    }
}