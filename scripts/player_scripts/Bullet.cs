using Godot;
using System;

namespace main
{
	public partial class Bullet : RigidBody2D
	{
		private Vector2 Velocity { get; set; }
		private Vector2 Direction { get; set; }
		private string TexturePath { get; set; }
		private float MaxDistance { get; set; }
		private Vector2 startPosition;

		private float Lifetime = 3f;

		// ⭐ NECESARIO PARA GODOT
		public Bullet() {}

		public Bullet(Vector2 velocity, Vector2 direction, string texturePath, float maxDistance)
		{
			Velocity = velocity;
			Direction = direction;
			TexturePath = texturePath;
			MaxDistance = maxDistance;
		}

		public override void _Ready()
		{
			startPosition = GlobalPosition;

			// ⭐ BALAS = LAYER 4
			// Detectan: Player (1), Obstacles (2), Enemies (3)
			SetDeferred("collision_layer", 4);
			SetDeferred("collision_mask", 1 | 2 | 3);

			ContactMonitor = true;
			MaxContactsReported = 4;
			ContinuousCd = RigidBody2D.CcdMode.CastShape;

			Sprite2D sprite = new Sprite2D();
			sprite.Texture = GD.Load<Texture2D>(TexturePath);
			AddChild(sprite);

			CollisionShape2D col = new CollisionShape2D();
			CapsuleShape2D shape = new CapsuleShape2D();
			shape.Radius = 3f;
			shape.Height = 12f;
			col.Shape = shape;
			AddChild(col);

			GravityScale = 0f;

			LinearVelocity = Direction.Normalized() * Velocity.Length();
		}

		public override void _Process(double delta)
		{
			Lifetime -= (float)delta;
			if (Lifetime <= 0f)
			{
				QueueFree();
				return;
			}

			float traveled = GlobalPosition.DistanceTo(startPosition);
			if (traveled >= MaxDistance)
				QueueFree();
		}

		public override void _IntegrateForces(PhysicsDirectBodyState2D state)
		{
			int count = state.GetContactCount();

			for (int i = 0; i < count; i++)
			{
				GodotObject colliderObj = state.GetContactColliderObject(i);
				if (colliderObj == null)
					continue;

				Node collider = colliderObj as Node;
				if (collider == null)
					continue;

				if (collider is Body body)
				{
					body.TakeDamage(20);
					QueueFree();
				}

				if (collider is Dummy dummy)
				{
					dummy.TakeDamage(20);
					QueueFree();
				}

				if (collider is StaticBody2D)
				{
					QueueFree();
				}
			}
		}
	}
}
