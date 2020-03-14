namespace Othello
{
    public class Move
    {
        public int X { get; }
        public int Y { get; }
        public int Score { get; }

        public Move(int x, int y, int score)
        {
            X = x;
            Y = y;
            Score = score;
        }
    }
}