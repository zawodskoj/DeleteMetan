using ZaborPokraste.API.Models.Enums;
using ZaborPokraste.API.Models.Game;

namespace ZaborPokraste.API.Models.Actions
{
    public class TurnCommand
    {
        public Location Location { get; set; }
        public int Acceleration { get; set; }
        public Direction MovementDirection { get; set; }
        public Direction Heading { get; set; }
        public int Speed { get; set; }
        public int Fuel { get; set; }
    }
}