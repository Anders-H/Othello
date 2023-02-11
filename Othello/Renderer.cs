using System.Drawing;

namespace Othello;

public class Renderer
{
    private readonly int _boardSize;
    private int BoardX { get; }
    private int BoardY { get; }
    private const int TileSize = 38;

    public Renderer(int containerWidth, int containerHeight)
    {
        _boardSize = TileSize * 8;
        BoardX = containerWidth / 2 - _boardSize / 2;
        BoardY = containerHeight / 2 - _boardSize / 2;
    }

    public Point? GetBoardPosition(int x, int y)
    {
        if (x < BoardX || y < BoardY || x > BoardX + _boardSize || y > BoardY + _boardSize)
            return null;

        x -= BoardX;
        y -= BoardY;
        x /= TileSize;
        y /= TileSize;

        if (x < 0 || y < 0 || x > 7 || y > 7)
            return null;

        return new Point(x, y);
    }
        
    public void Draw(Graphics g, Game game)
    {
        using var bgBrush = new SolidBrush(Color.FromArgb(190, 190, 190));
        using var highlightPen = new Pen(Color.FromArgb(220, 220, 220));
        using var shadowPen = new Pen(Color.FromArgb(150, 150, 150));
        using var black = new SolidBrush(game.GameStatus == GameStatus.NotStarted ? Color.FromArgb(90, 90, 90) : Color.FromArgb(0, 0, 0));
        using var white = new SolidBrush(game.GameStatus == GameStatus.NotStarted ? Color.FromArgb(230, 230, 230) : Color.FromArgb(255, 255, 255));
        g.ResetClip();
        g.Clear(Color.FromArgb(180, 180, 180));
        const int t = TileSize - 1;

        var xpos = BoardX;
        var ypos = BoardY;

        for (var y = 0; y < 8; y++)
        {
            for (var x = 0; x < 8; x++)
            {
                g.SetClip(new Rectangle(xpos, ypos, t, t));

                g.FillRectangle(bgBrush, xpos, ypos, t, t);
                switch (game.GetTileAt(x, y))
                {
                    case Tile.Black:
                        g.FillEllipse(black, xpos + 2, ypos + 2, t - 5, t - 5);
                        break;
                    case Tile.White:
                        g.FillEllipse(white, xpos + 2, ypos + 2, t - 5, t - 5);
                        break;
                }

                g.DrawRectangle(shadowPen, xpos - 1, ypos - 1, t, t);
                g.DrawRectangle(highlightPen, xpos, ypos, t, t);
                xpos += TileSize;
            }

            xpos = BoardX;
            ypos += TileSize;
        }
    }
}