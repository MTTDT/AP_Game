using Godot;
using System;

namespace main
{
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

			int index = 0;
			foreach (var player in _playersManager.Players.Players.Values)
			{
				SpawnBody(player.Id, player.BodyType, player.GunType, index);
				index++;
			}

			_playersManager.Players.OnPlayersChanged += OnPlayersChanged;
		}

		public override void _ExitTree()
		{
			if (_playersManager != null)
				_playersManager.Players.OnPlayersChanged -= OnPlayersChanged;
		}

		private void OnPlayersChanged() { }

		// ───────────────────────────────────────────────────────────────
		// SPAWN
		// ───────────────────────────────────────────────────────────────

		private void SpawnBody(long peerId, BodyType bodyType, GunType gunType, int spawnIndex)
		{
			float xOffset = spawnIndex * 150f;
			Color color = colors[peerId % colors.Length];
			Vector2 position = new Vector2(300f + xOffset, 400f);

			Body body = bodyType switch
			{
				BodyType.Bulky => new BulkyBody(color, this, position, gunType),
				BodyType.Snappy => new SnapyBody(color, this, position, gunType),
				_ => new DefaultBody(color, this, position, gunType)
			};

			body.Name = $"Player_{peerId}";
			body.SetMultiplayerAuthority((int)peerId);

			body.BodyDestroyed += () =>
			{
				if (Multiplayer.GetUniqueId() == peerId)
					OnBodyDestroyed(peerId, body);
			};

			AddChild(body);

			GD.Print($"Spawned {bodyType} body + {gunType} gun for player {peerId} (I am {Multiplayer.GetUniqueId()})");
			
		}

		// ───────────────────────────────────────────────────────────────
		// DEATH & WIN (SERVER DECIDES)
		// ───────────────────────────────────────────────────────────────

		private void OnBodyDestroyed(long peerId, Body body)
		{
			Rpc(nameof(RpcRemoveBody), peerId);

			if (Multiplayer.IsServer())
				HandlePlayerEliminated(peerId);
			else
				RpcId(1, nameof(ServerNotifyEliminated), peerId);
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
		private void RpcRemoveBody(long peerId)
		{
			var body = GetNodeOrNull<Body>($"Player_{peerId}");
			if (body != null)
				body.QueueFree();
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
		private void ServerNotifyEliminated(long peerId)
		{
			HandlePlayerEliminated(peerId);
		}

		private void HandlePlayerEliminated(long peerId)
		{
			GD.Print($"Player {peerId} eliminated.");

			if (!Multiplayer.IsServer()) return;

			int aliveCount = 0;
			long lastAliveId = -1;

			foreach (Node child in GetChildren())
			{
				if (child is Body body)
				{
					if (body.Name == $"Player_{peerId}") continue;

					if (body.Name.ToString().StartsWith("Player_"))
					{
						string idStr = body.Name.ToString().Substring("Player_".Length);
						if (long.TryParse(idStr, out long id))
						{
							aliveCount++;
							lastAliveId = id;
						}
					}
				}
			}

			GD.Print($"Alive bodies after elimination: {aliveCount}");

			if (aliveCount > 1)
				_playersManager.NotifyOutcome(winnerId: -1, loserId: peerId);
			else if (aliveCount == 1)
			{
				GD.Print($"Player {lastAliveId} wins!");
				_playersManager.NotifyOutcome(winnerId: lastAliveId, loserId: peerId);
			}
			else
			{
				GD.Print("No players alive — no winner.");
				_playersManager.NotifyOutcome(winnerId: -1, loserId: peerId);
			}
		}

		// ───────────────────────────────────────────────────────────────
		// INPUT
		// ───────────────────────────────────────────────────────────────

		public override void _Process(double delta)
		{
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
					if (child is Dummy) { dummyExists = true; break; }
				}
				if (!dummyExists)
					AddChild(new Dummy("res://dummy.svg", new Vector2(600f, 400f)));
			}
		}
	}
}