using Godot;
using System;
namespace main
{
	public partial class WinScreen : Control
	{
		private void _on_button_pressed()
		{
			var manager = GetNode<PlayersManager>("/root/PlayersManager");
			if (Multiplayer.IsServer())
				manager.ReturnToPool();          // broadcasts to all + resets GameActive
			else
				GetTree().ChangeSceneToFile("res://players_pool.tscn");
		}
	}
}


