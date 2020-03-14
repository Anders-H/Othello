namespace Othello
{
    public class Game
    {
        private readonly Tile[,] _tiles = new Tile[8, 8];
        public GameStatus GameStatus { get; set; }
        public int PlayerScore { get; private set; }
        public int ComputerScore { get; private set; }
        public int PassCount { get; set; }
        public Difficulty Difficulty { get; set; }
        
        public Game()
        {
            Reset();
        }

        public void Reset()
        {
            GameStatus = GameStatus.NotStarted;
            
            for (var y = 0; y < 8; y++)
                for (var x = 0; x < 8; x++)
                    _tiles[x, y] = Tile.None;
            
            _tiles[3, 3] = Tile.White;
            _tiles[3, 4] = Tile.Black;
            _tiles[4, 3] = Tile.Black;
            _tiles[4, 4] = Tile.White;
            PlayerScore = 2;
            ComputerScore = 2;
            PassCount = 0;
        }

        public bool IsFree(int x, int y) =>
            GetTileAt(x, y) == Tile.None;
        
        public Tile GetTileAt(int x, int y) =>
            _tiles[x, y];

        public void SetTileAt(int x, int y, Tile color) =>
            _tiles[x, y] = color;

        public void CalculateScore()
        {
            var black = 0;
            var white = 0;
            for (var y = 0; y < 8; y++)
            {
                for (var x = 0; x < 8; x++)
                {
                    switch (_tiles[x, y])
                    {
                        case Tile.Black:
                            black++;
                            break;
                        case Tile.White:
                            white++;
                            break;
                    }
                }
            }
            PlayerScore = black;
            ComputerScore = white;
        }

        public bool BlackCanMove()
        {
            var ruleEngine = new RuleEngine(this);
            for (var y = 0; y < 8; y++)
            {
                for (var x = 0; x < 8; x++)
                {
                    if (ruleEngine.CanMove(true, x, y))
                        return true;
                }
            }
            return false;
        }

        public bool BlackCanMove(int x, int y) =>
            new RuleEngine(this).CanMove(true, x, y);

        public bool WhiteCanMove()
        {
            var ruleEngine = new RuleEngine(this);
            for (var y = 0; y < 8; y++)
            {
                for (var x = 0; x < 8; x++)
                {
                    if (ruleEngine.CanMove(false, x, y))
                        return true;
                }
            }
            return false;
        }

        public void BlackFlipAt(int x, int y) =>
            new RuleEngine(this).FlipAt(true, x, y);

        public void WhiteFlipAt(int x, int y) =>
            new RuleEngine(this).FlipAt(false, x, y);

        public bool CheckGameOverState()
        {
            if (PassCount > 2)
                return true;
            var blackCount = 0;
            var whiteCount = 0;
            var noneCount = 0;
            for (var y = 0; y < 8; y++)
            {
                for (var x = 0; x < 8; x++)
                {
                    var tile = GetTileAt(x, y);
                    switch (tile)
                    {
                        case Tile.Black:
                            blackCount++;
                            break;
                        case Tile.White:
                            whiteCount++;
                            break;
                        case Tile.None:
                            noneCount++;
                            break;
                    }
                }
            }
            return blackCount == 0 || whiteCount == 0 || noneCount == 0;
        }
    }
}