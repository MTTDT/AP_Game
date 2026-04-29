using Godot;
using System;

namespace main
{
    public partial class Gun : Node2D
    {
        private String TexturePath { get; set; }
        private Color Color { get; set; }
        private CharacterBody2D ParentBody { get; set; }
        private Marker2D shooter;
        
        // 1. Define the Timer and a flag
        private Timer shootTimer;
        private bool canShoot = true;

        public Gun(string texturePath, Color color)
        {
            Color = color;
            TexturePath = texturePath;
        }

        public override void _Ready()
        {
            Sprite2D sprite = new Sprite2D();
            sprite.Texture = GD.Load<Texture2D>(TexturePath);
            sprite.Modulate = Color;
            sprite.Offset = new Vector2(0, -30f);
            sprite.RotationDegrees = 90f;
            AddChild(sprite);

            shooter = new Marker2D();
            shooter.Position = new Vector2(60f, 0f);
            AddChild(shooter);

            // 2. Initialize the Timer
            shootTimer = new Timer();
            shootTimer.WaitTime = 0.5f; // Your 0.5s gap
            shootTimer.OneShot = true;  // We only want it to run once per shot
            AddChild(shootTimer);

            // 3. Connect the timeout signal using a Lambda or Method
            shootTimer.Timeout += () => canShoot = true;
        }

        private void Shoot()
        {
            // 4. Check the flag before shooting
            if (!canShoot) return;

            Vector2 dir = (shooter.GlobalPosition - GlobalPosition).Normalized();
            Rpc(nameof(SpawnBullet), shooter.GlobalPosition, GlobalRotation, dir);

            // 5. Start the cooldown
            canShoot = false;
            shootTimer.Start();
        }

        public override void _Process(double delta)
        {
            if (!IsMultiplayerAuthority()) return;

            LookAt(GetGlobalMousePosition());
            Rpc(nameof(SyncGunRotation), GlobalRotation);
        }

        public override void _Input(InputEvent @event)
        {
            if (!IsMultiplayerAuthority()) return;

            // Tip: For rapid fire, usually 'mouseEvent.Pressed' is preferred over '!mouseEvent.Pressed'
            if (@event is InputEventMouseButton mouseEvent &&
                mouseEvent.ButtonIndex == MouseButton.Left &&
                mouseEvent.Pressed) 
            {
                Shoot();
            }
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
        private void SpawnBullet(Vector2 spawnPos, float spawnRot, Vector2 dir)
        {
            Bullet bullet = new Bullet(new Vector2(500f, 0f), dir, "res://bullet.svg", 400f);
            bullet.GlobalPosition = spawnPos;
            bullet.Rotation = spawnRot;
            GetTree().CurrentScene.AddChild(bullet);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false,
             TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
        private void SyncGunRotation(float rot)
        {
            GlobalRotation = rot;
        }
    }
}