using System.Collections.Generic;
using System;

namespace main
{
    public class PlayersRegister
    {
        public event Action OnPlayersChanged;

        private readonly Dictionary<long, Player> _players = new();
        public IReadOnlyDictionary<long, Player> Players => _players;

        public void AddPlayer(Player player)
        {
            _players[player.Id] = player;
            OnPlayersChanged?.Invoke();
        }

        public int Count() => _players.Count;

        public void RemovePlayer(long index)
        {
            if (_players.Remove(index)) OnPlayersChanged?.Invoke();
        }

        public void NotifyChanged() => OnPlayersChanged?.Invoke();
    }
}