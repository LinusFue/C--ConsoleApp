using SEW_Game.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEW_Game.Menu
{
    public class MainMenu
    {
        public MainMenu() { }

        private string[] logo =
        {
            "\r\n░▒▓███████▓▒░ ░▒▓██████▓▒░ ░▒▓██████▓▒░       ░▒▓██████████████▓▒░ ░▒▓██████▓▒░░▒▓███████▓▒░  \r\n░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░      ░▒▓█▓▒░░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░ \r\n░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░             ░▒▓█▓▒░░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░ \r\n░▒▓███████▓▒░░▒▓████████▓▒░▒▓█▓▒░             ░▒▓█▓▒░░▒▓█▓▒░░▒▓█▓▒░▒▓████████▓▒░▒▓█▓▒░░▒▓█▓▒░ \r\n░▒▓█▓▒░      ░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░             ░▒▓█▓▒░░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░ \r\n░▒▓█▓▒░      ░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░      ░▒▓█▓▒░░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░ \r\n░▒▓█▓▒░      ░▒▓█▓▒░░▒▓█▓▒░░▒▓██████▓▒░       ░▒▓█▓▒░░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░▒▓█▓▒░░▒▓█▓▒░ \r\n                                                                                              \r\n                                                                                              \r\n"
        };

        public void PrintMenu()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.SetWindowSize(93, 40);
            foreach (string logo in logo) 
            {
                Console.WriteLine(logo);
            }

            Console.ForegroundColor= ConsoleColor.Cyan;
            Console.WriteLine("     von Leon Stark");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("                                 Select Option: ");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("                                 r ==> Regeln");
            Console.WriteLine("                                 s ==> Spielen");
            Console.WriteLine("                                 x ==> Exit");
            Console.SetCursorPosition(47, 14);
            
            var key = Console.ReadKey(true);

            MainGame game1 = new MainGame();

            switch (key.Key)
            {
                case ConsoleKey.R:
                    this.Regelwerk();
                    break;

                case ConsoleKey.S:
                    game1.Start();
                    break;

                case ConsoleKey.X:
                    Environment.Exit(0);
                    break;

                default:
                    Console.WriteLine("Fehler!!!");
                    Console.ReadKey(true);
                    this.PrintMenu();
                    break;
            }
        }

        public void Regelwerk()
        {
            Console.Clear();
            Console.WriteLine("Regeln:");
            Console.WriteLine("-------");
            Console.WriteLine();
            Console.WriteLine("Du bist Pac Man und dein Ziel ist es alle Punkte auf dem Spielfeld zu essen.");
            Console.WriteLine("Verwende folgende Tasten zur Steuerung:");
            Console.WriteLine("W....nach oben bewegen");
            Console.WriteLine("A....nach links bewegen");
            Console.WriteLine("S....nach unten bewegen");
            Console.WriteLine("D....nach rechts bewegen");
            Console.WriteLine();
            Console.WriteLine("Aber Achtung! Die bösen Geister wollen dich verspeisen!");
            Console.WriteLine();
            Console.Write("Drücke eine beliebige Taste um zurück zu kommen.");
            Console.ReadKey();
            this.PrintMenu();

        }

        public void Testauswahl()
        {
            string[] options = { "Play", "Options", "Exit" };
            int selectedIndex = 0;

            ConsoleKey key;
            do
            {
                Console.Clear();
                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"> {options[i]}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"  {options[i]}");
                    }
                }

                key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex == 0) ? options.Length - 1 : selectedIndex - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex == options.Length - 1) ? 0 : selectedIndex + 1;
                        break;
                }
            } while (key != ConsoleKey.Enter);

            Console.Clear();
            switch (options[selectedIndex])
            {
                case "Play":
                    Console.WriteLine("Your Are player, Your playing logic will be here");
                    //Game.startGame();
                    break;
                case "Options":
                    Console.WriteLine("Your Are in Options, Your Options logic will be here");
                    //Options.callOptions();
                    break;
                case "Exit":
                    Console.Beep();
                    Environment.Exit(69);
                    break;
                default:
                    Console.WriteLine("Invalid Option");
                    break;
            }
        }
    }
}
