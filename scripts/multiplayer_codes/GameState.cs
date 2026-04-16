
namespace main
{
	public static class GameState

	{
		public enum NetworkRole { None, Server, Client }

		public static NetworkRole Role { get; set; } = NetworkRole.None;
		public static string HostIP { get; set; } = "127.0.0.1";

		public static int Port { get; private set; } = 999; 
		
		public static bool GameActive { get; set; } = false;
	}
}
