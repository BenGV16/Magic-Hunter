using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Magic_Hunter.src;

public class GamePlay {
    public bool IsGameOver { get; private set; } = false;
    public void ResetInputState() {
        _previousMouseState = Mouse.GetState();
    }
    private Texture2D _pixel;
    private List<Enemies> _entities = new();
    private MouseState _previousMouseState;
    private Queue<int> _enemyWaveQueue = new();
    private double _spawnTimer = 0;
    private double _spawnInterval = 2.0;
    private int _waveNumber = 0;
    private bool _waveInProgress = false;
    private float _difficultyMultiplier = 1.0f;
    private double _waveBreakTimer = 0; 
    private double _waveMessageTimer = 0;
    private bool _showingWaveMessage = false;
    private string _waveMessageText = "";
    private float _flashbangAlpha = 0f;
    
    private class MudSpot { public Vector2 Position; public Gusano Owner; }
    private List<MudSpot> _mudSpots = new();
    private Texture2D _mudOverlayTexture;
    
    private int _playerMaxHealth = 100;
    private int _playerHealth = 100;
    private Texture2D _wandTexture;
    private PlayerWand _playerWand;
    private float _wandXPosition; 
    private Texture2D _shieldTexture;
    private Rectangle[] _shieldSourceRects; 
    private int _currentShieldFrame = 0; 
    private float _shieldDurability = 100f;
    private float _maxShieldDurability = 100f;
    private bool _isShieldBroken = false;
    private double _blockFeedbackTimer = 0; 
    private List<Projectile> _projectiles = new();
    private Texture2D _projectileTexture; 
    private Texture2D _playerProjectileTexture; 
    private Texture2D _arenaProjectileTexture; 
    private ContentManager _content;
    private Texture2D _hadaTexture;
    private Texture2D _hada2Texture; 
    private Texture2D _sapoTexture;
    private Texture2D _gusanoSaliendoTexture;
    private Texture2D _gusanoNormalTexture;
    private Texture2D _gusanoAtaqueTexture;
    private Texture2D _loboWalkTexture;
    private Texture2D _loboAttackTexture;
    private SpriteFont _font; 
    private float depth = 1.0f;
    private Random _random = new();

    private class Projectile {
        public Vector2 Position;
        public Vector2 Target; 
        public float Scale;    
        public float Speed;
        public int Damage;
        public bool IsActive;
        public Texture2D Texture; 
        public bool IsMudEffect; 
        public bool IsPlayerProjectile; 
        public double LifeTime; 
        public bool HasStopped = false;
        public double StopDecayTimer = 0.1; 
        public int CurrentFrame;
        public double FrameTimer;
        public int TotalFrames; 
        public int FrameWidth;
        public double ActivationDelay = 0;
        public Enemies Owner; 
    }

    public void Initialize(GraphicsDevice graphicsDevice, Viewport viewport, ContentManager content) {
        _content = content;
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _wandXPosition = viewport.Width * 0.8f; 

        try {
            _hadaTexture = _content.Load<Texture2D>("pixilart-sprite(Hada1)");
            _hada2Texture = _content.Load<Texture2D>("pixilart-sprite(Hada2)"); 
            _sapoTexture = _content.Load<Texture2D>("pixilart-sprite (Sapito)");
            _gusanoSaliendoTexture = _content.Load<Texture2D>("pixilart-sprite (Gusano-Saliendo)");
            _gusanoNormalTexture = _content.Load<Texture2D>("pixil-sprite(Gusano)");
            _wandTexture = _content.Load<Texture2D>("pixilart-sprite (varita)");
            _projectileTexture = _content.Load<Texture2D>("pixilart-sprite(bola de fuego)");
            _shieldTexture = _content.Load<Texture2D>("pixilart-sprite(Escudo)");
            _loboWalkTexture = _content.Load<Texture2D>("pixilart-sprite (lobo caminando)");
            _loboAttackTexture = _content.Load<Texture2D>("pixil-frame-0 (lobo-atacando)");
            _gusanoAtaqueTexture = _content.Load<Texture2D>("Gusano-ataque");
            _playerProjectileTexture = _content.Load<Texture2D>("pixilart-sprite (ataque-mago)");

            Texture2D fullArenaTex = _content.Load<Texture2D>("pixilart-sprite (ataque-arena)");
            int w = fullArenaTex.Width / 2;
            int h = fullArenaTex.Height;
            Color[] dataProjectile = new Color[w * h];
            fullArenaTex.GetData(0, new Rectangle(0, 0, w, h), dataProjectile, 0, w * h);
            _arenaProjectileTexture = new Texture2D(graphicsDevice, w, h);
            _arenaProjectileTexture.SetData(dataProjectile);
            Color[] dataMud = new Color[w * h];
            fullArenaTex.GetData(0, new Rectangle(w, 0, w, h), dataMud, 0, w * h);
            _mudOverlayTexture = new Texture2D(graphicsDevice, w, h);
            _mudOverlayTexture.SetData(dataMud);

            int sWidth = _shieldTexture.Width / 3;
            int sHeight = _shieldTexture.Height;
            _shieldSourceRects = new Rectangle[3];
            _shieldSourceRects[0] = new Rectangle(0, 0, sWidth, sHeight);       
            _shieldSourceRects[1] = new Rectangle(sWidth, 0, sWidth, sHeight);  
            _shieldSourceRects[2] = new Rectangle(sWidth * 2, 0, sWidth, sHeight); 

            try { _font = _content.Load<SpriteFont>("PixelFont"); } catch { } 
        }
        catch (Exception e) { System.Diagnostics.Debug.WriteLine(e.Message); }

        if (_wandTexture != null) {
            Vector2 wandPos = new Vector2(_wandXPosition, viewport.Height - 400f);
            _playerWand = new PlayerWand(_wandTexture, 1, 3, _wandTexture.Width / 3, _wandTexture.Height, 0.05f, wandPos);
        }
        Reset(); 
    }

    public void Reset() { 
        _entities.Clear(); 
        _projectiles.Clear(); 
        _enemyWaveQueue.Clear(); 
        _mudSpots.Clear(); 
        _waveNumber = 0; 
        _playerHealth = _playerMaxHealth; 
        _shieldDurability = _maxShieldDurability; 
        _isShieldBroken = false; 
        _difficultyMultiplier = 1.0f; 
        _flashbangAlpha = 0f; 
        IsGameOver = false; 
        StartNextWave(); 
    }
    
    private void StartNextWave() { 
        if (_waveNumber > 0) {
            int healAmount = (int)(_playerMaxHealth * 0.10f);
            _playerHealth += healAmount;
            if (_playerHealth > _playerMaxHealth) _playerHealth = _playerMaxHealth;
            _shieldDurability = _maxShieldDurability;
            _isShieldBroken = false;
        }

        _waveNumber++; 
        _waveInProgress = true; 
        _spawnTimer = 0; 
        _showingWaveMessage = true; 
        _waveMessageTimer = 3.0; 
        _waveMessageText = "OLEADA " + _waveNumber; 
        
        if (_waveNumber > 1) { 
            _difficultyMultiplier += 0.1f; 
            _spawnInterval = Math.Max(0.8, _spawnInterval - 0.1); 
        } 
        
        GenerateWaveEnemies(); 
    }
    
    private void GenerateWaveEnemies() {
        int totalEnemies = 3 + _waveNumber; 
        int maxSapos = 1 + (_waveNumber / 3); 
        int currentSapos = 0;
        int maxHadas2 = 2;
        int currentHadas2 = 0;

        List<int> waveComposition = new List<int>();
        for (int i = 0; i < totalEnemies; i++) {
            int type = _random.Next(5); 
            if (type == 2) { 
                if (currentSapos < maxSapos) { currentSapos++; waveComposition.Add(type); } 
                else waveComposition.Add(_random.Next(0, 2)); 
            }
            else if (type == 4) {
                if (currentHadas2 < maxHadas2) { currentHadas2++; waveComposition.Add(type); }
                else waveComposition.Add(_random.Next(0, 2)); 
            }
            else waveComposition.Add(type);
        }
        waveComposition = waveComposition.OrderBy(x => _random.Next()).ToList();
        foreach (var t in waveComposition) _enemyWaveQueue.Enqueue(t);
    }

    private Vector2 GetRandomWormTarget(Viewport viewport) {
        int maxTries = 10;
        int padding = 100;
        for(int i=0; i<maxTries; i++) {
            float x = _random.Next(padding, viewport.Width - padding);
            float y = _random.Next(padding, viewport.Height - padding);
            Vector2 candidate = new Vector2(x, y);
            
            bool overlaps = false;
            foreach(var spot in _mudSpots) {
                if(Vector2.Distance(candidate, spot.Position) < 200f) {
                    overlaps = true;
                    break;
                }
            }
            if(!overlaps) return candidate;
        }
        return new Vector2(viewport.Width/2, viewport.Height/2);
    }

    public void Update(GameTime gameTime, Viewport viewport, MouseState mouseState) {
        if (IsGameOver) return;
        var ms = Mouse.GetState();
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Point mousePos = new Point(mouseState.X, mouseState.Y);

        if (_flashbangAlpha > 0) { _flashbangAlpha -= 0.5f * delta; if (_flashbangAlpha < 0) _flashbangAlpha = 0; }
        
        _mudSpots.RemoveAll(spot => !_entities.Contains(spot.Owner));

        bool isShielding = false;
        _currentShieldFrame = 0; 
        if (_shieldDurability < _maxShieldDurability && !isShielding && !_isShieldBroken) _shieldDurability += 2f * delta;
        if (_isShieldBroken) { _shieldDurability += 4f * delta; if (_shieldDurability >= _maxShieldDurability * 0.5f) _isShieldBroken = false; }
        if (mouseState.RightButton == ButtonState.Pressed && !_isShieldBroken && mouseState.LeftButton == ButtonState.Released) { isShielding = true; _currentShieldFrame = 1; }
        if (_blockFeedbackTimer > 0) { _blockFeedbackTimer -= delta; _currentShieldFrame = 2; }

        if (_showingWaveMessage) { _waveMessageTimer -= delta; if (_waveMessageTimer <= 0) _showingWaveMessage = false; }

        if (_waveInProgress) {
            if (_enemyWaveQueue.Count > 0) { 
                if (!_showingWaveMessage) { 
                    _spawnTimer += delta; 
                    if (_spawnTimer >= _spawnInterval) { 
                        int candidateType = _enemyWaveQueue.Dequeue();
                        if (candidateType == 2) {
                            bool sapoExists = _entities.Any(e => e is Sapo);
                            if (sapoExists) {
                                do { candidateType = _random.Next(5); } while (candidateType == 2);
                            }
                        }
                        SpawnEnemy(viewport, candidateType); 
                        _spawnTimer = 0; 
                    } 
                } 
            }
            else if (_entities.Count == 0) { 
                _waveInProgress = false; 
                _waveBreakTimer = 3.0; 
            }
        }
        else { _waveBreakTimer -= delta; if (_waveBreakTimer <= 0) StartNextWave(); }

        for (int i = _entities.Count - 1; i >= 0; i--) {
            var entity = _entities[i];
            entity.Update(gameTime, viewport);
            if (entity is Hada2 && entity.Rect.Bottom < 0) { _entities.RemoveAt(i); continue; }
            if (entity is Hada hada && hada.IsReadyToAttack) { _playerHealth -= hada.Damage; _entities.RemoveAt(i); continue; }
            
            if (entity is Lobo lobo && lobo.IsReadyToDamage) {
                _playerHealth -= lobo.Damage; 
                lobo.StartRebound(); 
                continue;
            }

            if (entity is Sapo sapo) { 
                if (sapo.ReadyToShoot) { 
                    SpawnProjectile(sapo.Rect.Center.ToVector2(), viewport, _projectileTexture, false, false, owner: sapo); 
                    sapo.ReadyToShoot = false; 
                } 
            }
            else if (entity is Gusano gusano) { 
                if (gusano.ReadyToShoot) { 
                    Vector2 target = GetRandomWormTarget(viewport); 
                    SpawnProjectile(gusano.Rect.Center.ToVector2(), viewport, _arenaProjectileTexture, true, false, targetPos: target, owner: gusano); 
                    gusano.ReadyToShoot = false; 
                } 
            }
            else if (entity.Damage > 0 && !(entity is Hada) && !(entity is Lobo) && !(entity is Hada2) && !(entity is Gusano)) { 
                if (_random.NextDouble() < 0.005 * _difficultyMultiplier) SpawnProjectile(entity.Rect.Center.ToVector2(), viewport, _projectileTexture, false, false, owner: entity); 
            }
        }

        UpdateProjectiles(gameTime, viewport, isShielding, mousePos);

        if (_playerHealth <= 0) { _playerHealth = 0; IsGameOver = true; }

        if (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released && mouseState.RightButton == ButtonState.Released) {
            _playerWand?.Attack();
            Vector2 startPos = new Vector2(viewport.Width * 0.8f, viewport.Height - 400f); 
            SpawnProjectile(startPos, viewport, _playerProjectileTexture, false, true, mousePos.ToVector2(), owner: null); 
        }
        _previousMouseState = ms;
        _playerWand?.Update(gameTime);
    }

    private void SpawnProjectile(Vector2 startPos, Viewport viewport, Texture2D texture, bool isMud, bool isPlayer, Vector2? targetPos = null, Enemies owner = null) {
        Projectile p = new Projectile();
        p.Position = startPos;
        p.Scale = isPlayer ? 0.3f : 0.1f; 
        p.Speed = (isPlayer ? 600f : 200f) * (isPlayer ? 1 : _difficultyMultiplier); 
        p.IsActive = true;
        p.Damage = isMud ? 0 : (isPlayer ? 1 : 10);
        p.Texture = texture;
        p.IsMudEffect = isMud;
        p.IsPlayerProjectile = isPlayer;
        p.LifeTime = 3.0; 
        p.HasStopped = false;
        p.StopDecayTimer = 0.1; 
        p.ActivationDelay = 0; 
        p.Owner = owner; 

        if (!isPlayer && !isMud) { 
            p.TotalFrames = 2; 
            p.FrameWidth = (texture != null) ? texture.Width / 2 : 0; 
            p.CurrentFrame = 0; 
            p.FrameTimer = 0; 
        } else { 
            p.TotalFrames = 1; 
            p.FrameWidth = (texture != null) ? texture.Width : 0; 
        }
        
        if (isMud) { 
            Vector2 finalTarget = targetPos ?? new Vector2(viewport.Width/2, viewport.Height/2); 
            p.Target = finalTarget; 
            float distance = Vector2.Distance(startPos, finalTarget); 
            p.Speed = distance / 2.0f; 
            p.LifeTime = 10.0; 
            p.ActivationDelay = 2.0; 
        } else if (targetPos.HasValue) { 
            p.Target = targetPos.Value; 
        } else { 
            Vector2 screenCenter = new Vector2(viewport.Width / 2, viewport.Height / 2); 
            p.Target = screenCenter; 
        }
        
        _projectiles.Add(p);
    }

    private void UpdateProjectiles(GameTime gameTime, Viewport viewport, bool isShielding, Point mousePos) {
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

        for (int i = _projectiles.Count - 1; i >= 0; i--) {
            var p = _projectiles[i];
            
            if (p.TotalFrames > 1) {
                p.FrameTimer += delta;
                if (p.FrameTimer > 0.1f) {
                    p.FrameTimer = 0;
                    p.CurrentFrame = (p.CurrentFrame + 1) % p.TotalFrames;
                }
            }

            p.LifeTime -= delta;
            if (p.LifeTime <= 0) { _projectiles.RemoveAt(i); continue; }

            if (p.ActivationDelay > 0) {
                p.ActivationDelay -= delta;
                
                if (p.IsMudEffect && p.Speed > 0) {
                    p.Scale += 0.5f * delta; 
                    Vector2 dir = p.Target - p.Position;
                    if (dir.Length() > 0) dir.Normalize();
                    p.Position += dir * p.Speed * delta;
                } else if (!p.IsPlayerProjectile && !p.IsMudEffect) {
                    p.Scale += 0.5f * delta;
                }
                
                if (p.ActivationDelay <= 0) {
                    if (p.IsMudEffect) {
                        if (p.Owner is Gusano g) {
                            _mudSpots.Add(new MudSpot { Position = p.Position, Owner = g });
                        }
                        _projectiles.RemoveAt(i);
                        continue;
                    }
                }
                if (p.IsMudEffect) continue; 
            }

            if (p.IsPlayerProjectile) {
                if (!p.HasStopped) {
                    Vector2 dir = p.Target - p.Position;
                    float dist = dir.Length();
                    if (dist < p.Speed * delta) { p.Position = p.Target; p.HasStopped = true; }
                    else { dir.Normalize(); p.Position += dir * p.Speed * delta; }
                } else { 
                    p.StopDecayTimer -= delta; 
                    if (p.StopDecayTimer <= 0) { _projectiles.RemoveAt(i); continue; } 
                }
            } else if (!p.IsMudEffect) {
                p.Scale += 0.5f * delta; 
                if (p.Speed > 0) {
                    Vector2 dir = p.Target - p.Position;
                    if (dir.Length() > 0) dir.Normalize();
                    p.Position += dir * p.Speed * delta; 
                }
            }

            int pW = (p.Texture != null) ? (int)(p.FrameWidth * p.Scale) : 10; 
            if (p.IsPlayerProjectile && p.Texture != null) pW /= 3;

            int hitboxSize = (p.IsPlayerProjectile) ? (int)(pW * 0.4f) : pW; 

            Rectangle pRect = new Rectangle((int)(p.Position.X - hitboxSize / 2), (int)(p.Position.Y - hitboxSize / 2), hitboxSize, hitboxSize);

            if (p.IsPlayerProjectile) {
                if (!viewport.Bounds.Contains(p.Position)) { _projectiles.RemoveAt(i); continue; }
                bool hit = false;
                for (int j = _entities.Count - 1; j >= 0; j--) {
                    if (_entities[j] is Gusano g && (g.CurrentState == Gusano.State.Hidden || g.CurrentState == Gusano.State.Burrowing)) continue;

                    if (_entities[j].Rect.Intersects(pRect)) {
                        if (_entities[j].TakeDamage(p.Damage)) { 
                            if (_entities[j] is Hada2) _flashbangAlpha = 1.5f; 
                            _entities.RemoveAt(j); 
                        }
                        hit = true; 
                        break; 
                    }
                }
                if (hit) _projectiles.RemoveAt(i);
            } else {
                if (p.Scale > 1.5f && !p.IsMudEffect) {
                    bool blocked = false;
                    if (isShielding) {
                        int shieldRadius = 100; 
                        Rectangle shieldRect = new Rectangle(mousePos.X - shieldRadius, mousePos.Y - shieldRadius, shieldRadius * 2, shieldRadius * 2);
                        if (shieldRect.Intersects(pRect)) {
                            blocked = true; 
                            _blockFeedbackTimer = 0.2; 
                            int shieldDmg = (p.Damage > 0) ? p.Damage * 2 : 5;
                            _shieldDurability -= shieldDmg; 
                            if (_shieldDurability <= 0) { _shieldDurability = 0; _isShieldBroken = true; }
                        }
                    }
                    if (!blocked) {
                        _playerHealth -= p.Damage; 
                    }
                    _projectiles.RemoveAt(i); 
                }
            }
        }
    }

    private void SpawnEnemy(Viewport viewport, int type) {
        Enemies entity = null;
        Vector2 startPos = Vector2.Zero;

        switch (type) {
            case 0: 
                if (_hadaTexture != null) { float spawnX = _random.Next(100, viewport.Width - 100); startPos = new Vector2(spawnX, -50f); entity = new Hada(startPos, _random, depth, Color.White, _hadaTexture, 2, _hadaTexture.Width/2, _hadaTexture.Height, 0.3f, _difficultyMultiplier); }
                break;

            case 1: 
                if (_loboWalkTexture != null) {
                    bool leftDoor = _random.Next(2) == 0;
                    float spawnX = leftDoor ? (viewport.Width * 0.35f) : (viewport.Width * 0.65f);
                    startPos = new Vector2(spawnX, viewport.Height * 0.48f);
                    int frameW = _loboWalkTexture.Width / 2;
                    entity = new Lobo(startPos, _random, depth, Color.White, _loboWalkTexture, _loboAttackTexture, frameW, _loboWalkTexture.Height, _difficultyMultiplier);
                }
                break;

            case 2: 
                if (_sapoTexture != null) { startPos = new Vector2(viewport.Width * 0.5f, viewport.Height * 0.46f); int frameW = _sapoTexture.Width / 5; entity = new Sapo(startPos, _random, depth, Color.White, _sapoTexture, 5, frameW, _sapoTexture.Height, 0.2f, _difficultyMultiplier); }
                break;

            case 3: 
                if (_gusanoSaliendoTexture != null) { int minX = (int)(viewport.Width * 0.1f); int maxX = (int)(viewport.Width * 0.7f); float spawnX = _random.Next(minX, maxX); int minY = (int)(viewport.Height * 0.5f); int maxY = (int)(viewport.Height * 0.8f); float spawnY = _random.Next(minY, maxY); startPos = new Vector2(spawnX, spawnY); int eW = _gusanoSaliendoTexture.Width / 14; int iW = _gusanoNormalTexture.Width; int aW = (_gusanoAtaqueTexture != null) ? _gusanoAtaqueTexture.Width / 7 : 100; entity = new Gusano(startPos, _random, depth, Color.Orange, _gusanoSaliendoTexture, _gusanoNormalTexture, _gusanoAtaqueTexture, 14, 1, 7, eW, _gusanoSaliendoTexture.Height, iW, _gusanoNormalTexture.Height, aW, _gusanoAtaqueTexture?.Height ?? 100, 0.15f, _difficultyMultiplier); }
                break;
            
            case 4: 
                if (_hada2Texture != null) { float spawnX = _random.Next(100, viewport.Width - 100); startPos = new Vector2(spawnX, -50f); int frameWidth = _hada2Texture.Width / 2; entity = new Hada2(startPos, _random, depth, Color.White, _hada2Texture, 2, frameWidth, _hada2Texture.Height, 0.3f, _difficultyMultiplier); }
                break;
        }
        if (entity != null) _entities.Add(entity);
    }

    public void Draw(SpriteBatch spriteBatch) {
        foreach (var entity in _entities.OrderBy(e => e.Rect.Y)) entity.Draw(spriteBatch, _pixel);

        foreach (var p in _projectiles) {
            if (p.Texture != null) {
                Rectangle sourceRect;
                int size;

                if (p.IsPlayerProjectile) {
                    sourceRect = new Rectangle(0, 0, p.Texture.Width / 3, p.Texture.Height);
                    size = (int)((p.Texture.Width / 3) * p.Scale);
                } else {
                    sourceRect = new Rectangle(p.CurrentFrame * p.FrameWidth, 0, p.FrameWidth, p.Texture.Height);
                    size = (int)(p.FrameWidth * p.Scale);
                }

                Rectangle dest = new Rectangle((int)p.Position.X - size/2, (int)p.Position.Y - size/2, size, size);
                spriteBatch.Draw(p.Texture, dest, sourceRect, Color.White);
            }
        }

        _playerWand?.Draw(spriteBatch);

        if (_mudSpots.Count > 0 && _mudOverlayTexture != null) {
            foreach(var spot in _mudSpots) {
                int size = 800;
                Rectangle mudRect = new Rectangle((int)spot.Position.X - size/2, (int)spot.Position.Y - size/2, size, size);
                Color mudColor = new Color(120, 100, 80, 255); 
                spriteBatch.Draw(_mudOverlayTexture, mudRect, mudColor);
            }
        }

        if (_flashbangAlpha > 0) { spriteBatch.Draw(_pixel, new Rectangle(0, 0, spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height), Color.White * _flashbangAlpha); }

        int margin = 20; int barW = 200; int barH = 20;
        spriteBatch.Draw(_pixel, new Rectangle(margin, margin, barW, barH), Color.DarkRed);
        float hpPct = (float)_playerHealth / _playerMaxHealth;
        
        // Barra de vida verde
        spriteBatch.Draw(_pixel, new Rectangle(margin, margin, (int)(barW * hpPct), barH), Color.Green);

        int shieldY = margin + barH + 5;
        spriteBatch.Draw(_pixel, new Rectangle(margin, shieldY, barW, barH/2), Color.Gray);
        float shieldPct = _shieldDurability / _maxShieldDurability;
        Color shieldColor = _isShieldBroken ? Color.Red : Color.Cyan; 
        spriteBatch.Draw(_pixel, new Rectangle(margin, shieldY, (int)(barW * shieldPct), barH/2), shieldColor);

        if (_showingWaveMessage && _font != null) { 
            Vector2 textSize = _font.MeasureString(_waveMessageText); 
            Vector2 centerPos = new Vector2((spriteBatch.GraphicsDevice.Viewport.Width / 2) - (textSize.X / 2), (spriteBatch.GraphicsDevice.Viewport.Height / 2) - (textSize.Y / 2)); 
            spriteBatch.DrawString(_font, _waveMessageText, centerPos + new Vector2(2, 2), Color.Black); 
            spriteBatch.DrawString(_font, _waveMessageText, centerPos, Color.Gold); 
        } else if (_font != null) { 
            spriteBatch.DrawString(_font, $"Oleada: {_waveNumber}", new Vector2(margin, shieldY + 20), Color.White); 
        }

        MouseState ms = Mouse.GetState();
        if (_shieldTexture != null) {
            Rectangle source = _shieldSourceRects[_currentShieldFrame]; 
            Vector2 origin = new Vector2(source.Width / 2, source.Height / 2);
            spriteBatch.Draw(_shieldTexture, new Vector2(ms.X, ms.Y), source, Color.White, 0f, origin, 0.5f, SpriteEffects.None, 0f);
        }
    }
}