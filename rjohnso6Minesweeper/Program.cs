using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace rjohnso6Minesweeper
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Setup the application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Create a window to start the game.
            Form1 board = new Form1();
            Application.Run(board);
        }
    }
}
