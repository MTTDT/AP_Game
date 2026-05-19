using Godot;
using System;

namespace main
{

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
        private ProgressBar _hpBar; 
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
            var bodyShape = new CollisionShape2D();
            bodyShape.Shape = new RectangleShape2D { Size = new Vector2(60f, 60f) };
            AddChild(bodyShape);

            Position = _Position;

            SetDeferred("collision_layer", 1);
            SetDeferred("collision_mask", 2 | 3 | 5);

            var sprite = new Sprite2D
            {
                Texture = GD.Load<Texture2D>(TexturePath),
                Modulate = Color
            };
            AddChild(sprite);

            if (IsMultiplayerAuthority())
            {
                var camera = new Camera2D { Enabled = true };
                AddChild(camera);
                camera.LimitLeft = -1075;
                camera.LimitTop = -622;
                camera.LimitRight = 1075;
                camera.LimitBottom = 622;
            }

            Gun gun = _gunType switch
            {
                GunType.Sniper => new SniperGun(Color),
                GunType.MachineGun => new MachineGun(Color),
                _ => new DefaultGun(Color)
            };
            gun.Name = "Gun";
            gun.SetMultiplayerAuthority(GetMultiplayerAuthority());
            AddChild(gun);

            _nameLabel = new Label { Position = new Vector2(-50f, -115f) };
            _nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _nameLabel.CustomMinimumSize = new Vector2(100f, 0f);
            if (IsMultiplayerAuthority())
            {
                _nameLabel.Text = GameState.PlayerName;
            }
            AddChild(_nameLabel);

            _hpBar = new ProgressBar
            {
                Position = new Vector2(-50f, -90f),
                Size = new Vector2(100f, 20f),
                MinValue = 0,
                MaxValue = _maxHp,
                Value = _hp,
                ShowPercentage = false 
            };

            AddChild(_hpBar);
            var barText = new Label
            {
                Name = "BarText",
                Size = _hpBar.Size, 
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _hpBar.AddChild(barText);
            UpdateHpBarText();

            _hitbox = new Area2D { Name = "Hitbox" };
            _hitbox.CollisionLayer = 2;
            _hitbox.CollisionMask = 4;
            var hitShape = new CollisionShape2D();
            hitShape.Shape = new RectangleShape2D { Size = new Vector2(90f, 70f) };
            _hitbox.AddChild(hitShape);
            _hitbox.AreaEntered += OnBulletHit;
            AddChild(_hitbox);

            _resetTimer = new Timer { WaitTime = 5.0, OneShot = true };
            _resetTimer.Timeout += OnResetTimer;
            AddChild(_resetTimer);

            AddChild(new BodyDodge());

            AddUniqueAbility();
        }

        protected virtual void AddUniqueAbility() { }


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


        [Signal] public delegate void BodyDestroyedEventHandler();


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
            _hpBar.Position     = barPos;
            _hpBar.Rotation     = barRot;
            _nameLabel.Position = namePos;
            _nameLabel.Rotation = nameRot;
            _nameLabel.Text     = playerName;
        }
    }
}