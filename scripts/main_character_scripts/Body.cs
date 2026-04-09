using Godot;
using System;

namespace main
{
	public partial class Body : CharacterBody2D
	{
		[Export] public string TexturePath { get; set; }
		[Export] public Color Color { get; set; }
		[Export] public Vector2 Size { get; set; }
		[Export] public Vector2 StartPosition { get; set; }

		[Export] public int Speed { get; set; } = 400;
		[Export] public float RotationSpeed { get; set; } = 4f;

		private float _rotationDirection;

		public override void _Ready()
		{
			Position = StartPosition;

			CollisionLayer = 1; // jugador
			CollisionMask = 2 | 3; // bordes + dummy

			Sprite2D sprite = new Sprite2D();
			sprite.Texture = GD.Load<Texture2D>(TexturePath);
			sprite.Modulate = Color;
			AddChild(sprite);

			CollisionShape2D col = new CollisionShape2D();
			RectangleShape2D shape = new RectangleShape2D();
			shape.Size = Size;
			col.Shape = shape;
			AddChild(col);

			Camera2D camera = new Camera2D();
			camera.Enabled = true;
			AddChild(camera);

			Gun gun = new Gun("res://gun.svg", Color);
			AddChild(gun);
		}

		public void GetInput()
		{
			_rotationDirection = Input.GetAxis("a", "d");
			Velocity = Transform.Y * Input.GetAxis("w", "s") * Speed;
		}

		public override void _PhysicsProcess(double delta)
		{
			GetInput();
			Rotation += _rotationDirection * RotationSpeed * (float)delta;
			MoveAndSlide();
		}
	}
}
