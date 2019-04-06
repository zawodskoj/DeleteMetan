using System.Net.Http;
using System.Threading.Tasks;
using ZaborPokraste.API.Models.Actions;

namespace ZaborPokraste.API.Actions.Race
{
    /// <summary>
    /// Set new state to race
    /// </summary>
    public class RacePut : APIAction<TurnModel, TurnResult>
    {
        public RacePut(string sessionId, TurnModel model) : base($"/raceapi/race/{sessionId}", model)
        {
        }
    }
}