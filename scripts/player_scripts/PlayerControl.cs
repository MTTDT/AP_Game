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
        private CanvasLayer _overlayLayer;
        private Control _overlay;

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

            _overlayLayer = new CanvasLayer();
            _overlayLayer.Layer = 10; // render above everything
            _overlay = BuildOverlay();
            _overlayLayer.AddChild(_overlay);
            AddChild(_overlayLayer);
        }

        public override void _ExitTree()
        {
            if (_playersManager != null)
                _playersManager.Players.OnPlayersChanged -= OnPlayersChanged;
        }

        private void OnPlayersChanged() { }

        // ── Spawning ─────────────────────────────────────────────────────────

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
            body.BodyDestroyed += () => OnBodyDestroyed(peerId, body);
            AddChild(body);

            GD.Print($"Spawned {bodyType} body for player {peerId} (I am {Multiplayer.GetUniqueId()})");
        }

        // ── Death & Win ──────────────────────────────────────────────────────

        private void OnBodyDestroyed(long peerId, Body body)
        {
            body.QueueFree();

            long myId = Multiplayer.GetUniqueId();

            if (peerId == myId)
            {
                ShowDeathScreen();
            }
            else
            {
                CheckIfWinner();
            }

            if (Multiplayer.IsServer())
                HandlePlayerEliminated(peerId);
            else
                RpcId(1, nameof(ServerNotifyEliminated), peerId);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
        private void ServerNotifyEliminated(long peerId)
        {
            HandlePlayerEliminated(peerId);
        }

        private void HandlePlayerEliminated(long peerId)
        {
            GD.Print($"Player {peerId} eliminated.");
        }

        private void ShowDeathScreen()
        {
            var label = _overlay.GetNode<Label>("VBox/Label");
            var button = _overlay.GetNode<Button>("VBox/Button");

            label.Text = "You died!";
            button.Text = "Return to Lobby";
            button.Visible = true;
            button.Pressed += ReturnToPool;

            _overlay.Visible = true;
        }

        private void CheckIfWinner()
        {
            long myId = Multiplayer.GetUniqueId();

            int othersAlive = 0;
            foreach (Node child in GetChildren())
            {
                if (child is Body b && b.Name != $"Player_{myId}")
                    othersAlive++;
            }

            if (othersAlive == 0)
                ShowWinScreen();
        }

        private void ShowWinScreen()
        {
            var label = _overlay.GetNode<Label>("VBox/Label");
            var button = _overlay.GetNode<Button>("VBox/Button");

            label.Text = "You win!";
            button.Text = "Return to Lobby";
            button.Visible = true;
            button.Pressed += ReturnToPool;

            _overlay.Visible = true;
        }

        private void ReturnToPool()
        {
            _playersManager.Players.OnPlayersChanged -= OnPlayersChanged;
            _playersManager.ReturnToPool();
        }

        // ── Overlay builder ──────────────────────────────────────────────────

        private Control BuildOverlay()
        {
            var root = new Control();
            root.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            root.Visible = false;
            root.Name = "Overlay";

            var bg = new ColorRect();
            bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            bg.Color = new Color(0, 0, 0, 0.6f);
            root.AddChild(bg);

            var center = new CenterContainer();
            center.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            root.AddChild(center);

            var vbox = new VBoxContainer();
            vbox.Name = "VBox";
            vbox.AddThemeConstantOverride("separation", 20);
            center.AddChild(vbox);

            var label = new Label();
            label.Name = "Label";
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.AddThemeFontSizeOverride("font_size", 48);
            vbox.AddChild(label);

            var button = new Button();
            button.Name = "Button";
            button.CustomMinimumSize = new Vector2(220, 50);
            button.Visible = false;
            vbox.AddChild(button);

            return root;
        }

        // ── Input ────────────────────────────────────────────────────────────

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