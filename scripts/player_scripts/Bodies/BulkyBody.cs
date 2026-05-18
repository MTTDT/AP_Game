using Godot;

namespace main
{
    /// <summary>
    /// Bulky Body — slow and tanky.
    /// Stats: Speed = 250, RotationSpeed = 2.5, starting HP = 150.
    /// Unique ability: Durability (E) — halves incoming damage while active.
    /// </summary>
    public partial class BulkyBody : Body
    {
        public BulkyBody() : base() { }

        public BulkyBody(Color color, Node owner, Vector2 position,
                         GunType gunType = GunType.Default)
            : base("res://body_bulky.svg", color, owner, new Vector2(80, 80), position, gunType)
        {
            Speed         = 250;
            RotationSpeed = 2.5f;
        }

        public override void _Ready()
        {
            _hp    = 150;
            _maxHp = 150;
            base._Ready();
        }

        protected override void AddUniqueAbility()
        {
            var durability = new DurabilityIncreaseAbility();
            durability.DurabilityActivated   += () => SetDamageReduction(0.5f);
            durability.DurabilityDeactivated += () => SetDamageReduction(1.0f);
            AddChild(durability);
        }
    }
}