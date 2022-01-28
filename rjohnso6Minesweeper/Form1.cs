using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace rjohnso6Minesweeper
{
    // Class Form1 is our window that contains the whole game.
    public partial class Form1 : Form
    {
        // We'll need a grid of squares to play the game
        public Cell[,] grid = new Cell[10, 10];
        // Use minesSet to remember if we set the mines already or not
        bool minesSet = false;
        // gameContinues and loss are used to perform our endgame tasks in the right order.
        bool gameContinues = true;
        bool loss = false;
        // Here are our form events.
        // cellsNumbers tells each cell to count the mines it has as neighbors, which is done right after setting the mines
        // tellWin tells the cells to autoclick when the game ends.
        // reset tells all cells to reset and prepare for the next game.
        public delegate void EventHandler(object sender, EventArgs e);
        public event EventHandler cellsNumbers;
        public event EventHandler tellWin;
        public event EventHandler reset;
        // Initialize our timer data
        Timer timer = new Timer();
        int gameTime = 0;
        int totalTime = 0;
        // Initialize our win count and loss count
        int wins = 0;
        int losses = 0;
        // Initializer to setup the very first game
        public Form1()
        {
            InitializeComponent();
            // Setup each cell in the 10x10 grid
            for(int x = 0; x < 10; x++)
            {
                for(int y = 0; y < 10; y++)
                {
                    Cell myCell = new Cell(x, y);
                    grid[x, y] = myCell;
                    this.Controls.Add(myCell);
                }
            }
            // Now that our grid is setup, we have a lot of event subscribing to do.
            // Let's start by looping through the grid again.
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    // We want our OnCellClick function to run when a cell is clicked
                    this.grid[x, y].listeningForClick += this.OnCellClick;
                    // Our cellsNumbers will tell each cell to count its mine neighbors
                    this.cellsNumbers += this.grid[x, y].setNumber;
                    // tellWin will autoclick all cells
                    this.tellWin += this.grid[x, y].OnButtonClick;

                    // Some of our events require cells to subscribe to each other
                    // So we need to double loop through our grid
                    for (int x2 = 0; x2 < 10; x2++)
                    {
                        for (int y2 = 0; y2 < 10; y2++)
                        {
                            // NEVER subscribe a cell to its own event
                            if (x != x2 || y != y2)
                            {
                                // zeroClick will allow all cells to check if they should autoclick when a zero cell is clicked
                                this.grid[x, y].zeroClick += this.grid[x2, y2].OnZeroClick;
                                // loseGame will tell all cells to autoclick with no conditions when a mine is clicked
                                this.grid[x, y].loseGame += this.grid[x2, y2].OnButtonClick;
                            }
                        }
                    }
                    // When a mine is clicked, we want it to tell us to begin the process of losing the game.
                    // Subscribe the form LAST so the message is displayed after autoclicking all cells
                    this.grid[x, y].loseGame += this.loseGame;
                    // reserveLoss lets us make sure we don't accidentally win when we click a mine while autoclicking everything
                    this.grid[x, y].reserveLoss += this.reserveLoss;
                    // Tell every cell when to restart the game
                    this.reset += this.grid[x, y].Restart;
                }
            }
            // END OF LOOPING THROUGH GRID TO SUBSCRIBE EVENTS

            // Subscribe our Game menu strip to their respective functions
            this.restartGameToolStripMenuItem.Click += this.Restart;
            this.showLifetimeStatsToolStripMenuItem.Click += this.sayGameStats;
            this.exitToolStripMenuItem.Click += this.Exit;
            this.helpToolStripMenuItem.Click += this.Help;
            // Setup our timer
            timer.Enabled = true;
            timer.Tick += this.timerTick;
            timer.Interval = 1000;
            // We don't want the timer to go until after we make our first click
            timer.Stop();
            TimerLabel.Text = "Timer: 0";
        }

        // Function SetMines to set some cells to mines, run after first click
        // input is the coordinates of the first clicked square which is guaranteed safe
        private void SetMines(int clickedX, int clickedY)
        {
            // Let's set the number to how many mines we have so it can be easily changed later
            int numMines = 10;
            // We should keep track of where our mines are so we don't accidentally place two in the same place
            int[,] mines = new int[numMines, 2];
            // Setup our randomness
            Random rand = new Random();
            
            // Count to make sure we place the correct number of mines
            for(int count = 0; count < numMines; count++)
            {
                // Initialize our mine coordinates
                int x = 0;
                int y = 0;
                
                // Make sure we never place two mines in the same place or place a mine on the safe square
                bool unique = false;
                while(!unique)
                {
                    // get random coords
                    x = rand.Next(10);
                    y = rand.Next(10);
                    // prevent it from turning the clicked on cell into a mine
                    if(x == clickedX && y == clickedY)
                    {
                        continue;
                    }
                    // Check if we put a mine on a new square, assume this one is unique and good to go
                    unique = true;
                    for (int checkMine = 0; checkMine < 10; checkMine++)
                    {
                        if (mines[checkMine, 0] == x && mines[checkMine, 1] == y)
                        {
                            // A mine on the same square! Redo.
                            unique = false;
                        }
                    }
                }
                // Set our mine
                grid[x, y].SetMine();
                // Record where we place it so we don't place two in the same spot
                mines[count, 0] = x;
                mines[count, 1] = y;
            }
            // Remember we set our mines so we don't place them again
            this.minesSet = true;
            // Tell all our cells to count their mines neighbors
            this.SetNumbers();
        }

        // Function OnCellClick handles whatever needs to happen when any generic cell is clicked
        // SetMines at the start of the game, then check if we won
        private void OnCellClick(object sender, EventArgs e)
        {
            // Set mines if not done already
            if(!this.minesSet)
            {
                this.SetMines(((Cell)(sender)).x, ((Cell)(sender)).y);
                timer.Start();
            }
            // check if we won the game
            this.CheckWin();
        }
        // Function SetNumbers to tell all cells to count their mine neighbors
        private void SetNumbers()
        {
            if(this.cellsNumbers != null)
            {
                this.cellsNumbers(this, EventArgs.Empty);
            }
        }

        // Function loseGame for when we lost the game
        private void loseGame(object sender, EventArgs e)
        {
            // Check that we already reserved the loss (loss) and haven't said "you lose!" twice (gameContinues)
            if (this.loss && this.gameContinues)
            {
                // Stop the timer, count a loss, and tell the player that lost.
                timer.Stop();
                this.losses++;
                MessageBox.Show("You lose!");
            }
            // Make sure we don't tell the player they lost twice.
            this.gameContinues = false;
        }

        // Function reserveLoss prevents the player from winning after they lose.
        private void reserveLoss(object sender, EventArgs e)
        {
            this.loss = true;
        }

        // Function checkWin checks if the player already won
        private void CheckWin()
        {
            // They already won if not gameContinues and already lost if loss, so skip in either case
            if (this.gameContinues && !this.loss)
            {
                // Assume there are no safe squares left and we won to check
                this.gameContinues = false;
                // Check every square to see if a safe one is left. If so, it must be clicked before you win.
                // Loop through grid
                for (int x = 0; x < 10; x++)
                {
                    for (int y = 0; y < 10; y++)
                    {
                        // Check if square is safe (checkMine()) and unclicked (clickable)
                        if (!grid[x, y].CheckMine() && grid[x, y].clickable)
                        {
                            // Game's not over, you have to click this one!
                            this.gameContinues = true;
                            // end loop
                            break;
                        }
                    }
                    // If we already found a safe square, end loop
                    if (this.gameContinues)
                    {
                        break;
                    }
                }
                // If we actually DID win
                if (!this.gameContinues)
                {
                    // tell all squares to autoclick. The mines won't lose because gameContinues is false.
                    if (this.tellWin != null)
                    {
                        this.tellWin(this, EventArgs.Empty);
                    }
                    // Stop the timer, count a win, and tell the player they won.
                    timer.Stop();
                    this.wins++;
                    MessageBox.Show("You win!");
                }
            }
        }
        
        // Function restart to restart the game.
        private void Restart(object sender, EventArgs e)
        {
            // Restart each individual cell using the event.
            if(this.reset != null)
            {
                this.reset(this, EventArgs.Empty);
            }
            // Reset some variables to start again.
            this.loss = false;
            this.gameContinues = true;
            this.minesSet = false;
            this.gameTime = 0;
        }
        
        // Function for when the timer counts one second
        private void timerTick(object sender, EventArgs e)
        {
            // Add to the game time, total time, and update the timer.
            this.gameTime++;
            this.totalTime++;
            TimerLabel.Text = $"Timer: {this.gameTime}";
        }

        // Function for when Game --> Show lifetime statistics is clicked
        private void sayGameStats(object sender, EventArgs e)
        {
            String output = "Wins: " + this.wins + "\nLosses: " + this.losses +
                "\nTotal play time: " + this.totalTime;
            MessageBox.Show(output);
        }

        // Function for when Game --> Exit is clicked
        private void Exit(object sender, EventArgs e)
        {
            this.Close();
        }

        // Function for when Game --> Help is clicked
        private void Help(object sender, EventArgs e)
        {
            String message1 = "Click a square if you think it's safe from a mine to reveal it.\n" +
                "The number on a square tells you how many mines are next to it.\n" +
                "Try to figure out where the mines are and where is safe to click.\n" +
                "Can you reveal all the safe squares without clicking a mine?";
            MessageBox.Show(message1);
            String message2 = "Programmed by Ryan Johnson\n" +
                "rjohnso6@uccs.edu\n" +
                "For CS 3020 Summer 2021.";
            MessageBox.Show(message2);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
    }
}
