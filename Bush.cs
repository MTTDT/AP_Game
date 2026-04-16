using Godot;

public partial class Bush : Area2D
{
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is main.Body player)
		{
			// Make player semi-transparent
			player.Modulate = new Color(1, 1, 1, 0.4f);
		}
	}

	private void OnBodyExited(Node2D body)
	{
		if (body is main.Body player)
		{
			// Restore visibility
			player.Modulate = Colors.White;
		}
	}
}
