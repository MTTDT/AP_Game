using Godot;
using System;
using System.Linq;

namespace main
{
	public partial class PlayerControl : Node
	{
		Color[] colors = new Color[]
		{
			Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.Cyan,
			Colors.Magenta, Colors.Orange, Colors.Purple, Colors.Pink, Colors.Brown,
			Colors.Gray, Colors.Black, Colors.White, Colors.Gold, Colors.Silver,
			Colors.Maroon, Colors.Olive, Colors.Teal, Colors.Lime
		};

		private PlayersRegister _players = new PlayersRegister();

		public override void _Ready()
		{
			if (GameState.Role == GameState.NetworkRole.Server)
				StartServer();
			else
				StartClient();
		}

		private void StartServer()
		{
			GD.Print("Starting SERVER...");
			var peer = new ENetMultiplayerPeer();
			var err = peer.CreateServer(GameState.Port, 4);

			if (err != Error.Ok)
			{
				GD.PrintErr("Failed to start server: " + err);
				return;
			}

			Multiplayer.MultiplayerPeer = peer;
			Multiplayer.PeerConnected += OnPeerConnected;
			Multiplayer.PeerDisconnected += OnPeerDisconnected;

			SpawnPlayer(1);
		}

		private void StartClient()
		{
			GD.Print("Starting CLIENT...");
			var peer = new ENetMultiplayerPeer();
			var err = peer.CreateClient(GameState.HostIP, GameState.Port);

			if (err != Error.Ok)
			{
				GD.PrintErr("Client failed");
				return;
			}

			Multiplayer.MultiplayerPeer = peer;
			Multiplayer.ConnectedToServer += OnConnectedToServer;
		}

		private void OnConnectedToServer()
		{
			RpcId(1, nameof(ServerRequestSpawn));
		}

		private void OnPeerConnected(long id)
		{
			foreach (var existingId in _players.Players.Keys)
				RpcId(id, nameof(SpawnPlayerOnClient), existingId);
		}

		private void OnPeerDisconnected(long id)
		{
			_players.RemovePlayer(id);
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
		private void ServerRequestSpawn()
		{
			long newId = Multiplayer.GetRemoteSenderId();
			SpawnPlayer(newId);
			Rpc(nameof(SpawnPlayerOnClient), newId);
		}

		[Rpc(MultiplayerApi.RpcMode.Authority)]
		private void SpawnPlayerOnClient(long peerId)
		{
			SpawnPlayer(peerId);
		}

		private void SpawnPlayer(long peerId)
		{
			float xOffset = _players.Count() * 150f;

			var body = new Body(
				"res://body.svg",
				colors[peerId % colors.Length],
				new Vector2(64, 64),
				new Vector2(300f + xOffset, 400f)
			);

			body.Name = $"Player_{peerId}";
			body.SetMultiplayerAuthority((int)peerId);

			_players.AddPlayer(new Player(peerId, body, $"Player{peerId}"));

			GetTree().CurrentScene.AddChild(body);
		}

		public override void _Process(double delta)
		{
			if (Input.IsActionJustPressed("ui_accept"))
			{
				bool dummyExists = GetTree().CurrentScene.GetChildren()
					.OfType<Dummy>().Any();

				if (!dummyExists)
				{
					Dummy dummy = new Dummy("res://dummy.svg", new Vector2(600, 400));
					GetTree().CurrentScene.AddChild(dummy);
				}
			}
		}
	}
}
