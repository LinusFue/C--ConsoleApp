using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEW_Game.Menu;

namespace SEW_Game
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.SetWindowSize(75, 37);
            MainMenu menu1 = new MainMenu();
            menu1.PrintMenu();

            
        }

        public static StreamReader GetFile()
        {
            string filePath = "highscore.txt";
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }
            return new StreamReader(filePath);
        }
    }
}
