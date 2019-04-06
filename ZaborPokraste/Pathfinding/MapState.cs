using System.Collections.Generic;
using ZaborPokraste.API.Models.Enums;
using ZaborPokraste.API.Models.Game;

namespace ZaborPokraste.Pathfinding
{
    public class MapState
    {
        public Location EndLocation { get; set; }
        
        public Stack<CarState> OldStates { get; } = new Stack<CarState>();
        public CarState CarState { get; set; }
        
        public List<CellState> Cells { get; } = new List<CellState>();
    }

    public class CarState
    {
        public CarState(Location location, int speed, Direction direction)
        {
            Location = location;
            Speed = speed;
            Direction = direction;
        }

        public Location Location { get; }
        public int Speed { get; }
        public Direction Direction { get; }

        public override string ToString() =>
            $"loc {Location} speed {Speed} dir {Direction}";
    }

    public class CellState
    {
        public Location Location { get; set; }
        
        public CellType Type { get; set; }
        
        public List<(int, Direction)> States { get; } = new List<(int, Direction)>();
    }
}