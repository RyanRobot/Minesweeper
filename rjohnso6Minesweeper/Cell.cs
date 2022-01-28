using System;
using System.Drawing;
using System.Windows.Forms;

namespace rjohnso6Minesweeper
{
    // Class cell makes up one grid square of the minesweeper game.
    // Can be clicked, might be a mine, contains number of neighbors who are mines.
    public partial class Cell : UserControl
    {
        // A cell's controls will be the button to click the cell,
        // a panel to display a color after it's clicked,
        // and a label to display the number on the cell.
        Panel myPanel = new Panel();
        Button myButton = new Button();
        Label text = new Label();
        // x and y will be the coordinates with a distance of 1 between cells
        public int x;
        public int y;
        // mine will store if this cell is a mine and number will be how many mines are nearby
        bool mine = false;
        int number = 0;
        // clickable will become false after clicked so it's not clicked again
        public bool clickable = true;

        // Our event publishers will include, in order, when the cell is clicked, when the cell is clicked with a zero,
        // and two events for when a mine is clicked.
        // The first will prevent the player from winning, the second will end the game.
        public delegate void EventHandler(object sender, EventArgs e);
        public event EventHandler listeningForClick;
        public event EventHandler zeroClick;
        public event EventHandler loseGame;
        public event EventHandler reserveLoss;
        public Cell(int x, int y)
        {
            InitializeComponent();
            // Initialize our coordinates and size
            this.x = x;
            this.y = y;
            this.Location = new Point(40*x, 40+40*y);
            this.Size = new Size(40, 40);
            // Initialize our components' coordinates and sizes
            myPanel.Size = new Size(40, 40);
            myPanel.Location = new Point(0, 0);
            myButton.Size = new Size(40, 40);
            myButton.Location = new Point(0, 0);
            // Initialize our label
            text.Text = "";
            text.Location = new Point(14, 12);
            this.text.Hide();
            // Add everything to our controls
            this.Controls.Add(myButton);
            this.Controls.Add(myPanel);
            myPanel.Controls.Add(text);
            // Connect the button to its function
            myButton.Click += OnButtonClick;
        }

        private void Cell_Load(object sender, EventArgs e)
        {

        }

        // Function for when button is clicked
        // Display square/mine.
        // If it's a mine, lose the game.
        // If it's a zero, auto click nearby squares
        public void OnButtonClick(object sender, EventArgs e)
        {
            // Make sure this can ONLY be clicked once per game
            if (this.clickable)
            {
                // set clickable to false first
                this.clickable = false;
                // Tell Form1 cell was clicked. It might initialize mines and it will check for if we won.
                if (listeningForClick != null)
                {
                    listeningForClick(this, EventArgs.Empty);
                }
                // Check if this is a mine
                if (this.mine)
                {
                    // Set mine visuals
                    this.myButton.Visible = false;
                    this.myPanel.BackColor = Color.Red;
                    this.text.Text = "!";
                    this.text.Show();
                    // Run the loseGame event to click all cells and display lose message.
                    if(this.loseGame != null && this.reserveLoss != null)
                    {
                        this.reserveLoss(this, EventArgs.Empty);
                        this.loseGame(this, EventArgs.Empty);
                    }
                }
                // If this is not a mine
                else
                {
                    // If this is a zero, no text is necessary. Use zeroClick event to autoclick nearby cells.
                    if (this.number == 0 && zeroClick != null)
                    {
                        this.myButton.Visible = false;
                        this.myPanel.BackColor = Color.Yellow;
                        zeroClick(this, EventArgs.Empty);
                    }
                    // If this is NOT a zero, display text and no further events are required.
                    else
                    {
                        this.myPanel.BackColor = Color.Yellow;
                        this.text.Text = this.number.ToString();
                        this.text.Show();
                        this.myButton.Visible = false;
                    }
                }
            }
        }

        // Function to set this cell to be a mine
        public void SetMine()
        {
            this.mine = true;
        }

        // Function to check if this cell is a mine.
        public bool CheckMine()
        {
            return this.mine;
        }

        // setNumber function
        // At the beginning of the game, this will check its neighbors and initialize number
        // to be how many mines are this cell's neighbor.
        // Sender is Form1 to allow us to look through the grid at our neighbors.
        public void setNumber(object sender, EventArgs e)
        {
            // Loop through nearby cells in a 3x3 square
            // If this is a mine, we don't care about it counting itself. That doesn't matter.
            for(int x = -1; x < 2; x++)
            {
                for(int y = -1; y < 2; y++)
                {
                    // This code will fail if there's an out-of-bounds error (like cell [0,0] checking if cell [-1,-1] is a mine).
                    // Try/catch is a fast way to stop that from happening.
                    try
                    {
                        // Check if our neighbor is a mine and add to our number if it is.
                        // sender is Form1 which is how we see the grid.
                        if(((Form1)(sender)).grid[this.x + x, this.y + y].CheckMine())
                        {
                            this.number += 1;
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }
        // Function OnZeroClick is run anytime a zero cell is clicked anywhere.
        // Check if we're its neighbor, then autoclick if yes.
        public void OnZeroClick(object sender, EventArgs e)
        {
            // Convert sender to cell so we can check if it's our neighbor.
            Cell other = ((Cell)(sender));
            // Check if it's our neighbor, then autoclick.
            if(this.clickable && this.x >= other.x-1 && this.x <= other.x+1 && this.y >= other.y-1 && this.y <= other.y+1)
            {
                this.OnButtonClick(sender, e);
            }
        }
        // Restart function to restart the game and prepare this cell for the next game.
        public void Restart(object sender, EventArgs e)
        {
            // Set all our variables to default
            this.clickable = true;
            this.number = 0;
            this.mine = false;
            // Set our visuals to default
            this.myPanel.BackColor = Color.White;
            this.text.Text = "";
            this.text.Visible = false;
            this.myButton.Visible = true;
            this.text.Hide();
        }
    }
}
