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

            CollisionLayer = 4;
            CollisionMask = 2;

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
            float traveled = GlobalPosition.DistanceTo(startPosition);
            if (traveled >= MaxDistance) QueueFree();
        }
    }
}