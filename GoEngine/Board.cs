namespace GoEngine;

public enum Stone
{
	Empty,
	Black,
	White
}

public class Board
{
	public const int Size = 13;
	private Stone[,] board = new Stone[Size, Size];

	public Stone Get(int x, int y)
	{
		return board[x,y];
	}

	public bool Place(int x, int y, Stone stone)
	{
		if (board[x,y] != Stone.Empty) return false;

		board[x,y] = stone;
		return true;
	}

}
