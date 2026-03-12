namespace GoUI;

class Program
{

    static void Main(string[] args)
    {
        using var game = new GoBoard.Game1();
        game.Run();
    }
}

