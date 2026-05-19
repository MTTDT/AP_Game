using Godot;

namespace main
{
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
