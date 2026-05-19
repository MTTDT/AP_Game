using Godot;
using System;

namespace main
{
	public partial class GameOver : Control
	{

		private void _on_button_pressed()
		{
			var manager = GetNode<PlayersManager>("/root/PlayersManager");
			if (Multiplayer.IsServer())
				manager.ReturnToPool();
			else
				GetTree().ChangeSceneToFile("res://players_pool.tscn");
		}


	}
}