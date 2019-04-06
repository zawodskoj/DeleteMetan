using System.Collections.Generic;

namespace ZaborPokraste.Pathfinding
{
    public class MapState
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        
        public Direction CurrentDirection { get; set; }
        
        public List<CellState> Cells { get; } = new List<CellState>();
    }

    public class CellState
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        
        public CellType Type { get; set; }
        
        public List<(int, Direction)> States { get; } = new List<(int, Direction)>();
    }
    
    public enum Direction
    {
        West, East, NorthWest, NorthEast, SouthWest, SouthEast
    }
}