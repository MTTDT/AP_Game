using Godot;

namespace main
{
    public partial class SnapyBody : Body
    {
        private int _baseSpeed;

        public SnapyBody() : base() { }

        public SnapyBody(Color color, Node owner, Vector2 position,
                         GunType gunType = GunType.Default)
            : base("res://body_snappy.svg", color, owner, new Vector2(45, 45), position, gunType)
        {
            Speed = 500;
        }

        public override void _Ready()
        {
            _hp        = 70;
            _maxHp     = 70;
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