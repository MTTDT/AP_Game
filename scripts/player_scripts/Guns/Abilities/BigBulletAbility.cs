using Godot;

namespace main
{
    public partial class BigBulletAbility : Node, IGunAbility
    {
        public string AbilityName => "Big Bullet";
        public bool CanActivate => _canActivate;
        public float Cooldown => 12.0f;

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

            gun.ShootSpecial(
                speed: 500f,
                range: 400f,
                scale: 3.0f,
                pierceWalls: false,
                damage: 30);

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
            if (_cooldownTimer == null || _cooldownTimer.IsStopped())
            {
                return 0f;
            }
            return (float)_cooldownTimer.TimeLeft;
        }
    }
}
