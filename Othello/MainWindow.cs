using System;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Othello
{
    public partial class MainWindow : Form
    {
        private readonly Game _game;
        private Renderer _renderer;

        public MainWindow()
        {
            InitializeComponent();
            _game = new Game();
        }

        private void MainWindow_Shown(object sender, EventArgs e)
        {
            SetDifficulty();
            _renderer = new Renderer(pictureBox1.Width, pictureBox1.Height);
            lblStatus.Text = GetStatusText();
            Refresh();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            _renderer?.Draw(e.Graphics, _game);
        }

        private string GetStatusText() =>
            _game.GameStatus switch
            {
                GameStatus.NotStarted =>
                    $"Game over. Player (black): {_game.PlayerScore}, computer (white): {_game.ComputerScore}",
                GameStatus.PlayerTurnBlack =>
                    $"Your turn. Player (black): {_game.PlayerScore}, computer (white): {_game.ComputerScore}",
                GameStatus.ComputerTurnWhite =>
                    $"Computer turn. Player (black): {_game.PlayerScore}, computer (white): {_game.ComputerScore}",
                _ =>
                    throw new ArgumentOutOfRangeException()
            };

        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_game.GameStatus != GameStatus.NotStarted
                && MessageBox.Show(
                    @"Start a new game?",
                    @"New game",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;
            _game.Reset();
            _game.GameStatus = GameStatus.PlayerTurnBlack;
            easyToolStripMenuItem.Enabled = false;
            mediumToolStripMenuItem.Enabled = false;
            hardToolStripMenuItem.Enabled = false;
            Cursor = Cursors.Default;
            pictureBox1.Invalidate();
            lblStatus.Text = GetStatusText();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e) =>
            Close();

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            var currentCursor = Cursor;
            try
            {
                Cursor = Cursors.Default;
                if (_game.GameStatus == GameStatus.NotStarted)
                    return;
                if (MessageBox.Show(
                        @"Abandon current game?",
                        @"Quit",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                    e.Cancel = true;
            }
            finally
            {
                Cursor = currentCursor;
            }
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            var currentCursor = Cursor;
            try
            {
                if (_game.GameStatus == GameStatus.NotStarted && (e.Button & MouseButtons.Left) > 0)
                {
                    Cursor = Cursors.WaitCursor;
                    if (MessageBox.Show(
                            @"Start a new game?",
                            @"New game",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                        newGameToolStripMenuItem_Click(sender, new EventArgs());
                    return;
                }
            
                if (_renderer == null || _game.GameStatus != GameStatus.PlayerTurnBlack)
                    return;
            
                if ((e.Button & MouseButtons.Left) > 0)
                    MakePlayerMove(e.X, e.Y);
            }
            finally
            {
                Cursor = currentCursor;
            }
        }

        private void MakePlayerMove(int mouseX, int mouseY)
        {
            var boardPosition = _renderer.GetBoardPosition(mouseX, mouseY);
            if (boardPosition == null)
                return;
            var x = boardPosition.Value.X;
            var y = boardPosition.Value.Y;
            var pass = false;
            if (!_game.BlackCanMove(x, y))
            {
                if (_game.BlackCanMove())
                    return;
                
                MessageBox.Show(
                    @"You (black) have to pass.",
                    @"Black can't move",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                
                pass = true;
                _game.PassCount++;
            }
            Cursor = Cursors.WaitCursor;
            _game.GameStatus = GameStatus.ComputerTurnWhite;
            
            if (!pass)
                _game.SetTileAt(x, y, Tile.Black);
            
            _game.CalculateScore();
            lblStatus.Text = GetStatusText();
            pictureBox1.Refresh();
            System.Threading.Thread.Sleep(50);
            Application.DoEvents();
            _game.BlackFlipAt(x, y);
            pictureBox1.Refresh();
            System.Threading.Thread.Sleep(50);
            Application.DoEvents();
            
            if (_game.CheckGameOverState())
                GameOver();
            else
                MakeComputerMove();
        }

        private void MakeComputerMove()
        {
            if (!_game.WhiteCanMove())
            {
                _game.PassCount++;
                if (_game.CheckGameOverState())
                {
                    GameOver();
                    return;
                }
            }
            var ruleEngine = new RuleEngine(_game);
            var bestMove = ruleEngine.GetBestMove(false);
            if (bestMove == null)
            {
                GameOver();
                return;
            }
            _game.SetTileAt(bestMove.Value.X, bestMove.Value.Y, Tile.White);
            pictureBox1.Refresh();
            System.Threading.Thread.Sleep(50);
            Application.DoEvents();
            _game.WhiteFlipAt(bestMove.Value.X, bestMove.Value.Y);
            pictureBox1.Refresh();
            System.Threading.Thread.Sleep(50);
            Application.DoEvents();
            _game.CalculateScore();
            _game.GameStatus = GameStatus.PlayerTurnBlack;
            lblStatus.Text = GetStatusText();
            pictureBox1.Refresh();
            Cursor = Cursors.Default;

            if (_game.CheckGameOverState())
                GameOver();
        }

        private void GameOver()
        {
            Cursor = Cursors.Default;
            _game.GameStatus = GameStatus.NotStarted;
            _game.CalculateScore();
            lblStatus.Text = GetStatusText();
            pictureBox1.Refresh();
            var resultString = "Dead heat!";
            if (_game.ComputerScore > _game.PlayerScore)
                resultString = "Computer (white) wins!";
            else if (_game.ComputerScore < _game.PlayerScore)
                resultString = "You (black) wins!";

            easyToolStripMenuItem.Enabled = true;
            mediumToolStripMenuItem.Enabled = true;
            hardToolStripMenuItem.Enabled = true;

            MessageBox.Show(
                $@"{resultString}

You (black): {_game.PlayerScore}
Computer (white): {_game.ComputerScore}",
                @"Game over!",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void SetDifficulty()
        {
            if (easyToolStripMenuItem.Checked)
                _game.Difficulty = Difficulty.Easy;
            else if (mediumToolStripMenuItem.Checked)
                _game.Difficulty = Difficulty.Medium;
            else if (hardToolStripMenuItem.Checked)
                _game.Difficulty = Difficulty.Hard;
        }
        
        private void easyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            easyToolStripMenuItem.Checked = true;
            mediumToolStripMenuItem.Checked = false;
            hardToolStripMenuItem.Checked = false;
            SetDifficulty();
        }

        private void mediumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            easyToolStripMenuItem.Checked = false;
            mediumToolStripMenuItem.Checked = true;
            hardToolStripMenuItem.Checked = false;
            SetDifficulty();
        }

        private void hardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            easyToolStripMenuItem.Checked = false;
            mediumToolStripMenuItem.Checked = false;
            hardToolStripMenuItem.Checked = true;
            SetDifficulty();
        }
    }
}