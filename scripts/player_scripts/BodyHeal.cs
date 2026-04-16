using Godot;
using System;
namespace main
{
    public partial class BodyHeal : Node
    {
        private Timer _resetTimer;
        private bool canUse = true;

        public BodyHeal()
        {
        }

		[Signal]
		public delegate void HealBodyEventHandler();
        public override void _Ready()
        {
            _resetTimer = new Timer();
            _resetTimer.WaitTime = 15.0;
            _resetTimer.OneShot = true;
            _resetTimer.Timeout += OnResetTimer;
            AddChild(_resetTimer);
        }
        private void OnResetTimer()
        {
            canUse = true;
        }

        public void Heal()
        {
            if (!canUse) return;
            EmitSignal(SignalName.HealBody);
            canUse = false;
            _resetTimer.Start();
        }
                
        public override void _Process(double delta)
        {
            if (Input.IsActionJustPressed("e") && canUse)
            {
                Heal();
            }
        }
    }
}