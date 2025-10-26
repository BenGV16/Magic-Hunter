using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Magic_Hunter.src;

public class GamePlay
{
    private Texture2D _pixel;
    private Rectangle _box;
    private Vector2 _position;
    private Vector2 _targetPosition;
    private float _speed = 100f; // píxeles por segundo
    private double _timer = 0;
    private double _interval = 4.0; // segundos entre movimientos
    private Random _random = new();
    float _width = 40;
    float _height = 40;

    public void Initialize(GraphicsDevice graphicsDevice, Viewport viewport)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        int maxX = viewport.Width - (int)_width;
        float randomX = _random.Next(0, maxX);
        _position = new Vector2(randomX, viewport.Height / 2f);
        _targetPosition = _position;
        _box = new Rectangle((int)(_position.X - _width / 2f), (int)(_position.Y - _height / 2f), (int)_width, (int)_height);
    }

    public void Update(GameTime gameTime, Viewport viewport)
    {
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Mover hacia la posición objetivo
        Vector2 direction = _targetPosition - _position;
        if (direction.Length() > 1f)
        {
            direction.Normalize();
            _position += direction * _speed * delta;
            _width += 0.2f;
            _height += 0.2f;
            _box.X = (int)(_position.X - _box.Width / 2f);
            _box.Y = (int)(_position.Y - _box.Height / 2f);
        }
        _box = new Rectangle(
            (int)(_position.X - _width / 2f),
            (int)(_position.Y - _height / 2f),
            (int)_width,
            (int)_height
        );
        // Temporizador para cambiar de destino
        _timer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_timer >= _interval)
        {
            _timer = 0;

            // Elegir nueva posición aleatoria en X
            int maxX = viewport.Width - _box.Width;
            float newX = _random.Next(0, maxX);
            _targetPosition = new Vector2(newX, _position.Y);
        }
    }
    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_pixel, _box, Color.OrangeRed * 0.8f);
    }
}
