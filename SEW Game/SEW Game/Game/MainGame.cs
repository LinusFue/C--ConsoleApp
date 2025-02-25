using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SEW_Game.Game
{
    public class MainGame
    {
        public MainGame() { }

        private char zeichen = '0';

        private bool running = true;

        private string pacman = "C";

        private int leftPadding = 0;
        
        private int score = 0;

        public void Eingabe()
        {
            ConsoleKeyInfo ein;

            while (zeichen != 'x')
            {
                ein = Console.ReadKey(true);
                zeichen = ein.KeyChar;
            }
        }
        public void Game()
        {

            int x = leftPadding + 1; // lokale Variable
            int y = 0;

            const int MAX_X = 27;
            const int MAX_Y = 29;

            // parallele Methode zum Einlesen des Tastendrucks
            var myThread = new System.Threading.Thread(Eingabe);
            myThread.Start();

            while (zeichen != 'x')
            {
                // "alte" Position => löschen
                Console.SetCursorPosition(x, y);
                Console.WriteLine(" ");

                switch (zeichen)
                {
                    case 'w':
                        if(!checkCollition(x, y - 1)) y--;
                        break;
                    case 'a':
                        if (!checkCollition(x - 1, y)) x--;
                        break;
                    case 's':
                        if(!checkCollition(x, y + 1)) y++;
                        break;
                    case 'd':
                        if(!checkCollition(x + 1, y)) x++;
                        break;
                }

                if (x < leftPadding) x = leftPadding;
                if (y < 0) y = 0;
                if (x > MAX_X + leftPadding) x = MAX_X + leftPadding;
                if (y > MAX_Y) y = MAX_Y;

                Console.SetCursorPosition(x, y);
                Console.WriteLine(pacman);
                System.Threading.Thread.Sleep(200);

            } // END while (zeichen != 'x')

            Console.ForegroundColor = ConsoleColor.Red;
            Console.SetCursorPosition(MAX_X / 2 - 5, MAX_Y / 2);
            Console.WriteLine("Game over!!");
            Console.ReadLine();
        }

        private bool checkCollition(int x, int y)
        {
            if (MapLayout[y][x] == '#')
            {
                return true;
            }
            if (MapLayout[y][x] == '.')
            {
                score++;
                return false;
            }
            return false;
        }


        public string[] MapLayout { get; set; } = {
                "############################",
                "#............##............#",
                "#.####.#####.##.#####.####.#",
                "#.####.#####.##.#####.####.#",
                "#.####.#####.##.#####.####.#",
                "#..........................#",
                "#.####.##.########.##.####.#",
                "#.####.##.########.##.####.#",
                "#......##....##....##......#",
                "######.##### ## #####.######",
                "     #.##### ## #####.#     ",
                "     #.##          ##.#     ",
                "     #.## ###--### ##.#     ",
                "######.## #      # ##.######",
                "      .   #      #   .      ",
                "######.## #      # ##.######",
                "     #.## ######## ##.#     ",
                "     #.##          ##.#     ",
                "     #.## ######## ##.#     ",
                "######.## ######## ##.######",
                "#............##............#",
                "#.####.#####.##.#####.####.#",
                "#.####.#####.##.#####.####.#",
                "#...##................##...#",
                "###.##.##.########.##.##.###",
                "#......##....##....##......#",
                "#.##########.##.##########.#",
                "#.##########.##.##########.#",
                "#..........................#",
                "############################"
            };


        public void PrintMap(string[] map)
        {
            Console.Clear();

            string fileContent = Program.GetFile().ReadToEnd();
            Console.WriteLine("Score: ");
            Console.WriteLine("Highscore: " + fileContent);
            Console.WriteLine();
            Console.WriteLine();

            foreach (string line in map)
            {
                int windowWidth = Console.WindowWidth;
                int textLength = map.Length;
                leftPadding = (windowWidth - textLength) / 2;

                Console.SetCursorPosition(leftPadding, Console.CursorTop);
                Console.WriteLine(line);
            }
        }
    }
}
