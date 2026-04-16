// Menu.cs
using Godot;
using System.Net;
using System.Net.Sockets;

namespace main
{
    public partial class Menu : Node
    {
        private LineEdit _ipInput;
        private PlayersManager _playersManager;

        public override void _Ready()
        {
            _playersManager = GetNode<PlayersManager>("/root/PlayersManager");

            Button hostBtn = GetNode<Button>("HostBtn");
            Button joinBtn = GetNode<Button>("JoinBtn");

            Label hostingIP = new Label();
            hostingIP.Text = $"Your IP: {GetLocalIPAddress()}";
            AddChild(hostingIP);

            _ipInput = GetNode<LineEdit>("HostIPInput");
            _ipInput.PlaceholderText = "Enter host IP to join";
            _ipInput.Text = "";

            hostBtn.Pressed += OnHostPressed;
            joinBtn.Pressed += OnJoinPressed;
        }

        private void OnHostPressed()
        {
            GameState.Role = GameState.NetworkRole.Server;
            GameState.HostIP = GetLocalIPAddress();
            _playersManager.StartServer();
            GetTree().ChangeSceneToFile("res://players_pool.tscn");
        }

        private void OnJoinPressed()
        {
            GameState.Role = GameState.NetworkRole.Client;
            GameState.HostIP = _ipInput.Text;
            _playersManager.StartClient();
            GetTree().ChangeSceneToFile("res://players_pool.tscn");
        }

        private static string GetLocalIPAddress()
        {
            try
            {
                foreach (var iface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (iface.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up) continue;
                    foreach (var addr in iface.GetIPProperties().UnicastAddresses)
                    {
                        if (addr.Address.AddressFamily == AddressFamily.InterNetwork &&
                            !System.Net.IPAddress.IsLoopback(addr.Address))
                            return addr.Address.ToString();
                    }
                }
            }
            catch { }

            try
            {
                foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        return ip.ToString();
            }
            catch { }

            return "127.0.0.1";
        }
    }
}
