using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GoEngine;

namespace GoBoard;

public class Game1 : Game
{
    MouseState previousMouse;

    GameSession game;

    int cellSize = 40;
    int margin = 60;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    SpriteFont font;

    Texture2D pixel;
    Texture2D blackStone;
    Texture2D whiteStone;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);

        _graphics.PreferredBackBufferWidth = 900;
        _graphics.PreferredBackBufferHeight = 900;

        _graphics.ApplyChanges();
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        pixel = new Texture2D(GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });
        font = Content.Load<SpriteFont>("Font");

        blackStone = CreateStoneTexture(Color.Black);
        whiteStone = CreateStoneTexture(Color.White);

        var startBoard = new Board();
        var startPosition = new Position(startBoard, Player.Black);
        game = new GameSession(startPosition);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        MouseState mouse = Mouse.GetState();
        int size = game..Size;

        if (mouse.LeftButton == ButtonState.Pressed &&
            previousMouse.LeftButton == ButtonState.Released)
        {
            int bx = (mouse.X - margin + cellSize / 2) / cellSize;
            int by = (mouse.Y - margin + cellSize / 2) / cellSize;

            if (bx >= 0 && bx < size && by >= 0 && by < size)
            {
                game.Play(bx, by);
            }
        }

        previousMouse = mouse;

        base.Update(gameTime);
    }


    string GetLetter(int index)
    {
        char c = (char)('A' + index);
        if (c >= 'I') c++;
        return c.ToString();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(240, 200, 120)); // wood-like color

        _spriteBatch.Begin();
        int size = game..Size;

        for (int i = 0; i < size; i++)
        {
            int pos = margin + i * cellSize;

            _spriteBatch.Draw(pixel,
                new Rectangle(margin, pos, cellSize * (size - 1), 2),
                Color.Black);

            _spriteBatch.Draw(pixel,
                new Rectangle(pos, margin, 2, cellSize * (size - 1)),
                Color.Black);
        }

        for (int x = 0; x < size; x++)
        {
            int screenX = margin + x * cellSize;

            _spriteBatch.DrawString(
                font,
                GetLetter(x),
                new Vector2(screenX - 6, margin - 30),
                Color.Black);
        }

        for (int y = 0; y < size; y++)
        {
            int screenY = margin + y * cellSize;

            string number = (size - y).ToString();

            _spriteBatch.DrawString(
                font,
                number,
                new Vector2(margin - 35, screenY - 10),
                Color.Black);
        }

        for (int x = 0; x < size; x++)
        for (int y = 0; y < size; y++)
        {

            Stone stone = game..Get(x, y);

            if (stone == Stone.Empty)
                continue;

            int screenX = margin + x * cellSize;
            int screenY = margin + y * cellSize;

            Texture2D tex = stone == Stone.Black ? blackStone : whiteStone;

            _spriteBatch.Draw(
                tex,
                new Rectangle(
                    screenX - cellSize / 2,
                    screenY - cellSize / 2,
                    cellSize,
                    cellSize),
                Color.White);
        }

        string text = game..ToMove == Player.Black
            ? "Black to move"
            : "White to move";

        _spriteBatch.DrawString(font, text, new Vector2(750, 800), Color.Black);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    Texture2D CreateStoneTexture(Color color)
    {
        int size = cellSize;
        Texture2D tex = new Texture2D(GraphicsDevice, size, size);

        Color[] data = new Color[size * size];
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = x - radius;
            float dy = y - radius;

            if (dx * dx + dy * dy <= radius * radius)
                data[y * size + x] = color;
            else
                data[y * size + x] = Color.Transparent;
        }

        tex.SetData(data);
        return tex;
    }
}
