using Godot;
using System;

public partial class WinScreen : Control
{
	private void _on_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://players_pool.tscn");
	}
}
