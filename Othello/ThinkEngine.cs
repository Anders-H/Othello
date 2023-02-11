using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Othello;

public class ThinkEngine
{
    private static readonly Random Random;
    private readonly Game _game;

    static ThinkEngine()
    {
        Random = new Random();
    }

    public ThinkEngine(Game game)
    {
        _game = game;
    }

    public Point? GetBestMove(bool black)
    {
        var self = black
            ? Tile.Black
            : Tile.White;

        var score = new int[8, 8];
        CreateBasicScoreMatrix(ref score);
        AddSituationScore(self, ref score);
        ApplyDifficulty(ref score);

#if DEBUG
        System.Diagnostics.Debug.WriteLine("");
        for (var y = 0; y < 8; y++)
        {
            for (var x = 0; x < 8; x++)
            {
                System.Diagnostics.Debug.Write($"{score[x, y]} ");
            }
            System.Diagnostics.Debug.WriteLine("");
        }
        System.Diagnostics.Debug.WriteLine("");
#endif

        var moves = GetAllPossibleMoves(black, score);
            
        var move = moves
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();
            
        if (move == null)
            return null;
            
        return new Point(move.X, move.Y);
    }

    private IEnumerable<Move> GetAllPossibleMoves(bool black, int[,] score)
    {
        var rules = new RuleEngine(_game);
        var moves = new List<Move>();
        
        for (var y = 0; y < 8; y++)
        {
            for (var x = 0; x < 8; x++)
            {
                if (rules.CanMove(black, x, y))
                    moves.Add(new Move(x, y, score[x, y]));
            }
        }

        return moves;
    }

    private static void CreateBasicScoreMatrix(ref int[,] score)
    {
        score[0, 0] = 90;
        score[1, 0] = 10;
        score[2, 0] = 80;
        score[3, 0] = 50;
        score[0, 1] = 10;
        score[1, 1] = 10;
        score[2, 1] = 70;
        score[3, 1] = 40;
        score[0, 2] = 80;
        score[1, 2] = 70;
        score[2, 2] = 60;
        score[3, 2] = 30;
        score[0, 3] = 50;
        score[1, 3] = 40;
        score[2, 3] = 30;
        score[3, 3] = 20;
            
        for (var y = 0; y < 4; y++)
            for (var x = 0; x < 4; x++)
                score[7 - x , y] = score[x, y];
            
        for (var y = 0; y < 4; y++)
            for (var x = 0; x < 8; x++)
                score[x, 7 - y] = score[x, y];
    }

    private void AddSituationScore(Tile self, ref int[,] score)
    {
        AddHorizontalSituationScore(self, ref score, 1, 7, 0);
        AddHorizontalSituationScore(self, ref score, 1, 7, 7);
        AddHorizontalSituationScoreReversed(self, ref score, 6, 0, 0);
        AddHorizontalSituationScoreReversed(self, ref score, 6, 0, 7);
        AddVerticalSituationScore(self, ref score, 1, 7, 0);
        AddVerticalSituationScore(self, ref score, 1, 7, 7);
        AddVerticalSituationScoreReversed(self, ref score, 6, 0, 0);
        AddVerticalSituationScoreReversed(self, ref score, 6, 0, 7);
    }
        
    private void AddHorizontalSituationScore(Tile self, ref int[,] score, int xstart, int xend, int ystart)
    {
        if (!_game.IsFree(xstart, ystart))
            return;

        var q = 0;
        
        for (var x = xstart; x <= xend; x++)
            if (_game.GetTileAt(xstart, ystart) == self)
                q++;
        
        if (q >= 7)
            score[xstart, ystart] += 90;
    }
        
    private void AddHorizontalSituationScoreReversed(Tile self, ref int[,] score, int xstart, int xend, int ystart)
    {
        if (!_game.IsFree(xstart, ystart))
            return;

        var q = 0;
        
        for (var x = xstart; x >= xend; x--)
            if (_game.GetTileAt(xstart, ystart) == self)
                q++;
        
        if (q >= 7)
            score[xstart, ystart] += 90;
    }
        
    private void AddVerticalSituationScore(Tile self, ref int[,] score, int ystart, int yend, int xstart)
    {
        if (!_game.IsFree(xstart, ystart))
            return;

        var q = 0;
        
        for (var y = ystart; y <= yend; y++)
            if (_game.GetTileAt(xstart, ystart) == self)
                q++;
        
        if (q >= 7)
            score[xstart, ystart] += 90;
    }
        
    private void AddVerticalSituationScoreReversed(Tile self, ref int[,] score, int ystart, int yend, int xstart)
    {
        if (!_game.IsFree(xstart, ystart))
            return;

        var q = 0;
        
        for (var y = ystart; y >= yend; y--)
            if (_game.GetTileAt(xstart, ystart) == self)
                q++;
        
        if (q >= 7)
            score[xstart, ystart] += 90;
    }

    private void ApplyDifficulty(ref int[,] score)
    {
        var variation = _game.Difficulty switch
        {
            Difficulty.Easy => 25,
            Difficulty.Medium => 15,
            Difficulty.Hard => 5,
            _ => throw new SystemException()
        };

        for (var y = 0; y < 8; y++)
            for (var x = 0; x < 8; x++)
                score[x, y] += (Random.Next(variation * 2) - variation);
    }
}