using System;
using System.Diagnostics.SymbolStore;
using ZaborPokraste.API.Models.Enums;

namespace ZaborPokraste.API.Models.Game
{
    public class Location : IEquatable<Location>
    {
        public Location(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public int X { get; }
        public int Y { get; }
        public int Z { get; }

        public bool Equals(Location other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Location) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X;
                hashCode = (hashCode * 397) ^ Y;
                hashCode = (hashCode * 397) ^ Z;
                return hashCode;
            }
        }

        public static bool operator ==(Location a, Location b) => a?.Equals(b) ?? b == null;
        public static bool operator !=(Location a, Location b) => !(a == b);

        public Direction GetDirectionTo(Location otherLoc)
        {
            var xdif = otherLoc.X - X;
            var ydif = otherLoc.Y - Y;
            
            var p = Math.Abs(xdif) + Math.Abs(otherLoc.Y - Y) + Math.Abs(otherLoc.Z - Z);
            if (p != 2) throw new Exception("Cells are not neighbors");

            if ((xdif, ydif) == (-1, 1)) return Direction.West;
            if ((xdif, ydif) == (1, -1)) return Direction.East;
            if ((xdif, ydif) == (0, 1)) return Direction.NorthWest;
            if ((xdif, ydif) == (0, -1)) return Direction.SouthWest;
            if ((xdif, ydif) == (1, 0)) return Direction.NorthEast;
            if ((xdif, ydif) == (-1, 0)) return Direction.SouthEast;
            
            throw new Exception("wut?");
        }

        public override string ToString() => $"xyz: {X} {Y} {Z}";

        public bool IsNeighborTo(Location otherLoc)
        {
            var xdif = otherLoc.X - X;
            var ydif = otherLoc.Y - Y;
            
            var p = Math.Abs(xdif) + Math.Abs(otherLoc.Y - Y) + Math.Abs(otherLoc.Z - Z);
            return p == 2;
        }

        public Location SingleMove(Direction curDir)
        {
            switch (curDir)
            {
                case Direction.West: return new Location(X - 1, Y + 1, Z);
                case Direction.East: return new Location(X + 1, Y - 1, Z);
                case Direction.NorthWest: return new Location(X, Y + 1, Z - 1);
                case Direction.NorthEast: return new Location(X + 1, Y, Z - 1);
                case Direction.SouthWest: return new Location(X - 1, Y, Z + 1);
                case Direction.SouthEast: return new Location(X, Y - 1, Z + 1);
                default: throw new Exception("Все в говне");
            }
        }
    }
}