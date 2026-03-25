namespace GoEngine;

public class GameSession
{
    public Position Current { get; private set; }

    private readonly Stack<Position> undoStack = new();
    private readonly Stack<Position> redoStack = new();

    public GameSession(Position start)
    {
        Current = start;
    }

    public bool Play(int x, int y)
    {
        var next = Current.Play(x, y);
        if (next == null)
            return false;

        undoStack.Push(Current);
        Current = next;
        redoStack.Clear();

        return true;
    }

    public void Pass()
    {
        undoStack.Push(Current);
        Current = new Position(Current.Board, Opponent(Current.ToMove), null);
        redoStack.Clear();
    }

    public bool CanUndo => undoStack.Count > 0;
    public bool CanRedo => redoStack.Count > 0;

    public void Undo()
    {
        if (!CanUndo) return;

        redoStack.Push(Current);
        Current = undoStack.Pop();
    }

    public void Redo()
    {
        if (!CanRedo) return;

        undoStack.Push(Current);
        Current = redoStack.Pop();
    }

    private static Player Opponent(Player p)
    {
        return p == Player.Black ? Player.White : Player.Black;
    }
}
