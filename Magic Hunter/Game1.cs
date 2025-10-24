using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Magic_Hunter;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private enum GameState
    {
        Menu,
        Playing
    }
    private GameState _currentState = GameState.Menu;

    // Variables del menú
    private Rectangle[] _menuButtons;
    private string[] _menuOptions = { "Start", "Options", "Exit" };
    private int _selectedIndex = 0;
    private Texture2D _pixel;
    private KeyboardState _previousKeyboardState;
    private bool _isFullscreen = false;
    private Point _windowedSize;
    private Point _windowedPosition;

    private void HandleMenuInput()
    {
        var mouseState = Mouse.GetState();
        var mousePosition = new Point(mouseState.X, mouseState.Y);

        // Verificar si el mouse está sobre algún botón
        for (int i = 0; i < _menuButtons.Length; i++)
        {
            if (_menuButtons[i].Contains(mousePosition))
            {
                _selectedIndex = i;
                
                // Si hace clic
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    if (i == 0) // Start
                    {
                        _currentState = GameState.Playing;
                    }
                    else if (i == 1) // Options
                    {
                        System.Diagnostics.Debug.WriteLine("Options selected");
                    }
                    else if (i == 2) // Exit
                    {
                        Exit();
                    }
                }
                break;
            }
        }
    }
    private void DrawMenu()
    {
        // Dibujar botones del menú
        for (int i = 0; i < _menuButtons.Length; i++)
        {
            // Color del botón
            Color color = (i == _selectedIndex) ? Color.Yellow : Color.White;
            _spriteBatch.Draw(_pixel, _menuButtons[i], color * 0.7f);
            
            // Borde del botón
            int borderThickness = 2;
            var borderColor = (i == _selectedIndex) ? Color.Gold : Color.Gray;
            
            // Dibujar bordes
            var topBorder = new Rectangle(_menuButtons[i].X, _menuButtons[i].Y, _menuButtons[i].Width, borderThickness);
            var bottomBorder = new Rectangle(_menuButtons[i].X, _menuButtons[i].Y + _menuButtons[i].Height - borderThickness, _menuButtons[i].Width, borderThickness);
            var leftBorder = new Rectangle(_menuButtons[i].X, _menuButtons[i].Y, borderThickness, _menuButtons[i].Height);
            var rightBorder = new Rectangle(_menuButtons[i].X + _menuButtons[i].Width - borderThickness, _menuButtons[i].Y, borderThickness, _menuButtons[i].Height);

            _spriteBatch.Draw(_pixel, topBorder, borderColor);
            _spriteBatch.Draw(_pixel, bottomBorder, borderColor);
            _spriteBatch.Draw(_pixel, leftBorder, borderColor);
            _spriteBatch.Draw(_pixel, rightBorder, borderColor);
        }
    }

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Guardar tamaño inicial de ventana
        _windowedSize = new Point(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        _windowedPosition = new Point(Window.Position.X, Window.Position.Y);

        // Configurar sincronización vertical
        _graphics.SynchronizeWithVerticalRetrace = true;
        IsFixedTimeStep = true;
    }

    protected override void Initialize()
    {
        // Crear los rectángulos del menú
        _menuButtons = new Rectangle[_menuOptions.Length];
        int buttonWidth = 200;
        int buttonHeight = 40;
        int spacing = 20;
        
        for (int i = 0; i < _menuOptions.Length; i++)
        {
            int x = (GraphicsDevice.Viewport.Width - buttonWidth) / 2;
            int y = (GraphicsDevice.Viewport.Height - (_menuOptions.Length * (buttonHeight + spacing))) / 2 + i * (buttonHeight + spacing);
            _menuButtons[i] = new Rectangle(x, y, buttonWidth, buttonHeight);
        }

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        // Crear pixel blanco 1x1 para dibujar rectángulos
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    private void ToggleFullscreen()
    {
        _isFullscreen = !_isFullscreen;

        if (_isFullscreen)
        {
            // Guardar posición y tamaño de ventana actual
            _windowedSize = new Point(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            _windowedPosition = new Point(Window.Position.X, Window.Position.Y);

            // Cambiar a resolución de pantalla
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphics.HardwareModeSwitch = true; // true = pantalla completa real, false = ventana sin bordes
            _graphics.IsFullScreen = true;
        }
        else
        {
            // Restaurar tamaño y posición de ventana
            _graphics.PreferredBackBufferWidth = _windowedSize.X;
            _graphics.PreferredBackBufferHeight = _windowedSize.Y;
            _graphics.IsFullScreen = false;
            Window.Position = new Point(_windowedPosition.X, _windowedPosition.Y);
        }

        _graphics.ApplyChanges();

        // Recalcular posición de los botones del menú con el nuevo tamaño
        if (_currentState == GameState.Menu)
        {
            int buttonWidth = 200;
            int buttonHeight = 40;
            int spacing = 20;
            
            for (int i = 0; i < _menuOptions.Length; i++)
            {
                int x = (GraphicsDevice.Viewport.Width - buttonWidth) / 2;
                int y = (GraphicsDevice.Viewport.Height - (_menuOptions.Length * (buttonHeight + spacing))) / 2 + i * (buttonHeight + spacing);
                _menuButtons[i] = new Rectangle(x, y, buttonWidth, buttonHeight);
            }
        }
    }

    protected override void Update(GameTime gameTime)
    {
        var kb = Keyboard.GetState();
        
        // Alternar pantalla completa con F11
        if (kb.IsKeyDown(Keys.F11) && !_previousKeyboardState.IsKeyDown(Keys.F11))
        {
            ToggleFullscreen();
        }

        if (kb.IsKeyDown(Keys.Escape))
        {
            if (_currentState == GameState.Menu)
            {
                Exit();
            }
            else
            {
                _currentState = GameState.Menu;
            }
            return;
        }

        if (_currentState == GameState.Menu)
        {
            HandleMenuInput();
        }
        else if (_currentState == GameState.Playing)
        {
            // Aquí irá la lógica del juego
        }
        _previousKeyboardState = kb;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        if (_currentState == GameState.Menu)
        {
            DrawMenu();
        }
        else if (_currentState == GameState.Playing)
        {
            // Dibujar un rectángulo verde para mostrar que estamos jugando
            _spriteBatch.Draw(_pixel, new Rectangle(20, 20, 200, 40), Color.Green * 0.7f);
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
