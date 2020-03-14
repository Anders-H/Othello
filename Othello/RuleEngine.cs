using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Othello
{
    public class RuleEngine
    {
        private readonly Game _game;

        public RuleEngine(Game game)
        {
            _game = game;
        }

        public bool CanMove(bool black, int x, int y)
        {
            if (IsBusy(x, y) || !HasNeighbours(x, y))
                return false;
            return Enum.GetValues(
                typeof(Direction))
                .Cast<Direction>()
                .Any(direction => MoveIsLegal(black, x, y, direction)
            );
        }

        public void FlipAt(bool black, int x, int y)
        {
            foreach (var direction in Enum.GetValues(typeof(Direction)).Cast<Direction>())
                FlipAt(black, direction, x, y);
        }

        private void FlipAt(bool black, Direction direction, int x, int y)
        {
            var dir = DirectionHelper.DirectionToPoint(direction);
            var targetX = x;
            var targetY = y;
            var changes = new List<Point>();

            do
            {
                ApplyMovement(dir, ref targetX, ref targetY);
                
                if (!OnBoard(targetX, targetY) || _game.GetTileAt(targetX, targetY) == Tile.None)
                    return;
                
                if (IsFriendColor(black, targetX, targetY))
                {
                    if (changes.Count > 0)
                        break;
                    return;
                }

                if (IsEnemyColor(black, targetX, targetY))
                {
                    changes.Add(new Point(targetX, targetY));
                }

            } while (true);

            foreach (var point in changes)
                _game.SetTileAt(point.X, point.Y, black ? Tile.Black : Tile.White);
        }
        
        private bool IsBusy(int x, int y) =>
            !OnBoard(x, y) || _game.GetTileAt(x, y) != Tile.None;

        private static bool OnBoard(int x, int y) =>
            x >= 0 && y >= 0 && x < 8 && y < 8;

        private bool HasNeighbours(int x, int y) =>
            Enum.GetValues(
                typeof(Direction))
                .Cast<Direction>()
                .Any(direction => IsSet(x, y, DirectionHelper.DirectionToPoint(direction))
            );

        private bool IsSet(int x, int y, Point offset) =>
            IsSet(new Point(x + offset.X, y + offset.Y));
        
        private bool IsSet(Point p) =>
            OnBoard(p.X, p.Y)
            && _game.GetTileAt(p.X, p.Y) != Tile.None;

        private bool MoveIsLegal(bool black, int x, int y, Direction direction)
        {
            var dir = DirectionHelper.DirectionToPoint(direction);
            var targetX = x;
            var targetY = y;
            
            ApplyMovement(dir, ref targetX, ref targetY);
            if (!OnBoard(targetX, targetY))
                return false;
            if (_game.GetTileAt(targetX, targetY) == Tile.None || IsFriendColor(black, targetX, targetY))
                return false;

            do
            {
                ApplyMovement(dir, ref targetX, ref targetY);
                if (!OnBoard(targetX, targetY) || _game.GetTileAt(targetX, targetY) == Tile.None)
                    return false;
                if (IsEnemyColor(black, targetX, targetY))
                    continue;
                if (IsFriendColor(black, targetX, targetY))
                    return true;
            } while (true);
        }

        private bool IsEnemyColor(bool black, int x, int y) =>
            black
                ? _game.GetTileAt(x, y) == Tile.White
                : _game.GetTileAt(x, y) == Tile.Black;

        private bool IsFriendColor(bool black, int x, int y) =>
            black
                ? _game.GetTileAt(x, y) == Tile.Black
                : _game.GetTileAt(x, y) == Tile.White;
        
        private static void ApplyMovement(Point point, ref int x, ref int y)
        {
            x += point.X;
            y += point.Y;
        }

        public Point? GetBestMove(bool black) =>
            new ThinkEngine(_game)
                .GetBestMove(black);
    }
}