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
        private int updateInterval = 100; // Millisekunden zwischen Updates
        private bool isPaused = false;
        private readonly string highscoreFile = "highscore.txt";
        private int highscore = 0;
        
        // Tunnelkoordinaten
        private readonly (int x, int y)[] tunnels = {
            (0, 14), (27, 14)  // Linker und rechter Tunnel
        };
        
        // Geister
        private List<Ghost> ghosts = new List<Ghost>();
        private Random random = new Random();
        private DateTime lastGhostMove = DateTime.Now;
        private int ghostMoveInterval = 300; // Millisekunden zwischen Geisterbewegungen
        
        // Geister-Klasse
        private class Ghost
        {
            // Change from properties to fields
            public int _x;
            public int _y;
            public char Character { get; set; }
            public ConsoleColor Color { get; set; }
            public GhostBehavior Behavior { get; set; }
            public int LastDirX { get; set; }
            public int LastDirY { get; set; }

            // Add property accessors
            public int X 
            { 
                get => _x;
                set => _x = value;
            }
    
            public int Y
            {
                get => _y;
                set => _y = value;
            }

            public Ghost(int x, int y, char character, ConsoleColor color, GhostBehavior behavior)
            {
                _x = x;
                _y = y;
                Character = character;
                Color = color;
                Behavior = behavior;
                LastDirX = 0;
                LastDirY = 0;
            }
        }
        
        // Enumeration für Geister-Verhalten
        private enum GhostBehavior
        {
            Chase,      // Verfolgt Pac-Man direkt
            Random,     // Bewegt sich zufällig
            Scatter,    // Bewegt sich zu einer Ecke des Spielfelds
            Ambush      // Versucht, Pac-Man abzufangen
        }

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
            InitializeGhosts();
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
        
        private void InitializeGhosts()
        {
            // Geister starten im Zentrum - dem Geisterhaus
            ghosts.Add(new Ghost(13, 14, 'M', ConsoleColor.Red, GhostBehavior.Chase));      // Blinky - Rot
            ghosts.Add(new Ghost(14, 14, 'M', ConsoleColor.Cyan, GhostBehavior.Ambush));    // Inky - Blau
            ghosts.Add(new Ghost(13, 15, 'M', ConsoleColor.DarkYellow, GhostBehavior.Random)); // Clyde - Orange
            ghosts.Add(new Ghost(14, 15, 'M', ConsoleColor.Magenta, GhostBehavior.Scatter)); // Pinky - Rosa
        }

        private void GameLoop()
        {
            Console.Clear();
            DateTime lastUpdate = DateTime.Now;

            while (gameRunning)
            {
                if (isPaused) 
                {
                    Thread.Sleep(100);
                    continue;
                }
                
                if ((DateTime.Now - lastUpdate).TotalMilliseconds >= updateInterval)
                {
                    DrawGame();
                    lastUpdate = DateTime.Now;
                }

                // Geister bewegen
                if ((DateTime.Now - lastGhostMove).TotalMilliseconds >= ghostMoveInterval)
                {
                    MoveGhosts();
                    CheckGhostCollision();
                    lastGhostMove = DateTime.Now;
                }

                if (dots.Count == 0)
                {
                    EndGame(true);
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
                    // Prüfe, ob ein Geist an dieser Position ist
                    Ghost ghost = ghosts.FirstOrDefault(g => g.X == x && g.Y == y);
                    
                    if (x == pacmanX && y == pacmanY)
                    {
                        DrawCharacter(pacmanChar, ConsoleColor.Yellow);
                    }
                    else if (ghost != null)
                    {
                        DrawCharacter(ghost.Character, ghost.Color);
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
                    DrawCharacter('-', ConsoleColor.DarkMagenta);
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
                    pacmanChar = '<';
                    break;
                case ConsoleKey.RightArrow:
                    MovePacman(1, 0);
                    pacmanChar = '>';
                    break;
                case ConsoleKey.UpArrow:
                    MovePacman(0, -1);
                    pacmanChar = '^';
                    break;
                case ConsoleKey.DownArrow:
                    MovePacman(0, 1);
                    pacmanChar = 'v';
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
                
                // Nach jedem Zug prüfen, ob ein Geist getroffen wurde
                CheckGhostCollision();
            }
        }
        
        private void MoveGhosts()
        {
            foreach (var ghost in ghosts)
            {
                switch (ghost.Behavior)
                {
                    case GhostBehavior.Chase:
                        MoveGhostChase(ghost);
                        break;
                    case GhostBehavior.Random:
                        MoveGhostRandom(ghost);
                        break;
                    case GhostBehavior.Scatter:
                        MoveGhostScatter(ghost);
                        break;
                    case GhostBehavior.Ambush:
                        MoveGhostAmbush(ghost);
                        break;
                }
            }
        }
        
        private void MoveGhostChase(Ghost ghost)
        {
            // Direkte Verfolgung - versucht, direkten Weg zu Pac-Man zu finden
            List<(int dx, int dy)> possibleDirections = GetPossibleDirections(ghost.X, ghost.Y);
            
            // Entfernt die Umkehrrichtung, wenn möglich
            if (ghost.LastDirX != 0 || ghost.LastDirY != 0)
            {
                possibleDirections.RemoveAll(d => d.dx == -ghost.LastDirX && d.dy == -ghost.LastDirY);
            }
            
            if (possibleDirections.Count > 0)
            {
                // Bewegt sich in Richtung Pac-Man - wählt die Richtung, die die Distanz verringert
                var bestDirection = possibleDirections
                    .OrderBy(d => CalculateDistance(ghost.X + d.dx, ghost.Y + d.dy, pacmanX, pacmanY))
                    .First();
                
                ghost.X += bestDirection.dx;
                ghost.Y += bestDirection.dy;
                
                ghost.LastDirX = bestDirection.dx;
                ghost.LastDirY = bestDirection.dy;
                
                // Tunnel-Überprüfung
                if (IsTunnel(ghost.X, ghost.Y))
                {
                    HandleTunnelTransport(ref ghost._x, ref ghost._y);
                }
            }
        }
        
        private void MoveGhostRandom(Ghost ghost)
        {
            // Zufällige Bewegung
            List<(int dx, int dy)> possibleDirections = GetPossibleDirections(ghost.X, ghost.Y);
            
            // Entfernt die Umkehrrichtung, wenn möglich (um Hin- und Herbewegung zu reduzieren)
            if (ghost.LastDirX != 0 || ghost.LastDirY != 0 && possibleDirections.Count > 1)
            {
                possibleDirections.RemoveAll(d => d.dx == -ghost.LastDirX && d.dy == -ghost.LastDirY);
            }
            
            if (possibleDirections.Count > 0)
            {
                // Wählt eine zufällige Richtung
                var randomDirection = possibleDirections[random.Next(possibleDirections.Count)];
                
                ghost.X += randomDirection.dx;
                ghost.Y += randomDirection.dy;
                
                ghost.LastDirX = randomDirection.dx;
                ghost.LastDirY = randomDirection.dy;
                
                // Tunnel-Überprüfung
                if (IsTunnel(ghost.X, ghost.Y))
                {
                    HandleTunnelTransport(ref ghost._x, ref ghost._y);
                }
            }
        }
        
        private void MoveGhostScatter(Ghost ghost)
        {
            // Bewegt sich zu einer Ecke des Spielfelds (abhängig vom Geist)
            List<(int dx, int dy)> possibleDirections = GetPossibleDirections(ghost.X, ghost.Y);
            
            // Entfernt die Umkehrrichtung, wenn möglich
            if (ghost.LastDirX != 0 || ghost.LastDirY != 0)
            {
                possibleDirections.RemoveAll(d => d.dx == -ghost.LastDirX && d.dy == -ghost.LastDirY);
            }
            
            if (possibleDirections.Count > 0)
            {
                // Zielecke für diesen Geist (oben links)
                int targetX = 1;
                int targetY = 1;
                
                // Bewegt sich in Richtung der Zielecke
                var bestDirection = possibleDirections
                    .OrderBy(d => CalculateDistance(ghost.X + d.dx, ghost.Y + d.dy, targetX, targetY))
                    .First();
                
                ghost.X += bestDirection.dx;
                ghost.Y += bestDirection.dy;
                
                ghost.LastDirX = bestDirection.dx;
                ghost.LastDirY = bestDirection.dy;
                
                // Tunnel-Überprüfung
                if (IsTunnel(ghost.X, ghost.Y))
                {
                    HandleTunnelTransport(ref ghost._x, ref ghost._y);
                }
            }
        }
        
        private void MoveGhostAmbush(Ghost ghost)
        {
            // Berechnet eine Position vor Pac-Man (in seine Bewegungsrichtung) und versucht, dorthin zu gelangen
            List<(int dx, int dy)> possibleDirections = GetPossibleDirections(ghost.X, ghost.Y);
            
            // Entfernt die Umkehrrichtung, wenn möglich
            if (ghost.LastDirX != 0 || ghost.LastDirY != 0)
            {
                possibleDirections.RemoveAll(d => d.dx == -ghost.LastDirX && d.dy == -ghost.LastDirY);
            }
            
            if (possibleDirections.Count > 0)
            {
                // Berechnet eine Position 4 Felder vor Pac-Man in seine aktuelle Richtung
                int targetX = pacmanX;
                int targetY = pacmanY;
                
                // Ermittelt Pac-Man's Richtung anhand des Charakters
                switch (pacmanChar)
                {
                    case '<': // Links
                        targetX -= 4;
                        break;
                    case '>': // Rechts
                        targetX += 4;
                        break;
                    case '^': // Oben
                        targetY -= 4;
                        break;
                    case 'v': // Unten
                        targetY += 4;
                        break;
                }
                
                // Begrenzt die Zielposition auf das Spielfeld
                targetX = Math.Max(0, Math.Min(width - 1, targetX));
                targetY = Math.Max(0, Math.Min(height - 1, targetY));
                
                // Bewegt sich in Richtung des Ziels
                var bestDirection = possibleDirections
                    .OrderBy(d => CalculateDistance(ghost.X + d.dx, ghost.Y + d.dy, targetX, targetY))
                    .First();
                
                ghost.X += bestDirection.dx;
                ghost.Y += bestDirection.dy;
                
                ghost.LastDirX = bestDirection.dx;
                ghost.LastDirY = bestDirection.dy;
                
                // Tunnel-Überprüfung
                if (IsTunnel(ghost.X, ghost.Y))
                {
                    HandleTunnelTransport(ref ghost._x, ref ghost._y);
                }
            }
        }
        
        private List<(int dx, int dy)> GetPossibleDirections(int x, int y)
        {
            // Gibt alle möglichen Bewegungsrichtungen zurück (die nicht durch Wände blockiert sind)
            var directions = new List<(int dx, int dy)>
            {
                (0, -1), // Oben
                (1, 0),  // Rechts
                (0, 1),  // Unten
                (-1, 0)  // Links
            };
            
            return directions.Where(d => IsValidMove(x + d.dx, y + d.dy)).ToList();
        }
        
        private double CalculateDistance(int x1, int y1, int x2, int y2)
        {
            // Euklidische Distanz
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }
        
        private void CheckGhostCollision()
        {
            foreach (var ghost in ghosts)
            {
                if (ghost.X == pacmanX && ghost.Y == pacmanY)
                {
                    // Pac-Man wurde von einem Geist gefangen
                    EndGame(false);
                    return;
                }
            }
        }

        private bool IsTunnel(int x, int y)
        {
            return tunnels.Contains((x, y));
        }

        private void HandleTunnelTransport(ref int x, ref int y)
        {
            if (x <= 0 && y == 14) x = width - 1;
            else if (x >= width - 1 && y == 14) x = 0;
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
            
            // Beende den Input-Thread
            if (inputThread != null && inputThread.IsAlive)
            {
                inputThread.Join(100);
            }
            
            Console.Clear();
            
            // Speichere Highscore
            SaveHighscore();
            
            // Zeige Endergebnis
            Console.WriteLine(won ? "Gewonnen!" : "Game Over!");
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