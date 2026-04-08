using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace main
{


	//Applies to the main node/scene
	public partial class PlayerControl : Node
	{

        Color[] colors = new Color[19]
        {
            Colors.Red,
            Colors.Green,
            Colors.Blue,
            Colors.Yellow,
            Colors.Cyan,
            Colors.Magenta,
            Colors.Orange,
            Colors.Purple,
            Colors.Pink,
            Colors.Brown,
            Colors.Gray,
            Colors.Black,
            Colors.White,
            Colors.Gold,
            Colors.Silver,
            Colors.Maroon,
            Colors.Olive,
            Colors.Teal,
            Colors.Lime
        };
		private PlayersRegister _players = new PlayersRegister();
        
		public override void _Ready()
        {
            Sprite2D sprite = new Sprite2D();
            sprite.Texture = GD.Load<Texture2D>("res://icon.svg");
            sprite.Scale = new Vector2(10f, 10f);
            AddChild(sprite);



            if (GameState.Role == GameState.NetworkRole.Server)
                StartServer();
            else
                StartClient();

            // Label HostIP = GetNode<Label>("HostIP");
            // HostIP.Text = GameState.HostIP;



        }

	 	private void StartServer()
        {
            GD.Print("Starting SERVER...");
            var peer = new ENetMultiplayerPeer();
            var err = peer.CreateServer(GameState.Port, maxClients: 4);

            if (err != Error.Ok)
            {
                GD.PrintErr("Failed to start server: " + err);
                return;
            }

            Multiplayer.MultiplayerPeer = peer;
            Multiplayer.PeerConnected += OnPeerConnected;
            Multiplayer.PeerDisconnected += OnPeerDisconnected;

            SpawnPlayer(1);
            GD.Print("Server started on port " + GameState.Port);
        }

		 private void StartClient()
        {
            GD.Print("Starting CLIENT, connecting to: " + GameState.HostIP);
            var peer = new ENetMultiplayerPeer();
            var err = peer.CreateClient(GameState.HostIP, GameState.Port);

            if (err != Error.Ok)
            {
                GD.PrintErr("Client failed: " + err);
                return;
            }

            Multiplayer.MultiplayerPeer = peer;
            Multiplayer.ConnectedToServer += OnConnectedToServer;
            Multiplayer.ConnectionFailed += () => GD.PrintErr("Connection failed!");
        }

// ── EVENTS ────────────────────────────────────────────
        private void OnConnectedToServer()
        {
            GD.Print("Connected! My ID = " + Multiplayer.GetUniqueId());
            // Ask server to spawn us and notify others
            RpcId(1, nameof(ServerRequestSpawn));
        }

        private void OnPeerConnected(long id)
        {
            GD.Print("Peer connected: " + id);
            // Tell the newly joined peer about all existing players
            foreach (var existingId in _players.Players.Keys)
                RpcId(id, nameof(SpawnPlayerOnClient), existingId);
        }

        private void OnPeerDisconnected(long id)
        {
           
            _players.RemovePlayer(id);
        }
		 // Client → Server: "spawn me please"
        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
        private void ServerRequestSpawn()
        {
            long newId = Multiplayer.GetRemoteSenderId();
            SpawnPlayer(newId);                          // Spawn on server
            Rpc(nameof(SpawnPlayerOnClient), newId);     // Tell all clients
        }

        // Runs on all clients to create a remote player
        [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
        private void SpawnPlayerOnClient(long peerId)
        {
            SpawnPlayer(peerId);
        }

        private void SpawnPlayer(long peerId)
        {
            // Offset spawn positions so players don't overlap
            float xOffset = _players.Count() * 150f;
            var body = new Body(
                "res://body.svg",
                colors[peerId % colors.Length],
                this,
                new Vector2(64, 64),
                new Vector2(300f + xOffset, 400f)
            );
            body.Name = $"Player_{peerId}";
            body.SetMultiplayerAuthority((int)peerId); // Critical: assigns ownership
            Player player = new Player(peerId, body, $"Player{peerId}");
            _players.AddPlayer(player);
            AddChild(body);

            GD.Print($"Spawned player {peerId} (I am {Multiplayer.GetUniqueId()})");
        }

		//Constantly checks the state of dummy, if there is no dummy in chiuld nodes allows player to spawn a new one
		public override void _Process(double delta)
		{
			if (Input.IsActionJustPressed("quit"))
			{
				// Disconnect from multiplayer cleanly
				if (Multiplayer.MultiplayerPeer != null)
				{
					Multiplayer.MultiplayerPeer.Close();
					Multiplayer.MultiplayerPeer = null;
				}

				// Reset game state so menu starts fresh
				GameState.Role = GameState.NetworkRole.None;
				GameState.HostIP = "127.0.0.1";

				GetTree().ChangeSceneToFile("res://main_menu.tscn"); // your menu scene name
			}

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
