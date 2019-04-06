using System;
using System.Collections.Generic;
using ZaborPokraste.API.Models.Enums;

namespace ZaborPokraste.API.Models.Game
{
    public class PlayerSessionInfo
    {
        public string SessionId { get; set; }
        public string PlayerId { get; set; }
        public Direction CurrentDirection { get; set; }
        
        public Location CurrentLocation { get; set; }
        public Location Finish { get; set; }
        
        public int Radius { get; set; }
        public int CurrentSpeed { get; set; }
        public PlayerStatus PlayerStatus { get; set; }
        
        public List<(Location, CellType)> NeighbourCells { get; set; } = new List<(Location, CellType)>();
    }
}