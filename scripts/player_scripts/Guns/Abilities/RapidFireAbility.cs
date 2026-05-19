using Godot;

namespace main
{
    /// <summary>
    /// Machine Gun unique ability.
    /// Press Right Click to fire a burst of 10 rapid bullets,
    /// then enters a 15-second cooldown.
    /// </summary>
    public partial class RapidFireAbility : Node, IGunAbility
    {
        public string AbilityName => "Overdrive";
        public bool CanActivate => _canActivate;
        public float Cooldown => 15.0f;

        private bool _canActivate = true;
        private Timer _burstTimer;
        private Timer _cooldownTimer;
        
        private int _bulletsLeftToShoot = 0;
        private const int TotalBurstShots = 10;
        private const float TimeBetweenBurstShots = 0.08f; 

        public override void _Ready()
        {
            _burstTimer = new Timer { WaitTime = TimeBetweenBurstShots, OneShot = false };
            _burstTimer.Timeout += FireBurstBullet;
            AddChild(_burstTimer);

            _cooldownTimer = new Timer { WaitTime = Cooldown, OneShot = true };
            _cooldownTimer.Timeout += () => _canActivate = true;
            AddChild(_cooldownTimer);

            CallDeferred(nameof(InitializeUI));
        }

        private void InitializeUI()
        {
            if (GetParent() is Gun gun && gun.IsMultiplayerAuthority())
            {
                Vector2 offset = new Vector2(20f, 20f);
                CooldownUI.Create(this, _cooldownTimer, Cooldown, CooldownUI.ScreenCorner.BottomRight, offset, "MR");
            }
        }

        public void Activate()
        {
            if (!_canActivate || _bulletsLeftToShoot > 0) return;

            _canActivate = false;
            _bulletsLeftToShoot = TotalBurstShots;
            
            FireBurstBullet();
            _burstTimer.Start();
        }

        private void FireBurstBullet()
        {
            if (GetParent() is Gun gun)
            {

                gun.ShootSpecial(
                    gun.Get("BulletSpeed").AsSingle(), 
                    gun.Get("BulletRange").AsSingle(), 
                    gun.Get("BulletScale").AsSingle(), 
                    gun.Get("PierceWalls").AsBool(), 
                    gun.Get("BulletDamage").AsInt32()
                );
            }

            _bulletsLeftToShoot--;

            if (_bulletsLeftToShoot <= 0)
            {
                _burstTimer.Stop();
                _cooldownTimer.Start();
            }
        }

        public override void _Process(double delta)
        {
            if (GetParent() is Gun gun && !gun.IsMultiplayerAuthority()) return;
            
            if (Input.IsActionJustPressed("right_click")) Activate();
        }
        
        public float GetTimeLeft()
        {
            if (_bulletsLeftToShoot > 0)
            {
                return _bulletsLeftToShoot * TimeBetweenBurstShots;
            }
            if (_cooldownTimer != null && !_cooldownTimer.IsStopped())
            {
                return (float)_cooldownTimer.TimeLeft;
            }
            return 0f;
        }
    }
}