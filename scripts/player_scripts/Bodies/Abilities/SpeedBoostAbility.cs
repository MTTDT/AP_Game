using Godot;

namespace main
{
    /// <summary>
    /// Speed Boost Ability — exclusive to SnapyBody.
    /// Press E to double movement speed for 2 seconds.
    /// 8-second cooldown.
    /// </summary>
    public partial class SpeedBoostAbility : Node, IBodyAbility
    {
        public string AbilityName => "Speed Boost";
        public bool CanActivate => _canActivate;
        public float Cooldown => 8.0f;

        private bool _canActivate = true;
        private Timer _cooldownTimer;
        private Timer _durationTimer;

        [Signal]
        public delegate void BoostStartedEventHandler();

        [Signal]
        public delegate void BoostEndedEventHandler();

        public override void _Ready()
        {
            _cooldownTimer = new Timer();
            _cooldownTimer.WaitTime = Cooldown;
            _cooldownTimer.OneShot = true;
            _cooldownTimer.Timeout += OnCooldownExpired;
            AddChild(_cooldownTimer);

            _durationTimer = new Timer();
            _durationTimer.WaitTime = 2.0f;
            _durationTimer.OneShot = true;
            _durationTimer.Timeout += OnDurationExpired;
            AddChild(_durationTimer);
        }

        public void Activate()
        {
            if (!_canActivate) return;

            _canActivate = false;
            _durationTimer.Start();
            _cooldownTimer.Start();

            EmitSignal(SignalName.BoostStarted);
        }

        private void OnDurationExpired() => EmitSignal(SignalName.BoostEnded);

        private void OnCooldownExpired() => _canActivate = true;

        public override void _Process(double delta)
        {
            if (GetParent() is Body body && !body.IsMultiplayerAuthority()) return;

            if (Input.IsActionJustPressed("e"))
                Activate();
        }
    }
}