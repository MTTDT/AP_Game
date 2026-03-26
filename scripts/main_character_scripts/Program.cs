using Godot;
using System;
namespace main
{
	public partial class Program : Node
	{
		public override void _Ready()
		{
			Sprite2D sprite = new Sprite2D();
			sprite.Texture = GD.Load<Texture2D>("res://icon.svg");
			sprite.Scale = new Vector2(10f, 10f);
			AddChild(sprite);

			Body body = new Body("res://body.svg", Colors.Red, this, new Vector2(64, 64), new Vector2(400f, 400f));
			AddChild(body);

			Dummy dummy = new Dummy("res://dummy.svg", new Vector2(600f, 400f));
			AddChild(dummy);

			

		}
		public override void _Process(double delta)
		{
			if (Input.IsActionJustPressed("ui_accept"))
			{
				// Check if any Dummy exists among children
				bool dummyExists = false;
				foreach (Node child in GetChildren())
				{
					if (child is Dummy)
					{
						dummyExists = true;
						break;
					}
				}

				if (!dummyExists)
				{
					Dummy dummy = new Dummy("res://dummy.svg", new Vector2(600f, 400f));
					AddChild(dummy);
				}
			}
		}
	
	}
}
