using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEW_Game.Menu;
using SEW_Game.Game;

namespace SEW_Game
{
    public class Program
    {
        static void Main(string[] args)
        {

            MainMenu menu = new MainMenu();
            menu.printMenu();
        }
    }
}
