using Godot;
using System;

namespace main
{
    /// <summary>
    /// Base body class. Handles movement, HP, collision, camera, and gun.
    /// Subclasses (DefaultBody, BulkyBody, SnapyBody) attach their own
    /// unique IBodyAbility in AddUniqueAbility().
    /// All bodies also receive BodyDodge automatically.
    /// </summary>
    public partial class Body : CharacterBody2D
    {
        private string TexturePath { get; set; }
        private Color Color { get; set; }
        private new Node Owner { get; set; }
        private Vector2 Size { get; set; }
        private Vector2 _Position { get; set; }

        [Export] public int Speed { get; set; } = 300;
        [Export] public float RotationSpeed { get; set; } = 5f;

        private float _rotationDirection;
        protected int _hp = 100;
        protected int _maxHp = 100;
        private Label _hpLabel;
        private Area2D _hitbox;

        private Timer _resetTimer;

        public Body() { }

        public Body(string texturePath, Color color, Node owner, Vector2 size, Vector2 position)
        {
            TexturePath = texturePath;
            Color = color;
            Owner = owner;
            Size = size;
            _Position = position;
        }

        public override void _Ready()
        {
            // --- Collision shape ---
            var bodyShape = new CollisionShape2D();
            var rect = new RectangleShape2D { Size = new Vector2(60f, 60f) };
            bodyShape.Shape = rect;
            AddChild(bodyShape);

            Position = _Position;

            SetDeferred("collision_layer", 1);
            SetDeferred("collision_mask", 2 | 3 | 5);

            // --- Sprite ---
            var sprite = new Sprite2D();
            sprite.Texture = GD.Load<Texture2D>(TexturePath);
            sprite.Modulate = Color;
            AddChild(sprite);

            // --- Camera (authority only) ---
            if (IsMultiplayerAuthority())
            {
                var camera = new Camera2D { Enabled = true };
                AddChild(camera);
                camera.LimitLeft   = -1075;
                camera.LimitTop    = -622;
                camera.LimitRight  =  1075;
                camera.LimitBottom =  622;
            }

            // --- Gun ---
            var gun = new Gun("res://gun.svg", Color);
            gun.Name = "Gun";
            gun.SetMultiplayerAuthority(GetMultiplayerAuthority());
            AddChild(gun);

            // --- HP label ---
            _hpLabel = new Label();
            _hpLabel.Position = new Vector2(-20f, -90f);
            UpdateHpLabel();
            AddChild(_hpLabel);

            // --- Hitbox ---
            _hitbox = new Area2D { Name = "Hitbox" };
            _hitbox.CollisionLayer = 2;
            _hitbox.CollisionMask  = 4;
            var hitShape = new CollisionShape2D();
            hitShape.Shape = new RectangleShape2D { Size = new Vector2(90f, 70f) };
            _hitbox.AddChild(hitShape);
            _hitbox.BodyEntered += OnBulletHit;
            AddChild(_hitbox);

            // --- Auto-reset timer (respawn HP after death) ---
            _resetTimer = new Timer { WaitTime = 5.0, OneShot = true };
            _resetTimer.Timeout += OnResetTimer;
            AddChild(_resetTimer);

            // --- Shared dodge ability ---
            AddChild(new BodyDodge());

            // --- Subclass-specific ability ---
            AddUniqueAbility();
        }

        /// <summary>
        /// Override in each subclass to attach that body's unique IBodyAbility.
        /// </summary>
        protected virtual void AddUniqueAbility() { }

        // ── HP helpers ──────────────────────────────────────────────────────

        protected void UpdateHpLabel() => _hpLabel.Text = $"HP: {_hp}";

        public void SetHp(int value)
        {
            _hp = Mathf.Clamp(value, 0, 999);
            UpdateHpLabel();
        }

        public void SetHitboxEnabled(bool enabled) => _hitbox.SetDeferred("monitoring", enabled);

        private float _damageReduction = 1.0f;

        /// <summary>
        /// Sets the damage multiplier. 1.0 = full damage, 0.5 = half damage, etc.
        /// </summary>
        public void SetDamageReduction(float multiplier) => _damageReduction = multiplier;

        // ── Signals ─────────────────────────────────────────────────────────

        [Signal] public delegate void BodyDestroyedEventHandler();

        // ── Bullet hit ───────────────────────────────────────────────────────

        private void OnBulletHit(Node body)
        {
            if (body is not Bullet) return;

            body.QueueFree();

            int damage = Mathf.RoundToInt(10 * _damageReduction);
            _hp = Mathf.Max(0, _hp - damage);
            UpdateHpLabel();
            _resetTimer.Start();

            if (_hp <= 0)
            {
                GD.Print($"{Name} is dead!");
                EmitSignal(SignalName.BodyDestroyed);
            }
        }

        private void OnResetTimer()
        {
            _hp = _maxHp;
            UpdateHpLabel();
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

            var offset = new Vector2(-20f, -90f);
            _hpLabel.Position = offset.Rotated(-Rotation);
            _hpLabel.Rotation = -Rotation;

            Rpc(nameof(SyncTransform), Position, Rotation, _hpLabel.Position, _hpLabel.Rotation);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false,
             TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
        private void SyncTransform(Vector2 pos, float rot, Vector2 labelPos, float labelRot)
        {
            Position         = pos;
            Rotation         = rot;
            _hpLabel.Position = labelPos;
            _hpLabel.Rotation = labelRot;
        }
    }
}