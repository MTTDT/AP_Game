// using Godot;
// using System;
// namespace main
// {
// 	public partial class BodyDodge : Node
// 	{
// 		private Timer _resetTimer;
// 		private bool canDodge = true;

// 		public BodyDodge()
// 		{
// 		}

// 		public override void _Ready()
// 		{
// 			_resetTimer = new Timer();
// 			_resetTimer.WaitTime = 5.0;
// 			_resetTimer.OneShot = true;
// 			_resetTimer.Timeout += OnResetTimer;
// 			AddChild(_resetTimer);
// 		}
// 		private void OnResetTimer()
// 		{
// 			canDodge = true;
// 		}

// 		public void Dodge()
// 		{
// 			Node parent = this.GetParent();
// 			if (parent is Body body)
// 			{
// 				body.Position += body.Transform.Y * -300f;
// 			}
// 			canDodge = false;
// 			_resetTimer.Start();
// 		}
		
// 		public override void _Process(double delta)
// 		{
// 			// Example usage: Dodge in the direction of the mouse cursor when the player presses the spacebar
// 			if (Input.IsActionJustPressed("q") && canDodge)
// 			{
// 				Dodge();
// 			}
// 		}
// 	}
// }
