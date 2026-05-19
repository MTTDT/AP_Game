using Godot;

namespace main
{
    public partial class DefaultGun : Gun
    {
        public DefaultGun(Color color)
            : base("res://gun.svg", color)
        {
        }

        protected override void AddUniqueAbility()
        {
            AddChild(new BigBulletAbility());
        }
    }
}
