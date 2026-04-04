using GoEngine;
using System.Text.RegularExpressions;
using System.IO;
using GoSgf;

namespace GoCli;

public class ConsoleInterface
{
    GameSession game;
    bool debug;

    public ConsoleInterface(bool debug)
    {
        game = new GameSession();
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

            DrawBoard(game);

            Console.WriteLine();
            Console.WriteLine($"Turn: {(game.ToMove == Player.Black ? "Black (X)" : "White (O)")}");

            Console.Write(">");

            string? input = Console.ReadLine();
            if (input == null) continue;


            if (!HandleCommands(input, out bool handled)) break;

            if (handled) continue;

            if (TryParseMove(input, out int x, out int y))
            {
               if (!game.Play(x, y))
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
        game = new GameSession(size);
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

        handled = CommandHandler.Execute(ref game, input);
        return true;
    }

    static void Wait()
    {
        Console.WriteLine("Press anything to continue...");
        Console.ReadLine();
    }

    // drawing functions note: go boards skip I

    static void DrawBoard(GameSession game)
    {
        int size = game.Size;

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
                bool isLast = game.LastMove.HasValue &&
                              game.LastMove.Value.x == x &&
                              game.LastMove.Value.y == y;

                var stone = game.GetStone(x, y);

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

        int size = game.Size;

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




