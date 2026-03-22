using GoEngine;
using System.Text.RegularExpressions;

public class ConsoleInterface
{
    Board board;
    bool blackTurn = true;

    (int x, int y)? lastMove = null;

    bool debug;

    public ConsoleInterface(bool debug)
    {
        board = new Board();
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

            DrawBoard(board, lastMove);

            Console.WriteLine();
            Console.WriteLine($"Turn: {(blackTurn ? "Black (X)" : "White (O)")}");

            Console.Write(">");

            string? input = Console.ReadLine();
            if (input == null) continue;
            input = input.Trim().ToLower();

            if (!HandleCommands(input)) break;


            if (TryParseMove(input, out int x, out int y))
            {
                var stone = blackTurn ? Stone.Black : Stone.White;

                if (board.Place(x, y, stone))
                {
                    lastMove = (x, y);
                    blackTurn = !blackTurn;
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
    }

    private bool HandleCommands(string input)
    {
        if (input == "q") return false;

        if (input == "h")
        {
            ShowHelp();
            if (!debug) Wait();
            return true;
        }

        if (input == "clear")
        {
            board = new Board();
            lastMove = null;
            return true;
        }

        if (input.StartsWith("set"))
        {
            setFromSGF(board, input);
            lastMove = null;
            return true;
        }

        return true;
    }

    static (int x, int y) ParseSGFCoord(string s)
    {
        int x = s[0] - 'a';
        int y = s[1] - 'a';
        return (x, y);
    }
        
    static void Wait()
    {
        Console.WriteLine("Press anything to continue...");
        Console.ReadLine();
    }

    static void setFromSGF(Board board, string sgf)
    {
        board.Clear();

        var blackMatches = Regex.Matches(sgf, @"AB(\[[a-z]{2}\])+");
        var whiteMatches = Regex.Matches(sgf, @"AW(\[[a-z]{2}\])+");

        foreach (Match m in blackMatches)
        {
            foreach (Match coord in Regex.Matches(m.Value, @"\[([a-z]{2})\]"))
            {
                var (x, y) = ParseSGFCoord(coord.Groups[1].Value);
                board.Place(x, y, Stone.Black);
            }
        }

        foreach (Match m in whiteMatches)
        {
            foreach (Match coord in Regex.Matches(m.Value, @"\[([a-z]{2})\]"))
            {
                var (x, y) = ParseSGFCoord(coord.Groups[1].Value);
                board.Place(x, y, Stone.White);
            }
        }
    }

    static void DrawBoard(Board board, (int x, int y)? lastMove)
    {
        int size = Board.Size;

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

                var stone = board.Get(x, y);

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

    static bool TryParseMove(string input, out int x, out int y)
    {
        x = y = -1;

        if (string.IsNullOrWhiteSpace(input) || input.Length < 2)
            return false;

        char letter = char.ToUpper(input[0]);

        if (letter >= 'I') letter--; // skip I

        x = letter - 'A';

        if (!int.TryParse(input.Substring(1), out int row))
            return false;

        y = Board.Size - row;

        return x >= 0 && x < Board.Size &&
               y >= 0 && y < Board.Size;
    }

    static string GetLetter(int index)
    {
        char c = (char)('A' + index);
        if (c >= 'I') c++;
        return c.ToString();
    }
}





