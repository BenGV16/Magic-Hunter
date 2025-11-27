using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Magic_Hunter.src;

public abstract class Enemies
{
    public Rectangle Rect;
    public Vector2 Position => _position; 
    protected Vector2 _position;
    protected float _width = 200f;
    protected float _height = 200f;
    protected float _baseSpeed = 100f;
    protected float _speed;
    protected Random _random;
    protected double _timer = 0;
    protected double _hitTimer = 0; 

    protected SpriteEffects _spriteEffect = SpriteEffects.None;

    public int Health { get; protected set; }
    public int Damage { get; protected set; }

    public Color Color { get; protected set; }
    public float Depth { get; protected set; }
    protected Texture2D _texture;
    protected AnimationManager _animator;

    public Enemies(Vector2 startPos, Random random, float depth, Color color, Texture2D texture, int health, int damage, float speedMultiplier)
    {
        _position = startPos;
        _random = random;
        Depth = depth;
        Color = color;
        _texture = texture;
        
        Health = health;
        Damage = (int)(damage * speedMultiplier);
        _speed = _baseSpeed * speedMultiplier;

        UpdateRect();
    }

    public bool TakeDamage(int amount)
    {
        Health -= amount;
        _hitTimer = 0.1;
        return Health <= 0;
    }

    public virtual void Update(GameTime gameTime, Viewport viewport)
    {
        if (_hitTimer > 0)
        {
            _hitTimer -= gameTime.ElapsedGameTime.TotalSeconds;
        }
    }
    
    public void Teleport(Vector2 newPos)
    {
        _position = newPos;
        UpdateRect();
    }

    protected void UpdateRect()
    {
        Rect = new Rectangle(
            (int)(_position.X - _width / 2f),
            (int)(_position.Y - _height / 2f),
            (int)_width,
            (int)_height
        );
    }

    public virtual void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        Color drawColor = (_hitTimer > 0) ? Color.Red : Color;

        if (_animator != null)
        {
            _animator.Draw(spriteBatch, _position, drawColor, Depth, _width, _height, _spriteEffect);
        }
        else
        {
            spriteBatch.Draw(pixel, Rect, null, drawColor * 0.8f, 0f, Vector2.Zero, SpriteEffects.None, Depth);
        }
    }
}

public class Hada : Enemies
{
    private Vector2 _target;
    private float _baseWidth;
    private float _attackWidth = 400f;
    public bool IsReadyToAttack => _width >= _attackWidth; 

    public Hada(Vector2 startPos, Random random, float depth, Color color, Texture2D texture, int frameCount, int frameWidth, int frameHeight, float frameTime, float speedMultiplier)
        : base(startPos, random, depth, color, texture, 1, 5, speedMultiplier)
    {
        _speed *= 2.5f;
        _baseWidth = _width;
        _target = new Vector2(startPos.X, startPos.Y + 100); 
        _animator = new AnimationManager(texture, frameCount, frameWidth, frameHeight, frameTime);
        _animator.Play(true);
    }

    public override void Update(GameTime gameTime, Viewport viewport)
    {
        base.Update(gameTime, viewport);
        float growthRate = 35f * (float)gameTime.ElapsedGameTime.TotalSeconds;
        _width += growthRate;
        _height += growthRate;
        Vector2 dir = _target - _position;
        if (dir.Length() < 10f)
        {
            _target = new Vector2(
                _random.Next(50, viewport.Width - 50),
                _random.Next(50, (int)(viewport.Height * 0.7))
            );
            dir = _target - _position;
        }
        if (dir.Length() > 0)
        {
            dir.Normalize();
            _position += dir * _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        _animator.Update(gameTime);
        UpdateRect();
    }

    public override void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        float ratio = (_width - _baseWidth) / (_attackWidth - _baseWidth);
        ratio = MathHelper.Clamp(ratio, 0f, 1f);
        Color brightnessColor = Color.Lerp(Color.White, new Color(255, 255, 180), ratio);
        Color finalColor = (_hitTimer > 0) ? Color.Red : brightnessColor;
        _animator.Draw(spriteBatch, _position, finalColor, Depth, _width, _height, _spriteEffect);
    }
}

public class Lobo : Enemies
{
    private float _targetX;
    private AnimationManager _walkAnim;
    private AnimationManager _attackAnim;
    public bool IsAttacking { get; private set; } = false;
    
    private float _descentSpeed = 40f; 
    private float _lateralSpeed = 30f; 
    private float _growthSpeed = 25f;  

    public Lobo(Vector2 startPos, Random random, float depth, Color color, Texture2D walkTexture, Texture2D attackTexture, int frameWidth, int frameHeight, float speedMultiplier)
        : base(startPos, random, depth, color, walkTexture, 5, 10, speedMultiplier)
    {
        _targetX = 0; 
        
        _walkAnim = new AnimationManager(walkTexture, 2, frameWidth, frameHeight, 0.2f);
        _attackAnim = new AnimationManager(attackTexture, 1, attackTexture.Width, attackTexture.Height, 0.2f);
        
        _animator = _walkAnim;
        _animator.Play(true);
    }

    public override void Update(GameTime gameTime, Viewport viewport)
    {
        base.Update(gameTime, viewport);
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_targetX == 0) _targetX = viewport.Width / 2f;

        if (!IsAttacking)
        {
            _position.Y += _descentSpeed * delta;
            _width += _growthSpeed * delta;
            _height += _growthSpeed * delta;

            float dirX = (_targetX - _position.X);
            
            if (Math.Abs(dirX) > 5f)
            {
                _position.X += Math.Sign(dirX) * _lateralSpeed * delta;
                if (Math.Sign(dirX) < 0) _spriteEffect = SpriteEffects.FlipHorizontally;
                else _spriteEffect = SpriteEffects.None;
            }

            if (_position.Y > viewport.Height * 0.75f) 
            {
                IsAttacking = true;
                _animator = _attackAnim;
                _animator.Play(true);
            }
            
            if (!IsAttacking) _animator = _walkAnim; 
        }
        else
        {
            _animator = _attackAnim;
        }

        _animator.Update(gameTime);
        UpdateRect();
    }
}

// --- SAPO MODIFICADO ---
public class Sapo : Enemies
{
    public bool ReadyToShoot = false;
    private double _shootTimer = 0;
    private bool _hasFiredThisCycle = false; 
    private int _frameWidth; 

    public Sapo(Vector2 startPos, Random random, float depth, Color color, Texture2D texture, int frameCount, int frameWidth, int frameHeight, float frameTime, float speedMultiplier)
        : base(startPos, random, depth, color, texture, 8, 15, speedMultiplier)
    {
        _frameWidth = frameWidth;
        _animator = new AnimationManager(texture, frameCount, frameWidth, frameHeight, frameTime);
        _animator.Play(false);
    }

    public override void Update(GameTime gameTime, Viewport viewport)
    {
        base.Update(gameTime, viewport);

        // CAMBIO: Temporizador a 5 segundos
        _shootTimer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_shootTimer >= 5.0)
        {
            _shootTimer = 0;
            _hasFiredThisCycle = false; 
            _animator.Play(false);      
        }

        if (!_animator.IsDone && _animator.CurrentFrame == 4 && !_hasFiredThisCycle)
        {
            ReadyToShoot = true;       
            _hasFiredThisCycle = true; 
        }

        _animator.Update(gameTime);
        UpdateRect();
    }

    public override void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        Color drawColor = (_hitTimer > 0) ? Color.Red : Color;

        if (_animator.IsDone)
        {
            Rectangle destRect = new Rectangle((int)(_position.X - _width / 2f), (int)(_position.Y - _height / 2f), (int)_width, (int)_height);
            Rectangle sourceRect = new Rectangle(0, 0, _frameWidth, _texture.Height);
            spriteBatch.Draw(_texture, destRect, sourceRect, drawColor, 0f, Vector2.Zero, _spriteEffect, Depth);
        }
        else
        {
            _animator.Draw(spriteBatch, _position, drawColor, Depth, _width, _height, _spriteEffect);
        }
    }
}

public class Gusano : Enemies
{
    public enum State { Emerging, Idle, Attacking, Burrowing, Hidden }
    public State CurrentState => _state; 
    
    private State _state;
    private AnimationManager _emergeAnim;
    private AnimationManager _idleAnim;
    private AnimationManager _attackAnim;

    public bool ReadyToShoot = false;
    private double _hiddenTimer = 0;
    private double _waitToHideTimer = 0; 
    
    // Para controlar un solo disparo por posición
    private bool _hasShotAtCurrentPos = false; 

    public Gusano(Vector2 startPos, Random random, float depth, Color color,
              Texture2D emergeTexture, Texture2D idleTexture, Texture2D attackTexture,
              int emergeFrames, int idleFrames, int attackFrames,
              int emergeFrameWidth, int emergeFrameHeight,
              int idleFrameWidth, int idleFrameHeight,
              int attackFrameWidth, int attackFrameHeight,
              float frameTime, float speedMultiplier)
    : base(startPos, random, depth, color, emergeTexture, 3, 8, speedMultiplier)
    {
        _emergeAnim = new AnimationManager(emergeTexture, emergeFrames, emergeFrameWidth, emergeFrameHeight, frameTime);
        _idleAnim = new AnimationManager(idleTexture, idleFrames, idleFrameWidth, idleFrameHeight, frameTime);
        
        if (attackTexture != null)
            _attackAnim = new AnimationManager(attackTexture, attackFrames, attackFrameWidth, attackFrameHeight, frameTime);
        else 
            _attackAnim = _idleAnim;

        _state = State.Emerging;
        _animator = _emergeAnim;
        _animator.Play(isLooping: false);
    }

    public void TriggerAttack()
    {
        if (_state == State.Idle && !_hasShotAtCurrentPos)
        {
            _state = State.Attacking;
            _animator = _attackAnim;
            _animator.Play(isLooping: false);
            _hasShotAtCurrentPos = true; 
        }
    }

    public override void Update(GameTime gameTime, Viewport viewport)
    {
        base.Update(gameTime, viewport);
        _animator.Update(gameTime);

        switch (_state)
        {
            case State.Emerging:
                if (_animator.IsDone)
                {
                    _state = State.Idle;
                    _animator = _idleAnim;
                    _animator.Play(isLooping: true);
                    _waitToHideTimer = 0; 
                }
                break;

            case State.Attacking:
                if (_animator.IsDone)
                {
                    _state = State.Idle;
                    _animator = _idleAnim;
                    _animator.Play(isLooping: true);
                }
                break;

            case State.Idle:
                // Lógica de disparo (ahora controlada por Gameplay, pero aquí verificamos tiempos para esconderse)
                // Si ya disparó, se esconde rápido.
                _waitToHideTimer += gameTime.ElapsedGameTime.TotalSeconds;
                
                if (_hasShotAtCurrentPos || _waitToHideTimer > 4.0) 
                {
                    _state = State.Burrowing;
                    _animator = _emergeAnim; 
                    _animator.Play(isLooping: false, reversed: true); 
                }
                break;

            case State.Burrowing:
                if (_animator.IsDone)
                {
                    _state = State.Hidden;
                    _hiddenTimer = _random.NextDouble() * 2.0 + 3.0; 
                    _position = new Vector2(-1000, -1000); 
                }
                break;

            case State.Hidden:
                _hiddenTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                if (_hiddenTimer <= 0)
                {
                    int minX = (int)(viewport.Width * 0.1f);
                    int maxX = (int)(viewport.Width * 0.7f); 
                    float newX = _random.Next(minX, maxX);
                    
                    int minY = (int)(viewport.Height * 0.5f);
                    int maxY = (int)(viewport.Height * 0.8f);
                    float newY = _random.Next(minY, maxY);

                    Teleport(new Vector2(newX, newY));

                    _state = State.Emerging;
                    _animator = _emergeAnim;
                    _animator.Play(isLooping: false, reversed: false);
                    _hasShotAtCurrentPos = false; 
                }
                break;
        }

        if (_state != State.Hidden)
        {
            if (_position.X < viewport.Width / 2) _spriteEffect = SpriteEffects.FlipHorizontally;
            else _spriteEffect = SpriteEffects.None;
        }

        UpdateRect();
    }
    
    public override void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        if (_state == State.Hidden) return;
        base.Draw(spriteBatch, pixel);
    }
}

public class Hada2 : Enemies
{
    private Vector2 _target;
    private bool _leaving = false;

    public Hada2(Vector2 startPos, Random random, float depth, Color color, Texture2D texture, int frameCount, int frameWidth, int frameHeight, float frameTime, float speedMultiplier)
        : base(startPos, random, depth, color, texture, 1, 0, speedMultiplier) 
    {
        _speed *= 3.0f; 
        _target = new Vector2(startPos.X, startPos.Y + 150); 
        _animator = new AnimationManager(texture, frameCount, frameWidth, frameHeight, frameTime);
        _animator.Play(true);
    }

    public override void Update(GameTime gameTime, Viewport viewport)
    {
        base.Update(gameTime, viewport);
        _animator.Update(gameTime);
        _timer += gameTime.ElapsedGameTime.TotalSeconds;

        if (!_leaving && _timer > 8.0)
        {
            _leaving = true;
            _target = new Vector2(_position.X, -200); 
        }
        else if (!_leaving && Vector2.Distance(_position, _target) < 10f)
        {
             _target = new Vector2(
                _random.Next(50, viewport.Width - 50),
                _random.Next(50, (int)(viewport.Height * 0.7))
             );
        }

        Vector2 dir = _target - _position;
        if (dir.Length() > 0)
        {
            dir.Normalize();
            _position += dir * _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        UpdateRect();
    }
}