using Godot;

namespace main
{
    /// <summary>
    /// Heal Ability — exclusive to DefaultBody.
    /// Press E to boost HP to 150 for 5 seconds, then clamp back to 100.
    /// 10-second cooldown.
    /// </summary>
    public partial class HealAbility : Node, IBodyAbility
    {
        public string AbilityName => "Heal";
        public bool CanActivate => _canActivate;
        public float Cooldown => 10.0f;

        private bool _canActivate = true;
        private Timer _cooldownTimer;
        private Timer _durationTimer;

        // Emitted so Body can update its HP value & label
        [Signal]
        public delegate void HealStartedEventHandler(int newHp);

        [Signal]
        public delegate void HealEndedEventHandler(int clampedHp);

        public override void _Ready()
        {
            _cooldownTimer = new Timer();
            _cooldownTimer.WaitTime = Cooldown;
            _cooldownTimer.OneShot = true;
            _cooldownTimer.Timeout += OnCooldownExpired;
            AddChild(_cooldownTimer);

            _durationTimer = new Timer();
            _durationTimer.WaitTime = 5.0f;
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

            EmitSignal(SignalName.HealStarted, 150);
        }

        private void OnDurationExpired() => EmitSignal(SignalName.HealEnded, 100);

        private void OnCooldownExpired() => _canActivate = true;

        public override void _Process(double delta)
        {
            if (GetParent() is Body body && !body.IsMultiplayerAuthority()) return;

            if (Input.IsActionJustPressed("e"))
                Activate();
        }
    }
}