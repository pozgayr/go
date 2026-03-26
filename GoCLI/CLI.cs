using GoEngine;
using System.Text.RegularExpressions;
using System.IO;
using GoSgf;

namespace GoCli;

public class ConsoleInterface
{
    GameSession game;

    (int x, int y)? lastMove = null;

    bool debug;

    public ConsoleInterface(bool debug)
    {
        var startBoard = new Board();
        var startPosition = new Position(startBoard, Player.Black);
        game = new GameSession(startPosition);
        this.debug = debug;
    }

    public void Loop()
    {

        while (true)
        {
            if (!debug)
            {
                Console.Clear();
                Console.SetCursorPosition(0, 0);
            }

            DrawBoard(game.Current, lastMove);

            Console.WriteLine();
            Console.WriteLine($"Turn: {(game.Current.ToMove == Player.Black ? "Black (X)" : "White (O)")}");

            Console.Write(">");

            string? input = Console.ReadLine();
            if (input == null) continue;


            if (!HandleCommands(input, out bool handled)) break;

            if (handled) continue;

            if (TryParseMove(input, out int x, out int y))
            {
               if (game.Play(x, y))
                {
                    lastMove = (x, y);
                }
                else
                {
                    Console.WriteLine("Illegal move");
                    if (!debug) Wait();
                }
            }
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("Commands:");
        Console.WriteLine("  eg. D4    place stone");
        Console.WriteLine("  set ...   set board position");
        Console.WriteLine("  clear     clear board");
        Console.WriteLine("  h         help");
        Console.WriteLine("  q         quit");
        Console.WriteLine("  size      set board size");
    }

    void ResetGame(int size)
    {
        var board = new Board(size);
        var position = new Position(board, Player.Black);
        game = new GameSession(position);
    }

    private bool HandleCommands(string input, out bool handled)
    {
        handled = true;

        if (input.Equals("q", StringComparison.OrdinalIgnoreCase))
            return false;

        if (input.Equals("h", StringComparison.OrdinalIgnoreCase))
        {
            ShowHelp();
            if (!debug) Wait();
            return true;
        }

        if (input.Equals("clear", StringComparison.OrdinalIgnoreCase))
        {
            ResetGame(game.Current.Size);
            lastMove = null;
            return true;
        }

        if (input.StartsWith("set", StringComparison.OrdinalIgnoreCase))
        {
            var parts = input.Split(' ',  StringSplitOptions.RemoveEmptyEntries);

            bool fromFile = false;
            bool setupOnly = false;
            string? argument = null;

            // --- parse flags ---
            for (int j = 1; j < parts.Length; j++)
            {
                if (parts[j] == "-f")
                {
                    fromFile = true;
                }
                else if (parts[j] == "-s")
                {
                    setupOnly = true;
                }
                else
                {
                    argument = parts[j];
                    break;
                }
            }

            if (argument == null)
            {
                Console.WriteLine("Missing SGF input");
                if (!debug) Wait();
                return true;
            }
            try
            {
                string sgfText;

                if (fromFile)
                {
                    if (!File.Exists(argument))
                    {
                        Console.WriteLine("File not found");
                        if (!debug) Wait();
                        return true;
                    }

                    sgfText = File.ReadAllText(argument);
                }
                else
                {
                    sgfText = input.Substring(input.IndexOf(argument));
                }
                game = GoSgf.SgfLoader.LoadMainLine(sgfText, setupOnly);
                lastMove = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load SGF: {ex.Message}");
                if (!debug) Wait();
            }

            return true;
        }

        if (input.StartsWith("size ", StringComparison.OrdinalIgnoreCase))
        {
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2 && int.TryParse(parts[1], out int size) && size > 0)
            {
                ResetGame(size);
                lastMove = null;
            }

            return true;
        }

        handled = false;
        return true;
    }

    static void Wait()
    {
        Console.WriteLine("Press anything to continue...");
        Console.ReadLine();
    }

    static void DrawBoard(Position pos, (int x, int y)? lastMove)
    {
        int size = pos.Size;

        Console.Write("   ");
        for (int x = 0; x < size; x++)
            Console.Write(GetLetter(x) + " ");
        Console.WriteLine();

        for (int y = 0; y < size; y++)
        {
            int row = size - y;
            Console.Write($"{row,2} ");

            for (int x = 0; x < size; x++)
            {
                bool isLast = lastMove.HasValue &&
                              lastMove.Value.x == x &&
                              lastMove.Value.y == y;

                var stone = pos.Get(x, y);

                if (isLast)
                    Console.ForegroundColor = ConsoleColor.Green;

                char c = stone switch
                {
                    Stone.Black => 'X',
                    Stone.White => 'O',
                    _ => '+'
                };

                Console.Write(c + " ");
                Console.ResetColor();
            }

            Console.WriteLine();
        }
    }

    bool TryParseMove(string input, out int x, out int y)
    {
        input = input.Trim();
        x = y = -1;

        int size = game.Current.Size;

        if (string.IsNullOrWhiteSpace(input) || input.Length < 2)
            return false;

        char letter = char.ToUpper(input[0]);

        if (letter == 'I') return false;

        if (letter > 'I') letter--;

        x = letter - 'A';

        if (!int.TryParse(input.Substring(1), out int row))
            return false;

        y = size - row;

        return x >= 0 && x < size &&
               y >= 0 && y < size;
    }

    static string GetLetter(int index)
    {
        char c = (char)('A' + index);
        if (c >= 'I') c++;
        return c.ToString();
    }
}




