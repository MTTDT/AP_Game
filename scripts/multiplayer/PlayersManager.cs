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

        // Called by PlayersPool's Start Game button (server only)
        public void StartGame()
        {
            if (!Multiplayer.IsServer()) return;
            GameState.GameActive = true;
            Rpc(nameof(ReceiveStartGame)); // tell all clients including self
        }

        // Called on every peer (server via CallLocal equivalent using Rpc, clients via RPC)
        [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
        private void ReceiveStartGame()
        {
            GD.Print("Starting game!");
            GameState.GameActive = true;
            GetTree().ChangeSceneToFile("res://node_2d.tscn");
        }

        // Called by PlayerControl when a player quits (Ctrl+C)
        public void PlayerQuit()
        {
            long myId = Multiplayer.GetUniqueId();
            GD.Print($"Player {myId} quitting...");

            if (Multiplayer.IsServer())
            {
                // Server quitting: just remove self and check if game should end
                Players.RemovePlayer(myId);
                CheckGameEnd();
            }
            else
            {
                // Client: tell server we're leaving
                RpcId(1, nameof(ServerNotifyQuit));
            }

            // Clean up local multiplayer state
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
                GD.Print("Only one player left — returning to pool.");
                GameState.GameActive = false;
                Rpc(nameof(ReceiveReturnToPool));
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

            // Block joining if game is already active
            // We ask the server first via RPC
            RpcId(1, nameof(ServerRequestSpawn));
        }

        private void OnPeerConnected(long id)
        {
            if (!Multiplayer.IsServer()) return;
            GD.Print("Peer connected: " + id);
            foreach (var existingId in Players.Players.Keys)
            {
                GD.Print($"Sending existing player {existingId} to new peer {id}");
                RpcId(id, nameof(SpawnPlayerOnClient), existingId);
            }
        }

        private void OnPeerDisconnected(long id)
        {
            Players.RemovePlayer(id);
            CheckGameEnd();
        }

        private void AddPlayer(long peerId)
        {
            var player = new Player(peerId, null, $"Player{peerId}");
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
            // Optionally show a message in the menu — for now just go back
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