using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Magic_Hunter.src;

namespace Magic_Hunter;

public class Game1 : Game {
    private SpriteSheetManager _coliseumSheet;
    private int _currentFrame = 0;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public enum GameState {
        Menu,
        Intro,
        Playing,
        OptionsPanel, // Pausa
        VolumePanel,  // Configuración
        ConfirmExit,  // Cartel confirmación
        GameOver
    }

    private GameState _currentState = GameState.Menu;
    
    // Estado raíz para saber si "Volver" en volumen va al menú o a la pausa
    private GameState _rootState = GameState.Menu; 

    private MenuManager _menuManager;
    private GamePlay _gamePlay;

    private KeyboardState _previousKeyboardState;
    private MouseState _previousMouseState;
    
    private Point _windowedSize;
    private Point _windowedPosition;
    private SpriteFont _pixelFont;
    private Texture2D _pixel;

    private string _introTitle = "INTRODUCCION";
    private string _introText = 
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit.\n" +
        "Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.\n" +
        "Ut enim ad minim veniam, quis nostrud exercitation ullamco\n" +
        "laboris nisi ut aliquip ex ea commodo consequat.\n\n" +
        "PRESIONA [ESPACIO] PARA CONTINUAR";

    public Game1() {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true; 
        _graphics.SynchronizeWithVerticalRetrace = true;
        IsFixedTimeStep = true;
    }

    protected override void Initialize() {
        _windowedSize = new Point(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        _windowedPosition = new Point(Window.Position.X, Window.Position.Y);
        FullscreenHelper.ApplyFullscreen(_graphics, Window, ref _windowedSize, ref _windowedPosition);

        _menuManager = new MenuManager();
        _gamePlay = new GamePlay();

        _previousKeyboardState = Keyboard.GetState();
        _previousMouseState = Mouse.GetState();
        base.Initialize();
    }

    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        Texture2D coliseumTexture = Content.Load<Texture2D>("pixilart-sprite(Escenario)");
        _coliseumSheet = new SpriteSheetManager(coliseumTexture, 640, 360);
        
        try { _pixelFont = Content.Load<SpriteFont>("PixelFont"); } catch { }
        SpriteFont titleFont = null;
        try { titleFont = Content.Load<SpriteFont>("PixelFont2"); } catch { }

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _menuManager.Initialize(GraphicsDevice.Viewport, GraphicsDevice);
        _menuManager.LoadContent(_pixelFont, titleFont);

        _gamePlay.Initialize(GraphicsDevice, GraphicsDevice.Viewport, Content);
    }

    protected override void Update(GameTime gameTime) {
        _currentFrame = (int)(gameTime.TotalGameTime.TotalSeconds / 0.2) % _coliseumSheet.FrameCount;
        var kb = Keyboard.GetState();
        var mouseState = Mouse.GetState(); 

        // --- TECLAS GLOBALES ---

        if (kb.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape)) {
            // ESC: Lógica de retroceso
            if (_currentState == GameState.Menu) {
                Exit();
            } 
            else if (_currentState == GameState.Playing) {
                _rootState = GameState.Playing;
                _currentState = GameState.OptionsPanel;
                IsMouseVisible = true;
            }
            else if (_currentState == GameState.OptionsPanel || _currentState == GameState.VolumePanel) {
                // Si estamos en menús de pausa, ESC vuelve al juego
                _currentState = GameState.Playing;
                _gamePlay.ResetInputState(); // IMPORTANTE: Evita disparo
                IsMouseVisible = false;
            }
            else if (_currentState == GameState.ConfirmExit) {
                _currentState = GameState.OptionsPanel; // Cancelar salida
            }
            return;
        }

        // Tecla P: Pausa rápida
        if (kb.IsKeyDown(Keys.P) && !_previousKeyboardState.IsKeyDown(Keys.P)) {
            if (_currentState == GameState.Playing) {
                _rootState = GameState.Playing;
                _currentState = GameState.OptionsPanel;
                IsMouseVisible = true;
            } else if (_currentState == GameState.OptionsPanel) {
                _currentState = GameState.Playing;
                _gamePlay.ResetInputState(); // IMPORTANTE
                IsMouseVisible = false;
            }
        }

        // --- MÁQUINA DE ESTADOS ---

        if (_currentState == GameState.Menu) {
            IsMouseVisible = true; 
            int selected = _menuManager.HandleInput(mouseState, _previousMouseState);
            
            if (selected == 0) { // Jugar
                _currentState = GameState.Intro;
                IsMouseVisible = false;
            }
            else if (selected == 1) { // Opciones (desde Menú)
                _rootState = GameState.Menu; // Guardamos que venimos del menú
                _currentState = GameState.VolumePanel; 
            }
            else if (selected == 99) Exit();
        }
        else if (_currentState == GameState.Intro) {
            if (kb.IsKeyDown(Keys.Space) && !_previousKeyboardState.IsKeyDown(Keys.Space)) {
                _currentState = GameState.Playing;
                _rootState = GameState.Playing;
                _gamePlay.Reset();
                _gamePlay.ResetInputState(); // IMPORTANTE
            }
        }
        else if (_currentState == GameState.Playing) {
            _gamePlay.Update(gameTime, GraphicsDevice.Viewport, Mouse.GetState());
            
            if (_gamePlay.IsGameOver) {
                _currentState = GameState.GameOver;
                IsMouseVisible = true;
            }
        }
        else if (_currentState == GameState.OptionsPanel) {
            int selected = _menuManager.HandlePauseInput(mouseState, _previousMouseState);
            
            if (selected == 10) { // Reanudar
                _currentState = GameState.Playing;
                _gamePlay.ResetInputState(); // IMPORTANTE: Resetea el clic del mouse
                IsMouseVisible = false;
            }
            else if (selected == 11) { // Configuración
                _currentState = GameState.VolumePanel;
            }
            else if (selected == 12) { // Volver al Menu (Confirmación)
                _currentState = GameState.ConfirmExit;
            }
        }
        else if (_currentState == GameState.VolumePanel) {
            int selected = _menuManager.HandleVolumeInput(mouseState, _previousMouseState);
            
            if (selected == 15) { // Volver
                // Volver inteligente: ¿Vinimos del Menú o de la Pausa?
                if (_rootState == GameState.Menu) {
                    _currentState = GameState.Menu;
                } else {
                    _currentState = GameState.OptionsPanel;
                }
            }
        }
        else if (_currentState == GameState.ConfirmExit) {
            int selected = _menuManager.HandleConfirmInput(mouseState, _previousMouseState);
            
            if (selected == 30) { // SI (Salir al menú)
                _gamePlay.Reset(); 
                _currentState = GameState.Menu;
                _rootState = GameState.Menu;
                IsMouseVisible = true; 
            }
            else if (selected == 31) { // NO (Cancelar)
                _currentState = GameState.OptionsPanel;
            }
        }
        else if (_currentState == GameState.GameOver) {
            int selected = _menuManager.HandleGameOverInput(mouseState, _previousMouseState);
            if (selected == 20) { // Reintentar
                _gamePlay.Reset(); 
                _gamePlay.ResetInputState(); // IMPORTANTE
                _currentState = GameState.Playing;
                _rootState = GameState.Playing;
                IsMouseVisible = false;
            }
            else if (selected == 21) { // Salir
                Exit();
            }
        }

        _previousKeyboardState = kb;
        _previousMouseState = mouseState; 
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();
        
        _spriteBatch.Draw(_coliseumSheet.Texture, 
            new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), 
            _coliseumSheet.GetFrame(_currentFrame), 
            Color.White);

        if (_currentState == GameState.Menu) {
            _menuManager.DrawMainMenu(_spriteBatch, GraphicsDevice.Viewport);
        }
        else if (_currentState == GameState.Intro) {
            Rectangle screenRect = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            _spriteBatch.Draw(_pixel, screenRect, Color.Black * 0.7f);

            if (_pixelFont != null) {
                Vector2 titleSize = _pixelFont.MeasureString(_introTitle);
                Vector2 titlePos = new Vector2((screenRect.Width - titleSize.X) / 2, 100);
                _spriteBatch.DrawString(_pixelFont, _introTitle, titlePos, Color.Gold);

                string[] lines = _introText.Split('\n');
                float startY = titlePos.Y + titleSize.Y + 50; 
                float lineHeight = _pixelFont.MeasureString("A").Y + 5; 

                foreach (string line in lines) {
                    Vector2 lineSize = _pixelFont.MeasureString(line);
                    Vector2 linePos = new Vector2((screenRect.Width - lineSize.X) / 2, startY);
                    _spriteBatch.DrawString(_pixelFont, line, linePos, Color.White);
                    startY += lineHeight; 
                }
            }
        }
        else if (_currentState == GameState.Playing) {
            _gamePlay.Draw(_spriteBatch);
        }
        else if (_currentState == GameState.OptionsPanel) {
            // Fondo dependiendo de dónde venimos
            if (_rootState == GameState.Playing) _gamePlay.Draw(_spriteBatch);
            else _menuManager.DrawMainMenu(_spriteBatch, GraphicsDevice.Viewport);

            _menuManager.DrawOptionsPanel(_spriteBatch, GraphicsDevice.Viewport);
        }
        else if (_currentState == GameState.VolumePanel) {
            if (_rootState == GameState.Playing) _gamePlay.Draw(_spriteBatch);
            else _menuManager.DrawMainMenu(_spriteBatch, GraphicsDevice.Viewport);

            _menuManager.DrawVolumePanel(_spriteBatch, GraphicsDevice.Viewport);
        }
        else if (_currentState == GameState.ConfirmExit) {
            if (_rootState == GameState.Playing) _gamePlay.Draw(_spriteBatch);
            _menuManager.DrawOptionsPanel(_spriteBatch, GraphicsDevice.Viewport);
            _menuManager.DrawConfirmExit(_spriteBatch, GraphicsDevice.Viewport);
        }
        else if (_currentState == GameState.GameOver) {
            _gamePlay.Draw(_spriteBatch);
            _menuManager.DrawGameOver(_spriteBatch, GraphicsDevice.Viewport);
        }
        else if (_currentState == GameState.ConfirmExit)
        {
            if (_rootState == GameState.Playing) _gamePlay.Draw(_spriteBatch);
            
            // Dibujamos el panel de pausa de fondo, pero SIN efecto hover (true)
            _menuManager.DrawOptionsPanel(_spriteBatch, GraphicsDevice.Viewport, true); 
            
            _menuManager.DrawConfirmExit(_spriteBatch, GraphicsDevice.Viewport);
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}