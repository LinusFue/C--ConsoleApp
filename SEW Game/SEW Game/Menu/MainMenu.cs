using SEW_Game.Game;
using System;

namespace SEW_Game.Menu
{
    public class MainMenu
    {
        public MainMenu() { }


        public void printMenu()
        {
            Console.Clear();
            Console.WriteLine("####      #       ####       #       #     #     #     #");
            Console.WriteLine("#   #    # #     #           ##     ##    # #    ##    #");
            Console.WriteLine("#   #   #   #   #            # #   # #   #   #   # #   #");
            Console.WriteLine("####   #######  #            #  # #  #  #######  #  #  #");
            Console.WriteLine("#      #     #  #            #   #   #  #     #  #   # #");
            Console.WriteLine("#      #     #   #           #       #  #     #  #    ##");
            Console.WriteLine("#      #     #    ####       #       #  #     #  #     #");

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Highscore: " + MainGame.GetHightscore());
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Select Option: ");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("r ==> Regeln");
            Console.WriteLine("s ==> Spielen");
            Console.WriteLine("x ==> Exit");
            //Console.SetCursorPosition(0, 13);
            
            var key = Console.ReadKey(true);
            
            MainGame game = new MainGame();

            switch (key.Key)
            {
                case ConsoleKey.R:
                    this.regelwerk();
                    break;

                case ConsoleKey.S:
                    game.Start();
                    break;

                case ConsoleKey.X:
                    Environment.Exit(0);
                    break;
            }
        }

        public void regelwerk()
        {
            Console.Clear();
            Console.WriteLine("Regeln:");
            Console.WriteLine("Du bist Pac Man.");
            Console.WriteLine();
            Console.WriteLine();
            Console.Write("Drücke eine beliebige Taste um zurück zu kommen.");
            Console.ReadKey();
            this.printMenu();
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
