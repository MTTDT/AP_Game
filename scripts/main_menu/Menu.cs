using Godot;
using System.Net;
using System.Net.Sockets;

namespace main
{
	public partial class Menu : Node
	{
		private LineEdit _ipInput;

		public override void _Ready()
		{
			Button hostBtn = GetNode<Button>("HostBtn");
			Button joinBtn = GetNode<Button>("JoinBtn");

			// Show your IP — safe now, won't crash
			Label hostingIP = new Label();
			hostingIP.Text = $"Your IP: {GetLocalIPAddress()}";
			AddChild(hostingIP);

			// IP input field for joining
			_ipInput = GetNode<LineEdit>("HostIPInput");
			_ipInput.PlaceholderText = "Enter host IP to join";
			_ipInput.Text = "";

			

			// These now always get connected since no crash above
			hostBtn.Pressed += OnHostPressed;
			joinBtn.Pressed += OnJoinPressed;
		}

		private void OnHostPressed()
		{
			GameState.Role = GameState.NetworkRole.Server;
			GameState.HostIP = GetLocalIPAddress();
			GetTree().ChangeSceneToFile("res://node_2d.tscn");
		}

		private void OnJoinPressed()
		{
			GameState.Role = GameState.NetworkRole.Client;
			GameState.HostIP = _ipInput.Text; // use typed IP
			GetTree().ChangeSceneToFile("res://node_2d.tscn");
		}

		private static string GetLocalIPAddress()
		{
			// Try proper network interfaces first (more reliable on Mac)
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

			// Fallback: try DNS method
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
