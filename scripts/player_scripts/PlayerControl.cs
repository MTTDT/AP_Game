using Godot;
using System;

namespace main
{
	// Attached to node_2d.tscn root node
	public partial class PlayerControl : Node
	{
		private readonly Color[] colors = new Color[19]
		{
			Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.Cyan,
			Colors.Magenta, Colors.Orange, Colors.Purple, Colors.Pink, Colors.Brown,
			Colors.Gray, Colors.Black, Colors.White, Colors.Gold, Colors.Silver,
			Colors.Maroon, Colors.Olive, Colors.Teal, Colors.Lime
		};

		private PlayersManager _playersManager;

		public override void _Ready()
		{
			_playersManager = GetNode<PlayersManager>("/root/PlayersManager");

			// Spawn bodies for all players already registered in PlayersManager
			int index = 0;
			foreach (var player in _playersManager.Players.Players.Values)
			{
				SpawnBody(player.Id, index);
				index++;
			}

			// Listen for new players joining mid-game (shouldn't happen but safe)
			_playersManager.Players.OnPlayersChanged += OnPlayersChanged;
		}

		public override void _ExitTree()
		{
			if (_playersManager != null)
				_playersManager.Players.OnPlayersChanged -= OnPlayersChanged;
		}

		private void OnPlayersChanged()
		{
			// Could handle late-join body spawning here if needed
		}

		private void SpawnBody(long peerId, int spawnIndex)
		{
			float xOffset = spawnIndex * 150f;
			var body = new Body(
				"res://body.svg",
				colors[peerId % colors.Length],
				this,
				new Vector2(64, 64),
				new Vector2(300f + xOffset, 400f)
			);
			body.Name = $"Player_{peerId}";
			body.SetMultiplayerAuthority((int)peerId);
			body.BodyDestroyed += () => OnBodyDestroyed(peerId, body, spawnIndex);
			AddChild(body);

			GD.Print($"Spawned body for player {peerId} (I am {Multiplayer.GetUniqueId()})");
		}

		private void OnBodyDestroyed(long peerId, Body body, int spawnIndex)
		{
			body.QueueFree();
			SpawnBody(peerId, spawnIndex); // respawn in same slot
		}

		public override void _Process(double delta)
		{
			// Ctrl+C — quit game and return to menu
			if (Input.IsActionJustPressed("quit"))
			{
				_playersManager.Players.OnPlayersChanged -= OnPlayersChanged;
				_playersManager.PlayerQuit();
				return;
			}

			if (Input.IsActionJustPressed("ui_accept"))
			{
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
