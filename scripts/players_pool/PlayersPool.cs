using System;
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

            _playersManager.Players.OnPlayersChanged += RefreshList;
            RefreshList();
        }

        private void RefreshList()
        {
            _itemList.Clear();
            foreach (var player in _playersManager.Players.Players.Values)
                _itemList.AddItem(player.Name);
        }

        public override void _ExitTree()
        {
            if (_playersManager != null)
                _playersManager.Players.OnPlayersChanged -= RefreshList;
        }

        private void OnStartGamePressed()
        {
            // Tell PlayersManager to broadcast StartGame to all peers
            _playersManager.StartGame();
        }
    }
}
