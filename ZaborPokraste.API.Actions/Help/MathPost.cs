using System.Net.Http;
using System.Threading.Tasks;

namespace ZaborPokraste.API.Actions.Help
{
    public class MathPost : APIAction<object>
    {
        public MathPost(string urlEndpoint, object model) : base(urlEndpoint, model)
        {
        }
    }
}