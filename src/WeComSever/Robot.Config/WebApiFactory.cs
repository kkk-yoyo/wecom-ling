using Robot.Utils;

namespace Robot.Config;

public class WebApiFactory
{
	public static int BasePort { get; set; }

	public static WebServer Server { get; private set; }

	static WebApiFactory()
	{
		ConfigManager.LoadConfig();
		BasePort = ConfigManager.GetPort();
		Server = WebServer.Instance;
		Server.BaseAddress = $"http://0.0.0.0:{BasePort}/";
	}
}
