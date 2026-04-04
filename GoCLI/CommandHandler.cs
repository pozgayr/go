using GoEngine;
using System;
using System.IO;
using GoSgf;

public static class CommandHandler
{
    public static bool Execute(ref GameSession game, string input)
    {
        if (input.Equals("clear", StringComparison.OrdinalIgnoreCase))
        {
            game.Reset(game.Size);
            return true;
        }

        if (input.StartsWith("size ", StringComparison.OrdinalIgnoreCase))
        {
            var parts = input.Split(' ');
            if (parts.Length == 2 && int.TryParse(parts[1], out int size))
            {
                game = new GameSession(size);
            }
            return true;
        }

        if (input.StartsWith("set", StringComparison.OrdinalIgnoreCase))
        {
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            bool fromFile = false;
            bool setupOnly = false;
            string? argument = null;

            for (int i = 1; i < parts.Length; i++)
            {
                if (parts[i] == "-f") fromFile = true;
                else if (parts[i] == "-s") setupOnly = true;
                else { argument = parts[i]; break; }
            }

            if (argument == null) return true;

            string sgfText = fromFile
                ? File.ReadAllText(argument)
                : input.Substring(input.IndexOf(argument));

            game = GoSgf.SgfLoader.LoadMainLine(sgfText, setupOnly);
            return true;
        }

        return false;
    }
}
