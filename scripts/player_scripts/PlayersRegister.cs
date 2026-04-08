using System.Collections.Generic;

namespace main
{
    public class PlayersRegister
    {
        private Dictionary<long, Player> _players = new Dictionary<long, Player>();

        public IReadOnlyDictionary<long, Player> Players => _players;

        public void AddPlayer(Player player)
        {
            _players[player.Id] = player; 
        }

        public int Count() => _players.Count;

        private int FindIndex()
        {
            int index = 0;
            while (_players.ContainsKey(index))
                index++;

            return index;
        }

        public void RemovePlayer(long index)
        {
            if (_players.ContainsKey(index))
                _players.Remove(index);
        }

    }
}