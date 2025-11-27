using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Magic_Hunter.src;

public class MenuManager
{
    private Rectangle[] _mainButtons;
    private Rectangle[] _pauseButtons;
    private Rectangle[] _volumeButtons;
    private Rectangle[] _gameOverButtons;
    private Rectangle[] _confirmButtons;

    private string[] _mainOptions = { "Comenzar", "Opciones", "Salir" };
    private string[] _pauseOptions = { "Reanudar", "Opciones", "Menu" };
    private string[] _volumeOptions = { "Volver" };
    private string[] _gameOverOptions = { "Reintentar", "Salir" };
    private string[] _confirmOptions = { "Si", "No" };

    public Texture2D Pixel => _pixel;
    private Texture2D _pixel;
    
    private SpriteFont _font;       
    private SpriteFont _titleFont;  
    
    private string _title = "MAGIC HUNTER"; 
    private int _hoverIndex = -1; 

    public void Initialize(Viewport viewport, GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        int buttonWidth = 220;
        int buttonHeight = 40;
        int spacing = 20;

        _mainButtons = CreateCenteredMenu(viewport, buttonWidth, buttonHeight, spacing, _mainOptions.Length);
        _pauseButtons = CreateCenteredMenu(viewport, buttonWidth, buttonHeight, spacing, _pauseOptions.Length);
        _volumeButtons = CreateCenteredMenu(viewport, buttonWidth, buttonHeight, spacing, _volumeOptions.Length, 50);
        _gameOverButtons = CreateCenteredMenu(viewport, buttonWidth, buttonHeight, spacing, _gameOverOptions.Length, 150);
        
        // Inicializamos botones de confirmación (Importante para evitar el crash)
        _confirmButtons = CreateCenteredMenu(viewport, 100, buttonHeight, spacing + 20, _confirmOptions.Length, 60);
    }

    private Rectangle[] CreateCenteredMenu(Viewport viewport, int w, int h, int spacing, int count, int yOffset = 0)
    {
        Rectangle[] rects = new Rectangle[count];
        int totalHeight = count * (h + spacing) - spacing; 
        int startY = (viewport.Height - totalHeight) / 2 + yOffset;

        for (int i = 0; i < count; i++)
        {
            int x = (viewport.Width - w) / 2;
            int y = startY + i * (h + spacing);
            rects[i] = new Rectangle(x, y, w, h);
        }
        return rects;
    }

    public void LoadContent(SpriteFont font, SpriteFont titleFont = null)
    {
        _font = font;
        _titleFont = titleFont ?? font; 
    }

    // --- INPUTS ---

    public int HandleInput(MouseState current, MouseState previous)
    {
        return CheckButtons(current, previous, _mainButtons, new int[] { 0, 1, 99 });
    }

    public int HandlePauseInput(MouseState current, MouseState previous)
    {
        return CheckButtons(current, previous, _pauseButtons, new int[] { 10, 11, 12 });
    }

    public int HandleVolumeInput(MouseState current, MouseState previous)
    {
        return CheckButtons(current, previous, _volumeButtons, new int[] { 15 });
    }

    public int HandleGameOverInput(MouseState current, MouseState previous)
    {
        return CheckButtons(current, previous, _gameOverButtons, new int[] { 20, 21 });
    }

    public int HandleConfirmInput(MouseState current, MouseState previous)
    {
        // Verificamos que existan los botones para evitar crash
        if (_confirmButtons == null) return -1;
        return CheckButtons(current, previous, _confirmButtons, new int[] { 30, 31 });
    }

    private int CheckButtons(MouseState current, MouseState previous, Rectangle[] buttons, int[] returnValues)
    {
        Point mousePos = new Point(current.X, current.Y);
        _hoverIndex = -1; 

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].Contains(mousePos))
            {
                _hoverIndex = i; 
                if (current.LeftButton == ButtonState.Pressed && previous.LeftButton == ButtonState.Released)
                {
                    return returnValues[i];
                }
            }
        }
        return -1;
    }

    // --- DIBUJADO ---

    public void DrawMainMenu(SpriteBatch spriteBatch, Viewport viewport)
    {
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.Black);
        float scale = 2.0f;
        Vector2 titleSize = _font.MeasureString(_title) * scale;
        Vector2 titlePos = new Vector2((viewport.Width - titleSize.X) / 2, 80);
        spriteBatch.DrawString(_font, _title, titlePos, Color.Purple, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        DrawButtons(spriteBatch, _mainButtons, _mainOptions);
    }

    // Agregamos parámetro opcional 'ignoreHover' para cuando dibujamos esto de fondo
    public void DrawOptionsPanel(SpriteBatch spriteBatch, Viewport viewport, bool ignoreHover = false)
    {
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.Black * 0.7f);
        string title = "PAUSA";
        Vector2 titlePos = new Vector2((viewport.Width - _font.MeasureString(title).X * 1.5f) / 2, 100);
        spriteBatch.DrawString(_font, title, titlePos, Color.White, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);
        DrawButtons(spriteBatch, _pauseButtons, _pauseOptions, isGameOver: false, ignoreHover: ignoreHover);
    }

    public void DrawVolumePanel(SpriteBatch spriteBatch, Viewport viewport)
    {
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.Black * 0.9f);
        string title = "CONFIGURACION";
        Vector2 titlePos = new Vector2((viewport.Width - _font.MeasureString(title).X * 1.5f) / 2, 100);
        spriteBatch.DrawString(_font, title, titlePos, Color.White, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);
        string volText = "Volumen: 100%";
        Vector2 volPos = new Vector2((viewport.Width - _font.MeasureString(volText).X) / 2, 200);
        spriteBatch.DrawString(_font, volText, volPos, Color.Gray);
        DrawButtons(spriteBatch, _volumeButtons, _volumeOptions);
    }

    public void DrawConfirmExit(SpriteBatch spriteBatch, Viewport viewport)
    {
        // Fondo oscuro total para el cartel
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.Black * 0.8f);

        // Caja del cartel
        int boxW = 400;
        int boxH = 300;
        Rectangle bgRect = new Rectangle((viewport.Width - boxW)/2, (viewport.Height - boxH)/2, boxW, boxH);
        
        // Borde Rojo
        spriteBatch.Draw(_pixel, new Rectangle(bgRect.X - 2, bgRect.Y - 2, bgRect.Width + 4, bgRect.Height + 4), Color.Red);
        // Fondo Negro
        spriteBatch.Draw(_pixel, bgRect, Color.Black);

        string text1 = "ADVERTENCIA";
        // CORRECCIÓN CRÍTICA: Quitamos caracteres especiales (¿, @) que rompen PixelFont
        string text2 = "Estas seguro de que quieres salir?\nPerderas todo el progreso";
        
        Vector2 size1 = _font.MeasureString(text1);
        Vector2 pos1 = new Vector2(bgRect.X + (bgRect.Width - size1.X)/2, bgRect.Y + 30);
        
        // Dibujamos texto línea por línea para asegurar centrado
        string[] lines = text2.Split('\n');
        float currentY = pos1.Y + 40;

        spriteBatch.DrawString(_font, text1, pos1, Color.Red);

        foreach (var line in lines)
        {
            Vector2 lineSize = _font.MeasureString(line);
            Vector2 linePos = new Vector2(bgRect.X + (bgRect.Width - lineSize.X)/2, currentY);
            spriteBatch.DrawString(_font, line, linePos, Color.White);
            currentY += lineSize.Y + 5;
        }

        DrawButtons(spriteBatch, _confirmButtons, _confirmOptions);
    }

    public void DrawGameOver(SpriteBatch spriteBatch, Viewport viewport)
    {
        spriteBatch.Draw(_pixel, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.DarkRed * 0.6f);
        string title = "GAME OVER";
        float scale = 3.0f;
        SpriteFont fontToUse = _titleFont ?? _font;
        Vector2 titleSize = fontToUse.MeasureString(title) * scale;
        Vector2 titlePos = new Vector2((viewport.Width - titleSize.X) / 2, viewport.Height * 0.3f);
        spriteBatch.DrawString(fontToUse, title, titlePos + new Vector2(4, 4), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        spriteBatch.DrawString(fontToUse, title, titlePos, Color.Red, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        DrawButtons(spriteBatch, _gameOverButtons, _gameOverOptions, isGameOver: true);
    }

    private void DrawButtons(SpriteBatch spriteBatch, Rectangle[] buttons, string[] labels, bool isGameOver = false, bool ignoreHover = false)
    {
        if (buttons == null) return;

        for (int i = 0; i < buttons.Length; i++)
        {
            // Si ignoreHover es true, nunca iluminamos (útil para dibujar menú en segundo plano)
            bool isHovered = !ignoreHover && (i == _hoverIndex);
            
            Color fillColor = isGameOver ? (isHovered ? Color.Black : Color.DarkRed) : (isHovered ? Color.DarkSlateBlue : Color.Black);
            Color borderColor = isGameOver ? Color.White : (isHovered ? Color.Gold : Color.White);
            Color textColor = isGameOver ? Color.White : (isHovered ? Color.Gold : Color.White);

            spriteBatch.Draw(_pixel, buttons[i], fillColor * 0.8f);
            int b = 2;
            // Bordes
            spriteBatch.Draw(_pixel, new Rectangle(buttons[i].X, buttons[i].Y, buttons[i].Width, b), borderColor);
            spriteBatch.Draw(_pixel, new Rectangle(buttons[i].X, buttons[i].Y + buttons[i].Height - b, buttons[i].Width, b), borderColor);
            spriteBatch.Draw(_pixel, new Rectangle(buttons[i].X, buttons[i].Y, b, buttons[i].Height), borderColor);
            spriteBatch.Draw(_pixel, new Rectangle(buttons[i].X + buttons[i].Width - b, buttons[i].Y, b, buttons[i].Height), borderColor);

            Vector2 textSize = _font.MeasureString(labels[i]);
            Vector2 textPos = new Vector2(buttons[i].X + (buttons[i].Width - textSize.X) / 2, buttons[i].Y + (buttons[i].Height - textSize.Y) / 2);
            spriteBatch.DrawString(_font, labels[i], textPos, textColor);
        }
    }
}