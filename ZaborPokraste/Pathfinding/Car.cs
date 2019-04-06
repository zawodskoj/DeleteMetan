using System.Collections.Generic;
using System.Linq;
using ZaborPokraste.API.Models;
using ZaborPokraste.API.Models.Actions;
using ZaborPokraste.API.Models.Enums;
using ZaborPokraste.API.Models.Game;

namespace ZaborPokraste.Pathfinding
{
    public abstract class Car
    {
        private readonly MapState _state = new MapState();

        public Car(Location startPos, Location endPos, 
            IEnumerable<Cell> visibleCells,
            int startSpeed, Direction startDirection)
        {
            var curState = new CarState(startPos, startSpeed, startDirection);
            _state.CarState = curState;
            _state.EndLocation = endPos;
            
            foreach (var cell in visibleCells)
            {
                _state.Cells.Add(new CellState
                {
                    Location = cell.Location,
                    Type = cell.Type
                });
            }

            FindPath();
        }

        public abstract bool IsPathValid();
        
        public void FindPath()
        {
            if (IsPathValid()) return;
            
            
        }
        
        public abstract (Direction dir, int accel) GetBestTurn();
        
        public bool NextStep()
        {
            if (_state.EndLocation == _state.CarState.Location) return true;
            
            var (dir, accel) = GetBestTurn();
            var result = Move(dir, accel);

            foreach (var cell in result.VisibleCells)
            {
                var existing = _state.Cells.SingleOrDefault(x => x.Location == cell.Location);
                if (existing == null)
                {
                    _state.Cells.Add(new CellState
                    {
                        Location = cell.Location,
                        Type = cell.Type
                    });
                }
            }
            
            var visitedCell = _state.Cells.Single(x => x.Location == _state.CarState.Location);
            visitedCell.States.Add((_state.CarState.Speed, _state.CarState.Direction));
            
            _state.CarState = new CarState(result.Location, result.Speed, result.Heading);
           
            FindPath();

            return false;
        }

        public abstract TurnResult Move(Direction direction, int acceleration);
    }
}