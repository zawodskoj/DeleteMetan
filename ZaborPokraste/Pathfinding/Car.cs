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

        private HttpClient _httpClient = new HttpClient();
        public string sessionId;

        private const string DefaultMap = "test";

        private static readonly List<DriftsAngle> _driftsAngles = new List<DriftsAngle>
        {
            new DriftsAngle(60, 90, 30),
            new DriftsAngle(120, 60, 60),
            new DriftsAngle(180, 30, 90)
        };
        
        private readonly MapState _state = new MapState();
        private CarState _nextPos;

        public Car(ApiClient client, Location startPos, Location endPos, 
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

        public async Task InitClient()
        {
            var token = ((new LoginPost(new LoginDto())).Dispatch(_httpClient).GetAwaiter().GetResult()).Token;
            
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);


            var playerSessionInfo = (new RacePost(new CreateRaceDto(DefaultMap)))
                .Dispatch(_httpClient).GetAwaiter().GetResult();

            sessionId = playerSessionInfo.SessionId;
        }

        public async Task EventLoop()
        {  
            while (!await NextStep())
            {
                Console.WriteLine("Работаем, работяги");
            }
            
            Console.WriteLine("Всё не в говне");
        }

        public bool IsPathValid() => false;
        
        public void FindPath()
        {
            const int minPitSpeed = 70;
            const int maxDgrSpeed = 30;
            const int maxAccelSpeed = 30;
            const int speedStep = 10;
            
            if (IsPathValid()) return;

            var stack = new Stack<CarState>();
            stack.Push(_state.CarState);
            var passedLocs = new HashSet<Location>();
            passedLocs.Add(_state.CarState.Location);

            var current = _state.CarState;
            
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
                return (preSpeed, default); //curLoc.ApplyDirection(dir));
            }
            
            while (true)
            {
                foreach (var validCell in NeighborCellsForCurrentState()
                    .Where(x => x.Type != CellType.Rock && !passedLocs.Contains(x.Location)))
                {
                    switch (validCell.Type)
                    {
                        case CellType.Empty:
                            for (var accel = -maxAccelSpeed; accel <= maxAccelSpeed; accel += speedStep)
                            {
                                var preSpeed = current.Speed + accel;
                                var dir = current.Location.GetDirectionTo(validCell.Location);
                                var (speed, _) = ApplyDrift(preSpeed, current.Location, current.Direction, dir);
                                current = new CarState(validCell.Location, speed, dir);
                                passedLocs.Add(current.Location);
                                stack.Push(current);
                            }
                            // yay
                            break;
                        case CellType.Pit:
                            break;
                            if (current.Speed < minPitSpeed)
                            {
                                if (current.Speed + maxAccelSpeed < minPitSpeed) break;
                            }
                            break;
                        case CellType.DangerousArea:
                            break;
                            if (current.Speed > maxDgrSpeed)
                            {
                                if (current.Speed - maxAccelSpeed > maxDgrSpeed) break;
                            }
                            break;
                    }
                    
                    if (validCell.Location == _state.EndLocation)
                    {
                        _nextPos = stack.ToArray()[1];
                        
                    };
                }
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
            var result = await Move(dir, accel);

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

        public async Task<TurnResult> Move(Direction direction, int acceleration)
        {
            return await new RacePut(sessionId, new TurnModel(direction, acceleration))
                .Dispatch(_httpClient);
        }
    }
}