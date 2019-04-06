using System.Net.Http;
using System.Threading.Tasks;
using ZaborPokraste.API.Models.Actions;

namespace ZaborPokraste.API.Actions.Race
{
    /// <summary>
    /// Set new state to race
    /// </summary>
    public class RacePut : APIActionPut<TurnModel, TurnResult>
    {
        public RacePut(string sessionId, TurnModel model) : base($"/raceapi/race/{sessionId}", model)
        {
        }

        protected override HttpMethod Method => HttpMethod.Put;
    }
}