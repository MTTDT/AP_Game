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
				SpawnBody(player.Id, player.BodyType, index);
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

		private void SpawnBody(long peerId, BodyType bodyType, int spawnIndex)
		{
			float xOffset = spawnIndex * 150f;
			Color color = colors[peerId % colors.Length];
			Vector2 position = new Vector2(300f + xOffset, 400f);

			Body body = bodyType switch
			{
				BodyType.Bulky  => new BulkyBody(color, this, position),
				BodyType.Snappy => new SnapyBody(color, this, position),
				_               => new DefaultBody(color, this, position)
			};

			body.Name = $"Player_{peerId}";
			body.SetMultiplayerAuthority((int)peerId);

			// Solo el jugador que realmente muere ejecuta OnBodyDestroyed
			body.BodyDestroyed += () =>
			{
				if (Multiplayer.GetUniqueId() == peerId)
					OnBodyDestroyed(peerId, body);
			};

			AddChild(body);

			GD.Print($"Spawned {bodyType} body for player {peerId} (I am {Multiplayer.GetUniqueId()})");
		}

		// ───────────────────────────────────────────────────────────────
		// DEATH & WIN (SERVER DECIDES)
		// ───────────────────────────────────────────────────────────────

		private void OnBodyDestroyed(long peerId, Body body)
		{
			// Quitamos el cuerpo localmente
			body.QueueFree();

			// Quitamos el cuerpo en TODOS los peers
			Rpc(nameof(RpcRemoveBody), peerId);

			// Avisamos al servidor
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

		// ⭐ LÓGICA DE ELIMINACIÓN BASADA EN CUERPOS VIVOS ⭐
		private void HandlePlayerEliminated(long peerId)
		{
			GD.Print($"Player {peerId} eliminated.");

			if (!Multiplayer.IsServer())
				return;

			// 1) Mandar GAME OVER al que murió
			RpcId((int)peerId, nameof(RpcShowGameOver));

			// 2) Contar cuántos cuerpos siguen vivos en esta escena
			int aliveCount = 0;
			long lastAliveId = -1;

			foreach (Node child in GetChildren())
			{
				if (child is Body body)
				{
					// Ignoramos el cuerpo del que acaba de morir
					if (body.Name == $"Player_{peerId}")
						continue;

					// Intentamos sacar el peerId del nombre "Player_X"
					string name = body.Name;
					if (name.StartsWith("Player_"))
					{
						string idStr = name.Substring("Player_".Length);
						if (long.TryParse(idStr, out long id))
						{
							aliveCount++;
							lastAliveId = id;
						}
					}
				}
			}

			GD.Print($"Alive bodies after elimination: {aliveCount}");

			// 3) Si quedan MÁS de 1 vivo → la partida sigue
			if (aliveCount > 1)
			{
				GD.Print("More than one player alive — match continues.");
				return;
			}

			// 4) Si no queda nadie vivo → raro, pero no hacemos nada
			if (aliveCount == 0)
			{
				GD.Print("No players alive — no winner.");
				return;
			}

			// 5) Si queda SOLO 1 vivo → ese gana
			GD.Print($"Player {lastAliveId} wins!");
			RpcId((int)lastAliveId, nameof(RpcShowWinScreen));
		}

		// ───────────────────────────────────────────────────────────────
		// SCENE CHANGES (CLIENT)
		// ───────────────────────────────────────────────────────────────

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
		private void RpcShowGameOver()
		{
			GD.Print("Showing GAME OVER on peer " + Multiplayer.GetUniqueId());
			GetTree().ChangeSceneToFile("res://game_over.tscn");
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
		private void RpcShowWinScreen()
		{
			GD.Print("Showing WIN SCREEN on peer " + Multiplayer.GetUniqueId());
			GetTree().ChangeSceneToFile("res://win_screen.tscn");
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
