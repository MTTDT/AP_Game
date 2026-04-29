using Godot;

namespace main
{
    public partial class DefaultBody : Body
    {
        public DefaultBody() : base() { }

        public DefaultBody(Color color, Node owner, Vector2 position)
            : base("res://body_default.svg", color, owner, new Vector2(60, 60), position)
        {
            // Balanced defaults — Speed=600, RotationSpeed=5 inherited from Body
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