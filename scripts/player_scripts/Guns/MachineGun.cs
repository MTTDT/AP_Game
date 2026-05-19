using Godot;

namespace main
{
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
            base._Ready();
        }

        protected override void AddUniqueAbility()
        {
            var rapidBurst = new RapidFireAbility();
            AddChild(rapidBurst);
        }
    }
}