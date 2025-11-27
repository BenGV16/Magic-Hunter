using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Magic_Hunter.src
{
    public class AnimationManager
    {
        private Texture2D _texture;
        private List<Rectangle> _frames = new();
        private float _frameTime;
        private bool _isLooping;
        private bool _isReversed; // NUEVO: Para reproducir hacia atrás

        private int _currentFrame;
        private double _timer;

        public int CurrentFrame => _currentFrame;
        public bool IsDone { get; private set; }

        public AnimationManager(Texture2D texture, int frameCount, int frameWidth, int frameHeight, float frameTime)
        {
            _texture = texture;
            _frameTime = frameTime;
            _isLooping = true;
            IsDone = false;

            for (int i = 0; i < frameCount; i++)
            {   
                _frames.Add(new Rectangle(i * frameWidth, 0, frameWidth, frameHeight));
            }
        }

        // MODIFICADO: Ahora acepta parámetro reversed
        public void Play(bool isLooping, bool reversed = false)
        {
            _isLooping = isLooping;
            _isReversed = reversed;
            
            // Si es reverso, empezamos en el último frame, sino en el 0
            _currentFrame = _isReversed ? _frames.Count - 1 : 0;
            
            _timer = 0;
            IsDone = false;
        }

        public void Update(GameTime gameTime)
        {
            if (IsDone) return;
            _timer += gameTime.ElapsedGameTime.TotalSeconds;
            if (_timer >= _frameTime)
            {
                _timer -= _frameTime;

                // Lógica normal o reversa
                if (_isReversed)
                {
                    _currentFrame--;
                    if (_currentFrame < 0)
                    {
                        if (_isLooping) _currentFrame = _frames.Count - 1;
                        else { _currentFrame = 0; IsDone = true; }
                    }
                }
                else
                {
                    _currentFrame++;
                    if (_currentFrame >= _frames.Count)
                    {
                        if (_isLooping) _currentFrame = 0;
                        else { _currentFrame = _frames.Count - 1; IsDone = true; }
                    }
                }
            }
        }
        
        public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float depth, float width, float height, SpriteEffects effects = SpriteEffects.None)
        {
            Rectangle destinationRect = new Rectangle(
                (int)(position.X - width / 2f),
                (int)(position.Y - height / 2f),
                (int)width,
                (int)height
            );

            spriteBatch.Draw(
                _texture,
                destinationRect,
                _frames[_currentFrame],
                color,
                0f,
                Vector2.Zero,
                effects, 
                depth
            );
        }
    }
}