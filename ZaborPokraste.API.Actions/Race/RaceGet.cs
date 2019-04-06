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
        public RaceGet(string sessionId) : 
            base($"/raceapi/race?sessionId={sessionId}", null)
        {
        }

        protected override HttpMethod Method => HttpMethod.Get;
    }
}