namespace GoEngine;

public enum Stone
{
	Empty,
	Black,
	White
}

public class Board
{
	public int Size { get; }
	private Stone[] board; 

    public Board(int size=19)
    {
        Size = size;
        board = new Stone[Size * Size];
    }

    private int Index(int x, int y)
    {
        return y * Size + x;
    }

    public Stone Get(int x, int y)
    {
        if (!IsOnBoard(x, y))
            throw new ArgumentOutOfRangeException();

        return board[Index(x, y)];
    }

    public bool IsOnBoard(int x, int y)
    {
        return x >= 0 && x < Size && y >= 0 && y < Size;
    }

    public Board Clone()
    {
        var copy = new Board(Size);
        Array.Copy(board, copy.board, board.Length);
        return copy;
    }

	public bool Place(int x, int y, Stone stone)
    // call only on clone and then replace it with the clone if succesful to avoid bug later think of more clever solution
    // maybe separate legality check and place later
    {
        if (!IsOnBoard(x, y)) return false;
		if (board[Index(x, y)] != Stone.Empty) return false;

		board[Index(x, y)] = stone;
        var opponent = stone == Stone.Black ? Stone.White : Stone.Black;

        foreach (var (nx, ny) in GetNeighbors(x, y))
        {
            if (board[Index(nx, ny)] == opponent)
            {
                var group = GetGroup(nx, ny);
                if (!HasLiberty(group))
                    RemoveGroup(group);
            }
        }

        var myGroup = GetGroup(x, y);
        if (!HasLiberty(myGroup))
        {
            board[Index(x, y)] = Stone.Empty; 
            return false;
        }

        return true;

	}

    public void Clear()
    {
        board = new Stone[Size * Size];
    }

    private IEnumerable<(int x, int y)> GetNeighbors(int x, int y)
    {
        if (x > 0) yield return (x - 1, y);
        if (x < Size - 1) yield return (x + 1, y);
        if (y > 0) yield return (x, y - 1);
        if (y < Size - 1) yield return (x, y + 1);
    }

    private HashSet<(int x, int y)> GetGroup(int x, int y)
    // build data structure for groups later on and incrementally compute (union and find?)
    {

        if (board[Index(x, y)] == Stone.Empty) return new HashSet<(int,int)>();

        Stone color = board[Index(x, y)];
        var visited = new HashSet<(int, int)>();
        var stack = new Stack<(int, int)>();

        stack.Push((x, y));

        while (stack.Count > 0)
        {
            var (cx, cy) = stack.Pop();

            if (visited.Contains((cx, cy))) continue;

            visited.Add((cx, cy));

            foreach (var (nx, ny) in GetNeighbors(cx, cy))
            {
                if (board[Index(nx, ny)] == color) stack.Push((nx, ny));
            }
        }

        return visited;
    }

    private bool HasLiberty(HashSet<(int x, int y)> group)
    {
        foreach (var (x, y) in group)
        {
            foreach (var (nx, ny) in GetNeighbors(x, y))
            {
                if (board[Index(nx, ny)] == Stone.Empty) return true;    
            }

        }
        return false;
    }

    private void RemoveGroup(HashSet<(int x, int y)> group)
    {
        foreach (var (x, y) in group)
            board[Index(x, y)] = Stone.Empty;
    }
}
