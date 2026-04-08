// namespace playes_pool
// {
//     public partial class PlayersPool : Node
//     {
//         private Dictionary<long, Body> _players = new();

//         public void AddPlayer(long id, Body player)
//         {
//             _players[id] = player;
//         }

//         public void RemovePlayer(long id)
//         {
//             if (_players.ContainsKey(id))
//                 _players.Remove(id);
//         }

//         public Body GetPlayer(long id)
//         {
//             return _players.ContainsKey(id) ? _players[id] : null;
//         }
//     }
// }