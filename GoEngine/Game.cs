namespace GoEngine;

public class GameSession
{
    private  Position Current;

    private readonly Stack<Position> undoStack = new();
    private readonly Stack<Position> redoStack = new();

    public (int x, int y)? LastMove { get; private set; }

    public GameSession(int size = 19)
    // consturctor for UIs
    {
        var board = new Board(size);
        Current = new Position(board, Player.Black);
    }

    public GameSession(Position start)
    // load game with a position
    {
        Current = start;
    }

    public int Size => Current.Size;

    public Stone GetStone(int x, int y)
    {
        return Current.Get(x, y);
    }

    public Player ToMove => Current.ToMove;

    public bool Play(int x, int y)
    {
        var next = Current.Play(x, y);
        if (next == null) return false;

        undoStack.Push(Current);
        Current = next;
        redoStack.Clear();

        LastMove = (x, y);

        return true;
    }

    public void Pass()
    {
        undoStack.Push(Current);
        Current = Current.Pass(); 
        redoStack.Clear();
        LastMove = null;
    }

    public void Reset(int size)
    {
        Current = new Position(new Board(size), Player.Black);
        undoStack.Clear();
        redoStack.Clear();
        LastMove = null;
    }

    public bool CanUndo => undoStack.Count > 0;
    public bool CanRedo => redoStack.Count > 0;

    public void Undo()
    {
        if (!CanUndo) return;

        redoStack.Push(Current);
        Current = undoStack.Pop();
        LastMove = null;
    }

    public void Redo()
    {
        if (!CanRedo) return;

        undoStack.Push(Current);
        Current = redoStack.Pop();
        LastMove = null;
    }

}
