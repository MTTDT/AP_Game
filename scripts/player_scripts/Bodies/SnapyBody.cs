using Godot;

namespace main
{
    /// <summary>
    /// Snappy Body — fast but fragile.
    /// Stats: Speed = 900, RotationSpeed = 8, starting HP = 70.
    /// Unique ability: Speed Boost (E) — doubles movement speed for 2 s.
    /// Cooldown: 8 s.
    /// </summary>
    public partial class SnapyBody : Body
    {
        private int _baseSpeed;

        public SnapyBody() : base() { }

        public SnapyBody(Color color, Node owner, Vector2 position)
            : base("res://body_snappy.svg", color, owner, new Vector2(45, 45), position)
        {
            Speed         = 900;
            RotationSpeed = 8f;
        }

        public override void _Ready()
        {
            _hp      = 70;
            _maxHp   = 70;
            _baseSpeed = Speed;
            base._Ready();
        }

        protected override void AddUniqueAbility()
        {
            var boost = new SpeedBoostAbility();
            boost.BoostStarted += () => Speed = _baseSpeed * 2;
            boost.BoostEnded   += () => Speed = _baseSpeed;
            AddChild(boost);
        }
    }
}