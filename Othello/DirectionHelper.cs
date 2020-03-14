using System;
using System.Drawing;

namespace Othello
{
    public static class DirectionHelper
    {
        public static Point DirectionToPoint(Direction direction) =>
            direction switch
            {
                Direction.North => new Point(0, -1),
                Direction.NorthEast => new Point(1, -1),
                Direction.East => new Point(1, 0),
                Direction.SouthEast => new Point(1, 1),
                Direction.South => new Point(0, 1),
                Direction.SouthWest => new Point(-1, 1),
                Direction.West => new Point(-1, 0),
                Direction.NorthWest => new Point(-1, -1),
                _ => throw new Exception()
            };
    }
}