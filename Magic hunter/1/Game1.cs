using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Magic_Hunter.src;

namespace Magic_Hunter;

public class Game1 : Game
{
    private Texture2D _pixel;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    public enum GameState
    {
        Menu,
        Playing
    }
    private GameState _currentState = GameState.Menu;
    private MenuManager _menuManager;
    private GamePlay _gamePlay;

    private KeyboardState _previousKeyboardState;
    private Point _windowedSize;
    private Point _windowedPosition;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.SynchronizeWithVerticalRetrace = true;
        IsFixedTimeStep = true;
    }

    protected override void Initialize()
    {
        _windowedSize = new Point(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        _windowedPosition = new Point(Window.Position.X, Window.Position.Y);
        FullscreenHelper.ApplyFullscreen(_graphics, Window, ref _windowedSize, ref _windowedPosition);
        _menuManager = new MenuManager();
        _gamePlay = new GamePlay();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _menuManager.Initialize(GraphicsDevice.Viewport, GraphicsDevice);
        _gamePlay.Initialize(GraphicsDevice, GraphicsDevice.Viewport);
    }

    protected override void Update(GameTime gameTime)
    {
        var kb = Keyboard.GetState();
        if (kb.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape))
        {
            if (_currentState == GameState.Menu)
                Exit();
            else
                _currentState = GameState.Menu;
            return;
        }
        if (_currentState == GameState.Menu)
        {
            var mouseState = Mouse.GetState();
            int selected = _menuManager.HandleInput(mouseState);
            if (selected == 0) _currentState = GameState.Playing;
            else if (selected == 1) System.Diagnostics.Debug.WriteLine("Options selected");
            else if (selected == 2) Exit();
        }
        else if (_currentState == GameState.Playing)
        {
            _gamePlay.Update( gameTime,GraphicsDevice.Viewport, Mouse.GetState());
        }
        _previousKeyboardState = kb;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _pixel = Content.Load<Texture2D>("piupiu");
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();
        _spriteBatch.Draw(_pixel, new Rectangle(0, 0, 1920, 1080), Color.White);
        if (_currentState == GameState.Menu)
        {
            _menuManager.Draw(_spriteBatch);
        }
        else if (_currentState == GameState.Playing)
        {
            _gamePlay.Draw(_spriteBatch);
        }
        _spriteBatch.End();
        base.Draw(gameTime);
    }
}   