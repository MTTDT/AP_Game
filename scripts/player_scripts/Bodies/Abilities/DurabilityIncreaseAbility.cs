using Godot;

namespace main
{
    /// <summary>
    /// Durability Increase Ability — exclusive to BulkyBody.
    /// Press E to take half damage for 4 seconds.
    /// Cooldown: 12 s.
    /// </summary>
    public partial class DurabilityIncreaseAbility : Node, IBodyAbility
    {
        public string AbilityName => "Durability Increase";
        public bool CanActivate => _canActivate;
        public float Cooldown => 12.0f;

        public bool IsActive => _isActive;

        private bool _canActivate = true;
        private bool _isActive = false;
        private Timer _cooldownTimer;
        private Timer _durationTimer;

        [Signal]
        public delegate void DurabilityActivatedEventHandler();

        [Signal]
        public delegate void DurabilityDeactivatedEventHandler();

        public override void _Ready()
        {
            _cooldownTimer = new Timer();
            _cooldownTimer.WaitTime = Cooldown;
            _cooldownTimer.OneShot = true;
            _cooldownTimer.Timeout += OnCooldownExpired;
            AddChild(_cooldownTimer);

            _durationTimer = new Timer();
            _durationTimer.WaitTime = 4.0f;
            _durationTimer.OneShot = true;
            _durationTimer.Timeout += OnDurationExpired;
            AddChild(_durationTimer);
        }

        public void Activate()
        {
            if (!_canActivate) return;

            _isActive = true;
            _canActivate = false;
            _durationTimer.Start();
            _cooldownTimer.Start();

            EmitSignal(SignalName.DurabilityActivated);
        }

        private void OnDurationExpired()
        {
            _isActive = false;
            EmitSignal(SignalName.DurabilityDeactivated);
        }

        private void OnCooldownExpired() => _canActivate = true;

        public override void _Process(double delta)
        {
            if (GetParent() is Body body && !body.IsMultiplayerAuthority()) return;

            if (Input.IsActionJustPressed("e"))
                Activate();
        }
    }
}