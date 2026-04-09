using Godot;
using System;

namespace main
{
	public partial class Gun : Node2D
	{
		private string TexturePath { get; set; }
		private Color Color { get; set; }
		private Marker2D shooter;

		public Gun(string texturePath, Color color)
		{
			TexturePath = texturePath;
			Color = color;
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
		}

		private void Shoot()
		{
			Vector2 dir = (shooter.GlobalPosition - GlobalPosition).Normalized();

			Bullet bullet = new Bullet(dir, "res://bullet.svg", 600f);
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
				Shoot();
			}
		}

		public override void _PhysicsProcess(double delta)
		{
			LookAt(GetGlobalMousePosition());
		}
	}
}
