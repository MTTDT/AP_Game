using Godot;
using System;

namespace main
{
    /// <summary>
    /// Base gun class.  Subclasses set their own stats in their constructor and
    /// attach a unique IGunAbility by overriding AddUniqueAbility().
    ///
    /// Configurable fields (set before _Ready runs):
    ///   FireRate     – seconds between normal shots  (default 0.5)
    ///   BulletSpeed  – pixels/s of a normal bullet   (default 500)
    ///   BulletRange  – max travel distance in pixels (default 400)
    ///   BulletScale  – sprite / collision scale      (default 1.0)
    ///   PierceWalls  – whether bullets skip body/wall collision masks (default false)
    /// </summary>
    public partial class Gun : Node2D
    {
        // ── Configurable by subclasses ───────────────────────────────────────
        protected float FireRate    = 0.5f;
        protected float BulletSpeed = 500f;
        protected float BulletRange = 400f;
        protected float BulletScale = 1.0f;
        protected int   BulletDamage = 15;
        protected bool PierceWalls = false;

        // ── Private state ────────────────────────────────────────────────────
        private readonly string _texturePath;
        private readonly Color  _color;

        private Marker2D _shooter;
        private Timer    _shootTimer;
        private bool     _canShoot = true;

        // ── Constructor ──────────────────────────────────────────────────────
        public Gun(string texturePath, Color color)
        {
            _texturePath = texturePath;
            _color       = color;
        }

        // ── Godot lifecycle ──────────────────────────────────────────────────
        public override void _Ready()
        {
            // Sprite
            var sprite = new Sprite2D
            {
                Texture          = GD.Load<Texture2D>(_texturePath),
                Modulate         = _color,
                Offset           = new Vector2(0, -30f),
                RotationDegrees  = 90f
            };
            AddChild(sprite);

            // Muzzle marker
            _shooter          = new Marker2D { Position = new Vector2(60f, 0f) };
            AddChild(_shooter);

            // Shoot cooldown timer
            _shootTimer          = new Timer { WaitTime = FireRate, OneShot = true };
            _shootTimer.Timeout += () => _canShoot = true;
            AddChild(_shootTimer);

            // Subclass-specific ability (e.g. big bullet, pierce, burst)
            AddUniqueAbility();
        }

        /// <summary>Override in subclasses to add a unique IGunAbility child.</summary>
        protected virtual void AddUniqueAbility() { }

        // ── Shooting ─────────────────────────────────────────────────────────

        /// <summary>
        /// Fires one normal bullet.  Called by input handler and by abilities
        /// that need to trigger a shot programmatically.
        /// </summary>
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

        /// <summary>
        /// Fires a special bullet with overridden parameters.
        /// Used by abilities (e.g. BigBulletAbility, PierceAbility).
        /// </summary>
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

        // ── Replication ──────────────────────────────────────────────────────

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

        // ── Per-frame ────────────────────────────────────────────────────────

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
