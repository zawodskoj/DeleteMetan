using System;
using System.Collections.Concurrent;
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

        private const string DefaultMap = "yinyang";

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
            List<(Location, CellType)> visibleCells, int radius,
            int startSpeed, Direction startDirection)
        {
            this.sessionId = sessionId;
            _client = client;
            _radius = radius;
            var curState = new CarState(startPos, startSpeed, startDirection, 0, false, null);
            _state.CarState = curState;
            _state.EndLocation = endPos;

            foreach (var cell in visibleCells)
            {
                _state.Cells.Add(new CellState
                {
                    Location = cell.Item1,
                    Type = cell.Item2
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
                Console.WriteLine("Стейт " + _state.CarState);
            }
            catch
            {
                throw;
            }
        }
        
        public void FindPath()
        {
            const int minPitSpeed = 70;
            const int maxDgrSpeed = 30;
            const int maxAccelSpeed = 30;
            const int speedStep = 10;

            const int maxPasses = 8;
            
            var stateQueue = new List<(CarState carState, int prevIndex)>();
            stateQueue.Add((_state.CarState, 0));
            var passedLocs = new ConcurrentDictionary<Location, int>();
            passedLocs.TryAdd(_state.CarState.Location, 1);

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

            (bool gasgasgas, bool fuckedUp, int speed, Location actualLoc) ApplyDrift(int preSpeed, Location curLoc, Location targetLoc, Direction curDir, Direction dir)
            {
                if (dir == curDir) return (false, false, preSpeed, targetLoc);
                
                var angles = GetDriftAngles(curDir, dir);
                if (preSpeed <= angles.MaxSpeed) return (false, false, preSpeed, targetLoc);
                
                preSpeed -= angles.SpeedDownShift;
                if (preSpeed < 0) preSpeed = 0;

                targetLoc = curLoc.SingleMove(curDir);
                
                if (_state.Cells.FirstOrDefault(x => x.Location == targetLoc) is CellState cell)
                {
                    switch (cell.Type)
                    {
                        case CellType.Rock:
                            return (true, true, preSpeed, targetLoc);
                        case CellType.Pit:
                            return (true, preSpeed < minPitSpeed, preSpeed, targetLoc);
                        case CellType.DangerousArea:
                            return (true, preSpeed > maxDgrSpeed, preSpeed, targetLoc);
                    }
                }
                
                return (true, false, preSpeed, targetLoc);
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
                
                if (currIndex >= stateQueue.Count)
                {
                    Console.WriteLine("Все в говне");
                    Environment.Exit(0);
                }
                current = stateQueue[currIndex].carState;
                
                foreach (var validCell in NeighborCellsForCurrentState()
                    .Where(x => x.Type != CellType.Rock)
                    .OrderBy(x => passedLocs.TryGetValue(x.Location, out var pss) ? pss : 0))
                {
                    if (passedLocs.TryGetValue(validCell.Location, out var passes) && passes > maxPasses) continue;
                    
                    var minSpeedRequirement = 10;
                    var maxSpeedRequirement = 100;
                    
                    if (hasPath)
                    {
                        break;
                    }
                    switch (validCell.Type)
                    {
                        case CellType.Pit:
                            minSpeedRequirement = minPitSpeed;
                            goto case CellType.Empty;
                        case CellType.DangerousArea:
                            maxSpeedRequirement = maxDgrSpeed;
                            goto case CellType.Empty;
                        case CellType.Empty:
                            for (var accel = -maxAccelSpeed; accel <= maxAccelSpeed; accel += speedStep)
                            //for (var accel = maxAccelSpeed; accel >= -maxAccelSpeed; accel -= speedStep)
                            {
                                var preSpeed = current.Speed + accel;
                                if (preSpeed < minSpeedRequirement || preSpeed > maxSpeedRequirement) continue;
                                
                                var dir = current.Location.GetDirectionTo(validCell.Location);
                                var (gas, fuckedUp, speed, tarLoc) = ApplyDrift(preSpeed, current.Location, validCell.Location, current.Direction, dir);
                                if (fuckedUp) continue;
                                
                                var newCurrent = new CarState(tarLoc, speed, dir, accel, gas, validCell.Location);
                                passedLocs.AddOrUpdate(newCurrent.Location, 1, (_, v) => v + 1);

                                stateQueue.Add((newCurrent, currIndex));
                                if (newCurrent.Location == _state.EndLocation)
                                {
                                    hasPath = true;

                                    CarState nextState = null;
                                    var tmpIndex = currIndex;
                                    if (currIndex == 0)
                                    {
                                        nextState = newCurrent;
                                    }
                                    else
                                    {
                                        while (tmpIndex > 0)
                                        {
                                            nextState = stateQueue[tmpIndex].carState;
                                            tmpIndex = stateQueue[tmpIndex].prevIndex;
                                        }
                                    }

                                    if (nextState.Eurobeat)
                                        Console.WriteLine("assuming dejavu");
                                    Console.WriteLine("Next turn: " + nextState);
                                    _nextPos = nextState;
                                }
                            }
                            // yay
                            break;
                    }
                }

                currIndex++;
            }
        }

        public (Direction dir, int accel) GetBestTurn()
        {
//            if (_state.CarState.Location.IsNeighborTo(_state.EndLocation))
//                return (_state.CarState.Location.GetDirectionTo(_state.EndLocation), 100);
//            
            return (_nextPos.Direction, _nextPos.TurnAcceleration);
        }
        
        public async Task<bool> NextStep()
        {
            if (_state.EndLocation == _state.CarState.Location) return true;
            
            var (dir, accel) = GetBestTurn();
            Console.WriteLine("moving to {0} with accel {1}", dir, accel);
            _nextPos = null;
            
            var result = await Move(dir, accel);
            if (result.Location == _state.EndLocation) return true;
            Console.WriteLine("move result: loc {0} state {1} speed {2}", 
                result.Location, result.Status, result.Speed);

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
                else
                {
                    existing.Type = cell.Item2;
                }
            }
            
            var visitedCell = _state.Cells.Single(x => x.Location == _state.CarState.Location);
            visitedCell.States.Add((_state.CarState.Speed, _state.CarState.Direction));
            
            _state.CarState = new CarState(result.Location, result.Speed, result.Heading, 0, false, null);
           
            FindPath();

            return false;
        }

        public async Task<TurnResult> Move(Direction direction, int acceleration)
        {
            var r = await new RacePut(sessionId, new TurnModel(direction, acceleration))
                .Dispatch(_client);
            await new RaceGet(sessionId).Dispatch(_client);

            return r;
        }
    }
}