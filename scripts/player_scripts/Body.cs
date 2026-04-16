using Godot;
using System;

namespace main
{
    public partial class Body : CharacterBody2D
    {
        [Signal]
        public delegate void BodyDestroyedEventHandler();

        private string _texturePath;
        private Color _color;
        private Vector2 _size;
        private Vector2 _startPos;

        [Export]
        public float Speed { get; set; } = 350f;

        [Export]
        public float RotationSpeed { get; set; } = 3.5f;

        private Gun _gun;
        private Camera2D _camera;
        private Label _hpLabel;
        public int Health { get; private set; } = 100;

        private Rect2 _mapBounds;

        private Timer _resetTimer;
        private Timer _increasedHealthTimer;

        // Parameterless constructor required for Godot multiplayer instantiation
        public Body() { }

        public Body(string texturePath, Color color, Vector2 size, Vector2 startPos)
        {
            _texturePath = texturePath;
            _color = color;
            _size = size;
            _startPos = startPos;
        }

        public override void _Ready()
        {
            Position = _startPos;

            SetDeferred("collision_layer", 1);
            SetDeferred("collision_mask", 2 | 3 | 5);

            // SPRITE
            Sprite2D sprite = new Sprite2D();
            sprite.Texture = GD.Load<Texture2D>(_texturePath);
            sprite.Modulate = _color;
            sprite.Centered = true;
            AddChild(sprite);

            // BODY COLLISION SHAPE
            CollisionShape2D shape = new CollisionShape2D();
            RectangleShape2D rect = new RectangleShape2D();
            rect.Size = _size;
            shape.Shape = rect;
            AddChild(shape);

            // MAP BOUNDS
            Sprite2D map = GetTree().CurrentScene.GetNode<Sprite2D>("Sprite2D");
            Vector2 mapPos = map.GlobalPosition;
            Vector2 mapSize = map.Texture.GetSize() * map.Scale;
            _mapBounds = new Rect2(mapPos - mapSize / 2f, mapSize);

            // CAMERA
            _camera = new Camera2D();
            _camera.Enabled = IsMultiplayerAuthority();
            _camera.LimitLeft   = (int)_mapBounds.Position.X;
            _camera.LimitTop    = (int)_mapBounds.Position.Y;
            _camera.LimitRight  = (int)(_mapBounds.Position.X + _mapBounds.Size.X);
            _camera.LimitBottom = (int)(_mapBounds.Position.Y + _mapBounds.Size.Y);
            AddChild(_camera);

            // GUN
            _gun = new Gun("res://gun.svg", _color);
            _gun.Name = "Gun";
            _gun.SetMultiplayerAuthority(GetMultiplayerAuthority());
            AddChild(_gun);

            // HP LABEL
            _hpLabel = new Label();
            _hpLabel.Text = $"HP: {Health}";
            _hpLabel.Position = new Vector2(-20f, -90f);
            _hpLabel.Modulate = Colors.White;
            AddChild(_hpLabel);

            // BULLET HITBOX
            Area2D hitbox = new Area2D();
            hitbox.Name = "Hitbox";
            hitbox.CollisionLayer = 2;
            hitbox.CollisionMask = 4;

            CollisionShape2D hitShape = new CollisionShape2D();
            RectangleShape2D hitRect = new RectangleShape2D();
            hitRect.Size = new Vector2(90f, 70f);
            hitShape.Shape = hitRect;
            hitbox.AddChild(hitShape);

            hitbox.BodyEntered += OnBulletHit;
            AddChild(hitbox);

            // RESET TIMER (auto-heal to 100 after 5s)
            _resetTimer = new Timer();
            _resetTimer.WaitTime = 5.0;
            _resetTimer.OneShot = true;
            _resetTimer.Timeout += OnResetTimer;
            AddChild(_resetTimer);

            // INCREASED HEALTH TIMER (caps boosted HP back to 100 after 5s)
            _increasedHealthTimer = new Timer();
            _increasedHealthTimer.WaitTime = 5.0;
            _increasedHealthTimer.OneShot = true;
            _increasedHealthTimer.Timeout += OnIncreasedHealthTimer;
            AddChild(_increasedHealthTimer);

            // ABILITIES
            BodyDodge bodyDodge = new BodyDodge();
            AddChild(bodyDodge);

            BodyHeal bodyHeal = new BodyHeal();
            bodyHeal.HealBody += IncreaseHealth;
            AddChild(bodyHeal);
        }

        // ── Input & Physics ────────────────────────────────────────────────────

        public override void _PhysicsProcess(double delta)
        {
            if (!IsMultiplayerAuthority()) return;

            // Rotation
            float rotDir = Input.GetAxis("a", "d");
            Rotation += rotDir * RotationSpeed * (float)delta;

            // Movement (forward/back along local -Y axis)
            float moveDir = Input.GetAxis("w", "s");
            Velocity = -Transform.Y * moveDir * Speed;

            MoveAndSlide();

            // Clamp to map bounds
            GlobalPosition = new Vector2(
                Mathf.Clamp(GlobalPosition.X, _mapBounds.Position.X, _mapBounds.Position.X + _mapBounds.Size.X),
                Mathf.Clamp(GlobalPosition.Y, _mapBounds.Position.Y, _mapBounds.Position.Y + _mapBounds.Size.Y)
            );

            // Keep HP label upright and above the tank
            Vector2 offset = new Vector2(-20f, -90f);
            _hpLabel.Position = offset.Rotated(-Rotation);
            _hpLabel.Rotation = -Rotation;

            // Sync to all other peers
            Rpc(nameof(SyncTransform), Position, Rotation, _hpLabel.Position, _hpLabel.Rotation);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false,
             TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
        private void SyncTransform(Vector2 pos, float rot, Vector2 labelPos, float labelRot)
        {
            Position        = pos;
            Rotation        = rot;
            _hpLabel.Position = labelPos;
            _hpLabel.Rotation = labelRot;
        }

        // ── Damage & Health ────────────────────────────────────────────────────

        private void OnBulletHit(Node body)
        {
            if (body is not Bullet) return;

            body.QueueFree();
            TakeDamage(10);
            _resetTimer.Start();
        }

        public void TakeDamage(int amount)
        {
            Health = Mathf.Max(0, Health - amount);
            _hpLabel.Text = $"HP: {Health}";

            if (Health <= 0)
            {
                GD.Print("Dead!");
                EmitSignal(SignalName.BodyDestroyed);
                QueueFree();
            }
        }

        private void OnResetTimer()
        {
            Health = 100;
            _hpLabel.Text = "HP: 100";
        }

        private void OnIncreasedHealthTimer()
        {
            if (Health > 100)
            {
                Health = 100;
                _hpLabel.Text = "HP: 100";
            }
        }

        private void IncreaseHealth()
        {
            Health = 150;
            _hpLabel.Text = "HP: 150";
            _increasedHealthTimer.Start();
        }
    }
}