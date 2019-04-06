using System.Collections.Generic;
using ZaborPokraste.API.Models.Enums;
using ZaborPokraste.API.Models.Game;

namespace ZaborPokraste.API.Models.Actions
{
    public class TurnResult
    {
        public TurnCommand Command { get; set; }
        public List<Cell> VisibleCells { get; set; }
        public Location Location { get; set; }
        public int ShortestWayLength { get; set; }
        public int Speed { get; set; }
        public PlayerStatus Status { get; set; }
        public Direction Heading { get; set; }
        public int FuelWaste { get; set; }
    }
}