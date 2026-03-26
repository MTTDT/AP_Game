using Godot;
using System;
namespace main
{
	public partial class Body : CharacterBody2D
	{
		private String TexturePath { get; set; }
		private Color Color { get; set; }
		private Node Owner { get; set; }
		private Vector2 Size { get; set; }
		private Vector2 _Position { get; set; }

		[Export]
		public int Speed { get; set; } = 400;

		[Export]
		public float RotationSpeed { get; set; } = 4f;

		private float _rotationDirection;

		public Body(string texturePath, Color color, Node owner, Vector2 size, Vector2 position)
		{
			this.TexturePath = texturePath;
			this.Color = color;
			this.Owner = owner;
			this.Size = size;
			this._Position = position;



		}
		public override void _Ready()
		{

			Position = _Position;

			CollisionLayer = 1;
			CollisionMask = 2;


			Sprite2D sprite = new Sprite2D();
			sprite.Texture = GD.Load<Texture2D>(TexturePath);
			sprite.Modulate = Color;
			// sprite.Scale = Size / sprite.Texture.GetSize();
			AddChild(sprite);

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
			if (IsMultiplayerAuthority())
			{
				GetInput();
				Rotation += _rotationDirection * RotationSpeed * (float)delta;
				MoveAndSlide();
			}
		}

	}
}
