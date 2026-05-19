using Godot;

namespace main
{
    public partial class HealAbility : Node, IBodyAbility
    {
        public string AbilityName => "Heal";
        public bool CanActivate => _canActivate;
        public float Cooldown => 10.0f;

        private bool _canActivate = true;
        private Timer _cooldownTimer;
        private Timer _durationTimer;

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

            CallDeferred(nameof(InitializeUI));
        }

        private void InitializeUI()
        {
            if (GetParent() is Body body && body.IsMultiplayerAuthority())
            {
                Vector2 offset = new Vector2(140f, 20f);
                CooldownUI.Create(this, _cooldownTimer, Cooldown, CooldownUI.ScreenCorner.BottomLeft, offset, "E");
            }
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