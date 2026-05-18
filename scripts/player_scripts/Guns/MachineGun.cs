using Godot;

namespace main
{
    /// <summary>
    /// Machine Gun — rapid normal fire, shorter range than Default.
    /// Unique ability (E): "Overdrive" — doubles fire rate and range for 4 s.
    /// Stats: FireRate 0.15 s, BulletSpeed 500, BulletRange 300.
    /// </summary>
    public partial class MachineGun : Gun
    {
        private float _baseFireRate;
        private float _baseBulletRange;

        public MachineGun(Color color)
            : base("res://gun_machinegun.svg", color)
        {
            FireRate = 0.15f;
            BulletSpeed = 500f;
            BulletRange = 300f;
            BulletDamage = 10;
        }

        public override void _Ready()
        {
            // Capture base values before _Ready might be overridden
            _baseFireRate    = FireRate;
            _baseBulletRange = BulletRange;
            base._Ready();
        }

        protected override void AddUniqueAbility()
        {
            var overdrive = new RapidFireAbility();

            overdrive.OverdriveStarted += () =>
            {
                FireRate    = _baseFireRate    * 0.5f;   // twice as fast
                BulletRange = _baseBulletRange * 2.0f;   // twice the range
            };

            overdrive.OverdriveEnded += () =>
            {
                FireRate    = _baseFireRate;
                BulletRange = _baseBulletRange;
            };

            AddChild(overdrive);
        }
    }
}
