using Godot;

namespace main
{
    /// <summary>
    /// Machine Gun — rapid normal fire, shorter range than Default.
    /// Unique ability (Right Click): fires an intense burst of 10 rapid bullets.
    /// Stats: FireRate 0.15 s, BulletSpeed 500, BulletRange 300.
    /// </summary>
    public partial class MachineGun : Gun
    {
        public MachineGun(Color color)
            : base("res://gun_machinegun.svg", color)
        {
            FireRate     = 0.15f;
            BulletSpeed  = 500f;
            BulletRange  = 300f;
            BulletDamage = 10;
        }

        public override void _Ready()
        {
            // Simply call the base Gun initialization
            base._Ready();
        }

        protected override void AddUniqueAbility()
        {
            // Instantiate the updated burst ability and attach it
            var rapidBurst = new RapidFireAbility();
            AddChild(rapidBurst);
        }
    }
}