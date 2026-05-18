using Godot;

namespace main
{
    /// <summary>
    /// Sniper Gun unique ability.
    /// Press E to fire a single bullet that passes through walls and bounds
    /// (collision mask excludes environment layers — only body hitboxes stop it).
    /// The shot travels the full map width so it never fades mid-arena.
    /// 10-second cooldown.
    /// </summary>
    public partial class PierceAbility : Node, IGunAbility
    {
        public string AbilityName => "Pierce";
        public bool CanActivate => _canActivate;
        public float Cooldown => 10.0f;

        private bool _canActivate = true;
        private Timer _cooldownTimer;

        public override void _Ready()
        {
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
            if (!_canActivate) return;
            if (GetParent() is not Gun gun) return;

            // Piercing bullet: travels across the whole arena (3000 px), normal size
            gun.ShootSpecial(
                speed: 700f,
                range: 3000f,
                scale: 3.0f,
                pierceWalls: true,
                damage: 50);

            _canActivate = false;
            _cooldownTimer.Start();
        }

        public override void _Process(double delta)
        {
            if (GetParent() is Gun gun && !gun.IsMultiplayerAuthority()) return;
            if (Input.IsActionJustPressed("right_click")) Activate();
        }
        public float GetTimeLeft()
        {
            if (_cooldownTimer == null || _cooldownTimer.IsStopped()) return 0f;
            return (float)_cooldownTimer.TimeLeft;
        }
    }
}
