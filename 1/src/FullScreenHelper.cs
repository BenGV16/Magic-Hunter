using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Magic_Hunter.src;
public static class FullscreenHelper
{
    public static void ApplyFullscreen(
        GraphicsDeviceManager graphics,
        GameWindow window,
        ref Point windowedSize,
        ref Point windowedPosition)
    {
        windowedSize = new Point(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
        windowedPosition = new Point(window.Position.X, window.Position.Y);
        graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        graphics.HardwareModeSwitch = true;
        graphics.IsFullScreen = true;
        graphics.ApplyChanges();
    }
}