using Godot;

namespace main
{
    /// <summary>
    /// A manual Area2D projectile (non-RigidBody).
    /// Tracks manual movement via _PhysicsProcess and uses Area2D detection
    /// to hit players or walls.
    /// </summary>
    public partial class Bullet : Area2D
    {
        private Vector2 Velocity     { get; set; }
        private Vector2 Direction    { get; set; }
        private string  TexturePath  { get; set; }
        private float   MaxDistance  { get; set; }
        private float   Scale        { get; set; }
        private bool    PierceWalls  { get; set; }
        public int      Damage       { get; set; }

        private Vector2 _startPosition;

        // Original constructor — keeps existing call-sites working
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
            // Normal bullets scan bodies (2) and walls (1|3).
            // Piercing bullets skip layers 1 and 3 — only bodies (layer 2) stop them.
            SetDeferred("collision_layer", 4);
            SetDeferred("collision_mask", PierceWalls ? 1 : (1 | 2 | 3));

            // Wire up collision events for non-physics detection
            BodyEntered += OnObstacleOrPlayerHit;
            AreaEntered += OnAreaHit; 

            var sprite = new Sprite2D
            {
                Texture = GD.Load<Texture2D>(TexturePath),
                Scale = Vector2.One * Scale,
                RotationDegrees = 90f
                
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

            // Calculate exact movement vector per second
            Velocity = Direction.Normalized() * Velocity.Length();
        }

        public override void _PhysicsProcess(double delta)
        {
            // Manually move the bullet forward without rigid body physics
            GlobalPosition += Velocity * (float)delta;

            // Distance check to despawn
            if (GlobalPosition.DistanceTo(_startPosition) >= MaxDistance)
            {
                QueueFree();
            }
        }

        // Triggered when hitting CharacterBody2D, TileMaps, StaticBodies, etc.
        private void OnObstacleOrPlayerHit(Node2D body)
        {
            // If it hits a Wall/Boundary (Layer 1 or 3) or a Player Body (Layer 2)
            // Note: Damage to players is safely handled by the Player's Hitbox Area2D.
            QueueFree();
        }

        // Triggered if it hits another Area2D
        private void OnAreaHit(Area2D area)
        {
            // Optional: If you want bullets to destroy themselves when hitting 
            // other specific hitboxes/areas, handle it here.
        }
    }
}