using ZaborPokraste.API.Models.Enums;

namespace ZaborPokraste.API.Models.Actions
{
    public class TurnModel
    {
        public TurnModel(Direction direction, int acceleration)
        {
            Direction = direction;
            Acceleration = acceleration;
        }

        public Direction Direction { get; set; }
        public int Acceleration { get; set; }
    }
}