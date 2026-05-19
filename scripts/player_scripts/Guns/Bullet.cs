using Godot;

namespace main
{

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

            SetDeferred("collision_layer", 4);
            SetDeferred("collision_mask", PierceWalls ? 1 : (1 | 2 | 3));

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

            Velocity = Direction.Normalized() * Velocity.Length();
        }

        public override void _PhysicsProcess(double delta)
        {
            GlobalPosition += Velocity * (float)delta;

            if (GlobalPosition.DistanceTo(_startPosition) >= MaxDistance)
            {
                QueueFree();
            }
        }

        private void OnObstacleOrPlayerHit(Node2D body)
        {
            QueueFree();
        }

        private void OnAreaHit(Area2D area)
        {
        }
    }
}