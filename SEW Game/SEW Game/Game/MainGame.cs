using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;

namespace SEW_Game.Game
{
    public class MainGame
    {
        private readonly int width = 28;
        private readonly int height = 31;
        private char[,] map;
        private int score = 0;
        private int pacmanX, pacmanY;
        private bool gameRunning = true;
        private List<(int x, int y)> dots = new List<(int x, int y)>();
        private Thread inputThread;
        private char pacmanChar = 'C';
        private int updateInterval = 10; // Millisekunden zwischen Updates
        private bool isPaused = false;
        private readonly string highscoreFile = "highscore.txt";
        private int highscore = 0;
        
        // Tunnelkoordinaten
        private readonly (int x, int y)[] tunnels = {
            (0, 14), (27, 14)  // Linker und rechter Tunnel
        };

        public void Start()
        {
            try
            {
                Console.Title = "Pac-Man Console";
                Console.CursorVisible = false;
                SetConsoleSize();
                LoadHighscore();
                InitializeGame();

                inputThread = new Thread(ReadInput);
                inputThread.Start();

                GameLoop();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void SetConsoleSize()
        {
            try
            {
                Console.SetWindowSize(width + 2, height + 3);
                Console.SetBufferSize(width + 2, height + 3);
            }
            catch
            {
                // Falls die Konsolengröße nicht geändert werden kann, ignorieren
            }
        }

        private void InitializeGame()
        {
            map = new char[height, width];
            dots.Clear();
            score = 0;

            string[] mapLayout = {
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

            InitializeMapFromLayout(mapLayout);
            InitializePacman();
        }

        private void InitializeMapFromLayout(string[] mapLayout)
        {
            for (int y = 0; y < Math.Min(height, mapLayout.Length); y++)
            {
                string line = mapLayout[y];
                for (int x = 0; x < Math.Min(width, line.Length); x++)
                {
                    map[y, x] = line[x];
                    if (map[y, x] == '.')
                    {
                        dots.Add((x, y));
                    }
                }
            }
        }

        private void InitializePacman()
        {
            pacmanX = 14;
            pacmanY = 23;
        }

        private void GameLoop()
        {
            Console.Clear();
            DateTime lastUpdate = DateTime.Now;

            while (gameRunning)
            {
                if (isPaused) return;
                if (!isPaused)
                {
                    if ((DateTime.Now - lastUpdate).TotalMilliseconds >= updateInterval)
                    {
                        DrawGame();
                        lastUpdate = DateTime.Now;
                    }

                    if (dots.Count == 0)
                    {
                        EndGame(true);
                    }
                }
                Thread.Sleep(10); // Reduzierte CPU-Auslastung
            }
        }

        private void DrawGame()
        {
            try
            {
                Console.SetCursorPosition(0, 0);
                DrawHeader();
                DrawMap();
            }
            catch (Exception)
            {
                // Ignoriere Zeichenfehler
            }
        }

        private void DrawHeader()
        {
            Console.WriteLine($"Score: {score}  Highscore: {highscore}".PadRight(width));
            Console.WriteLine(new string('=', width));
        }


        private void DrawMap()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x == pacmanX && y == pacmanY)
                    {
                        DrawCharacter(pacmanChar, ConsoleColor.DarkMagenta);
                    }
                    else
                    {
                        DrawMapElement(x, y);
                    }
                }
                Console.WriteLine();
            }
        }

        private void DrawCharacter(char c, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(c);
            Console.ResetColor();
        }

        private void DrawMapElement(int x, int y)
        {
            switch (map[y, x])
            {
                case '#':
                    DrawCharacter('█', ConsoleColor.Blue);
                    break;
                case '.':
                    DrawCharacter('·', ConsoleColor.White);
                    break;
                case '-':
                    DrawCharacter('-', ConsoleColor.Magenta);
                    break;
                default:
                    Console.Write(' ');
                    break;
            }
        }

        private void ReadInput()
        {
            while (gameRunning)
            {
                var key = Console.ReadKey(true);
                HandleInput(key);
            }
        }

        private void HandleInput(ConsoleKeyInfo key)
        {
            if (isPaused && key.Key != ConsoleKey.P && key.Key != ConsoleKey.Escape)
                return;

            switch (key.Key)
            {
                case ConsoleKey.LeftArrow:
                    MovePacman(-1, 0);
                    pacmanChar = 'C';
                    break;
                case ConsoleKey.RightArrow:
                    MovePacman(1, 0);
                    pacmanChar = 'C';
                    break;
                case ConsoleKey.UpArrow:
                    MovePacman(0, -1);
                    pacmanChar = 'C';
                    break;
                case ConsoleKey.DownArrow:
                    MovePacman(0, 1);
                    pacmanChar = 'C';
                    break;
                case ConsoleKey.P:
                    TogglePause();
                    break;
                case ConsoleKey.Escape:
                    EndGame(false);
                    break;
            }
        }

        private void MovePacman(int dx, int dy)
        {
            int newX = pacmanX + dx;
            int newY = pacmanY + dy;

            // Tunnel-Überprüfung
            if (IsTunnel(newX, newY))
            {
                HandleTunnelTransport(ref newX, ref newY);
            }

            if (IsValidMove(newX, newY))
            {
                CollectDot(newX, newY);
                pacmanX = newX;
                pacmanY = newY;
            }
        }

        private bool IsTunnel(int x, int y)
        {
            return tunnels.Contains((x, y));
        }

        private void HandleTunnelTransport(ref int x, ref int y)
        {
            if (x < 0) x = width - 1;
            else if (x >= width) x = 0;
        }

        private bool IsValidMove(int x, int y)
        {
            return (x >= 0 && x < width && y >= 0 && y < height && map[y, x] != '#') ||
                   IsTunnel(x, y);
        }

        private void CollectDot(int x, int y)
        {
            if (map[y, x] == '.')
            {
                score += 1;
                map[y, x] = ' ';
                dots.RemoveAll(d => d.x == x && d.y == y);
            }
        }

        private void TogglePause()
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                DrawPauseScreen();
            }
        }

        private void DrawPauseScreen()
        {
            Console.SetCursorPosition(width / 2 - 5, height / 2);
            Console.WriteLine("PAUSED");
        }

        private void EndGame(bool won)
        {
            gameRunning = false;
            Console.Clear();
            
            // Speichere Highscore
            SaveHighscore();
            
            // Zeige Endergebnis
            Console.WriteLine(won ? "Gewonnen!" : "Spiel beendet");
            Console.WriteLine($"Deine Punktzahl: {score}");
            Console.WriteLine($"Highscore: {highscore}");
            
            if (score > highscore)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Neuer Highscore!");
                Console.ResetColor();
            }
            
            Console.WriteLine("\nDrücke eine Taste zum Beenden...");
            Console.ReadKey(true);
        }
        
        private void SaveHighscore()
        {
            try
            {
                if (score > highscore)
                {
                    highscore = score;
                    using (var writer = new StreamWriter(new FileStream(highscoreFile, FileMode.Create, FileAccess.Write, FileShare.Read)))
                    {
                        writer.WriteLine(score);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Speichern des Highscores: {ex.Message}");
            }
        }


        
        private void LoadHighscore()
        {
            try
            {
                if (!File.Exists(highscoreFile))
                {
                    File.WriteAllText(highscoreFile, "0");
                }

                using (var reader = new StreamReader(new FileStream(highscoreFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    string content = reader.ReadLine();
                    if (!string.IsNullOrEmpty(content))
                    {
                        int.TryParse(content, out highscore);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Laden des Highscores: {ex.Message}");
                highscore = 0;
            }
        }



        private void HandleError(Exception ex)
        {
            Console.Clear();
            Console.WriteLine($"Ein Fehler ist aufgetreten: {ex.Message}");
            Console.WriteLine("Drücke eine Taste zum Beenden...");
            Console.ReadKey();
        }
        
        public static int GetHightscore()
        {
            string filePath = "highscore.txt";

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "0");
            }

            using (var reader = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                string content = reader.ReadLine();
                if (!string.IsNullOrEmpty(content) && int.TryParse(content, out int hs))
                {
                    return hs;
                }
            }

            return 0;
        }
    }
}