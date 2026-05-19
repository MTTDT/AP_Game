using Godot;
using System;

namespace main
{
    public partial class Gun : Node2D
    {
        protected float FireRate    = 0.5f;
        protected float BulletSpeed = 500f;
        protected float BulletRange = 400f;
        protected float BulletScale = 1.0f;
        protected int   BulletDamage = 15;
        protected bool PierceWalls = false;

        private readonly string _texturePath;
        private readonly Color  _color;

        private Marker2D _shooter;
        private Timer    _shootTimer;
        private bool     _canShoot = true;

        public Gun(string texturePath, Color color)
        {
            _texturePath = texturePath;
            _color       = color;
        }

        public override void _Ready()
        {
            var sprite = new Sprite2D
            {
                Texture = GD.Load<Texture2D>(_texturePath),
                Modulate = _color,
                Offset = new Vector2(0, -30f),
                RotationDegrees = 90f
            };
            AddChild(sprite);

            _shooter = new Marker2D { Position = new Vector2(60f, 0f) };
            AddChild(_shooter);

            _shootTimer = new Timer { WaitTime = FireRate, OneShot = true };
            _shootTimer.Timeout += () => _canShoot = true;
            AddChild(_shootTimer);

            AddUniqueAbility();
            
            CallDeferred(nameof(InitializeUI));
        }

        private void InitializeUI()
        {
            if (GetParent() is Body body && body.IsMultiplayerAuthority())
            {
                Vector2 offset = new Vector2(140f, 20f);
                CooldownUI.Create(this, _shootTimer, (float)_shootTimer.WaitTime, CooldownUI.ScreenCorner.BottomRight, offset, "ML");
            }
        }

        protected virtual void AddUniqueAbility() { }

        protected void Shoot()
        {
            if (!_canShoot) return;

            Vector2 dir = (_shooter.GlobalPosition - GlobalPosition).Normalized();
            Rpc(nameof(SpawnBullet),
                _shooter.GlobalPosition,
                GlobalRotation,
                dir,
                BulletSpeed,
                BulletRange,
                BulletScale,
                PierceWalls,
                BulletDamage);

            _canShoot = false;
            _shootTimer.Start();
        }

        public void ShootSpecial(float speed, float range, float scale, bool pierceWalls, int damage)
        {
            Vector2 dir = (_shooter.GlobalPosition - GlobalPosition).Normalized();
            Rpc(nameof(SpawnBullet),
                _shooter.GlobalPosition,
                GlobalRotation,
                dir,
                speed,
                range,
                scale,
                pierceWalls,
                damage);
        }


        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
        private void SpawnBullet(
            Vector2 spawnPos, float spawnRot, Vector2 dir,
            float speed, float range, float scale, bool pierceWalls, int damage)
        {
            var bullet = new Bullet(
                new Vector2(speed, 0f), dir, "res://bullet.svg", range,
                scale, pierceWalls, damage);

            bullet.GlobalPosition = spawnPos;
            bullet.Rotation       = spawnRot;
            GetTree().CurrentScene.AddChild(bullet);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false,
             TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
        private void SyncGunRotation(float rot) => GlobalRotation = rot;


        public override void _Process(double delta)
        {
            if (!IsMultiplayerAuthority()) return;

            LookAt(GetGlobalMousePosition());
            Rpc(nameof(SyncGunRotation), GlobalRotation);
        }

        public override void _Input(InputEvent @event)
        {
            if (!IsMultiplayerAuthority()) return;

            if (@event is InputEventMouseButton mb &&
                mb.ButtonIndex == MouseButton.Left &&
                mb.Pressed)
            {
                Shoot();
            }
        }
    }
}
