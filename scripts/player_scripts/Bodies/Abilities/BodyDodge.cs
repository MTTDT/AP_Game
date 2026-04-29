using Godot;

namespace main
{
    /// <summary>
    /// Dodge ability — shared by ALL body types.
    /// Press Q to dash backward along the body's facing direction.
    /// 5-second cooldown.
    /// </summary>
    public partial class BodyDodge : Node, IBodyAbility
    {
        public string AbilityName => "Dodge";
        public bool CanActivate => _canActivate;
        public float Cooldown => 5.0f;

        private bool _canActivate = true;
        private Timer _cooldownTimer;

        public override void _Ready()
        {
            _cooldownTimer = new Timer();
            _cooldownTimer.WaitTime = Cooldown;
            _cooldownTimer.OneShot = true;
            _cooldownTimer.Timeout += OnCooldownExpired;
            AddChild(_cooldownTimer);
        }

        public void Activate()
        {
            if (!_canActivate) return;

            if (GetParent() is Body body)
            {
                // Dash backward (negative Y = forward in Godot's default orientation)
                body.Position += body.Transform.Y * -300f;
            }

            _canActivate = false;
            _cooldownTimer.Start();
        }

        private void OnCooldownExpired() => _canActivate = true;

        public override void _Process(double delta)
        {
            // Only the authority (local player) triggers input
            if (GetParent() is Body body && !body.IsMultiplayerAuthority()) return;

            if (Input.IsActionJustPressed("q"))
                Activate();
        }
    }
}