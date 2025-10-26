using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Magic_Hunter.src;

public class MenuManager
{
    public Rectangle[] Buttons { get; private set; }
    public int SelectedIndex { get; private set; }
    public string[] Options { get; } = { "Start", "Options", "Exit" };
    public Texture2D Pixel => _pixel;
    private Texture2D _pixel;

    public void Initialize(Viewport viewport, GraphicsDevice graphicsDevice)
    {
        Buttons = new Rectangle[Options.Length];
        int buttonWidth = 200;
        int buttonHeight = 40;
        int spacing = 20;
        for (int i = 0; i < Options.Length; i++)
        {
            int x = (viewport.Width - buttonWidth) / 2;
            int y = (viewport.Height - (Options.Length * (buttonHeight + spacing))) / 2 + i * (buttonHeight + spacing);
            Buttons[i] = new Rectangle(x, y, buttonWidth, buttonHeight);
        }
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }
    public int HandleInput(MouseState mouseState)
    {
        Point mousePosition = new Point(mouseState.X, mouseState.Y);
        for (int i = 0; i < Buttons.Length; i++)
        {
            if (Buttons[i].Contains(mousePosition))
            {
                SelectedIndex = i;
                if (mouseState.LeftButton == ButtonState.Pressed)
                    return i;
                break;
            }
        }
        return -1;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < Buttons.Length; i++)
        {
            Color color = (i == SelectedIndex) ? Color.Yellow : Color.White;
            spriteBatch.Draw(_pixel, Buttons[i], color * 0.7f);

            int borderThickness = 2;
            Color borderColor = (i == SelectedIndex) ? Color.Gold : Color.Gray;

            var top = new Rectangle(Buttons[i].X, Buttons[i].Y, Buttons[i].Width, borderThickness);
            var bottom = new Rectangle(Buttons[i].X, Buttons[i].Y + Buttons[i].Height - borderThickness, Buttons[i].Width, borderThickness);
            var left = new Rectangle(Buttons[i].X, Buttons[i].Y, borderThickness, Buttons[i].Height);
            var right = new Rectangle(Buttons[i].X + Buttons[i].Width - borderThickness, Buttons[i].Y, borderThickness, Buttons[i].Height);

            spriteBatch.Draw(_pixel, top, borderColor);
            spriteBatch.Draw(_pixel, bottom, borderColor);
            spriteBatch.Draw(_pixel, left, borderColor);
            spriteBatch.Draw(_pixel, right, borderColor);
        }
    }
}