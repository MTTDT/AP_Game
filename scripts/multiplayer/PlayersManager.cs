using Godot;

namespace main
{
	public partial class PlayersManager : Node
	{
		public PlayersRegister Players { get; } = new PlayersRegister();

		private readonly Color[] colors = new Color[19]
		{
			Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.Cyan,
			Colors.Magenta, Colors.Orange, Colors.Purple, Colors.Pink, Colors.Brown,
			Colors.Gray, Colors.Black, Colors.White, Colors.Gold, Colors.Silver,
			Colors.Maroon, Colors.Olive, Colors.Teal, Colors.Lime
		};

		public void StartServer()
		{
			GD.Print("Starting SERVER...");
			var peer = new ENetMultiplayerPeer();
			var err = peer.CreateServer(GameState.Port, maxClients: 4);
			if (err != Error.Ok) { GD.PrintErr("Failed to start server: " + err); return; }

			Multiplayer.MultiplayerPeer = peer;
			Multiplayer.PeerConnected += OnPeerConnected;
			Multiplayer.PeerDisconnected += OnPeerDisconnected;

			AddPlayer(1);
			GD.Print("Server started on port " + GameState.Port);
		}

		public void StartClient()
		{
			GD.Print("Starting CLIENT, connecting to: " + GameState.HostIP);
			var peer = new ENetMultiplayerPeer();
			var err = peer.CreateClient(GameState.HostIP, GameState.Port);
			if (err != Error.Ok) { GD.PrintErr("Client failed: " + err); return; }

			Multiplayer.MultiplayerPeer = peer;
			Multiplayer.ConnectedToServer += OnConnectedToServer;
			Multiplayer.ConnectionFailed += () => GD.PrintErr("Connection failed!");
		}

		public void SelectBodyType(BodyType type)
		{
			long myId = Multiplayer.GetUniqueId();

			if (Multiplayer.IsServer())
			{
				ServerApplyBodyType(myId, (int)type);
				Rpc(nameof(ReceiveBodyTypeChanged), myId, (int)type);
			}
			else
			{
				RpcId(1, nameof(ServerRequestBodyType), (int)type);
			}
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
		private void ServerRequestBodyType(int bodyTypeInt)
		{
			if (!Multiplayer.IsServer()) return;
			long senderId = Multiplayer.GetRemoteSenderId();
			ServerApplyBodyType(senderId, bodyTypeInt);
			Rpc(nameof(ReceiveBodyTypeChanged), senderId, bodyTypeInt);
		}

		private void ServerApplyBodyType(long peerId, int bodyTypeInt)
		{
			if (Players.Players.TryGetValue(peerId, out var player))
			{
				player.BodyType = (BodyType)bodyTypeInt;
				GD.Print($"Player {peerId} selected body: {player.BodyType}");
			}
		}

		[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
		private void ReceiveBodyTypeChanged(long peerId, int bodyTypeInt)
		{
			if (Players.Players.TryGetValue(peerId, out var player))
			{
				player.BodyType = (BodyType)bodyTypeInt;
				Players.NotifyChanged();
			}
		}

		public void StartGame()
		{
			if (!Multiplayer.IsServer()) return;
			GameState.GameActive = true;
			Rpc(nameof(ReceiveStartGame));
		}

		[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
		private void ReceiveStartGame()
		{
			GD.Print("Starting game!");
			GameState.GameActive = true;
			GetTree().ChangeSceneToFile("res://node_2d.tscn");
		}

		public void ReturnToPool()
		{
			GameState.GameActive = false;
			GetTree().ChangeSceneToFile("res://players_pool.tscn");
		}

		public void PlayerQuit()
		{
			long myId = Multiplayer.GetUniqueId();
			GD.Print($"Player {myId} quitting...");

			if (Multiplayer.IsServer())
			{
				Players.RemovePlayer(myId);
				CheckGameEnd();
			}
			else
			{
				RpcId(1, nameof(ServerNotifyQuit));
			}

			Multiplayer.MultiplayerPeer?.Close();
			Multiplayer.MultiplayerPeer = null;
			GameState.Role = GameState.NetworkRole.None;
			GameState.HostIP = "127.0.0.1";

			GetTree().ChangeSceneToFile("res://main_menu.tscn");
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
		private void ServerNotifyQuit()
		{
			long id = Multiplayer.GetRemoteSenderId();
			Players.RemovePlayer(id);
			GD.Print($"Player {id} quit. Remaining: {Players.Count()}");
			CheckGameEnd();
		}

		private void CheckGameEnd()
		{
			if (!Multiplayer.IsServer()) return;

			if (Players.Count() <= 1 && GameState.GameActive)
			{
				// ⭐ YA NO RESETEAMOS LA PARTIDA AQUÍ ⭐
				GD.Print("Only one player left — win/lose handled by game scene.");
			}
		}

		[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
		private void ReceiveReturnToPool()
		{
			GameState.GameActive = false;
			GetTree().ChangeSceneToFile("res://players_pool.tscn");
		}

		private void OnConnectedToServer()
		{
			GD.Print("Connected! My ID = " + Multiplayer.GetUniqueId());
			RpcId(1, nameof(ServerRequestSpawn));
		}

		private void OnPeerConnected(long id)
		{
			if (!Multiplayer.IsServer()) return;
			GD.Print("Peer connected: " + id);
			foreach (var existingId in Players.Players.Keys)
			{
				RpcId(id, nameof(SpawnPlayerOnClient), existingId);

				if (Players.Players.TryGetValue(existingId, out var p))
					RpcId(id, nameof(ReceiveBodyTypeChanged), existingId, (int)p.BodyType);
			}
		}

		private void OnPeerDisconnected(long id)
		{
			Players.RemovePlayer(id);
			CheckGameEnd();
		}

		private void AddPlayer(long peerId)
		{
			var player = new Player(peerId, $"Player{peerId}");
			Players.AddPlayer(player);
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
		private void ServerRequestSpawn()
		{
			long newId = Multiplayer.GetRemoteSenderId();

			if (GameState.GameActive)
			{
				GD.Print($"Rejecting player {newId} — game already active.");
				RpcId(newId, nameof(ReceiveJoinRejected));
				return;
			}

			GD.Print($"Server: registering new player {newId}");
			AddPlayer(newId);
			Rpc(nameof(SpawnPlayerOnClient), newId);
		}

		[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
		private void ReceiveJoinRejected()
		{
			GD.Print("Rejected: game already in progress.");
			Multiplayer.MultiplayerPeer?.Close();
			Multiplayer.MultiplayerPeer = null;
			GameState.Role = GameState.NetworkRole.None;
			GetTree().ChangeSceneToFile("res://main_menu.tscn");
		}

		[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
		private void SpawnPlayerOnClient(long peerId)
		{
			GD.Print($"Adding player {peerId} to local register");
			AddPlayer(peerId);
		}
	}
}
