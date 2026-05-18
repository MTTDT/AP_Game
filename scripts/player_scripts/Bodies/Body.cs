using Godot;
using System;

namespace main
{
    /// <summary>
    /// Base body class. Handles movement, HP, collision, camera, and gun.
    /// Subclasses (DefaultBody, BulkyBody, SnapyBody) attach their own
    /// unique IBodyAbility in AddUniqueAbility().
    /// All bodies also receive BodyDodge automatically.
    ///
    /// The GunType parameter selects which Gun subclass is instantiated.
    /// </summary>
    public partial class Body : CharacterBody2D
    {
        private string TexturePath { get; set; }
        private Color Color { get; set; }
        private new Node Owner { get; set; }
        private Vector2 Size { get; set; }
        private Vector2 _Position { get; set; }
        private GunType _gunType { get; set; }

        [Export] public int Speed { get; set; } = 300;
        [Export] public float RotationSpeed { get; set; } = 5f;

        private float _rotationDirection;
        protected int _hp = 100;
        protected int _maxHp = 100;
        
        private Label _nameLabel;
        private ProgressBar _hpBar; // Changed to standard ProgressBar for built-in text support
        private Area2D _hitbox;
        private Timer _resetTimer;

        public Body() { }

        public Body(string texturePath, Color color, Node owner,
                    Vector2 size, Vector2 position,
                    GunType gunType = GunType.Default)
        {
            TexturePath = texturePath;
            Color = color;
            Owner = owner;
            Size = size;
            _Position = position;
            _gunType = gunType;
        }

        public override void _Ready()
        {
            // --- Collision shape ---
            var bodyShape = new CollisionShape2D();
            bodyShape.Shape = new RectangleShape2D { Size = new Vector2(60f, 60f) };
            AddChild(bodyShape);

            Position = _Position;

            SetDeferred("collision_layer", 1);
            SetDeferred("collision_mask", 2 | 3 | 5);

            // --- Sprite ---
            var sprite = new Sprite2D
            {
                Texture = GD.Load<Texture2D>(TexturePath),
                Modulate = Color
            };
            AddChild(sprite);

            // --- Camera (authority only) ---
            if (IsMultiplayerAuthority())
            {
                var camera = new Camera2D { Enabled = true };
                AddChild(camera);
                camera.LimitLeft = -1075;
                camera.LimitTop = -622;
                camera.LimitRight = 1075;
                camera.LimitBottom = 622;
            }

            // --- Gun (subclass selected by GunType) ---
            Gun gun = _gunType switch
            {
                GunType.Sniper => new SniperGun(Color),
                GunType.MachineGun => new MachineGun(Color),
                _ => new DefaultGun(Color)
            };
            gun.Name = "Gun";
            gun.SetMultiplayerAuthority(GetMultiplayerAuthority());
            AddChild(gun);

            // --- Name label ---
            _nameLabel = new Label { Position = new Vector2(-50f, -115f) };
            _nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _nameLabel.CustomMinimumSize = new Vector2(100f, 0f);
            if (IsMultiplayerAuthority())
            {
                _nameLabel.Text = GameState.PlayerName;
            }
            AddChild(_nameLabel);

            // --- Built-in HP Progress Bar ---
            _hpBar = new ProgressBar
            {
                Position = new Vector2(-50f, -90f),
                Size = new Vector2(100f, 20f), // Defines a clear visual width/height
                MinValue = 0,
                MaxValue = _maxHp,
                Value = _hp,
                ShowPercentage = false // We will override this text manually below
            };

            AddChild(_hpBar);
            var barText = new Label
            {
                Name = "BarText",
                Size = _hpBar.Size, // Match the size of the progress bar
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _hpBar.AddChild(barText);
            UpdateHpBarText();

            // --- Hitbox ---
            _hitbox = new Area2D { Name = "Hitbox" };
            _hitbox.CollisionLayer = 2;
            _hitbox.CollisionMask = 4;
            var hitShape = new CollisionShape2D();
            hitShape.Shape = new RectangleShape2D { Size = new Vector2(90f, 70f) };
            _hitbox.AddChild(hitShape);
            _hitbox.AreaEntered += OnBulletHit;
            AddChild(_hitbox);

            // --- Auto-reset timer (respawn HP after damage) ---
            _resetTimer = new Timer { WaitTime = 5.0, OneShot = true };
            _resetTimer.Timeout += OnResetTimer;
            AddChild(_resetTimer);

            // --- Shared dodge ability ---
            AddChild(new BodyDodge());

            // --- Subclass-specific body ability ---
            AddUniqueAbility();
        }

        /// <summary>Override in each body subclass to attach that body's unique IBodyAbility.</summary>
        protected virtual void AddUniqueAbility() { }

        // ── HP helpers ──────────────────────────────────────────────────────

        protected void UpdateHpBarText()
        {
            if (_hpBar != null)
            {
                _hpBar.Value = _hp;

                var barText = _hpBar.GetNodeOrNull<Label>("BarText");
                if (barText != null)
                {
                    barText.Text = $"{_hp} / {_maxHp}";
                }
            }
        }
        public void SetHp(int value)
        {
            _hp = Mathf.Clamp(value, 0, 999);
            UpdateHpBarText();
        }

        public void SetHitboxEnabled(bool enabled) =>
            _hitbox.SetDeferred("monitoring", enabled);

        private float _damageReduction = 1.0f;

        public void SetDamageReduction(float multiplier) => _damageReduction = multiplier;

        // ── Signals ─────────────────────────────────────────────────────────

        [Signal] public delegate void BodyDestroyedEventHandler();

        // ── Bullet hit ───────────────────────────────────────────────────────

        private void OnBulletHit(Area2D body)
        {
            if (body is not Bullet bullet) return;

            int bulletDamage = bullet.Damage;

            bullet.QueueFree();

            if (!IsMultiplayerAuthority()) return;

            int damage = Mathf.RoundToInt(bulletDamage * _damageReduction);

            _hp = Mathf.Max(0, _hp - damage);
            UpdateHpBarText();

            Rpc(nameof(SyncHp), _hp);

            if (_hp <= 0)
            {
                _resetTimer.Stop();
                GD.Print($"{Name} is dead!");
                EmitSignal(SignalName.BodyDestroyed);
            }
            else
            {
                _resetTimer.Start();
            }
        }

        [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
        private void SyncHp(int hp)
        {
            _hp = hp;
            UpdateHpBarText();
        }

        private void OnResetTimer()
        {
            _hp = _maxHp;
            UpdateHpBarText();
            Rpc(nameof(SyncHp), _hp);
        }

        // ── Movement ─────────────────────────────────────────────────────────

        public void GetInput()
        {
            _rotationDirection = Input.GetAxis("a", "d");
            Velocity = Transform.Y * Input.GetAxis("w", "s") * Speed;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (!IsMultiplayerAuthority()) return;

            GetInput();
            Rotation += _rotationDirection * RotationSpeed * (float)delta;
            MoveAndSlide();

            // Rotate health bar layout offset around player space
            var barOffset = new Vector2(-50f, -90f);
            _hpBar.Position = barOffset.Rotated(-Rotation);
            _hpBar.Rotation = -Rotation;

            var nameOffset = new Vector2(-50f, -115f);
            _nameLabel.Position = nameOffset.Rotated(-Rotation);
            _nameLabel.Rotation = -Rotation;

            Rpc(nameof(SyncTransform),
                Position, Rotation, _hpBar.Position, _hpBar.Rotation, _nameLabel.Position, _nameLabel.Rotation, _nameLabel.Text);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false,
             TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
        private void SyncTransform(Vector2 pos, float rot,
                                   Vector2 barPos, float barRot, Vector2 namePos, float nameRot, string playerName)
        {
            Position            = pos;
            Rotation            = rot;
            
            // Sync UI components over network parameters
            _hpBar.Position     = barPos;
            _hpBar.Rotation     = barRot;

            _nameLabel.Position = namePos;
            _nameLabel.Rotation = nameRot;
            _nameLabel.Text     = playerName;
        }
    }
}