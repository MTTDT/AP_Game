using Godot;
using System;

namespace main
{
	public partial class Bullet : Area2D
	{
		private Vector2 _direction;
		private float _speed;

		public Bullet(Vector2 direction, string texturePath, float speed)
		{
			_direction = direction;
			_speed = speed;

			Sprite2D sprite = new Sprite2D();
			sprite.Texture = GD.Load<Texture2D>(texturePath);
			AddChild(sprite);

			CollisionLayer = 4; // balas
			CollisionMask = 3;  // dummy

			CollisionShape2D col = new CollisionShape2D();
			col.Shape = new CircleShape2D { Radius = 5 };
			AddChild(col);
		}

		public override void _PhysicsProcess(double delta)
		{
			Position += _direction * _speed * (float)delta;
		}
	}
}
