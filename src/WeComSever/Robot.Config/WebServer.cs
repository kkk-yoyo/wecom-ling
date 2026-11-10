using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.SelfHost;

namespace Robot.Config;

public class WebServer
{
	private static Lazy<WebServer> _lazy = new Lazy<WebServer>(() => new WebServer());

	private ManualResetEvent _webEvent;

	public static WebServer Instance => _lazy.Value;

	public string BaseAddress { get; set; }

	public Action<WebServer> StartSuccessfulCallback { get; set; }

	public Action<WebServer> RunEndCallback { get; set; }

	public Action<WebServer, AggregateException> StartExceptionCallback { get; set; }

	private WebServer()
	{
	}

	public void StartWebServer()
	{
		if (string.IsNullOrEmpty(BaseAddress))
		{
			return;
		}
		_webEvent = new ManualResetEvent(initialState: false);
		Task.Factory.StartNew(delegate
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			//IL_0016: Expected O, but got Unknown
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Expected O, but got Unknown
			HttpSelfHostConfiguration val = new HttpSelfHostConfiguration(BaseAddress);
			WebApiConfig.Register(val);
			HttpSelfHostServer val2 = new HttpSelfHostServer(val);
			try
			{
				try
				{
					val2.OpenAsync().Wait();
					if (StartSuccessfulCallback != null)
					{
						StartSuccessfulCallback(this);
					}
					_webEvent.WaitOne();
					if (RunEndCallback != null)
					{
						RunEndCallback(this);
					}
				}
				catch (AggregateException arg)
				{
					_webEvent.Set();
					_webEvent.Close();
					_webEvent = null;
					if (StartExceptionCallback != null)
					{
						StartExceptionCallback(this, arg);
					}
				}
				finally
				{
					val2.CloseAsync().Wait();
					((HttpMessageHandler)(object)val2).Dispose();
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		});
	}

	public void StopWebServer()
	{
		if (_webEvent != null)
		{
			_webEvent.Set();
			_webEvent.Close();
		}
	}
}
