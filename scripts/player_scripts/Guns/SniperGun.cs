using Godot;

namespace main
{
    /// <summary>
    /// Sniper Gun — long range, slower fire rate, high bullet speed.
    /// Unique ability (E): fires a wall-piercing bullet that travels the
    ///   full arena width ignoring bounds and wall collision layers.
    /// Stats: FireRate 1.2 s, BulletSpeed 800, BulletRange 1200.
    /// </summary>
    public partial class SniperGun : Gun
    {
        public SniperGun(Color color)
            : base("res://gun_sniper.svg", color)
        {
            FireRate = 2f;
            BulletSpeed = 800f;
            BulletRange = 800f;
            BulletDamage = 25;
        }

        protected override void AddUniqueAbility()
        {
            AddChild(new PierceAbility());
        }
    }
}
