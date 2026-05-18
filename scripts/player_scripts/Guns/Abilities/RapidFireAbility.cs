using Godot;

namespace main
{
    /// <summary>
    /// Machine Gun unique ability.
    /// Press E to enter "Overdrive" for 4 seconds:
    ///   • fire rate halved  (shoots twice as fast)
    ///   • bullet range doubled
    /// 15-second cooldown (starts when Overdrive ends).
    /// </summary>
    public partial class RapidFireAbility : Node, IGunAbility
    {
        public string AbilityName => "Overdrive";
        public bool CanActivate => _canActivate;
        public float Cooldown => 15.0f;

        private bool _canActivate = true;
        private Timer _durationTimer;
        private Timer _cooldownTimer;

        // Emitted so MachineGun can update its live FireRate / BulletRange
        [Signal] public delegate void OverdriveStartedEventHandler();
        [Signal] public delegate void OverdriveEndedEventHandler();

        public override void _Ready()
        {
            _durationTimer = new Timer { WaitTime = 4.0f, OneShot = true };
            _durationTimer.Timeout += OnDurationExpired;
            AddChild(_durationTimer);

            _cooldownTimer = new Timer { WaitTime = Cooldown, OneShot = true };
            _cooldownTimer.Timeout += () => _canActivate = true;
            AddChild(_cooldownTimer);
        }

        public void Activate()
        {
            if (!_canActivate) return;

            _canActivate = false;
            _durationTimer.Start();
            EmitSignal(SignalName.OverdriveStarted);
        }

        private void OnDurationExpired()
        {
            _cooldownTimer.Start();
            EmitSignal(SignalName.OverdriveEnded);
        }

        public override void _Process(double delta)
        {
            if (GetParent() is Gun gun && !gun.IsMultiplayerAuthority()) return;
            if (Input.IsActionJustPressed("right_click")) Activate();
        }
        
        public float GetTimeLeft()
        {
            // If we are currently in the 4-second active buff period
            if (_durationTimer != null && !_durationTimer.IsStopped())
            {
                return (float)_durationTimer.TimeLeft;
            }
            // If we are in the 15-second cooldown period
            if (_cooldownTimer != null && !_cooldownTimer.IsStopped())
            {
                return (float)_cooldownTimer.TimeLeft;
            }
            return 0f;
        }
    }
}
