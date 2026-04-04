using GoEngine;

namespace GoSgf;


public static class SgfLoader
{
    public static GameSession LoadMainLine(string sgfText, bool setupOnly)
    {
        var tree = Sgf.Parse(sgfText);

        if (tree.Sequence.Count == 0)
            throw new Exception("SGF has no root node");

        var root = tree.Sequence[0];

        int size = 19; //default size
        if (root.Props.TryGetValue("SZ", out var sz))
            size = int.Parse(sz[0]);

        var board = new Board(size);

        ApplySetup(board, root);

        Player toMove = Player.Black;

        if (root.Props.TryGetValue("PL", out var pl))
        {
            if (pl[0] == "W")
                toMove = Player.White;
        }

        var position = new Position(board, toMove); 
        var game = new GameSession(position);

        if (setupOnly) return game;

        var current = tree;

        while (true) 
        {
            foreach (var node in current.Sequence)
            {
                ApplyMove(game, node);
            }
            if (current.Children.Count == 0)
                break;

            current = current.Children[0]; // main line
        }

        return game;
    }

    private static void ApplySetup(Board board, SgfNode node)
    {
        if (node.Props.TryGetValue("AB", out var blacks))
        {
            foreach (var p in blacks)
            {
                var (x, y) = ParsePoint(p);
                board.Place(x, y, Stone.Black); 
            }
        }

        if (node.Props.TryGetValue("AW", out var whites))
        {
            foreach (var p in whites)
            {
                var (x, y) = ParsePoint(p);
                board.Place(x, y, Stone.White);
            }
        }
    }

    private static void ApplyMove(GameSession game, SgfNode node)
    {
        if (node.Props.TryGetValue("B", out var b))
        {
            PlayMove(game, b[0]);
        }
        else if (node.Props.TryGetValue("W", out var w))
        {
            PlayMove(game, w[0]);
        }
    }

    private static void PlayMove(GameSession game, string value)
    {
        // pass move
        if (string.IsNullOrEmpty(value))
        {
            game.Pass();
            return;
        }

        var (x, y) = ParsePoint(value);

        if (!game.Play(x, y))
            throw new Exception($"Illegal move at ({x},{y})");
    }

    private static (int x, int y) ParsePoint(string s)
    {
        if (s.Length != 2)
            throw new Exception($"Invalid point '{s}'");

        return (s[0] - 'a', s[1] - 'a');
    }
}
