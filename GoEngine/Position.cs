namespace GoEngine;

public enum Player
{
    Black,
    White
}

public class Position
{
    public Board Board { get; }
    public Player ToMove { get; }
    public (int x, int y)? KoPoint { get; }

    public Position(Board board, Player toMove, (int x, int y)? ko=null)
    {
        Board = board;
        ToMove = toMove;
        KoPoint = ko;
    }

    private static Stone ToStone(Player p)
    {
        return p == Player.Black ? Stone.Black : Stone.White;
    }

    private static Player Opponent(Player p)
    {
        return p == Player.Black ? Player.White : Player.Black;
    }

    public bool IsLegal(int x, int y)
    {
        if (KoPoint.HasValue && KoPoint.Value == (x, y))
            return false;

        // make move on a board copy
        var copy = Board.Clone();
        return copy.Place(x, y, ToStone(ToMove)); //also checks out of bounds and occupancy
    }

    public Position? Play(int x, int y)
    // make return new position makes position immutable from outside
    {
        if (!IsLegal(x, y)) return null;

        var nextBoard = Board.Clone();

        // apply move
        nextBoard.Place(x, y, ToStone(ToMove));

        (int x, int y)? newKo = ComputeKo(Board, nextBoard);

        return new Position(nextBoard, Opponent(ToMove), newKo);
    }

    public Position Pass()
    {
        return new Position(Board, Opponent(ToMove), null);
    }

    private (int x, int y)? ComputeKo(Board before, Board after)
    // iterates through whole board maybe think about doing this smarter
    {
        var removed = new List<(int, int)>();

        for (int y = 0; y < before.Size; y++)
        {
            for (int x = 0; x < before.Size; x++)
            {
                if (before.Get(x, y) != Stone.Empty &&
                    after.Get(x, y) == Stone.Empty)
                {
                    removed.Add((x, y));
                }
            }
        }

        if (removed.Count == 1)
            return removed[0];

        return null;
    }

    public IEnumerable<(int x, int y)> GenerateMoves()
    // move generation slow for now
    {
        for (int y = 0; y < Board.Size; y++)
        {
            for (int x = 0; x < Board.Size; x++)
            {
                if (IsLegal(x, y))
                    yield return (x, y);
            }
        }
    }

    public Stone Get(int x, int y)
    {
        return Board.Get(x, y);
    }

    public int Size => Board.Size;
}
