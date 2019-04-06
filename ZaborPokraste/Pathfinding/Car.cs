using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ZaborPokraste.API.Actions.Auth;
using ZaborPokraste.API.Actions.Race;
using ZaborPokraste.API.Client;
using ZaborPokraste.API.Models;
using ZaborPokraste.API.Models.Actions;
using ZaborPokraste.API.Models.Auth;
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
    
    public class Car
    {
        private readonly HttpClient _client;
        private readonly int _radius;

        public string sessionId;

        private const string DefaultMap = "test";

        private static readonly List<DriftsAngle> _driftsAngles = new List<DriftsAngle>
        {
            new DriftsAngle(60, 90, 30),
            new DriftsAngle(120, 60, 60),
            new DriftsAngle(180, 30, 90)
        };

        private static readonly Dictionary<Direction, Dictionary<Direction, DriftsAngle>> _drifts = new Dictionary<Direction, Dictionary<Direction, DriftsAngle>>()
        {
            {
                Direction.East,
                new Dictionary<Direction, DriftsAngle>()
                {
                    { Direction.NorthEast, _driftsAngles[0] },
                    { Direction.SouthEast, _driftsAngles[0] },
                    { Direction.NorthWest, _driftsAngles[1] },
                    { Direction.SouthWest, _driftsAngles[1] },
                    { Direction.West, _driftsAngles[2] }
                }
            },
            {
                Direction.NorthEast,
                new Dictionary<Direction, DriftsAngle>()
                {
                    { Direction.East, _driftsAngles[0] },
                    { Direction.NorthWest, _driftsAngles[0] },
                    { Direction.SouthEast, _driftsAngles[1] },
                    { Direction.West, _driftsAngles[1] },
                    { Direction.SouthWest, _driftsAngles[2] }
                }
            },
            {
                Direction.NorthWest,
                new Dictionary<Direction, DriftsAngle>()
                {
                    { Direction.NorthEast, _driftsAngles[0] },
                    { Direction.West, _driftsAngles[0] },
                    { Direction.East, _driftsAngles[1] },
                    { Direction.SouthWest, _driftsAngles[1] },
                    { Direction.SouthEast, _driftsAngles[2] }
                }
            },
            {
                Direction.West,
                new Dictionary<Direction, DriftsAngle>()
                {
                    { Direction.NorthWest, _driftsAngles[0] },
                    { Direction.SouthWest, _driftsAngles[0] },
                    { Direction.NorthEast, _driftsAngles[1] },
                    { Direction.SouthEast, _driftsAngles[1] },
                    { Direction.East, _driftsAngles[2] }
                }
            },
            {
                Direction.SouthWest,
                new Dictionary<Direction, DriftsAngle>()
                {
                    { Direction.West, _driftsAngles[0] },
                    { Direction.SouthEast, _driftsAngles[0] },
                    { Direction.NorthWest, _driftsAngles[1] },
                    { Direction.East, _driftsAngles[1] },
                    { Direction.NorthEast, _driftsAngles[2] }
                }
            },
            {
                Direction.SouthEast,
                new Dictionary<Direction, DriftsAngle>()
                {
                    { Direction.SouthWest, _driftsAngles[0] },
                    { Direction.East, _driftsAngles[0] },
                    { Direction.West, _driftsAngles[1] },
                    { Direction.NorthEast, _driftsAngles[1] },
                    { Direction.NorthWest, _driftsAngles[2] }
                }
            },
        };
        
        private readonly MapState _state = new MapState();
        private CarState _nextPos;

        public Car(HttpClient client, string sessionId, Location startPos, Location endPos, 
            IEnumerable<Cell> visibleCells, int radius,
            int startSpeed, Direction startDirection)
        {
            this.sessionId = sessionId;
            _client = client;
            _radius = radius;
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

            try
            {
                FindPath();
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public static async Task<Car> CreateClient()
        {
            var hc = new HttpClient();
            var token = ((new LoginPost(new LoginDto())).Dispatch(hc).GetAwaiter().GetResult()).Token;
            
            hc.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);


            var playerSessionInfo = await (new RacePost(new CreateRaceDto(DefaultMap)))
                .Dispatch(hc);

            var sessionId = playerSessionInfo.SessionId;
            
            return new Car(hc, sessionId, playerSessionInfo.CurrentLocation, playerSessionInfo.Finish,
                playerSessionInfo.NeighbourCells, playerSessionInfo.Radius, playerSessionInfo.CurrentSpeed, playerSessionInfo.CurrentDirection);
        }

        public async Task EventLoop()
        {
            try
            {
                while (!await NextStep())
                {
                    Console.WriteLine("Работаем, работяги");
                    Console.WriteLine("Стейт " + _state.CarState);
                }

                Console.WriteLine("Всё не в говне");
            }
            catch
            {
                throw;
            }
        }

        public bool IsPathValid() => false;
        
        public void FindPath()
        {
            const int minPitSpeed = 70;
            const int maxDgrSpeed = 30;
            const int maxAccelSpeed = 30;
            const int speedStep = 10;
            
            if (IsPathValid()) return;

            var stateQueue = new List<(CarState carState, int prevIndex)>();
            stateQueue.Add((_state.CarState, 0));
            var passedLocs = new HashSet<Location>();
            passedLocs.Add(_state.CarState.Location);

            CarState current = null;
            
            Cell GetOrEmpty(CarState state, int dx, int dy, int dz)
            {
                var x = state.Location.X + dx;
                var y = state.Location.Y + dy;
                var z = state.Location.Z + dz;

                var loc = new Location(x, y, z);
                var cell = _state.Cells.FirstOrDefault(a => a.Location == loc);

                var isOutOfBounds = Math.Abs(x) > _radius || Math.Abs(y) > _radius || Math.Abs(z) > _radius;
                
                return cell != null
                    ? new Cell {Type = cell.Type, Location = cell.Location}
                    : new Cell {Type = isOutOfBounds ? CellType.Rock : CellType.Empty, Location = loc};
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

            DriftsAngle GetDriftAngles(Direction curDir, Direction dir)
            {
                return _drifts[curDir][dir];
            }

            (int speed, Location actualLoc) ApplyDrift(int preSpeed, Location curLoc, Direction curDir, Direction dir)
            {
                return (preSpeed, default); //curLoc.ApplyDirection(dir));
            }

            var currIndex = 0;
            var hasPath = false;
            while (!hasPath)
            {
                if (stateQueue.Count == 0)
                {
                    Console.WriteLine("Все в говне");
                    Environment.Exit(0);
                }
                
                Console.WriteLine("trying to get state at index " + currIndex);
                if (currIndex >= stateQueue.Count)
                {
                    Console.WriteLine("Все в говне");
                }
                current = stateQueue[currIndex].carState;
                Console.WriteLine("cur state " + current);

                foreach (var validCell in NeighborCellsForCurrentState()
                    .Where(x => x.Type != CellType.Rock && !passedLocs.Contains(x.Location)))
                {
                    if (hasPath)
                    {
                        break;
                    }
                    switch (validCell.Type)
                    {
                        case CellType.Pit:
                        case CellType.DangerousArea:
                        case CellType.Empty:
                            for (var accel = -maxAccelSpeed; accel <= maxAccelSpeed; accel += speedStep)
                            {
                                var preSpeed = current.Speed + accel;
                                if (preSpeed < 0 || preSpeed > 100) continue;
                                
                                var dir = current.Location.GetDirectionTo(validCell.Location);
                                var (speed, _) = ApplyDrift(preSpeed, current.Location, current.Direction, dir);
                                var newCurrent = new CarState(validCell.Location, speed, dir);
                                passedLocs.Add(newCurrent.Location);

                                stateQueue.Add((newCurrent, currIndex));
                                if (newCurrent.Location == _state.EndLocation)
                                {
                                    hasPath = true;

                                    CarState nextState = null;
                                    var tmpIndex = currIndex;
                                    while (tmpIndex > 0)
                                    {
                                        nextState = stateQueue[tmpIndex].carState;
                                        tmpIndex = stateQueue[tmpIndex].prevIndex;
                                    }
                                    Console.WriteLine("Next turn: " + nextState);
                                    _nextPos = nextState;
                                }
                            }
                            // yay
                            break;
//                        case CellType.Pit:
//                            break;
//                            if (current.Speed < minPitSpeed)
//                            {
//                                if (current.Speed + maxAccelSpeed < minPitSpeed) break;
//                            }
//                            break;
//                        case CellType.DangerousArea:
//                            break;
//                            if (current.Speed > maxDgrSpeed)
//                            {
//                                if (current.Speed - maxAccelSpeed > maxDgrSpeed) break;
//                            }
//                            break;
                    }
                    
                    if (validCell.Location == _state.EndLocation)
                    {
                        Console.WriteLine("Охуеть дошли");
                        // _nextPos = stateQueue.ToArray()[1];
                        return;
                    };
                }

                currIndex++;
            }
        }

        public (Direction dir, int accel) GetBestTurn()
        {
            var dir = _state.CarState.Location.GetDirectionTo(_nextPos.Location);
            var accel = _nextPos.Speed - _state.CarState.Speed;

            return (dir, accel);
        }
        
        public async Task<bool> NextStep()
        {
            if (_state.EndLocation == _state.CarState.Location) return true;
            
            var (dir, accel) = GetBestTurn();
            if (_state.CarState.Speed == 0) accel += 30;
            var result = await Move(dir, accel);

            foreach (var cell in result.VisibleCells)
            {
                var existing = _state.Cells.SingleOrDefault(x => x.Location == cell.Item1);
                if (existing == null)
                {
                    _state.Cells.Add(new CellState
                    {
                        Location = cell.Item1,
                        Type = cell.Item2    
                    });
                }
            }
            
            var visitedCell = _state.Cells.Single(x => x.Location == _state.CarState.Location);
            visitedCell.States.Add((_state.CarState.Speed, _state.CarState.Direction));
            
            _state.CarState = new CarState(result.Location, result.Speed, result.Heading);
           
            FindPath();

            return false;
        }

        public async Task<TurnResult> Move(Direction direction, int acceleration)
        {
            return await new RacePut(sessionId, new TurnModel(direction, acceleration))
                .Dispatch(_client);
        }
    }
}