using System;
using System.Collections.Generic;
using System.Linq;
using ZaborPokraste.API.Models;
using ZaborPokraste.API.Models.Actions;
using ZaborPokraste.API.Models.Enums;
using ZaborPokraste.API.Models.Game;

namespace ZaborPokraste.Pathfinding
{
    public class DriftsAngle
    {
        public DriftsAngle(int angle, int maxSpeed, int speedDownShift)
        {
            Angle = angle;
            MaxSpeed = maxSpeed;
            SpeedDownShift = speedDownShift;
        }

        public int Angle { get; }
        public int MaxSpeed { get; }
        public int SpeedDownShift { get; }
    }
    
    public abstract class Car
    {
        private static readonly List<DriftsAngle> _driftsAngles = new List<DriftsAngle>
        {
            new DriftsAngle(60, 90, 30),
            new DriftsAngle(120, 60, 60),
            new DriftsAngle(180, 30, 90)
        };
        
        private readonly MapState _state = new MapState();

        public Car(Location startPos, Location endPos, 
            IEnumerable<Cell> visibleCells, int radius,
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
            const int minPitSpeed = 70;
            const int maxDgrSpeed = 30;
            const int maxAccelSpeed = 30;
            const int speedStep = 10;
            
            if (IsPathValid()) return;

            var stack = new Stack<CarState>();
            stack.Push(_state.CarState);

            var current = _state.CarState;
            
            while (true)
            {
                Cell GetOrEmpty(CarState state, int dx, int dy, int dz)
                {
                    var x = state.Location.X + dx;
                    var y = state.Location.Y + dy;
                    var z = state.Location.Z + dz;

                    var loc = new Location(x, y, z);
                    var cell = _state.Cells.FirstOrDefault(a => a.Location == loc);
                    return cell != null
                        ? new Cell {Type = cell.Type, Location = cell.Location}
                        : new Cell {Type = CellType.Empty, Location = loc};
                }

                IEnumerable<Cell> NeighborCellsForCurrentState()
                {
                    yield return GetOrEmpty(current, -1, 0, 1);
                    yield return GetOrEmpty(current, -1, 1, 0);
                    yield return GetOrEmpty(current, 1, -1, 0);
                    yield return GetOrEmpty(current, 1, 0, -1);
                    yield return GetOrEmpty(current, 0, -1, 1);
                    yield return GetOrEmpty(current, 0, 1, -1);
                }

                (int speed, Location actualLoc) ApplyDrift(int preSpeed, Location curLoc, Direction curDir, Direction dir)
                {
                    var 
                }

                foreach (var validCell in NeighborCellsForCurrentState().Where(x => x.Type != CellType.Rock))
                {
                    switch (validCell.Type)
                    {
                        case CellType.Empty:
                            for (var accel = -maxAccelSpeed; accel <= maxAccelSpeed; accel += speedStep)
                            {
                                var preSpeed = current.Speed + accel;
                                var dir = current.Location.GetDirectionTo(validCell.Location);
                                var (speed, actualLoc) = ApplyDrift(preSpeed, current.Location, current.Direction, dir);
                                stack.Push(new CarState(actualLoc, speed, dir));
                            }
                            // yay
                            break;
                        case CellType.Pit:
                            if (current.Speed < minPitSpeed)
                            {
                                if (current.Speed + maxAccelSpeed < minPitSpeed) break;
                            }
                            break;
                        case CellType.DangerousArea:
                            if (current.Speed > maxDgrSpeed)
                            {
                                if (current.Speed - maxAccelSpeed > maxDgrSpeed) break;
                            }
                            break;
                    }
                }
            }
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