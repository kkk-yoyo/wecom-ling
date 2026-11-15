using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Threading;
using A8rKktAdECkdokFCxq;
using Robot.Utils;
using SG9KiyIbtdgGDf12qr;
using WeComConsoleServer.Core;
using WeComSever.Utils;
using WebServerConsole.Utils;

namespace WeComSever;

public class App : Application
{
	private struct IconInfo
	{
		public bool IsIcon;

		public int HotspotX;

		public int HotspotY;

		public IntPtr Mask;

		public IntPtr Color;
	}

	private struct BitmapInfo
	{
		public int Size;

		public int Width;

		public int Height;

		public short Planes;

		public short BitCount;

		public int Compression;

		public int SizeImage;

		public int XPelsPerMeter;

		public int YPelsPerMeter;

		public int ClrUsed;

		public int ClrImportant;
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class <>c
	{
		public static readonly <>c <>9;

		public static DispatcherUnhandledExceptionEventHandler <>9__5_0;

		public static UnhandledExceptionEventHandler <>9__5_1;

		public static EventHandler<UnobservedTaskExceptionEventArgs> <>9__5_2;

		private static <>c O1XyfcDMx3s9icaL6Ow;

		static <>c()
		{
			int num = 1;
			int num2 = num;
			while (true)
			{
				switch (num2)
				{
				default:
					xRGQmXD4dTvCTiYGnv4();
					num2 = 3;
					break;
				case 1:
					I08qZeDlPKgHTxUeKpK();
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_5db1faeadc7a4faeaddf4f71b089648f != 0)
					{
						num2 = 0;
					}
					break;
				case 2:
					return;
				case 3:
					<>9 = new <>c();
					num2 = 2;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_9de6b4ecde674f909622ae29ccaad31e == 0)
					{
						num2 = 0;
					}
					break;
				}
			}
		}

		public <>c()
		{
			xRGQmXD4dTvCTiYGnv4();
			base..ctor();
			int num = 0;
			if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1fdc4536a75247b19a0801dd70bf34e9 == 0)
			{
				num = 0;
			}
			switch (num)
			{
			case 0:
				break;
			}
		}

		internal void <OnStartup>b__5_0(object s, DispatcherUnhandledExceptionEventArgs args)
		{
			int num = 3;
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
					cx789vD8TkQMQlroN7Z(args, true, y8iVbRET6QxQsChlnBy.bGvEGiLffy);
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_fd96f1535e384acb8fa11da99670a759 == 0)
					{
						num2 = 0;
					}
					break;
				case 3:
					Logger.Error((string)EduXFeDwjWw8A2RqxOp(-1464755110 ^ -1464755370), (Exception)RaV2qsDv61St5A3f40G(args, bL2N13EAbCHBlZTitMu.NLrEX4eGMQ));
					num2 = 2;
					break;
				case 2:
					UhUx6UPAwJdVRaVNiES.jnq8ilIcK((string)sakwnhD93LfYNUJRGyL(EduXFeDwjWw8A2RqxOp(0x780E5C1F ^ 0x780E5B2F), tYv648DgZQR1sPreAmy(RaV2qsDv61St5A3f40G(args, bL2N13EAbCHBlZTitMu.NLrEX4eGMQ), gjqRKtPV17ZUEQVAWxv.AdKPK0ve7w), RxUSOmPDJGsMmCCGPPg.hbkPt9hhua), (string)EduXFeDwjWw8A2RqxOp(-1680728354 ^ -1680729164), MessageBoxButton.OK, MessageBoxImage.Hand, UhUx6UPAwJdVRaVNiES.mITPXheM7o);
					num2 = 1;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c33375e472634b5ebc4d636397603446 == 0)
					{
						num2 = 0;
					}
					break;
				}
			}
		}

		internal void <OnStartup>b__5_1(object s, UnhandledExceptionEventArgs args)
		{
			int num = 1;
			int num2 = num;
			Exception ex = default(Exception);
			while (true)
			{
				switch (num2)
				{
				case 1:
					ex = XmXbC9DmJpnCA5uCo6u(args, PWWOMmE6dCMmTMJgeMO.vZHECjjNgK) as Exception;
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_2cca3d059d854b57aca52e0b80a8eb7d != 0)
					{
						num2 = 0;
					}
					break;
				default:
					Logger.Error((string)EduXFeDwjWw8A2RqxOp(-592637609 ^ -592636389), ex);
					num2 = 1;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_99797e4f3bab41baabcb75f7a30397b7 != 0)
					{
						num2 = 2;
					}
					break;
				case 2:
					return;
				}
			}
		}

		internal void <OnStartup>b__5_2(object s, UnobservedTaskExceptionEventArgs args)
		{
			int num = 2;
			int num2 = num;
			while (true)
			{
				switch (num2)
				{
				default:
					return;
				case 1:
					hbIFoBDZGNwH0b8vPhd(args, dsixtSPyYHjKiE5k7WK.HVQEShI6vO);
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_bd908099f891496e8274f46975b80fa0 == 0)
					{
						num2 = 0;
					}
					break;
				case 0:
					return;
				case 2:
					Logger.Error((string)EduXFeDwjWw8A2RqxOp(-1613644704 ^ -1613645056), (Exception)ji46kxDI7dnya37WlSD(args, Qgu0RjEH80PcrPycXSH.v3nEaAU2ac));
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_13470d6aa0454f7eb0318dc44f893926 == 0)
					{
						num2 = 1;
					}
					break;
				}
			}
		}

		internal static void I08qZeDlPKgHTxUeKpK()
		{
			z2jc63fLkugS1X8Q9N.TrhALsn43();
		}

		internal static void xRGQmXD4dTvCTiYGnv4()
		{
			z2jc63fLkugS1X8Q9N.rQ19jmJab();
		}

		internal static bool L7hf4jDuVnBmPs1ecbA()
		{
			return O1XyfcDMx3s9icaL6Ow == null;
		}

		internal static <>c tZcBcUDjoK0qe0YLo46()
		{
			return O1XyfcDMx3s9icaL6Ow;
		}

		internal static object EduXFeDwjWw8A2RqxOp(int P_0)
		{
			return z2jc63fLkugS1X8Q9N.tE1kD1vbB(P_0);
		}

		internal static object RaV2qsDv61St5A3f40G(object P_0, object P_1)
		{
			return bL2N13EAbCHBlZTitMu.jnq8ilIcK(P_0, (bL2N13EAbCHBlZTitMu)P_1);
		}

		internal static object tYv648DgZQR1sPreAmy(object P_0, object P_1)
		{
			return gjqRKtPV17ZUEQVAWxv.jnq8ilIcK(P_0, (gjqRKtPV17ZUEQVAWxv)P_1);
		}

		internal static object sakwnhD93LfYNUJRGyL(object P_0, object P_1, object P_2)
		{
			return RxUSOmPDJGsMmCCGPPg.jnq8ilIcK((string)P_0, (string)P_1, (RxUSOmPDJGsMmCCGPPg)P_2);
		}

		internal static void cx789vD8TkQMQlroN7Z(object P_0, bool P_1, object P_2)
		{
			y8iVbRET6QxQsChlnBy.jnq8ilIcK(P_0, P_1, (y8iVbRET6QxQsChlnBy)P_2);
		}

		internal static object XmXbC9DmJpnCA5uCo6u(object P_0, object P_1)
		{
			return PWWOMmE6dCMmTMJgeMO.jnq8ilIcK(P_0, (PWWOMmE6dCMmTMJgeMO)P_1);
		}

		internal static object ji46kxDI7dnya37WlSD(object P_0, object P_1)
		{
			return Qgu0RjEH80PcrPycXSH.jnq8ilIcK(P_0, (Qgu0RjEH80PcrPycXSH)P_1);
		}

		internal static void hbIFoBDZGNwH0b8vPhd(object P_0, object P_1)
		{
			dsixtSPyYHjKiE5k7WK.jnq8ilIcK(P_0, (dsixtSPyYHjKiE5k7WK)P_1);
		}
	}

	private Mutex _instanceMutex;

	private bool _ownsInstanceMutex;

	private static readonly List<IntPtr> TaskbarIcons;

	internal static App UYOh93K8Z4uhmZXNRuS;

	protected override void OnStartup(StartupEventArgs e)
	{
		int num = 9;
		while (true)
		{
			int num2 = num;
			while (true)
			{
				EventHandler<UnobservedTaskExceptionEventArgs> eventHandler;
				switch (num2)
				{
				case 4:
					eEh7s6K10VK0392hdWN(this, dsixtSPyYHjKiE5k7WK.Qa6P2fOWgc);
					num2 = 5;
					continue;
				case 6:
					try
					{
						MainWindow mainWindow = new MainWindow();
						int num3 = 2;
						while (true)
						{
							switch (num3)
							{
							default:
								return;
							case 2:
							{
								AICxWLKRsH96bvEjecu(this, mainWindow, D6WA9OP5vy8OaFUtOlI.t2IPBllGJR);
								int num4 = 3;
								num3 = num4;
								break;
							}
							case 3:
								dsixtSPyYHjKiE5k7WK.jnq8ilIcK(mainWindow, dsixtSPyYHjKiE5k7WK.O66P0I836x);
								num3 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_3c2786a3c96a4c30b2aea580e623804b != 0)
								{
									num3 = 1;
								}
								break;
							case 1:
								yq3PmpKZSbE0PBC5dhS(this, ShutdownMode.OnMainWindowClose, mui3PqPbTA6nXByRbay.rZWPxbhlKL);
								num3 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_080a2cdddf2144a182f32bab096ad248 != 0)
								{
									num3 = 0;
								}
								break;
							case 0:
								return;
							}
						}
					}
					catch (Exception ex)
					{
						int num5 = 3;
						int num6 = num5;
						while (true)
						{
							switch (num6)
							{
							default:
								rn2haAK3iMNigdNU5PJ(FkiYIqKNBuKy5pjZT0j(UmijCGKJkFtBcOwVs6J(-1033100007 ^ -1033101235), gjqRKtPV17ZUEQVAWxv.jnq8ilIcK(ex, gjqRKtPV17ZUEQVAWxv.AdKPK0ve7w), RxUSOmPDJGsMmCCGPPg.hbkPt9hhua), UmijCGKJkFtBcOwVs6J(-2018914802 ^ -2018915484), MessageBoxButton.OK, MessageBoxImage.Hand, UhUx6UPAwJdVRaVNiES.mITPXheM7o);
								num6 = 1;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_f9d0516f42ac4a4ca53e84f1dda1b106 == 0)
								{
									num6 = 0;
								}
								break;
							case 3:
								Logger.Error((string)FkiYIqKNBuKy5pjZT0j(UmijCGKJkFtBcOwVs6J(0x3BA46780 ^ 0x3BA462BE), p1GX9NKO2aOpGdHC4Oi(ex, gjqRKtPV17ZUEQVAWxv.AdKPK0ve7w), RxUSOmPDJGsMmCCGPPg.hbkPt9hhua), ex);
								num6 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1bbe7758123e4bc485e91d98227e25e4 == 0)
								{
									num6 = 0;
								}
								break;
							case 1:
								dsixtSPyYHjKiE5k7WK.jnq8ilIcK(this, dsixtSPyYHjKiE5k7WK.Qa6P2fOWgc);
								num6 = 2;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_fe6655b3fe1741f7b687606f3c613956 != 0)
								{
									num6 = 1;
								}
								break;
							case 2:
								return;
							}
						}
					}
				case 13:
					if (!EnsureLicensed())
					{
						num2 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_bd908099f891496e8274f46975b80fa0 != 0)
						{
							num2 = 0;
						}
					}
					else
					{
						num2 = 14;
					}
					continue;
				case 10:
					if (TryAcquireSingleInstance())
					{
						num2 = 7;
						continue;
					}
					goto case 4;
				case 15:
					return;
				case 8:
					yq3PmpKZSbE0PBC5dhS(this, ShutdownMode.OnExplicitShutdown, mui3PqPbTA6nXByRbay.rZWPxbhlKL);
					num = 10;
					break;
				case 1:
					return;
				case 11:
					num2 = 3;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_76fed529d5134afd95ce65e7c04235fc != 0)
					{
						num2 = 6;
					}
					continue;
				case 12:
					eventHandler = <>c.<>9__5_2;
					if (eventHandler == null)
					{
						num = 2;
						break;
					}
					goto IL_0491;
				default:
					dsixtSPyYHjKiE5k7WK.jnq8ilIcK(this, dsixtSPyYHjKiE5k7WK.Qa6P2fOWgc);
					num2 = 1;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c33375e472634b5ebc4d636397603446 == 0)
					{
						num2 = 0;
					}
					continue;
				case 14:
					try
					{
						DEAWsdKpgjsP6v6aBOK();
						int num7 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_b3eccb7902c54a0db09f3abb83d000b4 == 0)
						{
							num7 = 0;
						}
						switch (num7)
						{
						case 0:
							break;
						}
					}
					catch (Exception ex2)
					{
						int num8 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_cbea79c7647f4fe9ae446d7b78f25d28 == 0)
						{
							num8 = 0;
						}
						while (true)
						{
							switch (num8)
							{
							default:
								Logger.Error((string)FkiYIqKNBuKy5pjZT0j(UmijCGKJkFtBcOwVs6J(0x3EDBC22B ^ 0x3EDBC70F), p1GX9NKO2aOpGdHC4Oi(ex2, gjqRKtPV17ZUEQVAWxv.AdKPK0ve7w), RxUSOmPDJGsMmCCGPPg.hbkPt9hhua), ex2);
								num8 = 1;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1bbe7758123e4bc485e91d98227e25e4 == 0)
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
					goto case 11;
				case 9:
					cMpGn8PPU6quCMhag6e.jnq8ilIcK(this, e, cMpGn8PPU6quCMhag6e.HQEPEituBw);
					num2 = 1;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_a4eb1fc2846b481197982b0faf1ae1eb == 0)
					{
						num2 = 8;
					}
					continue;
				case 5:
					return;
				case 7:
					LifEQmKFgL45NqBrVVa(this, (DispatcherUnhandledExceptionEventHandler)delegate(object s, DispatcherUnhandledExceptionEventArgs args)
					{
						int num9 = 3;
						int num10 = num9;
						while (true)
						{
							switch (num10)
							{
							default:
								return;
							case 0:
								return;
							case 1:
								<>c.cx789vD8TkQMQlroN7Z(args, true, y8iVbRET6QxQsChlnBy.bGvEGiLffy);
								num10 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_fd96f1535e384acb8fa11da99670a759 == 0)
								{
									num10 = 0;
								}
								break;
							case 3:
								Logger.Error((string)<>c.EduXFeDwjWw8A2RqxOp(-1464755110 ^ -1464755370), (Exception)<>c.RaV2qsDv61St5A3f40G(args, bL2N13EAbCHBlZTitMu.NLrEX4eGMQ));
								num10 = 2;
								break;
							case 2:
								UhUx6UPAwJdVRaVNiES.jnq8ilIcK((string)<>c.sakwnhD93LfYNUJRGyL(<>c.EduXFeDwjWw8A2RqxOp(0x780E5C1F ^ 0x780E5B2F), <>c.tYv648DgZQR1sPreAmy(<>c.RaV2qsDv61St5A3f40G(args, bL2N13EAbCHBlZTitMu.NLrEX4eGMQ), gjqRKtPV17ZUEQVAWxv.AdKPK0ve7w), RxUSOmPDJGsMmCCGPPg.hbkPt9hhua), (string)<>c.EduXFeDwjWw8A2RqxOp(-1680728354 ^ -1680729164), MessageBoxButton.OK, MessageBoxImage.Hand, UhUx6UPAwJdVRaVNiES.mITPXheM7o);
								num10 = 1;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c33375e472634b5ebc4d636397603446 == 0)
								{
									num10 = 0;
								}
								break;
							}
						}
					}, phjGGaPUNxAwt4XffW9.RH9PQFVOcp);
					num2 = 3;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_dd83a9ff75d94001819d80b24ef9a897 != 0)
					{
						num2 = 1;
					}
					continue;
				case 3:
					PBDR4aPe8bqnDC7c5Or.jnq8ilIcK(B30MVCPh7T3lxdVbgjN.jnq8ilIcK(B30MVCPh7T3lxdVbgjN.d5MPYnWlQG), delegate(object s, UnhandledExceptionEventArgs args)
					{
						int num9 = 1;
						int num10 = num9;
						Exception ex3 = default(Exception);
						while (true)
						{
							switch (num10)
							{
							case 1:
								ex3 = <>c.XmXbC9DmJpnCA5uCo6u(args, PWWOMmE6dCMmTMJgeMO.vZHECjjNgK) as Exception;
								num10 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_2cca3d059d854b57aca52e0b80a8eb7d != 0)
								{
									num10 = 0;
								}
								break;
							default:
								Logger.Error((string)<>c.EduXFeDwjWw8A2RqxOp(-592637609 ^ -592636389), ex3);
								num10 = 1;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_99797e4f3bab41baabcb75f7a30397b7 != 0)
								{
									num10 = 2;
								}
								break;
							case 2:
								return;
							}
						}
					}, PBDR4aPe8bqnDC7c5Or.S8BPfEklNb);
					num = 12;
					break;
				case 2:
					{
						eventHandler = (<>c.<>9__5_2 = delegate(object s, UnobservedTaskExceptionEventArgs args)
						{
							int num9 = 2;
							int num10 = num9;
							while (true)
							{
								switch (num10)
								{
								default:
									return;
								case 1:
									<>c.hbIFoBDZGNwH0b8vPhd(args, dsixtSPyYHjKiE5k7WK.HVQEShI6vO);
									num10 = 0;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_bd908099f891496e8274f46975b80fa0 == 0)
									{
										num10 = 0;
									}
									break;
								case 0:
									return;
								case 2:
									Logger.Error((string)<>c.EduXFeDwjWw8A2RqxOp(-1613644704 ^ -1613645056), (Exception)<>c.ji46kxDI7dnya37WlSD(args, Qgu0RjEH80PcrPycXSH.v3nEaAU2ac));
									num10 = 0;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_13470d6aa0454f7eb0318dc44f893926 == 0)
									{
										num10 = 1;
									}
									break;
								}
							}
						});
						goto IL_0491;
					}
					IL_0491:
					RkEar0PoW1Q3tLvmt4Z.jnq8ilIcK(eventHandler, RkEar0PoW1Q3tLvmt4Z.GCZPiNJeOW);
					num2 = 13;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1601bb4cfcbc4e588ecdc797e83d5acc != 0)
					{
						num2 = 5;
					}
					continue;
				}
				break;
			}
		}
	}

	private bool TryAcquireSingleInstance()
	{
		int num = 6;
		while (true)
		{
			int num2 = num;
			while (true)
			{
				switch (num2)
				{
				case 2:
					return true;
				case 1:
					YHP7fnKzJWlT4HrwJKF();
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_dd83a9ff75d94001819d80b24ef9a897 != 0)
					{
						num2 = 0;
					}
					continue;
				case 6:
					break;
				case 4:
					return false;
				case 5:
					try
					{
						_ownsInstanceMutex = Pc64WNKsvUfgLFWWrPa(_instanceMutex, 0, false, t8mABAPT4ikBh9MWU7D.UAMPGMDoYM);
						int num3 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d34bf7edba6543039b52c92f6f825304 != 0)
						{
							num3 = 0;
						}
						switch (num3)
						{
						case 0:
							break;
						}
					}
					catch (AbandonedMutexException)
					{
						int num4 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_cbbf75a1fc4b4ad1ae851089027cdc57 == 0)
						{
							num4 = 0;
						}
						while (true)
						{
							switch (num4)
							{
							default:
								_ownsInstanceMutex = true;
								num4 = 1;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_289d7cd175a8485b8ea0e81b4750a0f3 != 0)
								{
									num4 = 1;
								}
								continue;
							case 1:
								break;
							}
							break;
						}
					}
					goto case 3;
				case 3:
					if (!_ownsInstanceMutex)
					{
						num2 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_ebde985f65fe4000b6ba504548c2703b == 0)
						{
							num2 = 1;
						}
						continue;
					}
					goto case 2;
				default:
					rn2haAK3iMNigdNU5PJ(UmijCGKJkFtBcOwVs6J(0x2F53E4C2 ^ 0x2F53E106), z2jc63fLkugS1X8Q9N.tE1kD1vbB(-1778800683 ^ -1778801985), MessageBoxButton.OK, MessageBoxImage.Asterisk, UhUx6UPAwJdVRaVNiES.mITPXheM7o);
					num2 = 4;
					continue;
				}
				break;
			}
			_instanceMutex = new Mutex(initiallyOwned: false, (string)UmijCGKJkFtBcOwVs6J(0x1FB30740 ^ 0x1FB30232));
			num = 5;
		}
	}

	private static void ActivateExistingInstance()
	{
		int num = 3;
		int num2 = num;
		int num3 = default(int);
		Process[] array = default(Process[]);
		Process process2 = default(Process);
		Process process = default(Process);
		while (true)
		{
			switch (num2)
			{
			case 9:
			case 11:
				if (num3 >= array.Length)
				{
					num2 = 5;
					continue;
				}
				goto case 4;
			case 10:
				if (kfHGSgPnOQlgJ5xXCSg.jnq8ilIcK(process2, kfHGSgPnOQlgJ5xXCSg.iYgPcMfokN) != i75S8uDEbaBtFDVP2Ek(process, kfHGSgPnOQlgJ5xXCSg.iYgPcMfokN))
				{
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d9b8fefff9de4f9c9c2f7dc1a6c24ab2 == 0)
					{
						num2 = 0;
					}
					continue;
				}
				break;
			default:
				if (!SfG9TaDxpCdlilvHnDe(HHTUklDba4Q3AvM0ToG(process2, jWWNjyPkJOAr6gWqCTf.BAWPrg49CT), IntPtr.Zero, E6GRu8P7P9F6qI8rcqV.ajPPdAfJO9))
				{
					num2 = 1;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_5138927ba3c1408e9ba282c6f1787fc1 != 0)
					{
						num2 = 0;
					}
					continue;
				}
				break;
			case 5:
				return;
			case 7:
				WinApi.SetForegroundWindow(HHTUklDba4Q3AvM0ToG(process2, jWWNjyPkJOAr6gWqCTf.BAWPrg49CT));
				num2 = 8;
				continue;
			case 4:
				process2 = array[num3];
				num2 = 10;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_76a1008ec43840dda25966a6b044ac77 != 0)
				{
					num2 = 7;
				}
				continue;
			case 1:
				WinApi.ShowWindow(HHTUklDba4Q3AvM0ToG(process2, jWWNjyPkJOAr6gWqCTf.BAWPrg49CT), 9);
				num2 = 7;
				continue;
			case 2:
				array = (Process[])DLVdT5DP2fBntSnSIuu(gjqRKtPV17ZUEQVAWxv.jnq8ilIcK(process, gjqRKtPV17ZUEQVAWxv.ddrPHEwGCY), vCD6HcPaOyR6Koj64nc.UfHPSCYhs5);
				num2 = 6;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_f0c3ce86ec4a4bf2b2132a6d66c41033 != 0)
				{
					num2 = 3;
				}
				continue;
			case 3:
				process = (Process)goJuFIDWZWfg4I1riSX(bx01EUP6YjantAQO76e.S9nPCIILsr);
				num2 = 2;
				continue;
			case 6:
				num3 = 0;
				num2 = 9;
				continue;
			case 8:
				return;
			}
			num3++;
			num2 = 11;
		}
	}

	private bool EnsureLicensed()
	{
		bool result = default(bool);
		switch (1)
		{
		case 1:
			try
			{
				ConfigManager.LoadConfig();
				int num = 10;
				bool? flag = default(bool?);
				while (true)
				{
					switch (num)
					{
					case 8:
						break;
					case 9:
						if (flag == true)
						{
							num = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_bd908099f891496e8274f46975b80fa0 == 0)
							{
								num = 0;
							}
							continue;
						}
						goto case 2;
					case 11:
						result = false;
						num = 13;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_5db1faeadc7a4faeaddf4f71b089648f != 0)
						{
							num = 10;
						}
						continue;
					case 2:
					case 7:
						rn2haAK3iMNigdNU5PJ(UmijCGKJkFtBcOwVs6J(0x1DB17C88 ^ 0x1DB17ABE), UmijCGKJkFtBcOwVs6J(-909674956 ^ -909676448), MessageBoxButton.OK, MessageBoxImage.Exclamation, UhUx6UPAwJdVRaVNiES.mITPXheM7o);
						num = 11;
						continue;
					case 10:
						if (LicenseManager.IsActivated())
						{
							num = 6;
							continue;
						}
						goto case 12;
					case 12:
						T51vu1DUMMGfxBacxPp(UmijCGKJkFtBcOwVs6J(0x29DAFBE2 ^ 0x29DAFDF2));
						num = 4;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_80394cf8f0664ed7a0669b96499a0b6e != 0)
						{
							num = 4;
						}
						continue;
					case 3:
						result = true;
						num = 8;
						continue;
					case 1:
						break;
					default:
						if (!LicenseManager.IsActivated())
						{
							num = 2;
							continue;
						}
						goto case 5;
					case 5:
						result = true;
						num = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_80394cf8f0664ed7a0669b96499a0b6e == 0)
						{
							num = 1;
						}
						continue;
					case 4:
						flag = KBMWK0PLXIxq1L4gI6T.jnq8ilIcK(new LicenseWindow(), KBMWK0PLXIxq1L4gI6T.eZ7PqN3Bi4);
						num = 9;
						continue;
					case 6:
						zKyF8jD2VxGDrHMR6NA(FkiYIqKNBuKy5pjZT0j(UmijCGKJkFtBcOwVs6J(-1510018411 ^ -1510017159), ((LicenseInfo)SQ4UH1Dyp3CUQBYaZPl())?.ExpiryText ?? UmijCGKJkFtBcOwVs6J(--1508596617 ^ 0x59EB5D81), RxUSOmPDJGsMmCCGPPg.hbkPt9hhua));
						num = 3;
						continue;
					case 13:
						break;
					}
					break;
				}
			}
			catch (Exception ex)
			{
				int num2 = 2;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_4819ae0b3e934df4906fdac118b2ddf1 == 0)
				{
					num2 = 0;
				}
				while (true)
				{
					switch (num2)
					{
					default:
						UhUx6UPAwJdVRaVNiES.jnq8ilIcK(z2jc63fLkugS1X8Q9N.tE1kD1vbB(-1019290801 ^ -1019290309), (string)UmijCGKJkFtBcOwVs6J(0x4A43C73F ^ 0x4A43C16B), MessageBoxButton.OK, MessageBoxImage.Hand, UhUx6UPAwJdVRaVNiES.mITPXheM7o);
						num2 = 1;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_2d8e9ecb9a5b407690948cd8111b2980 != 0)
						{
							num2 = 1;
						}
						continue;
					case 2:
						Logger.Error((string)FkiYIqKNBuKy5pjZT0j(z2jc63fLkugS1X8Q9N.tE1kD1vbB(0x3B865138 ^ 0x3B865758), p1GX9NKO2aOpGdHC4Oi(ex, gjqRKtPV17ZUEQVAWxv.AdKPK0ve7w), RxUSOmPDJGsMmCCGPPg.hbkPt9hhua), ex);
						num2 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_3c2786a3c96a4c30b2aea580e623804b == 0)
						{
							num2 = 0;
						}
						continue;
					case 1:
						result = false;
						num2 = 3;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_305aaddad70a4f7fae7b0b1dbdd6ed1f != 0)
						{
							num2 = 3;
						}
						continue;
					case 3:
						break;
					}
					break;
				}
			}
			break;
		}
		return result;
	}

	protected override void OnExit(ExitEventArgs e)
	{
		int num = 1;
		int num2 = num;
		while (true)
		{
			switch (num2)
			{
			case 1:
				if (!_ownsInstanceMutex)
				{
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_76fed529d5134afd95ce65e7c04235fc == 0)
					{
						num2 = 0;
					}
					break;
				}
				goto case 2;
			default:
				PersonalWeChatHelper.Stop();
				num2 = 4;
				break;
			case 2:
				if (_instanceMutex != null)
				{
					num2 = 8;
					break;
				}
				goto default;
			case 4:
				JM98wBDQoRPbre5xkm8();
				num2 = 9;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_eb326c992e4b4c9f9cd930152533b185 == 0)
				{
					num2 = 10;
				}
				break;
			case 6:
				return;
			case 3:
				eEh7s6K10VK0392hdWN(_instanceMutex, dsixtSPyYHjKiE5k7WK.YW2PuIWLMA);
				num2 = 5;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_79220d9f20444b6c8b0df2558a43de3e == 0)
				{
					num2 = 1;
				}
				break;
			case 10:
				WinApi.CloseLogConsole();
				num2 = 9;
				break;
			case 5:
				_instanceMutex = null;
				num2 = 7;
				break;
			case 9:
				gCip1RDh0UewhoSl86u(this, e, nxtaUePjpMR4evsvQjS.FJlPlfsVSI);
				num2 = 6;
				break;
			case 8:
				try
				{
					dsixtSPyYHjKiE5k7WK.jnq8ilIcK(_instanceMutex, dsixtSPyYHjKiE5k7WK.NJxPMpn0Vw);
					int num3 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_03aacce0025f45d8a46eb3d2a6e41868 != 0)
					{
						num3 = 0;
					}
					switch (num3)
					{
					case 0:
						break;
					}
				}
				catch (ApplicationException)
				{
					int num4 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_a1f12b82c6a64d0b9c37bec032cf2c41 == 0)
					{
						num4 = 0;
					}
					switch (num4)
					{
					case 0:
						break;
					}
				}
				goto case 3;
			}
		}
	}

	internal static void ApplyTaskbarIcon(object window)
	{
		switch (1)
		{
		case 0:
			break;
		case 1:
			try
			{
				byte[] array = null;
				int num = 14;
				IntPtr intPtr3 = default(IntPtr);
				BitmapSource bitmapSource = default(BitmapSource);
				StreamResourceInfo streamResourceInfo = default(StreamResourceInfo);
				string text = default(string);
				MemoryStream memoryStream = default(MemoryStream);
				IntPtr intPtr = default(IntPtr);
				Stream stream = default(Stream);
				while (true)
				{
					switch (num)
					{
					case 10:
						return;
					case 17:
						return;
					case 19:
						return;
					case 3:
						intPtr3 = T2bB9nD552iyRRncZ9H(bitmapSource, 32);
						num = 6;
						break;
					case 4:
						streamResourceInfo = (StreamResourceInfo)xUmgCgDoHimgVwcDlXb(new Uri((string)UmijCGKJkFtBcOwVs6J(0x5B36FCC9 ^ 0x5B36FA65), UriKind.Absolute), Nr33A6PmX9dcAmtVqvR.FgaPIWHFbS);
						num = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_45ca2c3eaeb74f8484cde152ae6e3eef != 0)
						{
							num = 0;
						}
						break;
					case 2:
						array = (byte[])kOeNkPDfHt3VN4ivC5L(text, BRABe2P9Qi0rtcOslNw.OBgP8Oh6KC);
						num = 13;
						break;
					case 9:
						if (array.Length == 0)
						{
							return;
						}
						num = 15;
						break;
					case 7:
					case 15:
						memoryStream = new MemoryStream(array);
						num = 1;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_45ac76a6368943c6981dcfd9e66428ae == 0)
						{
							num = 1;
						}
						break;
					case 20:
					{
						IntPtr intPtr2 = HHTUklDba4Q3AvM0ToG(new WindowInteropHelper((Window)window), jWWNjyPkJOAr6gWqCTf.uKgEWTLvZp);
						WinApi.SendMessage(intPtr2, 128, new IntPtr(1), intPtr3);
						WinApi.SendMessage(intPtr2, 128, IntPtr.Zero, intPtr);
						RjhA6PDBQ2N782XquH5(intPtr2, -14, intPtr3);
						RjhA6PDBQ2N782XquH5(intPtr2, -34, intPtr);
						num = 17;
						break;
					}
					case 21:
						YxHkTSDtOJ5r1AueS7c(window, bitmapSource, hCWGk9PsOyARjE8TotO.WH7Pz0pE6S);
						num = 3;
						break;
					case 11:
					case 13:
					case 18:
						if (array == null)
						{
							num = 10;
							break;
						}
						goto case 9;
					case 22:
						if (fWn4mSDeJaUn8eUtohP(text, lRNPJoPvsXOpQ7Ucdon.i4TPgmP1uC))
						{
							num = 2;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_bf7dadb9c4914436a2302bdcb2bac558 != 0)
							{
								num = 2;
							}
							break;
						}
						goto case 4;
					case 5:
						TaskbarIcons.Add(intPtr3);
						num = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d34bf7edba6543039b52c92f6f825304 == 0)
						{
							num = 16;
						}
						break;
					case 14:
						text = (string)FkiYIqKNBuKy5pjZT0j(p1GX9NKO2aOpGdHC4Oi(K8RQtgDYlbEqxBZuoir(B30MVCPh7T3lxdVbgjN.d5MPYnWlQG), gjqRKtPV17ZUEQVAWxv.MW0P4ac2q7), UmijCGKJkFtBcOwVs6J(-233972548 ^ -233973208), RxUSOmPDJGsMmCCGPPg.rZAPwta2Dr);
						num = 22;
						break;
					case 12:
						try
						{
							array = new byte[hZF2VODVN65cyFqoD7N(stream, r6X9LJPF1AuhGoKm99N.Ak7PpykuJw)];
							int num4 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_0e5a04c5423148499c9b265cadee0019 != 0)
							{
								num4 = 1;
							}
							while (true)
							{
								switch (num4)
								{
								case 1:
									JH5XuNDK7Lfvr6UP8iY(stream, array, 0, array.Length, dwdfmjPJFfMGpR80D2F.D8LPOGgPLO);
									num4 = 0;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_6d61bc3aa64a47bfb8a09d13c4deb53b == 0)
									{
										num4 = 0;
									}
									continue;
								case 0:
									break;
								}
								break;
							}
						}
						finally
						{
							if (stream != null)
							{
								int num5 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_4067485f69dd42b795335d8574adc3dd != 0)
								{
									num5 = 1;
								}
								while (true)
								{
									switch (num5)
									{
									case 1:
										eEh7s6K10VK0392hdWN(stream, dsixtSPyYHjKiE5k7WK.VtIPNcTU9E);
										num5 = 0;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_0e5a04c5423148499c9b265cadee0019 == 0)
										{
											num5 = 0;
										}
										continue;
									case 0:
										break;
									}
									break;
								}
							}
						}
						goto case 11;
					case 1:
						try
						{
							bitmapSource = (BitmapSource)suH6GJDDjoXlyIwYsas(memoryStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad, PllGv4PRJ0t44YbwhGE.YtHP3uEYHl);
							int num2 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_7f545199031c4e3289afcf313932f580 != 0)
							{
								num2 = 0;
							}
							switch (num2)
							{
							case 0:
								break;
							}
						}
						finally
						{
							int num3;
							if (memoryStream == null)
							{
								num3 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_79220d9f20444b6c8b0df2558a43de3e != 0)
								{
									num3 = 0;
								}
								goto IL_03df;
							}
							goto IL_03f5;
							IL_03df:
							switch (num3)
							{
							default:
								goto end_IL_03ba;
							case 2:
								break;
							case 0:
								goto end_IL_03ba;
							case 1:
								goto end_IL_03ba;
							}
							goto IL_03f5;
							IL_03f5:
							eEh7s6K10VK0392hdWN(memoryStream, dsixtSPyYHjKiE5k7WK.VtIPNcTU9E);
							num3 = 1;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_5bb1f583c3f24addaf7d69d4facec67c == 0)
							{
								num3 = 1;
							}
							goto IL_03df;
							end_IL_03ba:;
						}
						goto case 21;
					case 16:
						TaskbarIcons.Add(intPtr);
						num = 20;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_76a1008ec43840dda25966a6b044ac77 != 0)
						{
							num = 9;
						}
						break;
					case 8:
						stream = (Stream)YcY9FKDiO9llACrhE8V(streamResourceInfo, LpbD8CPZhydZn8LGB1A.wfEP1nnN1e);
						num = 12;
						break;
					default:
						if (streamResourceInfo == null)
						{
							num = 3;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_dc0e5af6a1d1414daf07ee1c98387cef != 0)
							{
								num = 18;
							}
							break;
						}
						goto case 8;
					case 6:
						intPtr = CreateHIcon(bitmapSource, 16);
						num = 5;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_b0daf8cde81544818533e86ea9e2d2be == 0)
						{
							num = 5;
						}
						break;
					}
				}
			}
			catch (Exception ex)
			{
				int num6 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_4819ae0b3e934df4906fdac118b2ddf1 != 0)
				{
					num6 = 0;
				}
				while (true)
				{
					switch (num6)
					{
					case 1:
						return;
					}
					Logger.Warning((string)FkiYIqKNBuKy5pjZT0j(UmijCGKJkFtBcOwVs6J(0x62E15CB8 ^ 0x62E15A4A), p1GX9NKO2aOpGdHC4Oi(ex, gjqRKtPV17ZUEQVAWxv.AdKPK0ve7w), RxUSOmPDJGsMmCCGPPg.hbkPt9hhua));
					num6 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d604b1379fb540e5b6d79e791c2f5762 != 0)
					{
						num6 = 1;
					}
				}
			}
		}
	}

	private static void SetClassIcon(IntPtr hwnd, int index, IntPtr icon)
	{
		int num = 1;
		int num2 = num;
		while (true)
		{
			switch (num2)
			{
			case 1:
				if (udk34VD0ZQ24WV8rtsL(wxuXMNEPBwAMyhQ4pNJ.TV1EEDlsPj) == 8)
				{
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_f8d69a0d83e2420b83127060d822cafd != 0)
					{
						num2 = 0;
					}
					break;
				}
				SetClassLong32(hwnd, index, znfhe1EbxKHQdEgRmN9.jnq8ilIcK(ref icon, znfhe1EbxKHQdEgRmN9.i2UExQhs3q));
				num2 = 2;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_cb5bc3d191fc4bc3b6ad04816cd3def1 == 0)
				{
					num2 = 2;
				}
				break;
			default:
				SetClassLongPtr64(hwnd, index, icon);
				num2 = 1;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_5db1faeadc7a4faeaddf4f71b089648f == 0)
				{
					num2 = 3;
				}
				break;
			case 2:
				return;
			case 3:
				return;
			}
		}
	}

	private static IntPtr CreateHIcon(object source, int size)
	{
		int num = 8;
		BitmapInfo bmi = default(BitmapInfo);
		BitmapInfo bitmapInfo = default(BitmapInfo);
		IconInfo iconInfo2 = default(IconInfo);
		IconInfo iconInfo = default(IconInfo);
		IntPtr intPtr2 = default(IntPtr);
		DrawingVisual drawingVisual = default(DrawingVisual);
		byte[] array = default(byte[]);
		IntPtr bits = default(IntPtr);
		IntPtr intPtr = default(IntPtr);
		DrawingContext drawingContext = default(DrawingContext);
		while (true)
		{
			int num2 = num;
			while (true)
			{
				switch (num2)
				{
				default:
					bmi = bitmapInfo;
					num2 = 16;
					continue;
				case 13:
					iconInfo2 = iconInfo;
					num2 = 12;
					continue;
				case 2:
					iconInfo.Color = intPtr2;
					num = 9;
					break;
				case 5:
					bitmapInfo.Height = -size;
					num2 = 1;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c09e27dcabbb4f6484836a4e1fd5da8e == 0)
					{
						num2 = 1;
					}
					continue;
				case 3:
				{
					RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(size, size, 96.0, 96.0, SKeEsiDTGjILvKfU8YX(cBwIvCEhW99O0JymvMv.mioEY9KDm4));
					MKZCY5DGpOoxAj2AjgJ(renderTargetBitmap, drawingVisual, R8hKf9Ee60tUNEjki78.YbrEfjVppu);
					int num5 = size * 4;
					array = new byte[num5 * size];
					PiFfgOD69pOdKfIqmLI(renderTargetBitmap, array, num5, 0, orQ2BkEobw5VwQ29uWN.UEkEih5D6B);
					num2 = 10;
					continue;
				}
				case 16:
					intPtr2 = CreateDIBSection(IntPtr.Zero, ref bmi, 0u, out bits, IntPtr.Zero, 0u);
					num2 = 19;
					continue;
				case 18:
					intPtr = CreateBitmap(size, size, 1u, 1u, IntPtr.Zero);
					num2 = 17;
					continue;
				case 1:
					bitmapInfo.Planes = 1;
					num2 = 11;
					continue;
				case 15:
					bitmapInfo.Width = size;
					num2 = 5;
					continue;
				case 9:
					iconInfo.Mask = intPtr;
					num2 = 13;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c1db6875e3a1467ca73ac64ce11e3ed4 == 0)
					{
						num2 = 7;
					}
					continue;
				case 7:
					drawingContext = (DrawingContext)rOH0csDAljKAvXfloRk(drawingVisual, a1SwYsEylF5x4EA12iS.ywOE2noIKB);
					num2 = 12;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_db887f4a099249249dfc8abe84bd3ac9 == 0)
					{
						num2 = 14;
					}
					continue;
				case 20:
					iconInfo.IsIcon = true;
					num2 = 2;
					continue;
				case 19:
					aODBSMDa9hAk1DB5Ij2(array, 0, bits, array.Length, HfUQN9E5FSEebq0t6rg.e6QEBdZsyR);
					num2 = 18;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_2879e5bb2d3341f08a6f59d9f884899e != 0)
					{
						num2 = 5;
					}
					continue;
				case 12:
				{
					IntPtr result = CreateIconIndirect(ref iconInfo2);
					DeleteObject(intPtr2);
					DeleteObject(intPtr);
					return result;
				}
				case 4:
					bitmapInfo.SizeImage = array.Length;
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d34bf7edba6543039b52c92f6f825304 != 0)
					{
						num2 = 0;
					}
					continue;
				case 11:
					bitmapInfo.BitCount = 32;
					num = 4;
					break;
				case 8:
					drawingVisual = new DrawingVisual();
					num2 = 3;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_eb326c992e4b4c9f9cd930152533b185 == 0)
					{
						num2 = 7;
					}
					continue;
				case 14:
					try
					{
						IoxNaBDXCSFn2usWnf8(drawingContext, source, new Rect(0.0, 0.0, size, size), v5NnRdEU2o0tou5bDu7.x9bEQ8Qsr0);
						int num3 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_7f545199031c4e3289afcf313932f580 == 0)
						{
							num3 = 0;
						}
						switch (num3)
						{
						case 0:
							break;
						}
					}
					finally
					{
						int num4;
						if (drawingContext == null)
						{
							num4 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_23f2079970f0474d9a38aefb1de2f52d == 0)
							{
								num4 = 0;
							}
							goto IL_02fc;
						}
						goto IL_0321;
						IL_02fc:
						switch (num4)
						{
						default:
							goto end_IL_02d7;
						case 0:
							goto end_IL_02d7;
						case 2:
							break;
						case 1:
							goto end_IL_02d7;
						}
						goto IL_0321;
						IL_0321:
						eEh7s6K10VK0392hdWN(drawingContext, dsixtSPyYHjKiE5k7WK.VtIPNcTU9E);
						num4 = 1;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_bf7dadb9c4914436a2302bdcb2bac558 != 0)
						{
							num4 = 1;
						}
						goto IL_02fc;
						end_IL_02d7:;
					}
					goto case 3;
				case 6:
					bitmapInfo.Size = N08AKIDHkKxJfVn5eWp(gm8is3DCZngOtqyZtwt(typeof(BitmapInfo).TypeHandle, THNTxYEVPVbLybtAqi1.PLCEKy6KBP), RNf552EDJrbs9F9iSGt.niNEts2C7C);
					num2 = 15;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d34bf7edba6543039b52c92f6f825304 != 0)
					{
						num2 = 8;
					}
					continue;
				case 17:
					iconInfo = default(IconInfo);
					num2 = 20;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1fdc4536a75247b19a0801dd70bf34e9 != 0)
					{
						num2 = 18;
					}
					continue;
				case 10:
					bitmapInfo = default(BitmapInfo);
					num2 = 6;
					continue;
				}
				break;
			}
		}
	}

	[DllImport("user32.dll")]
	private static extern IntPtr CreateIconIndirect(ref IconInfo iconInfo);

	[DllImport("gdi32.dll")]
	private static extern IntPtr CreateDIBSection(IntPtr hdc, ref BitmapInfo bmi, uint usage, out IntPtr bits, IntPtr section, uint offset);

	[DllImport("gdi32.dll")]
	private static extern IntPtr CreateBitmap(int width, int height, uint planes, uint bitCount, IntPtr bits);

	[DllImport("gdi32.dll")]
	private static extern bool DeleteObject(IntPtr handle);

	[DllImport("user32.dll", EntryPoint = "SetClassLong")]
	private static extern int SetClassLong32(IntPtr hWnd, int index, int newLong);

	[DllImport("user32.dll", EntryPoint = "SetClassLongPtr")]
	private static extern IntPtr SetClassLongPtr64(IntPtr hWnd, int index, IntPtr newLong);

	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	[DebuggerNonUserCode]
	[STAThread]
	public static void Main()
	{
		int num = 2;
		int num2 = num;
		while (true)
		{
			switch (num2)
			{
			default:
				return;
			case 1:
				kfHGSgPnOQlgJ5xXCSg.jnq8ilIcK(new App(), kfHGSgPnOQlgJ5xXCSg.suiE0PdUut);
				num2 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_cc95e336569e4097a8a610a6e511075d == 0)
				{
					num2 = 0;
				}
				break;
			case 2:
				I6976P597uOR8TGW3o.kLjw4iIsCLsZtxc4lksN0j();
				num2 = 1;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_aaf9213cc3c34825a5d331618f885f65 == 0)
				{
					num2 = 0;
				}
				break;
			case 0:
				return;
			}
		}
	}

	public App()
	{
		GS2H4HDS3SnuqAOnNEF();
		base..ctor();
		int num = 0;
		if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_aaf9213cc3c34825a5d331618f885f65 != 0)
		{
			num = 0;
		}
		switch (num)
		{
		case 0:
			break;
		}
	}

	static App()
	{
		int num = 1;
		int num2 = num;
		while (true)
		{
			switch (num2)
			{
			case 1:
				oeZIy4DnHNfP2eg08bf();
				num2 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d86987deeedc4dab9e9f358fe117cd1f == 0)
				{
					num2 = 0;
				}
				break;
			case 3:
				kuIEqmDc8JURK4pdLEf();
				num2 = 2;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c09e27dcabbb4f6484836a4e1fd5da8e == 0)
				{
					num2 = 2;
				}
				break;
			case 5:
				return;
			default:
				oeZIy4DnHNfP2eg08bf();
				num2 = 4;
				break;
			case 2:
				TaskbarIcons = new List<IntPtr>();
				num2 = 5;
				break;
			case 4:
				GS2H4HDS3SnuqAOnNEF();
				num2 = 3;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c09e27dcabbb4f6484836a4e1fd5da8e == 0)
				{
					num2 = 2;
				}
				break;
			}
		}
	}

	internal static void yq3PmpKZSbE0PBC5dhS(object P_0, ShutdownMode P_1, object P_2)
	{
		mui3PqPbTA6nXByRbay.jnq8ilIcK(P_0, P_1, (mui3PqPbTA6nXByRbay)P_2);
	}

	internal static void eEh7s6K10VK0392hdWN(object P_0, object P_1)
	{
		dsixtSPyYHjKiE5k7WK.jnq8ilIcK(P_0, (dsixtSPyYHjKiE5k7WK)P_1);
	}

	internal static void LifEQmKFgL45NqBrVVa(object P_0, object P_1, object P_2)
	{
		phjGGaPUNxAwt4XffW9.jnq8ilIcK(P_0, (DispatcherUnhandledExceptionEventHandler)P_1, (phjGGaPUNxAwt4XffW9)P_2);
	}

	internal static void DEAWsdKpgjsP6v6aBOK()
	{
		BundledNativeFiles.EnsureExtracted();
	}

	internal static object UmijCGKJkFtBcOwVs6J(int P_0)
	{
		return z2jc63fLkugS1X8Q9N.tE1kD1vbB(P_0);
	}

	internal static object p1GX9NKO2aOpGdHC4Oi(object P_0, object P_1)
	{
		return gjqRKtPV17ZUEQVAWxv.jnq8ilIcK(P_0, (gjqRKtPV17ZUEQVAWxv)P_1);
	}

	internal static object FkiYIqKNBuKy5pjZT0j(object P_0, object P_1, object P_2)
	{
		return RxUSOmPDJGsMmCCGPPg.jnq8ilIcK((string)P_0, (string)P_1, (RxUSOmPDJGsMmCCGPPg)P_2);
	}

	internal static void AICxWLKRsH96bvEjecu(object P_0, object P_1, object P_2)
	{
		D6WA9OP5vy8OaFUtOlI.jnq8ilIcK(P_0, (Window)P_1, (D6WA9OP5vy8OaFUtOlI)P_2);
	}

	internal static MessageBoxResult rn2haAK3iMNigdNU5PJ(object P_0, object P_1, MessageBoxButton P_2, MessageBoxImage P_3, object P_4)
	{
		return UhUx6UPAwJdVRaVNiES.jnq8ilIcK((string)P_0, (string)P_1, P_2, P_3, (UhUx6UPAwJdVRaVNiES)P_4);
	}

	internal static bool tA54rPKmtTTbQEO5dmm()
	{
		return UYOh93K8Z4uhmZXNRuS == null;
	}

	internal static App ifxy4ZKIyidTPqfGLWp()
	{
		return UYOh93K8Z4uhmZXNRuS;
	}

	internal static bool Pc64WNKsvUfgLFWWrPa(object P_0, int P_1, bool P_2, object P_3)
	{
		return t8mABAPT4ikBh9MWU7D.jnq8ilIcK(P_0, P_1, P_2, (t8mABAPT4ikBh9MWU7D)P_3);
	}

	internal static void YHP7fnKzJWlT4HrwJKF()
	{
		ActivateExistingInstance();
	}

	internal static object goJuFIDWZWfg4I1riSX(object P_0)
	{
		return bx01EUP6YjantAQO76e.jnq8ilIcK((bx01EUP6YjantAQO76e)P_0);
	}

	internal static object DLVdT5DP2fBntSnSIuu(object P_0, object P_1)
	{
		return vCD6HcPaOyR6Koj64nc.jnq8ilIcK((string)P_0, (vCD6HcPaOyR6Koj64nc)P_1);
	}

	internal static int i75S8uDEbaBtFDVP2Ek(object P_0, object P_1)
	{
		return kfHGSgPnOQlgJ5xXCSg.jnq8ilIcK(P_0, (kfHGSgPnOQlgJ5xXCSg)P_1);
	}

	internal static IntPtr HHTUklDba4Q3AvM0ToG(object P_0, object P_1)
	{
		return jWWNjyPkJOAr6gWqCTf.jnq8ilIcK(P_0, (jWWNjyPkJOAr6gWqCTf)P_1);
	}

	internal static bool SfG9TaDxpCdlilvHnDe(IntPtr P_0, IntPtr P_1, object P_2)
	{
		return E6GRu8P7P9F6qI8rcqV.jnq8ilIcK(P_0, P_1, (E6GRu8P7P9F6qI8rcqV)P_2);
	}

	internal static object SQ4UH1Dyp3CUQBYaZPl()
	{
		return LicenseManager.Current;
	}

	internal static void zKyF8jD2VxGDrHMR6NA(object P_0)
	{
		Logger.Info((string)P_0);
	}

	internal static void T51vu1DUMMGfxBacxPp(object P_0)
	{
		Logger.Warning((string)P_0);
	}

	internal static void JM98wBDQoRPbre5xkm8()
	{
		Logger.Shutdown();
	}

	internal static void gCip1RDh0UewhoSl86u(object P_0, object P_1, object P_2)
	{
		nxtaUePjpMR4evsvQjS.jnq8ilIcK(P_0, (ExitEventArgs)P_1, (nxtaUePjpMR4evsvQjS)P_2);
	}

	internal static object K8RQtgDYlbEqxBZuoir(object P_0)
	{
		return B30MVCPh7T3lxdVbgjN.jnq8ilIcK((B30MVCPh7T3lxdVbgjN)P_0);
	}

	internal static bool fWn4mSDeJaUn8eUtohP(object P_0, object P_1)
	{
		return lRNPJoPvsXOpQ7Ucdon.jnq8ilIcK((string)P_0, (lRNPJoPvsXOpQ7Ucdon)P_1);
	}

	internal static object kOeNkPDfHt3VN4ivC5L(object P_0, object P_1)
	{
		return BRABe2P9Qi0rtcOslNw.jnq8ilIcK((string)P_0, (BRABe2P9Qi0rtcOslNw)P_1);
	}

	internal static object xUmgCgDoHimgVwcDlXb(object P_0, object P_1)
	{
		return Nr33A6PmX9dcAmtVqvR.jnq8ilIcK((Uri)P_0, (Nr33A6PmX9dcAmtVqvR)P_1);
	}

	internal static object YcY9FKDiO9llACrhE8V(object P_0, object P_1)
	{
		return LpbD8CPZhydZn8LGB1A.jnq8ilIcK(P_0, (LpbD8CPZhydZn8LGB1A)P_1);
	}

	internal static long hZF2VODVN65cyFqoD7N(object P_0, object P_1)
	{
		return r6X9LJPF1AuhGoKm99N.jnq8ilIcK(P_0, (r6X9LJPF1AuhGoKm99N)P_1);
	}

	internal static int JH5XuNDK7Lfvr6UP8iY(object P_0, object P_1, int P_2, int P_3, object P_4)
	{
		return dwdfmjPJFfMGpR80D2F.jnq8ilIcK(P_0, (byte[])P_1, P_2, P_3, (dwdfmjPJFfMGpR80D2F)P_4);
	}

	internal static object suH6GJDDjoXlyIwYsas(object P_0, BitmapCreateOptions P_1, BitmapCacheOption P_2, object P_3)
	{
		return PllGv4PRJ0t44YbwhGE.jnq8ilIcK((Stream)P_0, P_1, P_2, (PllGv4PRJ0t44YbwhGE)P_3);
	}

	internal static void YxHkTSDtOJ5r1AueS7c(object P_0, object P_1, object P_2)
	{
		hCWGk9PsOyARjE8TotO.jnq8ilIcK(P_0, (ImageSource)P_1, (hCWGk9PsOyARjE8TotO)P_2);
	}

	internal static IntPtr T2bB9nD552iyRRncZ9H(object P_0, int size)
	{
		return CreateHIcon(P_0, size);
	}

	internal static void RjhA6PDBQ2N782XquH5(IntPtr hwnd, int index, IntPtr icon)
	{
		SetClassIcon(hwnd, index, icon);
	}

	internal static int udk34VD0ZQ24WV8rtsL(object P_0)
	{
		return wxuXMNEPBwAMyhQ4pNJ.jnq8ilIcK((wxuXMNEPBwAMyhQ4pNJ)P_0);
	}

	internal static object rOH0csDAljKAvXfloRk(object P_0, object P_1)
	{
		return a1SwYsEylF5x4EA12iS.jnq8ilIcK(P_0, (a1SwYsEylF5x4EA12iS)P_1);
	}

	internal static void IoxNaBDXCSFn2usWnf8(object P_0, object P_1, Rect P_2, object P_3)
	{
		v5NnRdEU2o0tou5bDu7.jnq8ilIcK(P_0, (ImageSource)P_1, P_2, (v5NnRdEU2o0tou5bDu7)P_3);
	}

	internal static PixelFormat SKeEsiDTGjILvKfU8YX(object P_0)
	{
		return cBwIvCEhW99O0JymvMv.jnq8ilIcK((cBwIvCEhW99O0JymvMv)P_0);
	}

	internal static void MKZCY5DGpOoxAj2AjgJ(object P_0, object P_1, object P_2)
	{
		R8hKf9Ee60tUNEjki78.jnq8ilIcK(P_0, (Visual)P_1, (R8hKf9Ee60tUNEjki78)P_2);
	}

	internal static void PiFfgOD69pOdKfIqmLI(object P_0, object P_1, int P_2, int P_3, object P_4)
	{
		orQ2BkEobw5VwQ29uWN.jnq8ilIcK(P_0, (Array)P_1, P_2, P_3, (orQ2BkEobw5VwQ29uWN)P_4);
	}

	internal static Type gm8is3DCZngOtqyZtwt(RuntimeTypeHandle P_0, object P_1)
	{
		return THNTxYEVPVbLybtAqi1.jnq8ilIcK(P_0, (THNTxYEVPVbLybtAqi1)P_1);
	}

	internal static int N08AKIDHkKxJfVn5eWp(Type P_0, object P_1)
	{
		return RNf552EDJrbs9F9iSGt.jnq8ilIcK(P_0, (RNf552EDJrbs9F9iSGt)P_1);
	}

	internal static void aODBSMDa9hAk1DB5Ij2(object P_0, int P_1, IntPtr P_2, int P_3, object P_4)
	{
		HfUQN9E5FSEebq0t6rg.jnq8ilIcK((byte[])P_0, P_1, P_2, P_3, (HfUQN9E5FSEebq0t6rg)P_4);
	}

	internal static void GS2H4HDS3SnuqAOnNEF()
	{
		z2jc63fLkugS1X8Q9N.rQ19jmJab();
	}

	internal static void oeZIy4DnHNfP2eg08bf()
	{
		z2jc63fLkugS1X8Q9N.TrhALsn43();
	}

	internal static void kuIEqmDc8JURK4pdLEf()
	{
		I6976P597uOR8TGW3o.kLjw4iIsCLsZtxc4lksN0j();
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.2.0.10050' (yours is '9.1.0.7988')
