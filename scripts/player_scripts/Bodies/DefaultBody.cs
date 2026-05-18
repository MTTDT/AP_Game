using Godot;

namespace main
{
    public partial class DefaultBody : Body
    {
        public DefaultBody() : base() { }

        public DefaultBody(Color color, Node owner, Vector2 position,
                           GunType gunType = GunType.Default)
            : base("res://body_default.svg", color, owner, new Vector2(60, 60), position, gunType)
        {
            // Balanced defaults — Speed=300, RotationSpeed=5 inherited from Body
        }

        protected override void AddUniqueAbility()
        {
            var heal = new HealAbility();
            heal.HealStarted += hp => SetHp(hp);
            heal.HealEnded   += hp => SetHp(hp);
            AddChild(heal);
        }
    }
}