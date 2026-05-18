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
        private const float TimeBetweenBurstShots = 0.08f; // Interval between rapid shots (seconds)

        public override void _Ready()
        {
            // Timer responsible for the delay between each bullet in the burst
            _burstTimer = new Timer { WaitTime = TimeBetweenBurstShots, OneShot = false };
            _burstTimer.Timeout += FireBurstBullet;
            AddChild(_burstTimer);

            // Cooldown timer
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
            
            // Start firing immediately and kick off the interval timer
            FireBurstBullet();
            _burstTimer.Start();
        }

        private void FireBurstBullet()
        {
            if (GetParent() is Gun gun)
            {
                // Call Gun's ShootSpecial to bypass the standard weapon fire rate delay.
                // It uses the gun's standard stats, but ignores the gun's internal _canShoot flag.
                // We double the speed slightly or customize parameters here if you want!
                gun.ShootSpecial(
                    gun.Get("BulletSpeed").AsSingle(), 
                    gun.Get("BulletRange").AsSingle(), 
                    gun.Get("BulletScale").AsSingle(), 
                    gun.Get("PierceWalls").AsBool(), 
                    gun.Get("BulletDamage").AsInt32()
                );
            }

            _bulletsLeftToShoot--;

            // Once 10 bullets have been fired, stop the burst and start the cooldown
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
            // If we are currently firing the rapid burst
            if (_bulletsLeftToShoot > 0)
            {
                return _bulletsLeftToShoot * TimeBetweenBurstShots;
            }
            // If we are waiting out the 15-second cooldown
            if (_cooldownTimer != null && !_cooldownTimer.IsStopped())
            {
                return (float)_cooldownTimer.TimeLeft;
            }
            return 0f;
        }
    }
}