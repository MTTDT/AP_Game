using Godot;

namespace main
{
    /// <summary>
    /// Default Gun — balanced stats, identical to the original Gun.
    /// Unique ability (E): fires a single tank-shell-sized bullet.
    /// Stats: FireRate 0.5 s, BulletSpeed 500, BulletRange 400.
    /// </summary>
    public partial class DefaultGun : Gun
    {
        public DefaultGun(Color color)
            : base("res://gun.svg", color)
        {
            // Inherits the base defaults — nothing to override.
        }

        protected override void AddUniqueAbility()
        {
            AddChild(new BigBulletAbility());
        }
    }
}
