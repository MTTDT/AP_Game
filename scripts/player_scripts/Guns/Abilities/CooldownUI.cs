using Godot;

namespace main
{
    public partial class CooldownUI : CanvasLayer
    {
        private TextureProgressBar _progressBar;
        private TextureRect _bulletIcon;
        private IGunAbility _activeAbility;

        public override void _Ready()
        {
            _progressBar = FindChild("CooldownProgress") as TextureProgressBar;
            _bulletIcon = FindChild("BulletIcon") as TextureRect;

            if (_progressBar != null)
            {
                _progressBar.MinValue = 0;
                _progressBar.MaxValue = 100;
                _progressBar.Value = 0;
            }
            
            // Hide by default until an ability is assigned
            Visible = false;
        }

        // CALL THIS FROM YOUR SPAWN SCRIPT
        public void SetTrackedAbility(IGunAbility ability)
        {
            _activeAbility = ability;
            Visible = (_activeAbility != null);
        }

        public override void _Process(double delta)
        {
            if (_activeAbility == null) return;

            if (_activeAbility.CanActivate)
            {
                _progressBar.Value = 0;
                _bulletIcon.Modulate = Colors.White;
            }
            else
            {
                float timeLeft = _activeAbility.GetTimeLeft();
                float totalCooldown = _activeAbility.Cooldown;

                if (totalCooldown > 0)
                {
                    _progressBar.Value = (timeLeft / totalCooldown) * 100f;
                }
                _bulletIcon.Modulate = new Color(0.3f, 0.3f, 0.3f, 0.6f);
            }
        }
    }
}