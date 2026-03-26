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

        public override void _Ready()
        {

            Sprite2D sprite = new Sprite2D();
            sprite.Texture = GD.Load<Texture2D>(TexturePath);
            sprite.Modulate = Color;
            sprite.Offset = new Vector2(0, -30f);
            sprite.RotationDegrees = 90f;
            AddChild(sprite);

            shooter = new Marker2D();
            shooter.Position = new Vector2(60f,0f);
            AddChild(shooter);

        }

        private void Shoot()
        {
            Vector2 dir = (shooter.GlobalPosition - GlobalPosition).Normalized();

            Bullet bullet = new Bullet(new Vector2(500f, 0f), dir, texturePath: "res://bullet.svg",200f);

            bullet.GlobalPosition = shooter.GlobalPosition;
            bullet.Rotation = GlobalRotation;

            GetTree().CurrentScene.AddChild(bullet);
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseEvent &&
                mouseEvent.ButtonIndex == MouseButton.Left &&
                !mouseEvent.Pressed)
            {
                			if (IsMultiplayerAuthority()) Shoot();
            }
        }

        public void GetInput()
        {
            LookAt(GetGlobalMousePosition());
        }
        public override void _PhysicsProcess(double delta)
        {
            if (IsMultiplayerAuthority()) GetInput();
        }
    }
}