using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using GoEngine;
using GoCli;

namespace GoBoard;

public class Game1 : Game
{
    MouseState previousMouse;
    KeyboardState previousKeyboard;

    GameSession game;

    bool consoleActive = false;
    string consoleInput = "";

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

        game = new GameSession();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        MouseState mouse = Mouse.GetState();
        int size = game.Size;

        KeyboardState keyboard = Keyboard.GetState();

        if (consoleActive)
        {
            foreach (Keys key in keyboard.GetPressedKeys())
            {
                if (previousKeyboard.IsKeyUp(key))
                {
                    if (key == Keys.Enter)
                    {

                        try
                        {
                            CommandHandler.Execute(ref game, consoleInput);
                        }
                        catch (Exception ex)
                        {
                            consoleInput = "ERROR: " + ex.Message;
                        }
                        consoleActive = false;
                    }
                    else if (key == Keys.Back && consoleInput.Length > 0)
                    {
                        consoleInput = consoleInput[..^1];
                    }
                    else if (key == Keys.Escape)
                    {
                        consoleActive = false;
                    }
                    else
                    {
                        char c = KeyToChar(key);
                        if (c != '\0')
                            consoleInput += c;
                    }
                }
            } 
        }
        else keyboardActions();



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
        previousKeyboard = keyboard;

        base.Update(gameTime);
    }

    void keyboardActions()
    {
        KeyboardState keyboard = Keyboard.GetState();

        if (keyboard.IsKeyDown(Keys.R) && previousKeyboard.IsKeyUp(Keys.R))
        {
            game.Reset(game.Size);
        }

        if (keyboard.IsKeyDown(Keys.U) && previousKeyboard.IsKeyUp(Keys.U))
        {
            game.Undo();
        }

        if (keyboard.IsKeyDown(Keys.Y) && previousKeyboard.IsKeyUp(Keys.Y))
        {
            game.Redo();
        }

        if (keyboard.IsKeyDown(Keys.OemQuestion) && previousKeyboard.IsKeyUp(Keys.OemQuestion))
        {
            consoleActive = true;
            consoleInput = "";
        }
        
    }


    string GetLetter(int index)
    {
        char c = (char)('A' + index);
        if (c >= 'I') c++;
        return c.ToString();
    }

    char KeyToChar(Keys key)
    {
        if (key >= Keys.A && key <= Keys.Z)
            return (char)('a' + (key - Keys.A));

        if (key >= Keys.D0 && key <= Keys.D9)
            return (char)('0' + (key - Keys.D0));

        if (key == Keys.Space) return ' ';
        if (key == Keys.OemMinus) return '-';
        if (key == Keys.OemPeriod) return '.';
        if (key == Keys.OemComma) return ',';
        if (key == Keys.OemQuestion) return '/'; // depends on layout
        if (key == Keys.OemPlus) return '+';

        return '\0';
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(240, 200, 120)); // wood-like color

        _spriteBatch.Begin();
        int size = game.Size;

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

            Stone stone = game.GetStone(x, y);

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

            bool isLast = game.LastMove.HasValue &&
                          game.LastMove.Value.x == x &&
                          game.LastMove.Value.y == y;

            if (isLast)
            {
                Color outlineColor = stone == Stone.Black
                    ? Color.White
                    : Color.Black;

                DrawCircleOutline(
                    _spriteBatch,
                    screenX,
                    screenY,
                    cellSize / 5,
                    outlineColor);
            }
        }

        if (consoleActive)
        {
            _spriteBatch.Draw(pixel,
                new Rectangle(50, 800, 800, 40),
                Color.Black * 0.8f);

            _spriteBatch.DrawString(font,
                "> " + consoleInput,
                new Vector2(60, 810),
                Color.White);
        }

        string text = game.ToMove == Player.Black ? "Black to move" : "White to move";

        _spriteBatch.DrawString(font, text, new Vector2(750, 800), Color.Black);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    void DrawCircleOutline(SpriteBatch sb, int centerX, int centerY, int radius, Color color)
    {
        for (int y = -radius; y <= radius; y++)
        for (int x = -radius; x <= radius; x++)
        {
            // no sqrt → faster
            if (x * x + y * y <= radius * radius)
            {
                sb.Draw(pixel,
                    new Rectangle(centerX + x, centerY + y, 1, 1),
                    color);
            }
        }
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
