using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Magic_Hunter.src;

public class GamePlay
{
    private Texture2D _pixel;
    private List<BoxEntity> _boxes = new();
    private MouseState _previousMouseState;
    private double _spawnTimer = 0;
    private double _spawnInterval;
    float depth = 1.0f;
    private const int MaxBoxes = 10;
    private Random _random = new();

    public void Initialize(GraphicsDevice graphicsDevice, Viewport viewport)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        AddBox(viewport);
    }

    public void Update(GameTime gameTime, Viewport viewport, MouseState mouseState)
    {
        var ms = Mouse.GetState();
        _spawnInterval = _random.NextDouble() * 4.0 + 1.0;
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        foreach (var box in _boxes)
        {
            box.Update(gameTime, viewport);
        }
        if (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
        {
            Point mousePosition = new Point(mouseState.X, mouseState.Y);
            var clickedBoxes = _boxes.Where(box => box.Rect.Contains(mousePosition)).ToList();

            if (clickedBoxes.Count > 0)
            {
                var frontBox = clickedBoxes.OrderBy(box => box.Depth).First();
                _boxes.Remove(frontBox);
            }
        }

        _spawnTimer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_spawnTimer >= _spawnInterval && _boxes.Count < MaxBoxes)
        {
            _spawnTimer = 0;
            AddBox(viewport);
        }
        _previousMouseState = ms;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var box in _boxes)
        {
            Color[] colors = { Color.Blue, Color.Green, Color.Purple };
            Color col = colors[_random.Next(colors.Length)];
            spriteBatch.Draw(_pixel, box.Rect, null, box.Color * 0.8f, 0f, Vector2.Zero, SpriteEffects.None, box.Depth);
        }
    }

    private void AddBox(Viewport viewport)
    {
        Color[] colors = { Color.Blue, Color.Green, Color.Purple, Color.Yellow, Color.Red, Color.White };
        Color col = colors[_random.Next(colors.Length)];
        int startX = _random.Next(0, viewport.Width - 40);
        Vector2 startPos = new Vector2(startX, viewport.Height / 2f);
        _boxes.Add(new BoxEntity(startPos, _random, depth, col));
    }
    private class BoxEntity
    {
        public Rectangle Rect;
        private Vector2 _position;
        private Vector2 _targetPosition;
        public Color Color { get; private set; }
        private float _depth;
        public bool IsMoving => (_targetPosition - _position).Length() > 1f;
        public float Depth => _depth;
        private float _speed = 100f;
        private float _width = 40f;
        private float _height = 40f;
        private double _moveTimer = 0;
        private double _moveInterval;
        private Random _random;

        public BoxEntity(Vector2 startPos, Random sharedRandom, float depth, Color color)
        {
            _position = startPos;
            _targetPosition = startPos;
            _random = sharedRandom;
            _depth = depth;
            Color = color;
            _moveInterval = _random.NextDouble() * 4.0 + 1.0;
            UpdateRect();
        }
        public void Update(GameTime gameTime, Viewport viewport)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 direction = _targetPosition - _position;
            if (direction.Length() > 1f)
            {
                direction.Normalize();
                _position += direction * _speed * delta;
                _width += 0.2f;
                _height += 0.2f;
                while (_depth > 0.1f)
                {
                    _depth += 0.001f;
                }
            }  
            UpdateRect();
            _moveTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (_moveTimer >= _moveInterval)
            {
                _moveTimer = 0;
                int maxX = viewport.Width - (int)_width;
                float newX = _random.Next(0, maxX);
                _targetPosition = new Vector2(newX, _position.Y);
            }
        }
        private void UpdateRect()
        {
            Rect = new Rectangle(
                (int)(_position.X - _width / 2f),
                (int)(_position.Y - _height / 2f),
                (int)_width,
                (int)_height
            );
        }
    }
}