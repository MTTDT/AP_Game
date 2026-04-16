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
        public Gun(string texturePath, Color color)
        {
            Color = color;
            TexturePath = texturePath;

        }


        //Creates a complete gun/barel
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

        }

        //Creates a bnullet in the direction a a barrel
        private void Shoot()
        {
            Vector2 dir = (shooter.GlobalPosition - GlobalPosition).Normalized();

            // Tell ALL peers (including self) to spawn this bullet
            Rpc(nameof(SpawnBullet), shooter.GlobalPosition, GlobalRotation, dir);
        }

        //Checks for the input
        public override void _Process(double delta)
        {
            if (!IsMultiplayerAuthority()) return;

            LookAt(GetGlobalMousePosition());
            Rpc(nameof(SyncGunRotation), GlobalRotation);
        }
        public override void _Input(InputEvent @event)
        {
            if (!IsMultiplayerAuthority()) return;
            if (@event is InputEventMouseButton mouseEvent &&
                mouseEvent.ButtonIndex == MouseButton.Left &&
                !mouseEvent.Pressed)
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

        // Runs on all OTHER peers to rotate this gun visually
        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false,
             TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
        private void SyncGunRotation(float rot)
        {
            GlobalRotation = rot;
        }
    }
}
