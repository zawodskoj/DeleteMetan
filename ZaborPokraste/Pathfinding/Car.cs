using System.Collections.Generic;
using System.Linq;
using ZaborPokraste.API.Models;
using ZaborPokraste.API.Models.Actions;
using ZaborPokraste.API.Models.Game;

namespace ZaborPokraste.Pathfinding
{
    public abstract class Car
    {
        private MapState _state = new MapState();

        public Car(Location startPos, Location endPos, IEnumerable<Cell> visibleCells)
        {
        }L=
        
        public void NextStep()
        {
            
        }

        public abstract TurnResult Move(Direction direction, int acceleration);
    }
}