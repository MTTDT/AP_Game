using Godot;

namespace main
{
    /// <summary>
    /// A RigidBody2D projectile.
    ///
    /// New parameters vs. original:
    ///   scale      – uniform scale applied to the sprite and collision shape.
    ///                1.0 = normal, 3.0 = giant tank shell.
    ///   pierceWalls – when true the bullet's collision mask excludes the
    ///                 environment layer (layer 1 = bodies, layer 3 = walls/bounds)
    ///                 so it passes straight through them.
    /// </summary>
    public partial class Bullet : RigidBody2D
    {
        private Vector2 Velocity     { get; set; }
        private Vector2 Direction    { get; set; }
        private string  TexturePath  { get; set; }
        private float   MaxDistance  { get; set; }
        private float   Scale        { get; set; }
        private bool    PierceWalls  { get; set; }
        public int     Damage       { get; set; }

        private Vector2 _startPosition;

        // Original constructor — keeps existing call-sites working (scale=1, no pierce)
        public Bullet(Vector2 velocity, Vector2 direction, string texturePath, float maxDistance)
            : this(velocity, direction, texturePath, maxDistance, 1.0f, false) { }

        public Bullet(Vector2 velocity, Vector2 direction, string texturePath,
                      float maxDistance, float scale, bool pierceWalls, int damage = 10)
        {
            Velocity = velocity;
            Direction = direction;
            TexturePath = texturePath;
            MaxDistance = maxDistance;
            Scale = scale;
            PierceWalls = pierceWalls;
            Damage = damage;
        }

        public override void _Ready()
        {
            _startPosition = GlobalPosition;

            // Layer 4 = bullets.
            // Normal bullets hit everything (bodies on 2, walls on 1|3).
            // Piercing bullets skip layers 1 and 3 — only bodies (layer 2) stop them.
            SetDeferred("collision_layer", 4);
            SetDeferred("collision_mask", PierceWalls ? 2 : (1 | 2 | 3));

            var sprite = new Sprite2D
            {
                Texture = GD.Load<Texture2D>(TexturePath),
                Scale   = Vector2.One * Scale
            };
            AddChild(sprite);

            var col   = new CollisionShape2D();
            var shape = new CapsuleShape2D
            {
                Radius = 3f * Scale,
                Height = 12f * Scale
            };
            col.Shape = shape;
            AddChild(col);

            GravityScale    = 0f;
            LinearVelocity  = Direction.Normalized() * Velocity.Length();
        }

        public override void _Process(double delta)
        {
            if (GlobalPosition.DistanceTo(_startPosition) >= MaxDistance)
                QueueFree();
        }
    }
}
