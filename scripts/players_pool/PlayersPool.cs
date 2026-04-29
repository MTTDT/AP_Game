using Godot;

namespace main
{
    public partial class PlayersPool : Node
    {
        private ItemList _itemList;
        private PlayersManager _playersManager;
        private Button _startGameBtn;

        public override void _Ready()
        {
            _itemList = GetNode<ItemList>("ItemList");
            _playersManager = GetNode<PlayersManager>("/root/PlayersManager");
            _startGameBtn = GetNode<Button>("Button");

            if (GameState.Role == GameState.NetworkRole.Server)
            {
                _startGameBtn.Pressed += OnStartGamePressed;
            }
            else
            {
                _startGameBtn.Disabled = true;
                _startGameBtn.Text = "Waiting for host...";
            }

            // ── Body selection UI ──────────────────────────────────────────
            // Container for the three body type buttons
            var selectionLabel = new Label();
            selectionLabel.Text = "Select your body:";
            selectionLabel.Position = new Vector2(20, 200);
            AddChild(selectionLabel);

            var hbox = new HBoxContainer();
            hbox.Position = new Vector2(20, 230);
            AddChild(hbox);

            AddBodyButton(hbox, "Default",  BodyType.Default,  "Balanced — 100 HP\nAbility: Heal");
            AddBodyButton(hbox, "Bulky",    BodyType.Bulky,    "Tanky — 150 HP\nAbility: Durability");
            AddBodyButton(hbox, "Snappy",   BodyType.Snappy,   "Fast — 70 HP\nAbility: Speed Boost");
            // ──────────────────────────────────────────────────────────────

            _playersManager.Players.OnPlayersChanged += RefreshList;
            RefreshList();
        }

        private void AddBodyButton(HBoxContainer parent, string label, BodyType type, string tooltip)
        {
            var vbox = new VBoxContainer();

            var btn = new Button();
            btn.Text = label;
            btn.TooltipText = tooltip;
            btn.CustomMinimumSize = new Vector2(110, 50);
            btn.Pressed += () =>
            {
                _playersManager.SelectBodyType(type);
                GD.Print($"Selected body type: {type}");
            };

            var desc = new Label();
            desc.Text = tooltip;
            desc.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            desc.CustomMinimumSize = new Vector2(110, 0);

            vbox.AddChild(btn);
            vbox.AddChild(desc);
            parent.AddChild(vbox);
        }

        private void RefreshList()
        {
            _itemList.Clear();
            foreach (var player in _playersManager.Players.Players.Values)
            {
                string bodyTag = player.BodyType switch
                {
                    BodyType.Bulky  => "[Bulky]",
                    BodyType.Snappy => "[Snappy]",
                    _               => "[Default]"
                };
                _itemList.AddItem($"{player.Name}  {bodyTag}");
            }
        }

        public override void _ExitTree()
        {
            if (_playersManager != null)
                _playersManager.Players.OnPlayersChanged -= RefreshList;
        }

        private void OnStartGamePressed()
        {
            _playersManager.StartGame();
        }

        public override void _Process(double delta)
        {
            // Ctrl+C in the lobby returns to main menu and cleans up multiplayer
            if (Input.IsActionJustPressed("quit"))
            {
                _playersManager.Players.OnPlayersChanged -= RefreshList;
                _playersManager.PlayerQuit();
            }
        }
    }
}