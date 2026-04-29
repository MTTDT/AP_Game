using Godot;

namespace main
{
    /// <summary>
    /// Bulky Body — slow and tanky.
    /// Stats: Speed = 350, RotationSpeed = 2.5, starting HP = 150.
    /// Unique ability: Shield (E) — 3 s invincibility (hitbox disabled).
    /// Cooldown: 12 s.
    /// </summary>
    public partial class BulkyBody : Body
    {
        public BulkyBody() : base() { }

        public BulkyBody(Color color, Node owner, Vector2 position)
            : base("res://body_bulky.svg", color, owner, new Vector2(80, 80), position)
        {
            Speed         = 350;
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