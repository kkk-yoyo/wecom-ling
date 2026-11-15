using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Web.Http.SelfHost;
using SG9KiyIbtdgGDf12qr;

namespace Robot.Config;

public class WebServer
{
	private static Lazy<WebServer> _lazy;

	private ManualResetEvent _webEvent;

	[CompilerGenerated]
	private string <BaseAddress>k__BackingField;

	private static WebServer y27JSNmL67SJucvDKTx;

	public static WebServer Instance => _lazy.Value;

	public string BaseAddress
	{
		[CompilerGenerated]
		get
		{
			return <BaseAddress>k__BackingField;
		}
		[CompilerGenerated]
		set
		{
			int num = 1;
			int num2 = num;
			while (true)
			{
				switch (num2)
				{
				default:
					return;
				case 0:
					return;
				case 1:
					<BaseAddress>k__BackingField = value;
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_fc096a82eb6f412ab19b9dee9fccbc6e != 0)
					{
						num2 = 0;
					}
					break;
				}
			}
		}
	}

	public Action<WebServer> StartSuccessfulCallback { get; set; }

	public Action<WebServer> RunEndCallback { get; set; }

	public Action<WebServer, AggregateException> StartExceptionCallback { get; set; }

	private WebServer()
	{
		wYMBaJmuluX93iYGiWj();
		base..ctor();
		int num = 0;
		if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_7f8cf60e2e7b454b95cc4d13162f5088 == 0)
		{
			num = 0;
		}
		switch (num)
		{
		case 0:
			break;
		}
	}

	public void StartWebServer()
	{
		int num = 4;
		int num2 = num;
		while (true)
		{
			switch (num2)
			{
			case 2:
				return;
			case 1:
				return;
			case 3:
				_webEvent = new ManualResetEvent(initialState: false);
				num2 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_fd96f1535e384acb8fa11da99670a759 == 0)
				{
					num2 = 0;
				}
				continue;
			case 4:
				if (U1n9pcmjIVWFBxeOcRe(BaseAddress, lRNPJoPvsXOpQ7Ucdon.VmKEqmiqNI))
				{
					return;
				}
				num2 = 3;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_76a1008ec43840dda25966a6b044ac77 != 0)
				{
					num2 = 2;
				}
				continue;
			}
			KYxnFqm4pjQaX6S4fRJ(v23bAImlnU7ARqOQdL9(G0JMU6iqZuWnrN7Stgg.DZhiMdZ4o3), (Action)delegate
			{
				//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
				//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
				//IL_02fa: Expected O, but got Unknown
				//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
				//IL_030a: Expected O, but got Unknown
				//IL_030a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0310: Expected O, but got Unknown
				//IL_0315: Expected O, but got Unknown
				//IL_0310: Unknown result type (might be due to invalid IL or missing references)
				//IL_0317: Expected O, but got Unknown
				int num3 = 1;
				int num4 = num3;
				HttpSelfHostServer val2 = default(HttpSelfHostServer);
				while (true)
				{
					switch (num4)
					{
					default:
						try
						{
							try
							{
								nVdXdwhub0GVhDyNlIF.jnq8ilIcK(pXtvtEm9WqCAhAEPxPd(val2, IOnXplfn1gvOcHpMr0d.mJTigeokEs), nVdXdwhub0GVhDyNlIF.FZ0i9oVaXA);
								int num5 = 4;
								while (true)
								{
									switch (num5)
									{
									case 1:
										if (RunEndCallback == null)
										{
											return;
										}
										num5 = 0;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_80394cf8f0664ed7a0669b96499a0b6e == 0)
										{
											num5 = 0;
										}
										break;
									case 4:
										if (StartSuccessfulCallback != null)
										{
											num5 = 3;
											break;
										}
										goto case 5;
									case 5:
										VCmOBAm8KCLJWJGS0JL(_webEvent, nsJ9LebXYwUJAHxJYqI.Kefi8KN8qU);
										num5 = 1;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_b4d4669a918b4b838c27ddb986baa9b6 == 0)
										{
											num5 = 1;
										}
										break;
									default:
										RunEndCallback(this);
										num5 = 2;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1bbe7758123e4bc485e91d98227e25e4 != 0)
										{
											num5 = 0;
										}
										break;
									case 3:
										StartSuccessfulCallback(this);
										num5 = 5;
										break;
									case 2:
										return;
									}
								}
							}
							catch (AggregateException arg)
							{
								int num6 = 1;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_6d61bc3aa64a47bfb8a09d13c4deb53b == 0)
								{
									num6 = 0;
								}
								while (true)
								{
									switch (num6)
									{
									case 3:
										if (StartExceptionCallback == null)
										{
											return;
										}
										num6 = 2;
										break;
									default:
										_webEvent = null;
										num6 = 3;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c33375e472634b5ebc4d636397603446 == 0)
										{
											num6 = 0;
										}
										break;
									case 2:
										StartExceptionCallback(this, arg);
										num6 = 5;
										break;
									case 4:
										GjhNf2mwPHmWT80sVpi(_webEvent, nVdXdwhub0GVhDyNlIF.yCJilOmafi);
										num6 = 0;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_eb326c992e4b4c9f9cd930152533b185 != 0)
										{
											num6 = 0;
										}
										break;
									case 1:
										VCmOBAm8KCLJWJGS0JL(_webEvent, nsJ9LebXYwUJAHxJYqI.x5Je801Glf);
										num6 = 4;
										break;
									case 5:
										return;
									}
								}
							}
							finally
							{
								GjhNf2mwPHmWT80sVpi(IOnXplfn1gvOcHpMr0d.jnq8ilIcK(val2, IOnXplfn1gvOcHpMr0d.elwimrMoWp), nVdXdwhub0GVhDyNlIF.FZ0i9oVaXA);
								int num7 = 1;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_a8ee04dc577b44a4b973e7119af8874c == 0)
								{
									num7 = 1;
								}
								while (true)
								{
									switch (num7)
									{
									case 1:
										GjhNf2mwPHmWT80sVpi(val2, nVdXdwhub0GVhDyNlIF.W4AiIgRUXi);
										num7 = 0;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_dd83a9ff75d94001819d80b24ef9a897 != 0)
										{
											num7 = 0;
										}
										continue;
									case 0:
										break;
									}
									break;
								}
							}
						}
						finally
						{
							if (val2 != null)
							{
								int num8 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_b45fee1d483247a7ad70d18ef7acd82c != 0)
								{
									num8 = 0;
								}
								while (true)
								{
									switch (num8)
									{
									default:
										Y0eVy9mmh4vBwsFt5fg(val2, dsixtSPyYHjKiE5k7WK.VtIPNcTU9E);
										num8 = 1;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d3b039c30242456eb8cdd20df225be7a == 0)
										{
											num8 = 1;
										}
										continue;
									case 1:
										break;
									}
									break;
								}
							}
						}
					case 2:
						return;
					case 1:
					{
						HttpSelfHostConfiguration val = new HttpSelfHostConfiguration(BaseAddress);
						cIOSELi4u978J67wbn2.jnq8ilIcK((object)val, 104857600L, cIOSELi4u978J67wbn2.sjEiw1ymI7);
						uTE1KtmvEHLaNwF0ill((object)val, 104857600, PS8pOy2Tcc7NPEunP4R.WIZivqqSrM);
						KkGrCxmgde7Lnclu2D7((object)val);
						val2 = new HttpSelfHostServer(val);
						num4 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_83095058e27b497188a2c88050ba8f5f != 0)
						{
							num4 = 0;
						}
						break;
					}
					}
				}
			}, l0LmT4iuPBh0IhZOjEn.zrIijEwDQw);
			num2 = 2;
		}
	}

	public void StopWebServer()
	{
		int num = 1;
		int num2 = num;
		while (true)
		{
			switch (num2)
			{
			default:
				return;
			case 1:
				if (_webEvent == null)
				{
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_89adf4ae56b64b628d8d7fe98ae19963 == 0)
					{
						num2 = 0;
					}
				}
				else
				{
					nsJ9LebXYwUJAHxJYqI.jnq8ilIcK(_webEvent, nsJ9LebXYwUJAHxJYqI.x5Je801Glf);
					num2 = 3;
				}
				break;
			case 3:
				GjhNf2mwPHmWT80sVpi(_webEvent, nVdXdwhub0GVhDyNlIF.yCJilOmafi);
				num2 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_eb326c992e4b4c9f9cd930152533b185 == 0)
				{
					num2 = 2;
				}
				break;
			case 2:
				return;
			case 0:
				return;
			}
		}
	}

	static WebServer()
	{
		int num = 3;
		int num2 = num;
		while (true)
		{
			switch (num2)
			{
			default:
				return;
			case 1:
				_lazy = new Lazy<WebServer>(() => new WebServer());
				num2 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_291da815e7d24e5fad1c5f19ff1c2dd7 != 0)
				{
					num2 = 0;
				}
				break;
			case 3:
				z2jc63fLkugS1X8Q9N.TrhALsn43();
				num2 = 2;
				break;
			case 2:
				wYMBaJmuluX93iYGiWj();
				num2 = 1;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_fa47eaa7900849fc99eccdf087262b8e == 0)
				{
					num2 = 1;
				}
				break;
			case 0:
				return;
			}
		}
	}

	internal static void wYMBaJmuluX93iYGiWj()
	{
		z2jc63fLkugS1X8Q9N.rQ19jmJab();
	}

	internal static bool iq0cmKmq9cCoeNILnBt()
	{
		return y27JSNmL67SJucvDKTx == null;
	}

	internal static WebServer nBWVpymMySKvHR9qJEr()
	{
		return y27JSNmL67SJucvDKTx;
	}

	internal static bool U1n9pcmjIVWFBxeOcRe(object P_0, object P_1)
	{
		return lRNPJoPvsXOpQ7Ucdon.jnq8ilIcK((string)P_0, (lRNPJoPvsXOpQ7Ucdon)P_1);
	}

	internal static object v23bAImlnU7ARqOQdL9(object P_0)
	{
		return G0JMU6iqZuWnrN7Stgg.jnq8ilIcK((G0JMU6iqZuWnrN7Stgg)P_0);
	}

	internal static object KYxnFqm4pjQaX6S4fRJ(object P_0, object P_1, object P_2)
	{
		return l0LmT4iuPBh0IhZOjEn.jnq8ilIcK(P_0, (Action)P_1, (l0LmT4iuPBh0IhZOjEn)P_2);
	}

	internal static void GjhNf2mwPHmWT80sVpi(object P_0, object P_1)
	{
		nVdXdwhub0GVhDyNlIF.jnq8ilIcK(P_0, (nVdXdwhub0GVhDyNlIF)P_1);
	}

	internal static void uTE1KtmvEHLaNwF0ill(object P_0, int P_1, object P_2)
	{
		PS8pOy2Tcc7NPEunP4R.jnq8ilIcK(P_0, P_1, (PS8pOy2Tcc7NPEunP4R)P_2);
	}

	internal static void KkGrCxmgde7Lnclu2D7(object P_0)
	{
		((HttpSelfHostConfiguration)P_0).Register();
	}

	internal static object pXtvtEm9WqCAhAEPxPd(object P_0, object P_1)
	{
		return IOnXplfn1gvOcHpMr0d.jnq8ilIcK(P_0, (IOnXplfn1gvOcHpMr0d)P_1);
	}

	internal static bool VCmOBAm8KCLJWJGS0JL(object P_0, object P_1)
	{
		return nsJ9LebXYwUJAHxJYqI.jnq8ilIcK(P_0, (nsJ9LebXYwUJAHxJYqI)P_1);
	}

	internal static void Y0eVy9mmh4vBwsFt5fg(object P_0, object P_1)
	{
		dsixtSPyYHjKiE5k7WK.jnq8ilIcK(P_0, (dsixtSPyYHjKiE5k7WK)P_1);
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.2.0.10050' (yours is '9.1.0.7988')
