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

		private float _speed = 350f;
		private float _rotationSpeed = 3.5f;

		private Gun _gun;
		private Camera2D _camera;
		private Label _healthLabel;

		public int Health { get; private set; } = 100;

		private Rect2 MapBounds;

		public Body() {}

		public Body(string texturePath, Color color, Vector2 size, Vector2 startPos)
		{
			_texturePath = texturePath;
			_color = color;
			_size = size;
			_startPos = startPos;
		}

		public override void _Ready()
		{
			SetDeferred("collision_layer", 1);
			SetDeferred("collision_mask", 2 | 3 | 5);

			Position = _startPos;

			// SPRITE
			Sprite2D sprite = new Sprite2D();
			sprite.Texture = GD.Load<Texture2D>(_texturePath);
			sprite.Modulate = _color;
			sprite.Centered = true;
			AddChild(sprite);

			// COLLISION
			CollisionShape2D shape = new CollisionShape2D();
			RectangleShape2D rect = new RectangleShape2D();
			rect.Size = _size;
			shape.Shape = rect;
			AddChild(shape);

			// MAP BOUNDS
			Sprite2D map = GetTree().CurrentScene.GetNode<Sprite2D>("Sprite2D");

			Vector2 mapPos = map.GlobalPosition;
			Vector2 mapSize = map.Texture.GetSize() * map.Scale;

			MapBounds = new Rect2(mapPos - mapSize / 2f, mapSize);

			// CAMERA
			_camera = new Camera2D();
			_camera.Enabled = IsMultiplayerAuthority();

			_camera.LimitLeft = (int)MapBounds.Position.X;
			_camera.LimitTop = (int)MapBounds.Position.Y;
			_camera.LimitRight = (int)(MapBounds.Position.X + MapBounds.Size.X);
			_camera.LimitBottom = (int)(MapBounds.Position.Y + MapBounds.Size.Y);

			AddChild(_camera);

			// GUN
			_gun = new Gun("res://gun.svg", _color);
			AddChild(_gun);

			// HEALTH LABEL
			_healthLabel = new Label();
			_healthLabel.Text = Health.ToString();
			_healthLabel.Position = new Vector2(-20, -50);
			_healthLabel.Modulate = Colors.White;
			AddChild(_healthLabel);
		}

		public override void _PhysicsProcess(double delta)
		{
			if (!IsMultiplayerAuthority())
				return;

			Vector2 input = Vector2.Zero;

			if (Input.IsActionPressed("ui_up"))
				input.Y -= 1;
			if (Input.IsActionPressed("ui_down"))
				input.Y += 1;

			if (Input.IsActionPressed("ui_left"))
				Rotation -= _rotationSpeed * (float)delta;
			if (Input.IsActionPressed("ui_right"))
				Rotation += _rotationSpeed * (float)delta;

			Vector2 forward = -Transform.Y;
			Velocity = forward * (-input.Y) * _speed;

			MoveAndSlide();

			// LIMITS
			GlobalPosition = new Vector2(
				Mathf.Clamp(GlobalPosition.X, MapBounds.Position.X, MapBounds.Position.X + MapBounds.Size.X),
				Mathf.Clamp(GlobalPosition.Y, MapBounds.Position.Y, MapBounds.Position.Y + MapBounds.Size.Y)
			);

			// DEBUG
			if (Input.IsActionJustPressed("ui_accept"))
				GD.Print("PLAYER MASK RUNTIME: ", CollisionMask);
		}

		public void TakeDamage(int amount)
		{
			Health -= amount;
			_healthLabel.Text = Health.ToString();

			if (Health <= 0)
			{
				EmitSignal(nameof(BodyDestroyed));
				QueueFree();
			}
		}
	}
}
