using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;
using Robot.Utils;
using SG9KiyIbtdgGDf12qr;

namespace WeComConsoleServer.Core;

internal static class WeComBridgeRuntime
{
	private enum AttachKind
	{
		Attached,
		Already,
		NeedsInject,
		Failed
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct ModuleEntry32
	{
		public uint dwSize;

		public uint th32ModuleID;

		public uint th32ProcessID;

		public uint GlblcntUsage;

		public uint ProccntUsage;

		public IntPtr modBaseAddr;

		public uint modBaseSize;

		public IntPtr hModule;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string szModule;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string szExePath;
	}

	private struct HandleEntry32
	{
		public int UniqueProcessId;

		public int HandleValue;

		public uint GrantedAccess;

		public ushort CreatorBackTraceIndex;

		public ushort ObjectTypeIndex;

		public uint HandleAttributes;

		public uint Reserved;

		public uint Unknown;
	}

	private struct HandleEntry64
	{
		public long Object;

		public long UniqueProcessId;

		public long HandleValue;

		public uint GrantedAccess;

		public ushort CreatorBackTraceIndex;

		public ushort ObjectTypeIndex;

		public uint HandleAttributes;

		public uint Reserved;
	}

	private struct ObjectName
	{
		public ushort Length;

		public ushort MaximumLength;

		public IntPtr Buffer;
	}

	private struct TokenPrivileges
	{
		public int Count;

		public long Luid;

		public int Attributes;
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class <>c
	{
		public static readonly <>c <>9;

		public static TimerCallback <>9__23_0;

		internal static <>c hCcvj1kZQoBmVl0PmBB;

		static <>c()
		{
			int num = 2;
			int num2 = num;
			while (true)
			{
				switch (num2)
				{
				default:
					<>9 = new <>c();
					num2 = 3;
					break;
				case 2:
					KRViBhkpwWDGysHSQOJ();
					num2 = 1;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_13470d6aa0454f7eb0318dc44f893926 == 0)
					{
						num2 = 1;
					}
					break;
				case 3:
					return;
				case 1:
					zktjF1kJRJVbOoXMAqk();
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_5bb1f583c3f24addaf7d69d4facec67c == 0)
					{
						num2 = 0;
					}
					break;
				}
			}
		}

		public <>c()
		{
			zktjF1kJRJVbOoXMAqk();
			base..ctor();
			int num = 0;
			if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_84f159339c0c4d909a5aafadeb20c94b == 0)
			{
				num = 0;
			}
			switch (num)
			{
			case 0:
				break;
			}
		}

		internal void <EnsureWatchTimer>b__23_0(object _)
		{
			int num = 2;
			int num2 = num;
			IEnumerator<BridgeSession> enumerator = default(IEnumerator<BridgeSession>);
			BridgeSession current = default(BridgeSession);
			while (true)
			{
				switch (num2)
				{
				default:
					return;
				case 0:
					return;
				case 1:
					try
					{
						while (true)
						{
							IL_0184:
							int num3;
							if (!G0OQecrWEciUt2KuMoG(enumerator, nsJ9LebXYwUJAHxJYqI.tcqbTnwQjH))
							{
								num3 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_93e9b0bbc1fe4ae1a0fca3879b36d9a6 != 0)
								{
									num3 = 0;
								}
								goto IL_0044;
							}
							goto IL_0081;
							IL_0040:
							int num4;
							num3 = num4;
							goto IL_0044;
							IL_0044:
							while (true)
							{
								switch (num3)
								{
								default:
									return;
								case 0:
									return;
								case 3:
									num4 = 5;
									goto IL_0040;
								case 6:
									break;
								case 1:
									num3 = 2;
									continue;
								case 8:
									if (current.Closed != null)
									{
										num3 = 7;
										continue;
									}
									goto case 1;
								case 2:
									try
									{
										aMU2i5y03HSWa4qUbCd.jnq8ilIcK(current.Pid, aMU2i5y03HSWa4qUbCd.gVwyAsDh1k);
										int num7 = 0;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_aaf9213cc3c34825a5d331618f885f65 == 0)
										{
											num7 = 0;
										}
										switch (num7)
										{
										case 0:
											break;
										}
									}
									catch
									{
										int num8 = 0;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_529db4a9032c417dbe74a080d04d36c2 == 0)
										{
											num8 = 1;
										}
										while (true)
										{
											switch (num8)
											{
											case 1:
												ddpshjkOcAIqkgL8gte(current, true);
												num8 = 0;
												if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_cb5bc3d191fc4bc3b6ad04816cd3def1 == 0)
												{
													num8 = 0;
												}
												continue;
											}
											break;
										}
										goto IL_0184;
									}
									goto case 3;
								case 4:
								case 7:
									goto IL_0184;
								case 5:
									try
									{
										qqMuFQkNHjRyI9wlQ39(current, false);
										int num5 = 0;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_cc95e336569e4097a8a610a6e511075d == 0)
										{
											num5 = 0;
										}
										while (true)
										{
											switch (num5)
											{
											default:
												ec2O1OkRnFs6hQ4Z9md(current);
												num5 = 0;
												if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_305aaddad70a4f7fae7b0b1dbdd6ed1f == 0)
												{
													num5 = 1;
												}
												continue;
											case 1:
												break;
											}
											break;
										}
									}
									catch (Exception ex)
									{
										int num6 = 1;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_529db4a9032c417dbe74a080d04d36c2 != 0)
										{
											num6 = 0;
										}
										while (true)
										{
											switch (num6)
											{
											case 1:
												QSJsW5kzQ4awbFka53h(kPikrOkswXT6VpyWFyV(z2jc63fLkugS1X8Q9N.tE1kD1vbB(-409114785 ^ -409126553), gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref current.ClientId, gGrLFhEdofbUymLMOb3.RREELfe2hN), z2jc63fLkugS1X8Q9N.tE1kD1vbB(0x2BF9058 ^ 0x2BFA802), X43Rxgk3oWnXBmQx0TW(ex, gjqRKtPV17ZUEQVAWxv.AdKPK0ve7w), m4SFBaEZmre5qY3dKfT.hGFE1HacV8));
												num6 = 0;
												if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_b4d4669a918b4b838c27ddb986baa9b6 == 0)
												{
													num6 = 0;
												}
												continue;
											case 0:
												break;
											}
											break;
										}
									}
									goto IL_0184;
								}
								break;
							}
							goto IL_0081;
							IL_0081:
							current = enumerator.Current;
							num4 = 8;
							goto IL_0040;
						}
					}
					finally
					{
						int num9;
						if (enumerator == null)
						{
							num9 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d91865d38b6c449295c8f801546a735d == 0)
							{
								num9 = 0;
							}
							goto IL_02e5;
						}
						goto IL_02fb;
						IL_02e5:
						switch (num9)
						{
						default:
							goto end_IL_02c0;
						case 2:
							break;
						case 0:
							goto end_IL_02c0;
						case 1:
							goto end_IL_02c0;
						}
						goto IL_02fb;
						IL_02fb:
						FZWfHnrPUIVT6r8KejK(enumerator, dsixtSPyYHjKiE5k7WK.VtIPNcTU9E);
						num9 = 1;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_37f38109cd8f46f5b0100026922644f5 == 0)
						{
							num9 = 0;
						}
						goto IL_02e5;
						end_IL_02c0:;
					}
				case 2:
					enumerator = ByClient.Values.GetEnumerator();
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_90f58c7ffb21444ca88319aa26ca6aa0 != 0)
					{
						num2 = 1;
					}
					break;
				}
			}
		}

		internal static void KRViBhkpwWDGysHSQOJ()
		{
			z2jc63fLkugS1X8Q9N.TrhALsn43();
		}

		internal static void zktjF1kJRJVbOoXMAqk()
		{
			z2jc63fLkugS1X8Q9N.rQ19jmJab();
		}

		internal static bool Yrbsjjk1Z4BoMhnk0TF()
		{
			return hCcvj1kZQoBmVl0PmBB == null;
		}

		internal static <>c jBbyg9kFVTsaYyvghu3()
		{
			return hCcvj1kZQoBmVl0PmBB;
		}

		internal static void ddpshjkOcAIqkgL8gte(object P_0, bool notify)
		{
			CloseSession(P_0, notify);
		}

		internal static void qqMuFQkNHjRyI9wlQ39(object P_0, bool initial)
		{
			RefreshLogin(P_0, initial);
		}

		internal static void ec2O1OkRnFs6hQ4Z9md(object P_0)
		{
			PullAccountEvents(P_0);
		}

		internal static object X43Rxgk3oWnXBmQx0TW(object P_0, object P_1)
		{
			return gjqRKtPV17ZUEQVAWxv.jnq8ilIcK(P_0, (gjqRKtPV17ZUEQVAWxv)P_1);
		}

		internal static object kPikrOkswXT6VpyWFyV(object P_0, object P_1, object P_2, object P_3, object P_4)
		{
			return m4SFBaEZmre5qY3dKfT.jnq8ilIcK((string)P_0, (string)P_1, (string)P_2, (string)P_3, (m4SFBaEZmre5qY3dKfT)P_4);
		}

		internal static void QSJsW5kzQ4awbFka53h(object P_0)
		{
			Logger.Warning((string)P_0);
		}

		internal static bool G0OQecrWEciUt2KuMoG(object P_0, object P_1)
		{
			return nsJ9LebXYwUJAHxJYqI.jnq8ilIcK(P_0, (nsJ9LebXYwUJAHxJYqI)P_1);
		}

		internal static void FZWfHnrPUIVT6r8KejK(object P_0, object P_1)
		{
			dsixtSPyYHjKiE5k7WK.jnq8ilIcK(P_0, (dsixtSPyYHjKiE5k7WK)P_1);
		}
	}

	[CompilerGenerated]
	private sealed class <>c__DisplayClass35_0
	{
		public BridgeSession session;

		private static <>c__DisplayClass35_0 YlT7o6rEkBS6GwC7bvs;

		public <>c__DisplayClass35_0()
		{
			pgpdwvry6cF3YdxIGid();
			base..ctor();
			int num = 0;
			if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d34bf7edba6543039b52c92f6f825304 != 0)
			{
				num = 0;
			}
			switch (num)
			{
			case 0:
				break;
			}
		}

		internal void <AcceptLoop>b__0(object _)
		{
			int num = 3;
			int num2 = num;
			<>c__DisplayClass35_1 <>c__DisplayClass35_ = default(<>c__DisplayClass35_1);
			while (true)
			{
				switch (num2)
				{
				case 4:
					return;
				case 6:
					ojygHBrUNJ5nrB2vG5C(new WaitCallback(<>c__DisplayClass35_.<AcceptLoop>b__1), X4U9kVhZ0DxxCJRjVmi.Tm8h1Vtbj3);
					num2 = 1;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_7dea470b4c15478bbfe1539429f98b80 == 0)
					{
						num2 = 1;
					}
					break;
				case 1:
				case 3:
					if (session.Closed != null)
					{
						num2 = 4;
						break;
					}
					goto case 2;
				case 2:
					<>c__DisplayClass35_ = new <>c__DisplayClass35_1();
					num2 = 1;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_a8ee04dc577b44a4b973e7119af8874c != 0)
					{
						num2 = 5;
					}
					break;
				case 5:
					<>c__DisplayClass35_.CS$<>8__locals1 = this;
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_291da815e7d24e5fad1c5f19ff1c2dd7 == 0)
					{
						num2 = 0;
					}
					break;
				default:
					try
					{
						<>c__DisplayClass35_.client = (TcpClient)L6dj2Ur2tL81o7XFx9L(session.Listener, Fu4m99YAmtAMDTqt5CV.aQrYXIJ4B6);
						int num3 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_23f2079970f0474d9a38aefb1de2f52d == 0)
						{
							num3 = 0;
						}
						switch (num3)
						{
						case 0:
							break;
						}
					}
					catch
					{
						int num4 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_23f2079970f0474d9a38aefb1de2f52d == 0)
						{
							num4 = 0;
						}
						switch (num4)
						{
						case 0:
							break;
						}
						return;
					}
					goto case 6;
				}
			}
		}

		static <>c__DisplayClass35_0()
		{
			z2jc63fLkugS1X8Q9N.TrhALsn43();
		}

		internal static void pgpdwvry6cF3YdxIGid()
		{
			z2jc63fLkugS1X8Q9N.rQ19jmJab();
		}

		internal static bool YWqjvDrbKvg6r3wTUI8()
		{
			return YlT7o6rEkBS6GwC7bvs == null;
		}

		internal static <>c__DisplayClass35_0 iyNtkqrxJ0SCTFlElYr()
		{
			return YlT7o6rEkBS6GwC7bvs;
		}

		internal static object L6dj2Ur2tL81o7XFx9L(object P_0, object P_1)
		{
			return Fu4m99YAmtAMDTqt5CV.jnq8ilIcK(P_0, (Fu4m99YAmtAMDTqt5CV)P_1);
		}

		internal static bool ojygHBrUNJ5nrB2vG5C(object P_0, object P_1)
		{
			return X4U9kVhZ0DxxCJRjVmi.jnq8ilIcK((WaitCallback)P_0, (X4U9kVhZ0DxxCJRjVmi)P_1);
		}
	}

	[CompilerGenerated]
	private sealed class <>c__DisplayClass35_1
	{
		public TcpClient client;

		public <>c__DisplayClass35_0 CS$<>8__locals1;

		private static <>c__DisplayClass35_1 DgYTL3rQMEqGQ0n36l7;

		public <>c__DisplayClass35_1()
		{
			TcigtUreKsQj3yS5Se3();
			base..ctor();
			int num = 0;
			if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_0444cf56387747139b0edc577cc00a72 == 0)
			{
				num = 0;
			}
			switch (num)
			{
			case 0:
				break;
			}
		}

		internal void <AcceptLoop>b__1(object __)
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
					ODDtvJrfTyl7AYWmGvH(CS$<>8__locals1.session, client);
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_90f58c7ffb21444ca88319aa26ca6aa0 == 0)
					{
						num2 = 0;
					}
					break;
				case 0:
					return;
				}
			}
		}

		static <>c__DisplayClass35_1()
		{
			rgn5e9ro6idlH242gfQ();
		}

		internal static void TcigtUreKsQj3yS5Se3()
		{
			z2jc63fLkugS1X8Q9N.rQ19jmJab();
		}

		internal static bool nchIRPrhUVPgy5uM9so()
		{
			return DgYTL3rQMEqGQ0n36l7 == null;
		}

		internal static <>c__DisplayClass35_1 FHr2n2rYZNEnK5TyALO()
		{
			return DgYTL3rQMEqGQ0n36l7;
		}

		internal static void ODDtvJrfTyl7AYWmGvH(object P_0, object P_1)
		{
			Serve(P_0, P_1);
		}

		internal static void rgn5e9ro6idlH242gfQ()
		{
			z2jc63fLkugS1X8Q9N.TrhALsn43();
		}
	}

	[CompilerGenerated]
	private sealed class <>c__DisplayClass36_0
	{
		public TcpClient client;

		public NetworkStream stream;

		public object writeLock;

		private static <>c__DisplayClass36_0 l8vALLritmsktqacy6e;

		public <>c__DisplayClass36_0()
		{
			osqCdNrD8Jr2kPC71j3();
			base..ctor();
			int num = 0;
			if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_84f159339c0c4d909a5aafadeb20c94b == 0)
			{
				num = 0;
			}
			switch (num)
			{
			case 0:
				break;
			}
		}

		internal void <Serve>b__0(object ___)
		{
			switch (1)
			{
			case 1:
				try
				{
					byte[] array = Frame(KGAeSJrtinZjZteuiF6(0x73CC8E5C ^ 0x73CCD000));
					int num = 3;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_0444cf56387747139b0edc577cc00a72 == 0)
					{
						num = 3;
					}
					object obj = default(object);
					bool flag = default(bool);
					while (true)
					{
						switch (num)
						{
						case 3:
							obj = writeLock;
							num = 2;
							continue;
						case 2:
							flag = false;
							num = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_a152e1a10f374e159091a60e0cb4b256 != 0)
							{
								num = 0;
							}
							continue;
						case 1:
							return;
						}
						try
						{
							BXDm5m2zddwJCBpuXOu.jnq8ilIcK(obj, ref flag, BXDm5m2zddwJCBpuXOu.wEHUWvu4fJ);
							int num2 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_dd83a9ff75d94001819d80b24ef9a897 != 0)
							{
								num2 = 0;
							}
							while (true)
							{
								switch (num2)
								{
								case 1:
									return;
								}
								DVsgufr5hFn9BpAn4NQ(stream, array, 0, array.Length, JQCHYDU4aG4wZATQALx.B7kUwE1Vtx);
								num2 = 1;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_305aaddad70a4f7fae7b0b1dbdd6ed1f != 0)
								{
									num2 = 1;
								}
							}
						}
						finally
						{
							int num3;
							if (!flag)
							{
								num3 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_a4eb1fc2846b481197982b0faf1ae1eb == 0)
								{
									num3 = 0;
								}
								goto IL_0115;
							}
							goto IL_014a;
							IL_0115:
							switch (num3)
							{
							default:
								goto end_IL_00f0;
							case 0:
								goto end_IL_00f0;
							case 1:
								break;
							case 2:
								goto end_IL_00f0;
							}
							goto IL_014a;
							IL_014a:
							d22tsFrBCbhiyuiHuVF(obj, Y3rNcgUPMP670sjp06c.FPeUEHAdO7);
							num3 = 2;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_99797e4f3bab41baabcb75f7a30397b7 == 0)
							{
								num3 = 2;
							}
							goto IL_0115;
							end_IL_00f0:;
						}
					}
				}
				catch
				{
					int num4 = 1;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_7f545199031c4e3289afcf313932f580 != 0)
					{
						num4 = 0;
					}
					switch (num4)
					{
					case 1:
						try
						{
							yYpXl5r0ivQyMf9e5tu(client, nVdXdwhub0GVhDyNlIF.CsEYT7nXFY);
							int num5 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_fc096a82eb6f412ab19b9dee9fccbc6e == 0)
							{
								num5 = 0;
							}
							switch (num5)
							{
							case 0:
								break;
							}
							break;
						}
						catch
						{
							int num6 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_9de6b4ecde674f909622ae29ccaad31e != 0)
							{
								num6 = 0;
							}
							switch (num6)
							{
							case 0:
								break;
							}
							break;
						}
					case 0:
						break;
					}
					break;
				}
			case 0:
				break;
			}
		}

		static <>c__DisplayClass36_0()
		{
			D6evXYrAB0wf4IgMbVU();
		}

		internal static void osqCdNrD8Jr2kPC71j3()
		{
			z2jc63fLkugS1X8Q9N.rQ19jmJab();
		}

		internal static bool LiYHrXrVynjNMV3B2CA()
		{
			return l8vALLritmsktqacy6e == null;
		}

		internal static <>c__DisplayClass36_0 etAuZWrKxLOGTpBpSFX()
		{
			return l8vALLritmsktqacy6e;
		}

		internal static object KGAeSJrtinZjZteuiF6(int P_0)
		{
			return z2jc63fLkugS1X8Q9N.tE1kD1vbB(P_0);
		}

		internal static void DVsgufr5hFn9BpAn4NQ(object P_0, object P_1, int P_2, int P_3, object P_4)
		{
			JQCHYDU4aG4wZATQALx.jnq8ilIcK(P_0, (byte[])P_1, P_2, P_3, (JQCHYDU4aG4wZATQALx)P_4);
		}

		internal static void d22tsFrBCbhiyuiHuVF(object P_0, object P_1)
		{
			Y3rNcgUPMP670sjp06c.jnq8ilIcK(P_0, (Y3rNcgUPMP670sjp06c)P_1);
		}

		internal static void yYpXl5r0ivQyMf9e5tu(object P_0, object P_1)
		{
			nVdXdwhub0GVhDyNlIF.jnq8ilIcK(P_0, (nVdXdwhub0GVhDyNlIF)P_1);
		}

		internal static void D6evXYrAB0wf4IgMbVU()
		{
			z2jc63fLkugS1X8Q9N.TrhALsn43();
		}
	}

	[CompilerGenerated]
	private sealed class <>c__DisplayClass42_0
	{
		public StringBuilder output;

		public StringBuilder err;

		internal static <>c__DisplayClass42_0 YHuSZRrXb7MZUHt5Iw8;

		public <>c__DisplayClass42_0()
		{
			z2jc63fLkugS1X8Q9N.rQ19jmJab();
			base..ctor();
			int num = 0;
			if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_727e87ea3e7d4023a3b4ffb001ad0da6 != 0)
			{
				num = 0;
			}
			switch (num)
			{
			case 0:
				break;
			}
		}

		internal void <RunInject>b__0(object sender, DataReceivedEventArgs args)
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
				case 3:
					return;
				case 1:
					if (YPtZcsr6yTQXITHHCFd(args, gjqRKtPV17ZUEQVAWxv.yb02YGrn01) == null)
					{
						num2 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_12acf0ef11b346c9ab1a5afc262afd8b != 0)
						{
							num2 = 0;
						}
						continue;
					}
					break;
				case 2:
					break;
				}
				e69lJRrCPyvxxrBqYGx(output, YPtZcsr6yTQXITHHCFd(args, gjqRKtPV17ZUEQVAWxv.yb02YGrn01), qtMqbjU60YAngWAOYRT.cF1UR4sLY5);
				num2 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_f8d69a0d83e2420b83127060d822cafd == 0)
				{
					num2 = 3;
				}
			}
		}

		internal void <RunInject>b__1(object sender, DataReceivedEventArgs args)
		{
			int num = 2;
			int num2 = num;
			while (true)
			{
				switch (num2)
				{
				default:
					return;
				case 0:
					return;
				case 2:
					if (YPtZcsr6yTQXITHHCFd(args, gjqRKtPV17ZUEQVAWxv.yb02YGrn01) == null)
					{
						return;
					}
					num2 = 1;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1090bd5856ad49da8290b4ab724faee3 != 0)
					{
						num2 = 1;
					}
					break;
				case 1:
					e69lJRrCPyvxxrBqYGx(err, gjqRKtPV17ZUEQVAWxv.jnq8ilIcK(args, gjqRKtPV17ZUEQVAWxv.yb02YGrn01), qtMqbjU60YAngWAOYRT.cF1UR4sLY5);
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_83095058e27b497188a2c88050ba8f5f == 0)
					{
						num2 = 0;
					}
					break;
				}
			}
		}

		static <>c__DisplayClass42_0()
		{
			LCWU43rHObhDtBg6Fx7();
		}

		internal static bool MPyfSarTWfIBdWUs13k()
		{
			return YHuSZRrXb7MZUHt5Iw8 == null;
		}

		internal static <>c__DisplayClass42_0 LFTCxurGbcplctjTRMJ()
		{
			return YHuSZRrXb7MZUHt5Iw8;
		}

		internal static object YPtZcsr6yTQXITHHCFd(object P_0, object P_1)
		{
			return gjqRKtPV17ZUEQVAWxv.jnq8ilIcK(P_0, (gjqRKtPV17ZUEQVAWxv)P_1);
		}

		internal static object e69lJRrCPyvxxrBqYGx(object P_0, object P_1, object P_2)
		{
			return qtMqbjU60YAngWAOYRT.jnq8ilIcK(P_0, (string)P_1, (qtMqbjU60YAngWAOYRT)P_2);
		}

		internal static void LCWU43rHObhDtBg6Fx7()
		{
			z2jc63fLkugS1X8Q9N.TrhALsn43();
		}
	}

	private static readonly ConcurrentDictionary<int, BridgeSession> ByClient;

	private static readonly ConcurrentDictionary<int, BridgeSession> ByPid;

	private static int _nextClientId;

	private static object _watchTimer;

	private static readonly object LaunchLock;

	private static readonly object InstanceMutexSuffixes;

	private static readonly int StatusInfoLengthMismatch;

	private static object HuT1VInzTSJ4ICh5NiA;

	public static bool Owns(int clientId)
	{
		int num = 1;
		int num2 = num;
		BridgeSession value = default(BridgeSession);
		while (true)
		{
			switch (num2)
			{
			case 1:
				if (ByClient.TryGetValue(clientId, out value))
				{
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_db887f4a099249249dfc8abe84bd3ac9 == 0)
					{
						num2 = 0;
					}
					break;
				}
				goto IL_0061;
			case 2:
				return value.Closed == null;
			default:
				{
					if (value != null)
					{
						num2 = 2;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_8ef1e93a2d294273a3eb5e31d5952179 == 0)
						{
							num2 = 2;
						}
						break;
					}
					goto IL_0061;
				}
				IL_0061:
				return false;
			}
		}
	}

	public static bool TryGet(int clientId, out BridgeSession session)
	{
		int num = 6;
		int num2 = num;
		while (true)
		{
			switch (num2)
			{
			case 4:
				return true;
			default:
				session = null;
				num2 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_9f3937ff2c594a3ebca747058292ba44 != 0)
				{
					num2 = 1;
				}
				break;
			case 7:
				if (session.Closed != null)
				{
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_cc95e336569e4097a8a610a6e511075d != 0)
					{
						num2 = 0;
					}
					break;
				}
				goto case 4;
			case 3:
				if (session == null)
				{
					num2 = 2;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_291da815e7d24e5fad1c5f19ff1c2dd7 != 0)
					{
						num2 = 1;
					}
					break;
				}
				goto case 7;
			case 1:
				return false;
			case 6:
				if (!ByClient.TryGetValue(clientId, out session))
				{
					num2 = 5;
					break;
				}
				goto case 3;
			}
		}
	}

	public static bool EnsureReady(out string error)
	{
		int num = 1;
		int num2 = num;
		while (true)
		{
			switch (num2)
			{
			case 1:
				error = null;
				num2 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_5db1faeadc7a4faeaddf4f71b089648f == 0)
				{
					num2 = 0;
				}
				break;
			default:
				if (WeComVersionCatalog.RequiredDllsExist((string)HBbvWMcE0vjQIqqqYSs(-1977692036 ^ -1977705406), out error))
				{
					num2 = 3;
					break;
				}
				goto case 4;
			case 2:
				return true;
			case 4:
				return false;
			case 3:
				JCaPQRcbbBeraAkb8OS();
				num2 = 1;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_b45f0fd64f6448d2958872ecffbb53cd == 0)
				{
					num2 = 2;
				}
				break;
			}
		}
	}

	public static void StopAll()
	{
		int num = 4;
		int num2 = num;
		IEnumerator<BridgeSession> enumerator = default(IEnumerator<BridgeSession>);
		while (true)
		{
			switch (num2)
			{
			default:
				return;
			case 1:
				ByPid.Clear();
				num2 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c1db6875e3a1467ca73ac64ce11e3ed4 != 0)
				{
					num2 = 0;
				}
				break;
			case 3:
				try
				{
					while (true)
					{
						int num3;
						if (!UYtwpfcyKsZ8PpyNh8D(enumerator, nsJ9LebXYwUJAHxJYqI.tcqbTnwQjH))
						{
							num3 = 1;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_47f3813d118644b6b4369b1318743526 == 0)
							{
								num3 = 1;
							}
							goto IL_0078;
						}
						goto IL_00b9;
						IL_00b9:
						oL8rrXcxFgFNZZs4QDf(enumerator.Current, false);
						int num4 = 2;
						num3 = num4;
						goto IL_0078;
						IL_0078:
						switch (num3)
						{
						case 2:
							break;
						default:
							goto IL_00b9;
						case 1:
							goto end_IL_008e;
						}
						continue;
						end_IL_008e:
						break;
					}
				}
				finally
				{
					int num5;
					if (enumerator == null)
					{
						num5 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_043809f5a9d2487fae267fd26a3fe35d == 0)
						{
							num5 = 1;
						}
						goto IL_00fe;
					}
					goto IL_0114;
					IL_00fe:
					switch (num5)
					{
					case 1:
						goto end_IL_00d9;
					case 2:
						goto end_IL_00d9;
					}
					goto IL_0114;
					IL_0114:
					dsixtSPyYHjKiE5k7WK.jnq8ilIcK(enumerator, dsixtSPyYHjKiE5k7WK.VtIPNcTU9E);
					num5 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_f0c3ce86ec4a4bf2b2132a6d66c41033 == 0)
					{
						num5 = 2;
					}
					goto IL_00fe;
					end_IL_00d9:;
				}
				goto case 2;
			case 0:
				return;
			case 2:
				LQTslmc2PAU8SkX837l(ByClient);
				num2 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_35fbc8ccb86642068729e08a6b4fe6e4 == 0)
				{
					num2 = 1;
				}
				break;
			case 4:
				enumerator = ByClient.Values.GetEnumerator();
				num2 = 3;
				break;
			}
		}
	}

	public static void ReobserveAll()
	{
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected O, but got Unknown
		//IL_00fc: Expected O, but got Unknown
		int num = 2;
		int num2 = num;
		IEnumerator<BridgeSession> enumerator = default(IEnumerator<BridgeSession>);
		BridgeSession current = default(BridgeSession);
		BridgeHttpResult bridgeHttpResult = default(BridgeHttpResult);
		while (true)
		{
			switch (num2)
			{
			default:
				return;
			case 1:
				try
				{
					while (true)
					{
						IL_01be:
						int num3;
						if (!nsJ9LebXYwUJAHxJYqI.jnq8ilIcK(enumerator, nsJ9LebXYwUJAHxJYqI.tcqbTnwQjH))
						{
							num3 = 2;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_3c2786a3c96a4c30b2aea580e623804b != 0)
							{
								num3 = 2;
							}
							goto IL_003c;
						}
						goto IL_008c;
						IL_008c:
						current = enumerator.Current;
						num3 = 4;
						goto IL_003c;
						IL_003c:
						while (true)
						{
							switch (num3)
							{
							case 6:
								if (current.Http != null)
								{
									num3 = 0;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_cb5bc3d191fc4bc3b6ad04816cd3def1 != 0)
									{
										num3 = 1;
									}
									continue;
								}
								goto IL_01be;
							case 5:
								break;
							case 3:
								if (!U7ArJXchIZR71xbpxuE(bridgeHttpResult))
								{
									num3 = 7;
									continue;
								}
								goto IL_01be;
							case 1:
							{
								object http = current.Http;
								object path = HBbvWMcE0vjQIqqqYSs(-1525746568 ^ -1525766912);
								JObject val = new JObject();
								dqfVK0cQl6eDUpgITv1((object)val, HBbvWMcE0vjQIqqqYSs(-1429904013 ^ -1429916433), Otg8HQcUiCoxCZe7p39(true, DL0i8Bh4Ywk5x07Pweq.ApkhwcKISi), WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
								bridgeHttpResult = ((WeComBridgeHttp)http).Post((string)path, val);
								num3 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1fdc4536a75247b19a0801dd70bf34e9 == 0)
								{
									num3 = 3;
								}
								continue;
							}
							case 7:
								lymWLNceeC5sWAA6hPE(m4SFBaEZmre5qY3dKfT.jnq8ilIcK((string)HBbvWMcE0vjQIqqqYSs(0x18907F3 ^ 0x189565D), gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref current.ClientId, gGrLFhEdofbUymLMOb3.RREELfe2hN), z2jc63fLkugS1X8Q9N.tE1kD1vbB(--509279704 ^ 0x1E5AC582), (string)CJXtFXcYuiyaN1xAjkp(bridgeHttpResult, HBbvWMcE0vjQIqqqYSs(0x789D7F68 ^ 0x789D2EBE)), m4SFBaEZmre5qY3dKfT.hGFE1HacV8));
								num3 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_b3eccb7902c54a0db09f3abb83d000b4 == 0)
								{
									num3 = 0;
								}
								continue;
							case 4:
								if (current.Closed == null)
								{
									num3 = 1;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_a8ee04dc577b44a4b973e7119af8874c != 0)
									{
										num3 = 6;
									}
									continue;
								}
								goto IL_01be;
							default:
								goto IL_01be;
							case 2:
								return;
							}
							break;
						}
						goto IL_008c;
					}
				}
				finally
				{
					if (enumerator != null)
					{
						int num4 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_b233073321b645be9f62df5c4fc3764e == 0)
						{
							num4 = 0;
						}
						while (true)
						{
							switch (num4)
							{
							default:
								dsixtSPyYHjKiE5k7WK.jnq8ilIcK(enumerator, dsixtSPyYHjKiE5k7WK.VtIPNcTU9E);
								num4 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_7f545199031c4e3289afcf313932f580 == 0)
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
				}
			case 2:
				enumerator = ByClient.Values.GetEnumerator();
				num2 = 1;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_aaf9213cc3c34825a5d331618f885f65 == 0)
				{
					num2 = 1;
				}
				break;
			case 0:
				return;
			}
		}
	}

	public static bool LaunchAndInject(object exePath, out string error)
	{
		int num = 2;
		int num2 = num;
		bool result = default(bool);
		while (true)
		{
			switch (num2)
			{
			case 3:
				return result;
			case 2:
				error = null;
				num2 = 1;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1fdc4536a75247b19a0801dd70bf34e9 == 0)
				{
					num2 = 1;
				}
				break;
			case 5:
				return false;
			case 4:
				error = (string)HBbvWMcE0vjQIqqqYSs(--2048869548 ^ 0x7A1F1572);
				num2 = 5;
				break;
			case 1:
				if (NwDJBjcf7WuuFEeb4MV(LaunchLock, UXlitYhvu2h0jmfceHJ.xSShgeigio))
				{
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_b233073321b645be9f62df5c4fc3764e == 0)
					{
						num2 = 0;
					}
				}
				else
				{
					num2 = 4;
				}
				break;
			default:
				try
				{
					result = LaunchAndInjectCore(exePath, out error);
					int num3 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_e0bd3b55ee8d41bab3f8a58185604cae == 0)
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
					Y3rNcgUPMP670sjp06c.jnq8ilIcK(LaunchLock, Y3rNcgUPMP670sjp06c.FPeUEHAdO7);
					int num4 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_b4d4669a918b4b838c27ddb986baa9b6 == 0)
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

	private unsafe static bool LaunchAndInjectCore(object exePath, out string error)
	{
		int num = 27;
		int num5 = default(int);
		int[] array = default(int[]);
		int port = default(int);
		int num3 = default(int);
		int httpPort = default(int);
		TcpListener tcpListener = default(TcpListener);
		int num4 = default(int);
		int num6 = default(int);
		int num9 = default(int);
		while (true)
		{
			int num2 = num;
			while (true)
			{
				switch (num2)
				{
				case 26:
					if (N1tlkEcoZnwFWqSBqQf(exePath, lRNPJoPvsXOpQ7Ucdon.yHSEjK7hI0))
					{
						num = 13;
						break;
					}
					if (!WeComVersionCatalog.MatchesExe((string)exePath, z2jc63fLkugS1X8Q9N.tE1kD1vbB(-592631919 ^ -592614481), out error))
					{
						num2 = 18;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_b233073321b645be9f62df5c4fc3764e == 0)
						{
							num2 = 11;
						}
						continue;
					}
					goto case 17;
				case 28:
				{
					num5 = WaitNewWxWorkPid(new HashSet<int>(array), 2000, out var _);
					num2 = 33;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_84f159339c0c4d909a5aafadeb20c94b == 0)
					{
						num2 = 29;
					}
					continue;
				}
				case 2:
					return false;
				case 24:
					error = dDG4K3E4O2Ahv334CHT.jnq8ilIcK((string)HBbvWMcE0vjQIqqqYSs(0x5C783E72 ^ 0x5C786F8C), gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref port, gGrLFhEdofbUymLMOb3.RREELfe2hN), z2jc63fLkugS1X8Q9N.tE1kD1vbB(-896341688 ^ -896353444), dDG4K3E4O2Ahv334CHT.mOZEwuXRLl);
					num2 = 31;
					continue;
				case 25:
					num3 = WaitStartedPid(httpPort, out error);
					num2 = 16;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_13470d6aa0454f7eb0318dc44f893926 == 0)
					{
						num2 = 20;
					}
					continue;
				case 12:
					return false;
				default:
					httpPort = cOLLLAcidlrWei9XWsK(19065, http: true);
					num2 = 8;
					continue;
				case 20:
					if (num3 <= 0)
					{
						num2 = 5;
						continue;
					}
					return CompleteAttach(num3, httpPort, port, tcpListener, out error);
				case 10:
					return false;
				case 16:
					if (num4 == 1306)
					{
						num = 29;
						break;
					}
					goto case 1;
				case 22:
					if (tcpListener == null)
					{
						num2 = 24;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_2d7366bbeab1424ea4ee5fb2951fe83a == 0)
						{
							num2 = 13;
						}
						continue;
					}
					array = (int[])t8DAwfcVwC66MrmQD5d();
					num = 15;
					break;
				case 3:
				{
					string[] obj3 = new string[5]
					{
						(string)HBbvWMcE0vjQIqqqYSs(0x15454A1E ^ 0x1545183C),
						null,
						null,
						null,
						null
					};
					int num12 = array.Length;
					obj3[1] = gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref num12, gGrLFhEdofbUymLMOb3.RREELfe2hN);
					obj3[2] = (string)HBbvWMcE0vjQIqqqYSs(0x3B7F5719 ^ 0x3B7F0535);
					obj3[3] = gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref num6, gGrLFhEdofbUymLMOb3.RREELfe2hN);
					obj3[4] = (string)HBbvWMcE0vjQIqqqYSs(0x15454A1E ^ 0x154577C4);
					k0NxU4cte3COFcR674G(smJ0NkcDnTFTfrSLPGO(obj3, dMZjkAEvscZOQ4dkL9w.GZiEg2xxMP));
					num2 = 11;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_aa36ee1765db47c28f80e17ee8aff510 == 0)
					{
						num2 = 21;
					}
					continue;
				}
				case 18:
					return false;
				case 17:
					if (EnsureReady(out error))
					{
						num2 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_93e9b0bbc1fe4ae1a0fca3879b36d9a6 == 0)
						{
							num2 = 0;
						}
						continue;
					}
					goto case 12;
				case 1:
					return CompleteAttach(num5, httpPort, port, tcpListener, out error);
				case 32:
					num2 = 9;
					continue;
				case 11:
					if (num9 != 0)
					{
						num2 = 4;
						continue;
					}
					goto IL_01e3;
				case 9:
					try
					{
						kixbTec00AbaPALMjAN(tcpListener, nVdXdwhub0GVhDyNlIF.OlTh9S5DxM);
						int num10 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_cc95e336569e4097a8a610a6e511075d == 0)
						{
							num10 = 0;
						}
						switch (num10)
						{
						case 0:
							break;
						}
					}
					catch
					{
						int num11 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_fa47eaa7900849fc99eccdf087262b8e == 0)
						{
							num11 = 0;
						}
						switch (num11)
						{
						case 0:
							break;
						}
					}
					goto case 10;
				case 31:
					return false;
				case 33:
					if (num5 <= 0)
					{
						num2 = 32;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c33375e472634b5ebc4d636397603446 == 0)
						{
							num2 = 15;
						}
						continue;
					}
					goto case 34;
				case 15:
					if (array.Length != 0)
					{
						num2 = 6;
						continue;
					}
					goto case 21;
				case 19:
					if (num4 == 1306)
					{
						num2 = 16;
						continue;
					}
					goto case 32;
				case 14:
					lymWLNceeC5sWAA6hPE(HBbvWMcE0vjQIqqqYSs(-1606537422 ^ -1606517684));
					num = 25;
					break;
				case 27:
					error = null;
					num2 = 4;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_b45f0fd64f6448d2958872ecffbb53cd == 0)
					{
						num2 = 26;
					}
					continue;
				case 4:
					if (num9 != 1306)
					{
						num2 = 28;
						continue;
					}
					goto IL_01e3;
				case 5:
					try
					{
						kixbTec00AbaPALMjAN(tcpListener, nVdXdwhub0GVhDyNlIF.OlTh9S5DxM);
						int num7 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_cc95e336569e4097a8a610a6e511075d == 0)
						{
							num7 = 0;
						}
						switch (num7)
						{
						case 0:
							break;
						}
					}
					catch
					{
						int num8 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_5bb1f583c3f24addaf7d69d4facec67c != 0)
						{
							num8 = 0;
						}
						switch (num8)
						{
						case 0:
							break;
						}
					}
					goto case 23;
				case 6:
					num6 = plRH4RcK7wdAxuhvKAI();
					num = 3;
					break;
				case 8:
					port = cOLLLAcidlrWei9XWsK(19165, http: false);
					num2 = 35;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_a7a51f657ad14a8fa2f52f4d42963791 != 0)
					{
						num2 = 34;
					}
					continue;
				case 7:
					num4 = RunInject(smJ0NkcDnTFTfrSLPGO(new string[7]
					{
						(string)HBbvWMcE0vjQIqqqYSs(-236383992 ^ -236370960),
						gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref num5, gGrLFhEdofbUymLMOb3.RREELfe2hN),
						(string)HBbvWMcE0vjQIqqqYSs(-1464755110 ^ -1464742062),
						gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref httpPort, gGrLFhEdofbUymLMOb3.RREELfe2hN),
						(string)HBbvWMcE0vjQIqqqYSs(0x2E33007 ^ 0x2E3627D),
						gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref port, gGrLFhEdofbUymLMOb3.RREELfe2hN),
						z2jc63fLkugS1X8Q9N.tE1kD1vbB(--445132931 ^ 0x1A886215)
					}, dMZjkAEvscZOQ4dkL9w.GZiEg2xxMP), out error);
					num2 = 30;
					continue;
				case 29:
					lymWLNceeC5sWAA6hPE(uhAnXmcBtDKqQjGYOsb(HBbvWMcE0vjQIqqqYSs(-1821667360 ^ -1821647620), gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref num5, gGrLFhEdofbUymLMOb3.RREELfe2hN), HBbvWMcE0vjQIqqqYSs(--1508596617 ^ 0x59EB08AF), dDG4K3E4O2Ahv334CHT.mOZEwuXRLl));
					num2 = 1;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_cbea79c7647f4fe9ae446d7b78f25d28 == 0)
					{
						num2 = 0;
					}
					continue;
				case 35:
					tcpListener = StartListener(ref port);
					num2 = 16;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_03aacce0025f45d8a46eb3d2a6e41868 == 0)
					{
						num2 = 22;
					}
					continue;
				case 30:
					if (num4 != 0)
					{
						num2 = 19;
						continue;
					}
					goto case 16;
				case 13:
					if (WeComVersionCatalog.TryResolveLaunchExe((string)HBbvWMcE0vjQIqqqYSs(-2134178576 ^ -2134161202), out *(string*)(&exePath), out error))
					{
						num2 = 17;
						continue;
					}
					goto case 2;
				case 21:
					num9 = RunInject(smJ0NkcDnTFTfrSLPGO(new string[7]
					{
						(string)HBbvWMcE0vjQIqqqYSs(-1977692036 ^ -1977712078),
						(string)exePath,
						z2jc63fLkugS1X8Q9N.tE1kD1vbB(-879603300 ^ -879623176),
						gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref httpPort, gGrLFhEdofbUymLMOb3.RREELfe2hN),
						(string)HBbvWMcE0vjQIqqqYSs(0x647ABFB ^ 0x647F981),
						gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref port, gGrLFhEdofbUymLMOb3.RREELfe2hN),
						(string)HBbvWMcE0vjQIqqqYSs(-1356900703 ^ -1356921801)
					}, dMZjkAEvscZOQ4dkL9w.GZiEg2xxMP), out error);
					num2 = 11;
					continue;
				case 34:
					Logger.Info((string)XnUyVEc5Tkm1KK2H1pc(HBbvWMcE0vjQIqqqYSs(-1232431428 ^ -1232419722), gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref num5, gGrLFhEdofbUymLMOb3.RREELfe2hN), RxUSOmPDJGsMmCCGPPg.hbkPt9hhua));
					num2 = 7;
					continue;
				case 23:
					{
						return false;
					}
					IL_01e3:
					if (num9 == 1306)
					{
						num2 = 14;
						continue;
					}
					goto case 25;
				}
				break;
			}
		}
	}

	public static bool InjectPid(int pid, out string error)
	{
		int num = 3;
		int num2 = num;
		AttachKind attachKind = default(AttachKind);
		while (true)
		{
			switch (num2)
			{
			case 3:
				attachKind = AttachOne(pid, allowInject: true, out error);
				num2 = 2;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_2cca3d059d854b57aca52e0b80a8eb7d == 0)
				{
					num2 = 0;
				}
				break;
			case 2:
				if (attachKind == AttachKind.Attached)
				{
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_5bb1f583c3f24addaf7d69d4facec67c == 0)
					{
						num2 = 0;
					}
					break;
				}
				goto case 1;
			case 1:
				return attachKind == AttachKind.Already;
			default:
				return true;
			}
		}
	}

	internal static void AttachRunning(object report)
	{
		int num = 17;
		AttachKind attachKind = default(AttachKind);
		string error = default(string);
		int num3 = default(int);
		List<int>.Enumerator enumerator = default(List<int>.Enumerator);
		List<int> list = default(List<int>);
		int num4 = default(int);
		int[] array = default(int[]);
		int[] array2 = default(int[]);
		AttachKind attachKind2 = default(AttachKind);
		string error2 = default(string);
		while (true)
		{
			int num2 = num;
			while (true)
			{
				switch (num2)
				{
				case 12:
					if (attachKind == AttachKind.Attached)
					{
						num2 = 15;
						continue;
					}
					goto case 23;
				case 5:
					((WeComAttachReport)report).Skipped.Add(error);
					num2 = 26;
					continue;
				case 19:
					return;
				case 2:
				case 7:
				case 10:
				case 20:
				case 26:
					num3++;
					num2 = 12;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_ebde985f65fe4000b6ba504548c2703b == 0)
					{
						num2 = 22;
					}
					continue;
				case 24:
					((WeComAttachReport)report).Already++;
					num2 = 7;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d3b039c30242456eb8cdd20df225be7a != 0)
					{
						num2 = 10;
					}
					continue;
				case 1:
					enumerator = list.GetEnumerator();
					num2 = 8;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_fc096a82eb6f412ab19b9dee9fccbc6e != 0)
					{
						num2 = 5;
					}
					continue;
				case 3:
					list.Add(num4);
					num2 = 20;
					continue;
				case 22:
				case 25:
					if (num3 < array.Length)
					{
						num2 = 11;
						continue;
					}
					goto case 1;
				case 14:
					if (N1tlkEcoZnwFWqSBqQf(error, lRNPJoPvsXOpQ7Ucdon.yHSEjK7hI0))
					{
						num2 = 7;
						continue;
					}
					goto case 5;
				case 15:
					((WeComAttachReport)report).Attached++;
					num = 2;
					break;
				default:
					list = new List<int>();
					num2 = 4;
					continue;
				case 23:
					if (attachKind == AttachKind.Already)
					{
						num2 = 24;
						continue;
					}
					goto case 9;
				case 16:
					((WeComAttachReport)report).Found = array2.Length;
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_a152e1a10f374e159091a60e0cb4b256 == 0)
					{
						num2 = 0;
					}
					continue;
				case 18:
					Logger.Info((string)smJ0NkcDnTFTfrSLPGO(new string[7]
					{
						(string)HBbvWMcE0vjQIqqqYSs(-10914152 ^ -10934966),
						gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref ((WeComAttachReport)report).Found, gGrLFhEdofbUymLMOb3.RREELfe2hN),
						(string)HBbvWMcE0vjQIqqqYSs(--445132931 ^ 0x1A88635F),
						gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref ((WeComAttachReport)report).Attached, gGrLFhEdofbUymLMOb3.RREELfe2hN),
						(string)HBbvWMcE0vjQIqqqYSs(-1356900703 ^ -1356895385),
						gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref ((WeComAttachReport)report).Already, gGrLFhEdofbUymLMOb3.RREELfe2hN),
						(string)HBbvWMcE0vjQIqqqYSs(-183227658 ^ -183232724)
					}, dMZjkAEvscZOQ4dkL9w.GZiEg2xxMP));
					num2 = 19;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_529db4a9032c417dbe74a080d04d36c2 != 0)
					{
						num2 = 15;
					}
					continue;
				case 13:
					num3 = 0;
					num = 25;
					break;
				case 8:
					try
					{
						while (true)
						{
							IL_03a1:
							int num5;
							if (!enumerator.MoveNext())
							{
								num5 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1fdc4536a75247b19a0801dd70bf34e9 == 0)
								{
									num5 = 0;
								}
								goto IL_02e4;
							}
							goto IL_0382;
							IL_02e4:
							while (true)
							{
								switch (num5)
								{
								case 1:
									if (attachKind2 == AttachKind.Already)
									{
										num5 = 4;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_0e5a04c5423148499c9b265cadee0019 != 0)
										{
											num5 = 8;
										}
										continue;
									}
									goto case 3;
								case 3:
									if (!N1tlkEcoZnwFWqSBqQf(error2, lRNPJoPvsXOpQ7Ucdon.yHSEjK7hI0))
									{
										num5 = 5;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d91865d38b6c449295c8f801546a735d != 0)
										{
											num5 = 1;
										}
										continue;
									}
									goto IL_03a1;
								case 9:
									goto IL_0382;
								case 4:
								case 7:
									goto IL_03a1;
								case 5:
									((WeComAttachReport)report).Skipped.Add(error2);
									num5 = 4;
									continue;
								case 6:
								case 8:
									goto IL_03fd;
								case 2:
									if (attachKind2 == AttachKind.Attached)
									{
										num5 = 6;
										continue;
									}
									goto case 1;
								case 0:
									break;
								}
								break;
							}
							break;
							IL_03fd:
							((WeComAttachReport)report).Attached++;
							int num6 = 7;
							goto IL_02e0;
							IL_0382:
							attachKind2 = AttachOne(enumerator.Current, allowInject: true, out error2);
							num6 = 2;
							goto IL_02e0;
							IL_02e0:
							num5 = num6;
							goto IL_02e4;
						}
					}
					finally
					{
						((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
						int num7 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1601bb4cfcbc4e588ecdc797e83d5acc == 0)
						{
							num7 = 0;
						}
						switch (num7)
						{
						case 0:
							break;
						}
					}
					goto case 18;
				case 17:
					array2 = (int[])t8DAwfcVwC66MrmQD5d();
					num2 = 16;
					continue;
				case 6:
				case 11:
					num4 = array[num3];
					num2 = 21;
					continue;
				case 21:
					attachKind = AttachOne(num4, allowInject: false, out error);
					num2 = 12;
					continue;
				case 4:
					array = array2;
					num2 = 13;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_5138927ba3c1408e9ba282c6f1787fc1 != 0)
					{
						num2 = 13;
					}
					continue;
				case 9:
					if (attachKind == AttachKind.NeedsInject)
					{
						num = 3;
						break;
					}
					goto case 14;
				}
				break;
			}
		}
	}

	private static AttachKind AttachOne(int pid, bool allowInject, out string error)
	{
		int num = 15;
		string version = default(string);
		BridgeSession value = default(BridgeSession);
		int num3 = default(int);
		int tcpPort = default(int);
		int pid2 = default(int);
		string text = default(string);
		while (true)
		{
			int num2 = num;
			while (true)
			{
				switch (num2)
				{
				case 13:
					lymWLNceeC5sWAA6hPE(error);
					num = 2;
					break;
				case 9:
					if (lRNPJoPvsXOpQ7Ucdon.jnq8ilIcK(version, lRNPJoPvsXOpQ7Ucdon.VmKEqmiqNI))
					{
						num2 = 28;
						continue;
					}
					goto case 4;
				case 22:
					error = dDG4K3E4O2Ahv334CHT.jnq8ilIcK((string)HBbvWMcE0vjQIqqqYSs(-1356900703 ^ -1356921411), gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref pid, gGrLFhEdofbUymLMOb3.RREELfe2hN), (string)HBbvWMcE0vjQIqqqYSs(-1033100007 ^ -1033080429), dDG4K3E4O2Ahv334CHT.mOZEwuXRLl);
					num2 = 30;
					continue;
				case 15:
					error = null;
					num2 = 14;
					continue;
				case 8:
					return RepairListener(value, out error);
				case 21:
				case 31:
					num3 = ReadSessionHttpPort(pid);
					num2 = 6;
					continue;
				case 6:
					tcpPort = b6QaDdcT87fQt05hwSA(pid);
					num2 = 20;
					continue;
				case 12:
					return AttachKind.Failed;
				case 29:
					lymWLNceeC5sWAA6hPE(error);
					num2 = 12;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_529db4a9032c417dbe74a080d04d36c2 != 0)
					{
						num2 = 1;
					}
					continue;
				case 10:
					return AttachKind.Failed;
				case 25:
					if (!allowInject)
					{
						num2 = 19;
						continue;
					}
					return InjectFresh(pid, out error);
				case 11:
					if (!TryReadHealth(num3, out pid2, out version))
					{
						num2 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c1db6875e3a1467ca73ac64ce11e3ed4 != 0)
						{
							num2 = 1;
						}
						continue;
					}
					goto case 18;
				case 19:
					return AttachKind.NeedsInject;
				case 17:
					return AttachKind.Failed;
				case 5:
					text = (string)Br5jxLcAl31ILg0XKsb(pid);
					num2 = 7;
					continue;
				case 30:
					Logger.Warning(error);
					num = 10;
					break;
				case 3:
					error = (string)smJ0NkcDnTFTfrSLPGO(new string[5]
					{
						(string)HBbvWMcE0vjQIqqqYSs(0x7C2E3737 ^ 0x7C2E642B),
						gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref pid, gGrLFhEdofbUymLMOb3.RREELfe2hN),
						(string)HBbvWMcE0vjQIqqqYSs(-824819615 ^ -824830923),
						version,
						(string)HBbvWMcE0vjQIqqqYSs(-1164549976 ^ -1164561216)
					}, dMZjkAEvscZOQ4dkL9w.GZiEg2xxMP);
					num2 = 8;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_291da815e7d24e5fad1c5f19ff1c2dd7 == 0)
					{
						num2 = 13;
					}
					continue;
				case 23:
					if (value != null)
					{
						num2 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_043809f5a9d2487fae267fd26a3fe35d == 0)
						{
							num2 = 0;
						}
						continue;
					}
					goto case 21;
				case 7:
					if (ionqQBcXf7e1A4eQlAg(text, HBbvWMcE0vjQIqqqYSs(-1680728354 ^ -1680715040)))
					{
						if (!ByPid.TryGetValue(pid, out value))
						{
							num2 = 21;
							continue;
						}
						goto case 23;
					}
					num2 = 7;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_727e87ea3e7d4023a3b4ffb001ad0da6 == 0)
					{
						num2 = 24;
					}
					continue;
				default:
					if (value.Closed != null)
					{
						num2 = 31;
						continue;
					}
					goto case 8;
				case 20:
					version = null;
					num2 = 26;
					continue;
				case 4:
					if (!NgtBADEr5RrpT3l6FkM.jnq8ilIcK(version, z2jc63fLkugS1X8Q9N.tE1kD1vbB(0x65DCA45F ^ 0x65DCE861), NgtBADEr5RrpT3l6FkM.YGgE7TJZkD))
					{
						num2 = 16;
						continue;
					}
					goto case 3;
				case 2:
					return AttachKind.Failed;
				case 16:
				case 28:
					return AdoptRunning(pid, num3, tcpPort, out error);
				case 27:
					if (!a1KdFRcGwaMuuhJ7tvn(pid))
					{
						num2 = 25;
						continue;
					}
					goto case 22;
				case 14:
					if (EnsureReady(out error))
					{
						num2 = 5;
						continue;
					}
					goto case 17;
				case 18:
					if (pid2 == pid)
					{
						goto case 9;
					}
					goto case 1;
				case 26:
					if (num3 > 0)
					{
						num2 = 11;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1601bb4cfcbc4e588ecdc797e83d5acc != 0)
						{
							num2 = 1;
						}
						continue;
					}
					goto case 1;
				case 24:
					error = (string)(N1tlkEcoZnwFWqSBqQf(text, lRNPJoPvsXOpQ7Ucdon.VmKEqmiqNI) ? uhAnXmcBtDKqQjGYOsb(HBbvWMcE0vjQIqqqYSs(-1525746568 ^ -1525768098), gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref pid, gGrLFhEdofbUymLMOb3.RREELfe2hN), HBbvWMcE0vjQIqqqYSs(-1948902920 ^ -1948889664), dDG4K3E4O2Ahv334CHT.mOZEwuXRLl) : smJ0NkcDnTFTfrSLPGO(new string[5]
					{
						z2jc63fLkugS1X8Q9N.tE1kD1vbB(-2068695740 ^ -2068683176),
						gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref pid, gGrLFhEdofbUymLMOb3.RREELfe2hN),
						z2jc63fLkugS1X8Q9N.tE1kD1vbB(-153892167 ^ -153910023),
						text,
						(string)HBbvWMcE0vjQIqqqYSs(0x1DB17C88 ^ 0x1DB12F7C)
					}, dMZjkAEvscZOQ4dkL9w.GZiEg2xxMP));
					num2 = 29;
					continue;
				case 1:
					num2 = 20;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_b4d4669a918b4b838c27ddb986baa9b6 == 0)
					{
						num2 = 27;
					}
					continue;
				}
				break;
			}
		}
	}

	private static AttachKind RepairListener(object session, out string error)
	{
		int num = 9;
		int num2 = num;
		TcpListener tcpListener = default(TcpListener);
		int num3 = default(int);
		while (true)
		{
			switch (num2)
			{
			case 14:
				tcpListener = (TcpListener)fwmFxlcCExxicVq5IYy(num3);
				num2 = 2;
				break;
			case 7:
				((BridgeSession)session).TcpPort = num3;
				num2 = 11;
				break;
			case 8:
				num3 = ReadStatusTcpPort(((BridgeSession)session).Pid);
				num2 = 13;
				break;
			case 5:
				return AttachKind.Failed;
			case 9:
				error = null;
				num2 = 8;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_311d03ca4084414a9ea0c8d2c1bd343d != 0)
				{
					num2 = 6;
				}
				break;
			default:
				if (num3 == ((BridgeSession)session).TcpPort)
				{
					num2 = 3;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_eb326c992e4b4c9f9cd930152533b185 == 0)
					{
						num2 = 4;
					}
					break;
				}
				Logger.Info((string)smJ0NkcDnTFTfrSLPGO(new string[7]
				{
					(string)HBbvWMcE0vjQIqqqYSs(-554298198 ^ -554285130),
					gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref ((BridgeSession)session).Pid, gGrLFhEdofbUymLMOb3.RREELfe2hN),
					z2jc63fLkugS1X8Q9N.tE1kD1vbB(-1019290801 ^ -1019310521),
					gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref num3, gGrLFhEdofbUymLMOb3.RREELfe2hN),
					(string)HBbvWMcE0vjQIqqqYSs(-30755866 ^ -30742790),
					gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref ((BridgeSession)session).TcpPort, gGrLFhEdofbUymLMOb3.RREELfe2hN),
					z2jc63fLkugS1X8Q9N.tE1kD1vbB(0x1FD323AC ^ 0x1FD37680)
				}, dMZjkAEvscZOQ4dkL9w.GZiEg2xxMP));
				num2 = 3;
				break;
			case 2:
				if (tcpListener != null)
				{
					((BridgeSession)session).Listener = tcpListener;
					num2 = 7;
				}
				else
				{
					num2 = 10;
				}
				break;
			case 6:
				return AttachKind.Attached;
			case 1:
				return AttachKind.Already;
			case 4:
			case 12:
				k0NxU4cte3COFcR674G(smJ0NkcDnTFTfrSLPGO(new string[6]
				{
					(string)HBbvWMcE0vjQIqqqYSs(0x18F258A ^ 0x18F7140),
					gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref ((BridgeSession)session).Pid, gGrLFhEdofbUymLMOb3.RREELfe2hN),
					(string)HBbvWMcE0vjQIqqqYSs(-10914152 ^ -10935744),
					gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref ((BridgeSession)session).ClientId, gGrLFhEdofbUymLMOb3.RREELfe2hN),
					z2jc63fLkugS1X8Q9N.tE1kD1vbB(0x780E5C1F ^ 0x780E08E5),
					gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref ((BridgeSession)session).TcpPort, gGrLFhEdofbUymLMOb3.RREELfe2hN)
				}, dMZjkAEvscZOQ4dkL9w.GZiEg2xxMP));
				num2 = 1;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_a34de5cebe4f4eeda550f236a75b91de == 0)
				{
					num2 = 1;
				}
				break;
			case 3:
				YovNMkc6sKgfHjsqhPH(session);
				num2 = 14;
				break;
			case 11:
				fbYNxjcH1DFr9g4cFNN(session);
				num2 = 6;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_f8d69a0d83e2420b83127060d822cafd != 0)
				{
					num2 = 1;
				}
				break;
			case 10:
				error = (string)smJ0NkcDnTFTfrSLPGO(new string[5]
				{
					z2jc63fLkugS1X8Q9N.tE1kD1vbB(-233972548 ^ -233992288),
					gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref ((BridgeSession)session).Pid, gGrLFhEdofbUymLMOb3.RREELfe2hN),
					(string)HBbvWMcE0vjQIqqqYSs(-280545938 ^ -280534958),
					gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref num3, gGrLFhEdofbUymLMOb3.RREELfe2hN),
					(string)HBbvWMcE0vjQIqqqYSs(0x7D3B8403 ^ 0x7D3BD617)
				}, dMZjkAEvscZOQ4dkL9w.GZiEg2xxMP);
				num2 = 5;
				break;
			case 13:
				if (num3 <= 0)
				{
					num2 = 12;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d91865d38b6c449295c8f801546a735d == 0)
					{
						num2 = 12;
					}
					break;
				}
				goto default;
			}
		}
	}

	private static AttachKind AdoptRunning(int pid, int httpPort, int tcpPort, out string error)
	{
		int num = 5;
		TcpListener tcpListener = default(TcpListener);
		while (true)
		{
			int num2 = num;
			while (true)
			{
				switch (num2)
				{
				case 3:
					return AttachKind.Failed;
				case 8:
					return AttachKind.Attached;
				case 5:
					error = null;
					num2 = 4;
					continue;
				case 6:
					break;
				case 2:
					if (CompleteAttach(pid, httpPort, tcpPort, tcpListener, out error))
					{
						num2 = 8;
						continue;
					}
					goto case 3;
				case 10:
					error = (string)uhAnXmcBtDKqQjGYOsb(HBbvWMcE0vjQIqqqYSs(-1510018411 ^ -1510039159), gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref pid, gGrLFhEdofbUymLMOb3.RREELfe2hN), HBbvWMcE0vjQIqqqYSs(-236383992 ^ -236370874), dDG4K3E4O2Ahv334CHT.mOZEwuXRLl);
					num2 = 6;
					continue;
				case 1:
					error = (string)smJ0NkcDnTFTfrSLPGO(new string[5]
					{
						z2jc63fLkugS1X8Q9N.tE1kD1vbB(0x47C9E567 ^ 0x47C9B67B),
						gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref pid, gGrLFhEdofbUymLMOb3.RREELfe2hN),
						(string)HBbvWMcE0vjQIqqqYSs(0x3B0AFC8F ^ 0x3B0AA9B3),
						gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref tcpPort, gGrLFhEdofbUymLMOb3.RREELfe2hN),
						z2jc63fLkugS1X8Q9N.tE1kD1vbB(0x46C0842F ^ 0x46C0D63B)
					}, dMZjkAEvscZOQ4dkL9w.GZiEg2xxMP);
					num2 = 9;
					continue;
				default:
					if (tcpListener == null)
					{
						num2 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_2d8e9ecb9a5b407690948cd8111b2980 == 0)
						{
							num2 = 1;
						}
						continue;
					}
					Logger.Info(dMZjkAEvscZOQ4dkL9w.jnq8ilIcK(new string[6]
					{
						(string)HBbvWMcE0vjQIqqqYSs(-1830890396 ^ -1830911506),
						gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref pid, gGrLFhEdofbUymLMOb3.RREELfe2hN),
						(string)HBbvWMcE0vjQIqqqYSs(-1204399808 ^ -1204419352),
						gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref httpPort, gGrLFhEdofbUymLMOb3.RREELfe2hN),
						(string)HBbvWMcE0vjQIqqqYSs(-19396624 ^ -19377398),
						gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref tcpPort, gGrLFhEdofbUymLMOb3.RREELfe2hN)
					}, dMZjkAEvscZOQ4dkL9w.GZiEg2xxMP));
					num2 = 2;
					continue;
				case 9:
					return AttachKind.Failed;
				case 7:
					return AttachKind.Failed;
				case 11:
					tcpListener = (TcpListener)fwmFxlcCExxicVq5IYy(tcpPort);
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_291da815e7d24e5fad1c5f19ff1c2dd7 == 0)
					{
						num2 = 0;
					}
					continue;
				case 4:
					if (tcpPort > 0)
					{
						num2 = 11;
						continue;
					}
					goto case 10;
				}
				break;
			}
			lymWLNceeC5sWAA6hPE(error);
			num = 7;
		}
	}

	private static AttachKind InjectFresh(int pid, out string error)
	{
		int num = 11;
		TcpListener tcpListener = default(TcpListener);
		int num4 = default(int);
		int num5 = default(int);
		int port = default(int);
		int tcpPort = default(int);
		int num3 = default(int);
		while (true)
		{
			int num2 = num;
			while (true)
			{
				switch (num2)
				{
				case 8:
					try
					{
						kixbTec00AbaPALMjAN(tcpListener, nVdXdwhub0GVhDyNlIF.OlTh9S5DxM);
						int num8 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_2b51064baece4c398bf908a830a54cbf == 0)
						{
							num8 = 0;
						}
						switch (num8)
						{
						case 0:
							break;
						}
					}
					catch
					{
						int num9 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_bd908099f891496e8274f46975b80fa0 == 0)
						{
							num9 = 0;
						}
						switch (num9)
						{
						case 0:
							break;
						}
					}
					goto case 6;
				case 2:
					try
					{
						kixbTec00AbaPALMjAN(tcpListener, nVdXdwhub0GVhDyNlIF.OlTh9S5DxM);
						int num6 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_47f3813d118644b6b4369b1318743526 == 0)
						{
							num6 = 0;
						}
						switch (num6)
						{
						case 0:
							break;
						}
					}
					catch
					{
						int num7 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_e3198421401646eabf32dc544445bbbe == 0)
						{
							num7 = 0;
						}
						switch (num7)
						{
						case 0:
							break;
						}
					}
					goto case 14;
				case 11:
					num4 = AllocatePort(19065, http: true);
					num2 = 10;
					continue;
				case 6:
					return AttachKind.Failed;
				case 16:
					if (num5 == 1306)
					{
						num2 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_9f3937ff2c594a3ebca747058292ba44 != 0)
						{
							num2 = 2;
						}
						continue;
					}
					if (num5 == 0)
					{
						if (CompleteAttach(pid, num4, port, tcpListener, out error))
						{
							num2 = 13;
							continue;
						}
						goto case 4;
					}
					num2 = 8;
					continue;
				case 7:
					tcpListener = StartListener(ref port);
					num = 3;
					break;
				case 15:
					return AttachKind.Failed;
				case 9:
					lymWLNceeC5sWAA6hPE(m4SFBaEZmre5qY3dKfT.jnq8ilIcK(z2jc63fLkugS1X8Q9N.tE1kD1vbB(-1521705861 ^ -1521692825), gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref pid, gGrLFhEdofbUymLMOb3.RREELfe2hN), (string)HBbvWMcE0vjQIqqqYSs(0x234FD180 ^ 0x234F845C), gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref tcpPort, gGrLFhEdofbUymLMOb3.RREELfe2hN), m4SFBaEZmre5qY3dKfT.hGFE1HacV8));
					num = 12;
					break;
				case 14:
					num3 = ReadSessionHttpPort(pid);
					num = 5;
					break;
				case 3:
					if (tcpListener != null)
					{
						Logger.Info(dMZjkAEvscZOQ4dkL9w.jnq8ilIcK(new string[6]
						{
							(string)HBbvWMcE0vjQIqqqYSs(-879603300 ^ -879623132),
							gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref pid, gGrLFhEdofbUymLMOb3.RREELfe2hN),
							(string)HBbvWMcE0vjQIqqqYSs(0x44D4B4E9 ^ 0x44D4E141),
							gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref num4, gGrLFhEdofbUymLMOb3.RREELfe2hN),
							(string)HBbvWMcE0vjQIqqqYSs(0x5FF7E885 ^ 0x5FF7BC7F),
							gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref port, gGrLFhEdofbUymLMOb3.RREELfe2hN)
						}, dMZjkAEvscZOQ4dkL9w.GZiEg2xxMP));
						num2 = 1;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c09e27dcabbb4f6484836a4e1fd5da8e == 0)
						{
							num2 = 0;
						}
					}
					else
					{
						num2 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_0444cf56387747139b0edc577cc00a72 != 0)
						{
							num2 = 0;
						}
					}
					continue;
				default:
					error = (string)uhAnXmcBtDKqQjGYOsb(HBbvWMcE0vjQIqqqYSs(-347673417 ^ -347685559), gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref port, gGrLFhEdofbUymLMOb3.RREELfe2hN), HBbvWMcE0vjQIqqqYSs(-347673417 ^ -347685213), dDG4K3E4O2Ahv334CHT.mOZEwuXRLl);
					num2 = 15;
					continue;
				case 4:
					return AttachKind.Failed;
				case 13:
					return AttachKind.Attached;
				case 5:
					tcpPort = b6QaDdcT87fQt05hwSA(pid);
					num2 = 2;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_79220d9f20444b6c8b0df2558a43de3e != 0)
					{
						num2 = 9;
					}
					continue;
				case 10:
					port = cOLLLAcidlrWei9XWsK(19165, http: false);
					num2 = 1;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_7dea470b4c15478bbfe1539429f98b80 != 0)
					{
						num2 = 7;
					}
					continue;
				case 1:
					num5 = RunInject(dMZjkAEvscZOQ4dkL9w.jnq8ilIcK(new string[7]
					{
						(string)HBbvWMcE0vjQIqqqYSs(-40068700 ^ -40055972),
						gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref pid, gGrLFhEdofbUymLMOb3.RREELfe2hN),
						(string)HBbvWMcE0vjQIqqqYSs(0x518E8229 ^ 0x518ED121),
						gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref num4, gGrLFhEdofbUymLMOb3.RREELfe2hN),
						z2jc63fLkugS1X8Q9N.tE1kD1vbB(0x29DAFBE2 ^ 0x29DAA998),
						gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref port, gGrLFhEdofbUymLMOb3.RREELfe2hN),
						z2jc63fLkugS1X8Q9N.tE1kD1vbB(-1229729227 ^ -1229750109)
					}, dMZjkAEvscZOQ4dkL9w.GZiEg2xxMP), out error);
					num2 = 16;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_83095058e27b497188a2c88050ba8f5f != 0)
					{
						num2 = 8;
					}
					continue;
				case 12:
					return AdoptRunning(pid, (num3 > 0) ? num3 : num4, tcpPort, out error);
				}
				break;
			}
		}
	}

	private static bool CompleteAttach(int pid, int httpPort, int tcpPort, object listener, out string error)
	{
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Expected O, but got Unknown
		//IL_01be: Expected O, but got Unknown
		int num = 6;
		BridgeSession bridgeSession = default(BridgeSession);
		BridgeHttpResult bridgeHttpResult = default(BridgeHttpResult);
		string text = default(string);
		while (true)
		{
			int num2 = num;
			while (true)
			{
				switch (num2)
				{
				case 3:
					return false;
				default:
					RefreshLogin(bridgeSession, initial: true);
					num2 = 12;
					break;
				case 2:
				case 11:
					WeComHelper.PublishBridgeOnline(bridgeSession.ClientId);
					num2 = 10;
					break;
				case 15:
					ByClient[bridgeSession.ClientId] = bridgeSession;
					num2 = 13;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c2d1dda68e3b4e11855617e481626480 != 0)
					{
						num2 = 10;
					}
					break;
				case 12:
					EnsureWatchTimer();
					num2 = 7;
					break;
				case 13:
					ByPid[pid] = bridgeSession;
					num2 = 9;
					break;
				case 14:
					if (bridgeHttpResult.Ok)
					{
						num2 = 2;
						break;
					}
					goto case 16;
				case 6:
					if (WaitHealth(httpPort, pid, out error))
					{
						text = WaitToken(pid, out error);
						num2 = 8;
						break;
					}
					goto end_IL_0012;
				case 17:
				{
					object http = bridgeSession.Http;
					string text2 = z2jc63fLkugS1X8Q9N.tE1kD1vbB(--1439621364 ^ 0x55CEB18C);
					JObject val = new JObject();
					WSJ5GQQ9myN2N0ma3xj.jnq8ilIcK((object)val, (string)HBbvWMcE0vjQIqqqYSs(-1694426734 ^ -1694414834), DL0i8Bh4Ywk5x07Pweq.jnq8ilIcK(true, DL0i8Bh4Ywk5x07Pweq.ApkhwcKISi), WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
					bridgeHttpResult = (BridgeHttpResult)dnGbEsca1E35BhtvuvG(http, text2, (object)val);
					num2 = 14;
					break;
				}
				case 16:
					lymWLNceeC5sWAA6hPE(RxUSOmPDJGsMmCCGPPg.jnq8ilIcK((string)HBbvWMcE0vjQIqqqYSs(0x18F258A ^ 0x18F7074), (string)CJXtFXcYuiyaN1xAjkp(bridgeHttpResult, z2jc63fLkugS1X8Q9N.tE1kD1vbB(-1948902920 ^ -1948891090)), RxUSOmPDJGsMmCCGPPg.hbkPt9hhua));
					num2 = 11;
					break;
				case 5:
					try
					{
						kixbTec00AbaPALMjAN(listener, nVdXdwhub0GVhDyNlIF.OlTh9S5DxM);
						int num5 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_5bb1f583c3f24addaf7d69d4facec67c == 0)
						{
							num5 = 0;
						}
						switch (num5)
						{
						case 0:
							break;
						}
					}
					catch
					{
						int num6 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d9b8fefff9de4f9c9c2f7dc1a6c24ab2 == 0)
						{
							num6 = 0;
						}
						switch (num6)
						{
						case 0:
							break;
						}
					}
					goto case 4;
				case 10:
					Logger.Info(dMZjkAEvscZOQ4dkL9w.jnq8ilIcK(new string[8]
					{
						(string)HBbvWMcE0vjQIqqqYSs(-396535719 ^ -396554673),
						gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref pid, gGrLFhEdofbUymLMOb3.RREELfe2hN),
						(string)HBbvWMcE0vjQIqqqYSs(-183227658 ^ -183206732),
						gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref bridgeSession.ClientId, gGrLFhEdofbUymLMOb3.RREELfe2hN),
						(string)HBbvWMcE0vjQIqqqYSs(0x7C2E3737 ^ 0x7C2E629F),
						gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref httpPort, gGrLFhEdofbUymLMOb3.RREELfe2hN),
						z2jc63fLkugS1X8Q9N.tE1kD1vbB(-879246903 ^ -879258317),
						gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref tcpPort, gGrLFhEdofbUymLMOb3.RREELfe2hN)
					}, dMZjkAEvscZOQ4dkL9w.GZiEg2xxMP));
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_84f159339c0c4d909a5aafadeb20c94b == 0)
					{
						num2 = 0;
					}
					break;
				case 7:
					return true;
				case 8:
					if (!N1tlkEcoZnwFWqSBqQf(text, lRNPJoPvsXOpQ7Ucdon.VmKEqmiqNI))
					{
						bridgeSession = new BridgeSession
						{
							ClientId = kJNr5TE9nGPD5ZHZBB3.jnq8ilIcK(ref _nextClientId, kJNr5TE9nGPD5ZHZBB3.MpJE88hbjE),
							Pid = pid,
							HttpPort = httpPort,
							TcpPort = tcpPort,
							Http = new WeComBridgeHttp(httpPort, text),
							Listener = listener
						};
						num2 = 15;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_90f58c7ffb21444ca88319aa26ca6aa0 != 0)
						{
							num2 = 15;
						}
					}
					else
					{
						num2 = 1;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_bd908099f891496e8274f46975b80fa0 == 0)
						{
							num2 = 0;
						}
					}
					break;
				case 1:
					try
					{
						nVdXdwhub0GVhDyNlIF.jnq8ilIcK(listener, nVdXdwhub0GVhDyNlIF.OlTh9S5DxM);
						int num3 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_9b487277fbad4617828ccf4adb74f260 != 0)
						{
							num3 = 0;
						}
						switch (num3)
						{
						case 0:
							break;
						}
					}
					catch
					{
						int num4 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_f0c3ce86ec4a4bf2b2132a6d66c41033 == 0)
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
				case 4:
					return false;
				case 9:
					fbYNxjcH1DFr9g4cFNN(bridgeSession);
					num2 = 10;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_b2c25dd18e364fe19659fd064f01adfa != 0)
					{
						num2 = 17;
					}
					break;
				}
				continue;
				end_IL_0012:
				break;
			}
			num = 5;
		}
	}

	private static int WaitStartedPid(int httpPort, out string error)
	{
		int num = 6;
		int num2 = num;
		DateTime dateTime2 = default(DateTime);
		int pid = default(int);
		string version = default(string);
		DateTime dateTime = default(DateTime);
		while (true)
		{
			switch (num2)
			{
			case 5:
				dateTime2 = YxhfgMcSVWyErXdREjP(Or2RQ3bh3dyXsRwDb52.KoIbYNfyXF);
				num2 = 12;
				break;
			case 2:
				return 0;
			case 10:
			case 14:
				return pid;
			case 8:
				if (TryReadHealth(httpPort, out pid, out version))
				{
					num2 = 4;
					break;
				}
				goto IL_0073;
			case 4:
				if (pid > 0)
				{
					num2 = 7;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_45ac76a6368943c6981dcfd9e66428ae == 0)
					{
						num2 = 7;
					}
					break;
				}
				goto IL_0073;
			default:
				if (!NgtBADEr5RrpT3l6FkM.jnq8ilIcK(version, (string)HBbvWMcE0vjQIqqqYSs(-1680728354 ^ -1680715040), NgtBADEr5RrpT3l6FkM.YGgE7TJZkD))
				{
					num2 = 14;
					break;
				}
				goto case 13;
			case 3:
				error = (string)uhAnXmcBtDKqQjGYOsb(HBbvWMcE0vjQIqqqYSs(0x789D7F68 ^ 0x789D290E), gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref httpPort, gGrLFhEdofbUymLMOb3.RREELfe2hN), z2jc63fLkugS1X8Q9N.tE1kD1vbB(-1517689377 ^ -1517702277), dDG4K3E4O2Ahv334CHT.mOZEwuXRLl);
				num2 = 1;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_fc096a82eb6f412ab19b9dee9fccbc6e != 0)
				{
					num2 = 1;
				}
				break;
			case 9:
			case 11:
				if (!hFANSiycpt2XpYYe2h8.jnq8ilIcK(Or2RQ3bh3dyXsRwDb52.jnq8ilIcK(Or2RQ3bh3dyXsRwDb52.KoIbYNfyXF), dateTime, hFANSiycpt2XpYYe2h8.yqKykUvbgx))
				{
					num2 = 3;
					break;
				}
				goto case 8;
			case 7:
				if (lRNPJoPvsXOpQ7Ucdon.jnq8ilIcK(version, lRNPJoPvsXOpQ7Ucdon.VmKEqmiqNI))
				{
					num2 = 10;
					break;
				}
				goto default;
			case 13:
				error = (string)uhAnXmcBtDKqQjGYOsb(HBbvWMcE0vjQIqqqYSs(0x1F6D7205 ^ 0x1F6D2453), version, z2jc63fLkugS1X8Q9N.tE1kD1vbB(-1857040513 ^ -1857053929), dDG4K3E4O2Ahv334CHT.mOZEwuXRLl);
				num2 = 2;
				break;
			case 1:
				return 0;
			case 6:
				error = null;
				num2 = 1;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_47f3813d118644b6b4369b1318743526 != 0)
				{
					num2 = 5;
				}
				break;
			case 12:
				{
					dateTime = rT7A8eUeTkEL4NogJZw.jnq8ilIcK(ref dateTime2, 20.0, rT7A8eUeTkEL4NogJZw.MWEQyQDa8m);
					num2 = 11;
					break;
				}
				IL_0073:
				iNxACn2fCKfnjj1l3cF.jnq8ilIcK(400, iNxACn2fCKfnjj1l3cF.zH92o9KkAB);
				num2 = 9;
				break;
			}
		}
	}

	private static void EnsureWatchTimer()
	{
		int num = 2;
		int num2 = num;
		while (true)
		{
			TimerCallback timerCallback;
			switch (num2)
			{
			default:
				return;
			case 1:
				return;
			case 2:
				if (_watchTimer == null)
				{
					timerCallback = <>c.<>9__23_0;
					if (timerCallback == null)
					{
						num2 = 3;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_2b51064baece4c398bf908a830a54cbf != 0)
						{
							num2 = 3;
						}
						continue;
					}
					break;
				}
				num2 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_a34de5cebe4f4eeda550f236a75b91de == 0)
				{
					num2 = 1;
				}
				continue;
			case 0:
				return;
			case 3:
				timerCallback = (<>c.<>9__23_0 = delegate
				{
					int num3 = 2;
					int num4 = num3;
					IEnumerator<BridgeSession> enumerator = default(IEnumerator<BridgeSession>);
					BridgeSession current = default(BridgeSession);
					while (true)
					{
						switch (num4)
						{
						default:
							return;
						case 0:
							return;
						case 1:
							try
							{
								while (true)
								{
									IL_0184:
									int num5;
									if (!<>c.G0OQecrWEciUt2KuMoG(enumerator, nsJ9LebXYwUJAHxJYqI.tcqbTnwQjH))
									{
										num5 = 0;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_93e9b0bbc1fe4ae1a0fca3879b36d9a6 != 0)
										{
											num5 = 0;
										}
										goto IL_0044;
									}
									goto IL_0081;
									IL_0040:
									int num6;
									num5 = num6;
									goto IL_0044;
									IL_0044:
									while (true)
									{
										switch (num5)
										{
										default:
											return;
										case 0:
											return;
										case 3:
											num6 = 5;
											goto IL_0040;
										case 6:
											break;
										case 1:
											num5 = 2;
											continue;
										case 8:
											if (current.Closed != null)
											{
												num5 = 7;
												continue;
											}
											goto case 1;
										case 2:
											try
											{
												aMU2i5y03HSWa4qUbCd.jnq8ilIcK(current.Pid, aMU2i5y03HSWa4qUbCd.gVwyAsDh1k);
												int num9 = 0;
												if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_aaf9213cc3c34825a5d331618f885f65 == 0)
												{
													num9 = 0;
												}
												switch (num9)
												{
												case 0:
													break;
												}
											}
											catch
											{
												int num10 = 0;
												if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_529db4a9032c417dbe74a080d04d36c2 == 0)
												{
													num10 = 1;
												}
												while (true)
												{
													switch (num10)
													{
													case 1:
														<>c.ddpshjkOcAIqkgL8gte(current, true);
														num10 = 0;
														if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_cb5bc3d191fc4bc3b6ad04816cd3def1 == 0)
														{
															num10 = 0;
														}
														continue;
													}
													break;
												}
												goto IL_0184;
											}
											goto case 3;
										case 4:
										case 7:
											goto IL_0184;
										case 5:
											try
											{
												<>c.qqMuFQkNHjRyI9wlQ39(current, false);
												int num7 = 0;
												if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_cc95e336569e4097a8a610a6e511075d == 0)
												{
													num7 = 0;
												}
												while (true)
												{
													switch (num7)
													{
													default:
														<>c.ec2O1OkRnFs6hQ4Z9md(current);
														num7 = 0;
														if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_305aaddad70a4f7fae7b0b1dbdd6ed1f == 0)
														{
															num7 = 1;
														}
														continue;
													case 1:
														break;
													}
													break;
												}
											}
											catch (Exception ex)
											{
												int num8 = 1;
												if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_529db4a9032c417dbe74a080d04d36c2 != 0)
												{
													num8 = 0;
												}
												while (true)
												{
													switch (num8)
													{
													case 1:
														<>c.QSJsW5kzQ4awbFka53h(<>c.kPikrOkswXT6VpyWFyV(z2jc63fLkugS1X8Q9N.tE1kD1vbB(-409114785 ^ -409126553), gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref current.ClientId, gGrLFhEdofbUymLMOb3.RREELfe2hN), z2jc63fLkugS1X8Q9N.tE1kD1vbB(0x2BF9058 ^ 0x2BFA802), <>c.X43Rxgk3oWnXBmQx0TW(ex, gjqRKtPV17ZUEQVAWxv.AdKPK0ve7w), m4SFBaEZmre5qY3dKfT.hGFE1HacV8));
														num8 = 0;
														if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_b4d4669a918b4b838c27ddb986baa9b6 == 0)
														{
															num8 = 0;
														}
														continue;
													case 0:
														break;
													}
													break;
												}
											}
											goto IL_0184;
										}
										break;
									}
									goto IL_0081;
									IL_0081:
									current = enumerator.Current;
									num6 = 8;
									goto IL_0040;
								}
							}
							finally
							{
								int num11;
								if (enumerator == null)
								{
									num11 = 0;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d91865d38b6c449295c8f801546a735d == 0)
									{
										num11 = 0;
									}
									goto IL_02e5;
								}
								goto IL_02fb;
								IL_02e5:
								switch (num11)
								{
								default:
									goto end_IL_02c0;
								case 2:
									break;
								case 0:
									goto end_IL_02c0;
								case 1:
									goto end_IL_02c0;
								}
								goto IL_02fb;
								IL_02fb:
								<>c.FZWfHnrPUIVT6r8KejK(enumerator, dsixtSPyYHjKiE5k7WK.VtIPNcTU9E);
								num11 = 1;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_37f38109cd8f46f5b0100026922644f5 == 0)
								{
									num11 = 0;
								}
								goto IL_02e5;
								end_IL_02c0:;
							}
						case 2:
							enumerator = ByClient.Values.GetEnumerator();
							num4 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_90f58c7ffb21444ca88319aa26ca6aa0 != 0)
							{
								num4 = 1;
							}
							break;
						}
					}
				});
				break;
			}
			_watchTimer = new Timer(timerCallback, null, 1000, 2000);
			num2 = 0;
			if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_265198d303964f58a1121245a89716c5 != 0)
			{
				num2 = 0;
			}
		}
	}

	private static void RefreshLogin(object session, bool initial)
	{
		int num = 2;
		string text = default(string);
		BridgeHttpResult bridgeHttpResult = default(BridgeHttpResult);
		bool flag2 = default(bool);
		bool flag = default(bool);
		string text2 = default(string);
		while (true)
		{
			int num2 = num;
			while (true)
			{
				int num4;
				int num3;
				object obj;
				switch (num2)
				{
				default:
					return;
				case 18:
					num4 = ((!zGiJK0EFxYWyQDBCddu.jnq8ilIcK((string)((BridgeSession)session).AccountId, text, StringComparison.Ordinal, zGiJK0EFxYWyQDBCddu.OaTEpqoroE)) ? 1 : 0);
					goto IL_047c;
				case 23:
					text = ((JToken)bridgeHttpResult.Json).Value<string>(HBbvWMcE0vjQIqqqYSs(0x1F6D7205 ^ 0x1F6D24D3));
					num2 = 15;
					continue;
				case 13:
					num3 = (((BridgeSession)session).LoginNotified ? 1 : 0);
					goto IL_0489;
				case 2:
					if (((BridgeSession)session).Closed == null)
					{
						num2 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d34bf7edba6543039b52c92f6f825304 == 0)
						{
							num2 = 1;
						}
						continue;
					}
					return;
				case 29:
					if (!((BridgeSession)session).LoginNotified)
					{
						num2 = 11;
						continue;
					}
					goto case 18;
				case 5:
					return;
				case 22:
					if (flag2)
					{
						num2 = 30;
						continue;
					}
					if (!flag)
					{
						num2 = 12;
						continue;
					}
					goto case 9;
				case 15:
					if (l2hSytck1xudCQ6WKdC(text2, z2jc63fLkugS1X8Q9N.tE1kD1vbB(-1521705861 ^ -1521694059), NgtBADEr5RrpT3l6FkM.bH7ElpvHlk))
					{
						num = 29;
						break;
					}
					num4 = 0;
					goto IL_047c;
				case 1:
					if (((BridgeSession)session).Http != null)
					{
						num2 = 10;
						continue;
					}
					return;
				case 4:
					if (bridgeHttpResult.Json != null)
					{
						num2 = 20;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_fd96f1535e384acb8fa11da99670a759 == 0)
						{
							num2 = 24;
						}
						continue;
					}
					return;
				case 9:
					oTNs1WcrNa24La2ssRx(session);
					num2 = 4;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_9b487277fbad4617828ccf4adb74f260 == 0)
					{
						num2 = 7;
					}
					continue;
				case 6:
					if (l2hSytck1xudCQ6WKdC(text2, HBbvWMcE0vjQIqqqYSs(-1420309017 ^ -1420322077), NgtBADEr5RrpT3l6FkM.bH7ElpvHlk))
					{
						num2 = 28;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_47f3813d118644b6b4369b1318743526 == 0)
						{
							num2 = 23;
						}
						continue;
					}
					return;
				case 25:
					if (!N1tlkEcoZnwFWqSBqQf(text, lRNPJoPvsXOpQ7Ucdon.VmKEqmiqNI))
					{
						num2 = 16;
						continue;
					}
					goto case 20;
				case 26:
					((BridgeSession)session).LoginState = text2;
					num2 = 25;
					continue;
				case 11:
					num4 = 1;
					goto IL_047c;
				case 0:
					return;
				case 21:
					return;
				case 10:
					bridgeHttpResult = (BridgeHttpResult)CFrTnvcniRQQXXSdR96(((BridgeSession)session).Http, z2jc63fLkugS1X8Q9N.tE1kD1vbB(0x780E5C1F ^ 0x780E0AAF));
					num2 = 8;
					continue;
				case 19:
					if (!l2hSytck1xudCQ6WKdC(text2, z2jc63fLkugS1X8Q9N.tE1kD1vbB(0x3D0866F ^ 0x3D0D16B), NgtBADEr5RrpT3l6FkM.bH7ElpvHlk))
					{
						num2 = 3;
						continue;
					}
					goto case 13;
				case 30:
					PublishLogin(session);
					num2 = 27;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_4067485f69dd42b795335d8574adc3dd == 0)
					{
						num2 = 26;
					}
					continue;
				case 8:
					if (bridgeHttpResult.Ok)
					{
						num2 = 4;
						continue;
					}
					return;
				case 14:
					return;
				case 24:
					obj = ((JToken)efjQ7ccc17w24KpXKpj(bridgeHttpResult)).Value<string>(HBbvWMcE0vjQIqqqYSs(-1680728354 ^ -1680716160));
					if (obj == null)
					{
						num2 = 17;
						continue;
					}
					goto IL_045f;
				case 7:
					return;
				case 12:
					if (!initial)
					{
						return;
					}
					num2 = 6;
					continue;
				case 27:
					return;
				case 16:
					((BridgeSession)session).AccountId = text;
					num = 20;
					break;
				case 28:
					k0NxU4cte3COFcR674G(uhAnXmcBtDKqQjGYOsb(HBbvWMcE0vjQIqqqYSs(-1916469888 ^ -1916491108), gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref ((BridgeSession)session).ClientId, gGrLFhEdofbUymLMOb3.RREELfe2hN), z2jc63fLkugS1X8Q9N.tE1kD1vbB(0x18F258A ^ 0x18F72A4), dDG4K3E4O2Ahv334CHT.mOZEwuXRLl));
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_291da815e7d24e5fad1c5f19ff1c2dd7 != 0)
					{
						num2 = 0;
					}
					continue;
				case 20:
					if (NgtBADEr5RrpT3l6FkM.jnq8ilIcK(text2, z2jc63fLkugS1X8Q9N.tE1kD1vbB(-1232431428 ^ -1232419328), NgtBADEr5RrpT3l6FkM.bH7ElpvHlk))
					{
						return;
					}
					num2 = 22;
					continue;
				case 17:
					obj = HBbvWMcE0vjQIqqqYSs(0x402CFC5F ^ 0x402CACE3);
					goto IL_045f;
				case 3:
					{
						num3 = 0;
						goto IL_0489;
					}
					IL_0489:
					flag = (byte)num3 != 0;
					num2 = 26;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_37f38109cd8f46f5b0100026922644f5 == 0)
					{
						num2 = 3;
					}
					continue;
					IL_045f:
					text2 = (string)obj;
					num2 = 9;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_2879e5bb2d3341f08a6f59d9f884899e == 0)
					{
						num2 = 23;
					}
					continue;
					IL_047c:
					flag2 = (byte)num4 != 0;
					num2 = 19;
					continue;
				}
				break;
			}
		}
	}

	private static void PublishLogin(object session)
	{
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Invalid comparison between Unknown and I4
		//IL_04ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e6: Expected O, but got Unknown
		//IL_04e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Expected O, but got Unknown
		//IL_0539: Expected O, but got Unknown
		//IL_0539: Unknown result type (might be due to invalid IL or missing references)
		//IL_058c: Expected O, but got Unknown
		//IL_058c: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e0: Expected O, but got Unknown
		//IL_05e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_062d: Expected O, but got Unknown
		//IL_062d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0675: Expected O, but got Unknown
		//IL_0675: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a0: Expected O, but got Unknown
		//IL_06a2: Expected O, but got Unknown
		int num = 19;
		JObject val3 = default(JObject);
		JToken val4 = default(JToken);
		JObject val2 = default(JObject);
		string text = default(string);
		while (true)
		{
			int num2 = num;
			while (true)
			{
				object obj;
				object obj3;
				object obj2;
				switch (num2)
				{
				case 7:
					dqfVK0cQl6eDUpgITv1(val3, z2jc63fLkugS1X8Q9N.tE1kD1vbB(-30119401 ^ -30130755), AJMSu3cdbMH0dIJx2DG("", b8txYwQmx8SvwU718wE.TU7QIcSjJQ), WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
					num2 = 14;
					continue;
				case 3:
					KqwxwacqR9oQoIEkWSs(val3, HBbvWMcE0vjQIqqqYSs(-1164549976 ^ -1164560566), EnngxCbLyiM9g6bVrXJ.ANvh8JX5AD);
					num2 = 15;
					continue;
				case 4:
					if (val4 != null)
					{
						num2 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_4067485f69dd42b795335d8574adc3dd == 0)
						{
							num2 = 0;
						}
						continue;
					}
					goto case 15;
				case 23:
					if (!N1tlkEcoZnwFWqSBqQf(Extensions.Value<string>((IEnumerable<JToken>)val4), lRNPJoPvsXOpQ7Ucdon.yHSEjK7hI0))
					{
						num2 = 27;
						continue;
					}
					goto case 3;
				case 28:
					CE9J7scLSNqIvXO9WKK(val3, val2);
					num2 = 6;
					continue;
				case 13:
					return;
				case 22:
					WSJ5GQQ9myN2N0ma3xj.jnq8ilIcK(val3, (string)HBbvWMcE0vjQIqqqYSs(0xAC5AB62 ^ 0xAC5FCA2), (JToken)AJMSu3cdbMH0dIJx2DG("", b8txYwQmx8SvwU718wE.TU7QIcSjJQ), WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
					num2 = 14;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_f8d69a0d83e2420b83127060d822cafd == 0)
					{
						num2 = 21;
					}
					continue;
				case 10:
				case 21:
					val4 = (JToken)yTaeC9c7k2aDsaY5WXu(val3, HBbvWMcE0vjQIqqqYSs(0x1BD51FB4 ^ 0x1BD54856), YNuvA8QCYEcgOkEROsU.YOmQH5bNe7);
					num = 4;
					break;
				case 9:
					dD6Uu9cM0cWoIyKJtb6(((BridgeSession)session).ClientId, 11179, val3);
					num2 = 9;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_4819ae0b3e934df4906fdac118b2ddf1 != 0)
					{
						num2 = 13;
					}
					continue;
				case 20:
					((BridgeSession)session).LoginNotified = true;
					num = 2;
					break;
				case 1:
					obj = ((BridgeSession)session).AccountId;
					if (obj == null)
					{
						num2 = 17;
						continue;
					}
					goto IL_0467;
				case 11:
				case 12:
					if (!N1tlkEcoZnwFWqSBqQf(((JToken)val3).Value<string>((object)z2jc63fLkugS1X8Q9N.tE1kD1vbB(0x18907F3 ^ 0x1895059)), lRNPJoPvsXOpQ7Ucdon.VmKEqmiqNI))
					{
						num2 = 8;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_9a207314288741c0b10cb80afbe48750 != 0)
						{
							num2 = 16;
						}
						continue;
					}
					goto case 7;
				case 25:
					((BridgeSession)session).UserId = text;
					num2 = 6;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_a34de5cebe4f4eeda550f236a75b91de == 0)
					{
						num2 = 8;
					}
					continue;
				case 15:
				case 27:
					PublishLoginEvent(((BridgeSession)session).ClientId, 11026, val3);
					num2 = 9;
					continue;
				default:
					if ((int)NeoPFwQk2iNfdIMalFB.jnq8ilIcK(val4, NeoPFwQk2iNfdIMalFB.WoaQrXAehF) == 8)
					{
						num2 = 14;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_e0bd3b55ee8d41bab3f8a58185604cae == 0)
						{
							num2 = 23;
						}
						continue;
					}
					goto case 15;
				case 14:
				case 16:
					if (!N1tlkEcoZnwFWqSBqQf(((JToken)val3).Value<string>(HBbvWMcE0vjQIqqqYSs(0x4802C059 ^ 0x48029799)), lRNPJoPvsXOpQ7Ucdon.VmKEqmiqNI))
					{
						num2 = 10;
						continue;
					}
					goto case 22;
				case 6:
					if (!N1tlkEcoZnwFWqSBqQf(((JToken)val3).Value<string>((object)z2jc63fLkugS1X8Q9N.tE1kD1vbB(0x2193D850 ^ 0x21938FC8)), lRNPJoPvsXOpQ7Ucdon.VmKEqmiqNI))
					{
						num2 = 11;
						continue;
					}
					goto case 24;
				case 19:
				{
					object obj4 = efjQ7ccc17w24KpXKpj(CFrTnvcniRQQXXSdR96(((BridgeSession)session).Http, HBbvWMcE0vjQIqqqYSs(0x234FD180 ^ 0x234F86BE)));
					if (obj4 == null)
					{
						num2 = 18;
						continue;
					}
					obj3 = yTaeC9c7k2aDsaY5WXu(obj4, HBbvWMcE0vjQIqqqYSs(0x518E8229 ^ 0x518ED573), YNuvA8QCYEcgOkEROsU.u3pQcyYiE5);
					goto IL_03d9;
				}
				case 18:
					obj3 = null;
					goto IL_03d9;
				case 26:
					obj2 = (object)new JObject();
					goto IL_0409;
				case 5:
					obj = ((JToken)val2).Value<string>(HBbvWMcE0vjQIqqqYSs(0x745A8A68 ^ 0x745ACD9C));
					if (obj != null)
					{
						goto IL_0467;
					}
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_e0bd3b55ee8d41bab3f8a58185604cae == 0)
					{
						num2 = 1;
					}
					continue;
				case 17:
					obj = "";
					goto IL_0467;
				case 8:
					((BridgeSession)session).AccountId = (N1tlkEcoZnwFWqSBqQf(((BridgeSession)session).AccountId, lRNPJoPvsXOpQ7Ucdon.VmKEqmiqNI) ? text : ((BridgeSession)session).AccountId);
					num2 = 20;
					continue;
				case 2:
				{
					JObject val = new JObject();
					dqfVK0cQl6eDUpgITv1((object)val, HBbvWMcE0vjQIqqqYSs(0x15454A1E ^ 0x15450DEA), AJMSu3cdbMH0dIJx2DG(text, b8txYwQmx8SvwU718wE.TU7QIcSjJQ), WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
					WSJ5GQQ9myN2N0ma3xj.jnq8ilIcK((object)val, (string)HBbvWMcE0vjQIqqqYSs(-1613644704 ^ -1613628544), (JToken)AJMSu3cdbMH0dIJx2DG(((JToken)val2).Value<string>(HBbvWMcE0vjQIqqqYSs(0x3EDBC22B ^ 0x3EDB954D)) ?? "", b8txYwQmx8SvwU718wE.TU7QIcSjJQ), WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
					dqfVK0cQl6eDUpgITv1((object)val, HBbvWMcE0vjQIqqqYSs(-546866494 ^ -546884924), b8txYwQmx8SvwU718wE.jnq8ilIcK(((JToken)val2).Value<string>(HBbvWMcE0vjQIqqqYSs(-30119401 ^ -30130843)) ?? "", b8txYwQmx8SvwU718wE.TU7QIcSjJQ), WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
					dqfVK0cQl6eDUpgITv1((object)val, z2jc63fLkugS1X8Q9N.tE1kD1vbB(-32776863 ^ -32795273), b8txYwQmx8SvwU718wE.jnq8ilIcK(((JToken)val2).Value<string>(HBbvWMcE0vjQIqqqYSs(-40068700 ^ -40054350)) ?? "", b8txYwQmx8SvwU718wE.TU7QIcSjJQ), WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
					dqfVK0cQl6eDUpgITv1((object)val, z2jc63fLkugS1X8Q9N.tE1kD1vbB(-569117117 ^ -569131419), AJMSu3cdbMH0dIJx2DG(((JToken)val2).Value<string>(HBbvWMcE0vjQIqqqYSs(-1947700697 ^ -1947715071)) ?? "", b8txYwQmx8SvwU718wE.TU7QIcSjJQ), WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
					dqfVK0cQl6eDUpgITv1((object)val, HBbvWMcE0vjQIqqqYSs(0x7AFEF0A3 ^ 0x7AFEB89B), AJMSu3cdbMH0dIJx2DG(((JToken)val2).Value<string>(HBbvWMcE0vjQIqqqYSs(-1977692036 ^ -1977710602)) ?? "", b8txYwQmx8SvwU718wE.TU7QIcSjJQ), WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
					dqfVK0cQl6eDUpgITv1((object)val, HBbvWMcE0vjQIqqqYSs(-1356900703 ^ -1356914967), E9hPr6QvNKRyIQM6QVf.jnq8ilIcK(((BridgeSession)session).Pid, E9hPr6QvNKRyIQM6QVf.MB5QgmjD5J), WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
					val3 = val;
					num2 = 28;
					continue;
				}
				case 24:
					{
						dqfVK0cQl6eDUpgITv1(val3, HBbvWMcE0vjQIqqqYSs(-1781563239 ^ -1781541119), b8txYwQmx8SvwU718wE.jnq8ilIcK(((JToken)val3).Value<string>((object)z2jc63fLkugS1X8Q9N.tE1kD1vbB(-2068695740 ^ -2068677252)) ?? "", b8txYwQmx8SvwU718wE.TU7QIcSjJQ), WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
						num2 = 8;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_2d7366bbeab1424ea4ee5fb2951fe83a != 0)
						{
							num2 = 12;
						}
						continue;
					}
					IL_0467:
					text = (string)obj;
					num2 = 14;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_e3198421401646eabf32dc544445bbbe == 0)
					{
						num2 = 25;
					}
					continue;
					IL_03d9:
					obj2 = ((obj3 is JObject) ? obj3 : null);
					if (obj2 != null)
					{
						goto IL_0409;
					}
					num2 = 15;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_f8d69a0d83e2420b83127060d822cafd == 0)
					{
						num2 = 26;
					}
					continue;
					IL_0409:
					val2 = (JObject)obj2;
					num2 = 5;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_32c4deae05384207a567e3dea0ef9d3f != 0)
					{
						num2 = 3;
					}
					continue;
				}
				break;
			}
		}
	}

	private static void PublishLoginEvent(int clientId, int type, object data)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_007e: Expected O, but got Unknown
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
			{
				JObject val = new JObject();
				WSJ5GQQ9myN2N0ma3xj.jnq8ilIcK((object)val, z2jc63fLkugS1X8Q9N.tE1kD1vbB(0x6DD7D0D0 ^ 0x6DD7EF2E), E9hPr6QvNKRyIQM6QVf.jnq8ilIcK(type, E9hPr6QvNKRyIQM6QVf.MB5QgmjD5J), WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
				WSJ5GQQ9myN2N0ma3xj.jnq8ilIcK((object)val, (string)HBbvWMcE0vjQIqqqYSs(-40068700 ^ -40066240), (JToken)data, WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
				MxoMeicuaYHy1qsmXMv(clientId, (object)val);
				num2 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_9de6b4ecde674f909622ae29ccaad31e == 0)
				{
					num2 = 0;
				}
				break;
			}
			}
		}
	}

	private static void PullAccountEvents(object session)
	{
		int num = 12;
		object gate = default(object);
		bool flag = default(bool);
		string text2 = default(string);
		string text3 = default(string);
		string text = default(string);
		BridgeHttpResult bridgeHttpResult = default(BridgeHttpResult);
		List<string> value = default(List<string>);
		IEnumerator<JToken> enumerator = default(IEnumerator<JToken>);
		JObject val2 = default(JObject);
		string text4 = default(string);
		JArray val = default(JArray);
		while (true)
		{
			int num2 = num;
			while (true)
			{
				switch (num2)
				{
				default:
					return;
				case 21:
					try
					{
						BXDm5m2zddwJCBpuXOu.jnq8ilIcK(gate, ref flag, BXDm5m2zddwJCBpuXOu.wEHUWvu4fJ);
						int num6 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_5a84d0ea826a44b299199b540d6e6fc8 == 0)
						{
							num6 = 0;
						}
						while (true)
						{
							switch (num6)
							{
							default:
								((BridgeSession)session).Cursors[(string)HBbvWMcE0vjQIqqqYSs(0x1A739410 ^ 0x1A73C3FC)] = new List<string> { text2 };
								num6 = 1;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_bc19572226e743cb89917af019cec5df == 0)
								{
									num6 = 1;
								}
								continue;
							case 1:
								break;
							}
							break;
						}
					}
					finally
					{
						int num7;
						if (!flag)
						{
							num7 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_f0c3ce86ec4a4bf2b2132a6d66c41033 == 0)
							{
								num7 = 0;
							}
							goto IL_0137;
						}
						goto IL_016c;
						IL_0137:
						switch (num7)
						{
						default:
							goto end_IL_0112;
						case 0:
							goto end_IL_0112;
						case 1:
							break;
						case 2:
							goto end_IL_0112;
						}
						goto IL_016c;
						IL_016c:
						Y3rNcgUPMP670sjp06c.jnq8ilIcK(gate, Y3rNcgUPMP670sjp06c.FPeUEHAdO7);
						num7 = 2;
						goto IL_0137;
						end_IL_0112:;
					}
					goto case 16;
				case 14:
					try
					{
						BXDm5m2zddwJCBpuXOu.jnq8ilIcK(gate, ref flag, BXDm5m2zddwJCBpuXOu.wEHUWvu4fJ);
						int num12 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_fc096a82eb6f412ab19b9dee9fccbc6e == 0)
						{
							num12 = 1;
						}
						while (true)
						{
							switch (num12)
							{
							case 1:
								((BridgeSession)session).Cursors.Remove((string)HBbvWMcE0vjQIqqqYSs(0x62E15CB8 ^ 0x62E10B54));
								num12 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_bd908099f891496e8274f46975b80fa0 != 0)
								{
									num12 = 0;
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
						int num13;
						if (!flag)
						{
							num13 = 2;
							goto IL_023f;
						}
						goto IL_0255;
						IL_0255:
						Y3rNcgUPMP670sjp06c.jnq8ilIcK(gate, Y3rNcgUPMP670sjp06c.FPeUEHAdO7);
						num13 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_32c4deae05384207a567e3dea0ef9d3f == 0)
						{
							num13 = 0;
						}
						goto IL_023f;
						IL_023f:
						switch (num13)
						{
						default:
							goto end_IL_022a;
						case 1:
							break;
						case 2:
							goto end_IL_022a;
						case 0:
							goto end_IL_022a;
						}
						goto IL_0255;
						end_IL_022a:;
					}
					goto case 24;
				case 22:
					text3 = (string)uhAnXmcBtDKqQjGYOsb(text3, HBbvWMcE0vjQIqqqYSs(0x53DF8D7A ^ 0x53DFD542), sGWPE2c4NepDsZccdh9(text, JjMeHVb3TPv6k2AB0Oq.Cd6havHdxR), dDG4K3E4O2Ahv334CHT.mOZEwuXRLl);
					num2 = 26;
					continue;
				case 8:
					text3 = (string)HBbvWMcE0vjQIqqqYSs(-153892167 ^ -153906507);
					num2 = 25;
					continue;
				case 25:
					if (!N1tlkEcoZnwFWqSBqQf(text, lRNPJoPvsXOpQ7Ucdon.VmKEqmiqNI))
					{
						num2 = 22;
						continue;
					}
					goto case 26;
				case 26:
					bridgeHttpResult = (BridgeHttpResult)CFrTnvcniRQQXXSdR96(((BridgeSession)session).Http, text3);
					num2 = 1;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_7f8cf60e2e7b454b95cc4d13162f5088 == 0)
					{
						num2 = 1;
					}
					continue;
				case 19:
					gate = ((BridgeSession)session).Gate;
					num2 = 9;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_fe6655b3fe1741f7b687606f3c613956 != 0)
					{
						num2 = 7;
					}
					continue;
				case 24:
					Bkkwbdcv6rQF0k1Yk9p(session, false);
					num2 = 18;
					continue;
				case 27:
					flag = false;
					num2 = 21;
					continue;
				case 5:
					return;
				case 10:
					return;
				case 23:
					try
					{
						BXDm5m2zddwJCBpuXOu.jnq8ilIcK(gate, ref flag, BXDm5m2zddwJCBpuXOu.wEHUWvu4fJ);
						int num3 = 2;
						int num4 = num3;
						while (true)
						{
							switch (num4)
							{
							case 1:
								text = value[0];
								num4 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d604b1379fb540e5b6d79e791c2f5762 == 0)
								{
									num4 = 0;
								}
								continue;
							case 2:
								if (((BridgeSession)session).Cursors.TryGetValue((string)HBbvWMcE0vjQIqqqYSs(0x73CC8E5C ^ 0x73CCD9B0), out value))
								{
									num4 = 0;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_a4eb1fc2846b481197982b0faf1ae1eb == 0)
									{
										num4 = 4;
									}
									continue;
								}
								break;
							case 3:
								break;
							case 4:
								if (XXbh4Wcj22P27vOKVyf(value) <= 0)
								{
									num4 = 3;
									continue;
								}
								goto case 1;
							case 0:
								break;
							}
							break;
						}
					}
					finally
					{
						int num5;
						if (!flag)
						{
							num5 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_90f58c7ffb21444ca88319aa26ca6aa0 == 0)
							{
								num5 = 0;
							}
							goto IL_04ac;
						}
						goto IL_04e1;
						IL_04ac:
						switch (num5)
						{
						default:
							goto end_IL_0487;
						case 0:
							goto end_IL_0487;
						case 1:
							break;
						case 2:
							goto end_IL_0487;
						}
						goto IL_04e1;
						IL_04e1:
						LEQw5hclW0ZORePYTDQ(gate, Y3rNcgUPMP670sjp06c.FPeUEHAdO7);
						num5 = 2;
						goto IL_04ac;
						end_IL_0487:;
					}
					goto case 8;
				case 4:
					flag = false;
					num2 = 23;
					continue;
				case 20:
					gate = ((BridgeSession)session).Gate;
					num = 27;
					break;
				case 6:
					if (efjQ7ccc17w24KpXKpj(bridgeHttpResult) != null)
					{
						text2 = ((JToken)efjQ7ccc17w24KpXKpj(bridgeHttpResult)).Value<string>(HBbvWMcE0vjQIqqqYSs(-1821667360 ^ -1821648980));
						num = 3;
						break;
					}
					num2 = 10;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_0a8dc1e2655145c5af97263c14d91cce == 0)
					{
						num2 = 1;
					}
					continue;
				case 12:
					text = null;
					num2 = 11;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_981aa8b195a34f3abc27084c24a88bcd == 0)
					{
						num2 = 2;
					}
					continue;
				case 13:
					try
					{
						while (true)
						{
							int num8;
							if (!nsJ9LebXYwUJAHxJYqI.jnq8ilIcK(enumerator, nsJ9LebXYwUJAHxJYqI.tcqbTnwQjH))
							{
								num8 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_fe6655b3fe1741f7b687606f3c613956 == 0)
								{
									num8 = 0;
								}
								goto IL_058c;
							}
							goto IL_07cf;
							IL_07cf:
							JToken current = enumerator.Current;
							val2 = (JObject)(object)((current is JObject) ? current : null);
							num8 = 7;
							goto IL_058c;
							IL_058c:
							while (true)
							{
								switch (num8)
								{
								default:
									return;
								case 5:
									flag = false;
									num8 = 3;
									continue;
								case 8:
									if (!N1tlkEcoZnwFWqSBqQf(text4, lRNPJoPvsXOpQ7Ucdon.VmKEqmiqNI))
									{
										num8 = 4;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_b0daf8cde81544818533e86ea9e2d2be == 0)
										{
											num8 = 3;
										}
										continue;
									}
									goto case 2;
								case 7:
									if (val2 != null)
									{
										num8 = 6;
										continue;
									}
									break;
								case 2:
									DispatchAccountEvent(session, val2, ((JToken)efjQ7ccc17w24KpXKpj(bridgeHttpResult)).Value<string>(HBbvWMcE0vjQIqqqYSs(-739817959 ^ -739828079)));
									num8 = 1;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_cbbf75a1fc4b4ad1ae851089027cdc57 == 0)
									{
										num8 = 1;
									}
									continue;
								case 6:
									text4 = ((JToken)val2).Value<string>(HBbvWMcE0vjQIqqqYSs(0x18907F3 ^ 0x1895F87));
									num8 = 8;
									continue;
								case 3:
									try
									{
										BXDm5m2zddwJCBpuXOu.jnq8ilIcK(gate, ref flag, BXDm5m2zddwJCBpuXOu.wEHUWvu4fJ);
										int num9 = 0;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d9b8fefff9de4f9c9c2f7dc1a6c24ab2 == 0)
										{
											num9 = 0;
										}
										while (true)
										{
											switch (num9)
											{
											case 1:
											case 2:
												break;
											case 3:
												goto end_IL_0675;
											default:
												if (!((BridgeSession)session).SeenEventIds.Add(text4))
												{
													goto end_IL_058c;
												}
												num9 = 1;
												if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_a7a51f657ad14a8fa2f52f4d42963791 == 0)
												{
													num9 = 2;
												}
												continue;
											}
											break;
										}
										goto case 2;
										end_IL_0675:;
									}
									finally
									{
										if (flag)
										{
											int num10 = 0;
											if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1bbe7758123e4bc485e91d98227e25e4 != 0)
											{
												num10 = 0;
											}
											while (true)
											{
												switch (num10)
												{
												default:
													Y3rNcgUPMP670sjp06c.jnq8ilIcK(gate, Y3rNcgUPMP670sjp06c.FPeUEHAdO7);
													num10 = 1;
													if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_b3eccb7902c54a0db09f3abb83d000b4 == 0)
													{
														num10 = 1;
													}
													continue;
												case 1:
													break;
												}
												break;
											}
										}
									}
									break;
								case 4:
									gate = ((BridgeSession)session).Gate;
									num8 = 3;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_fc096a82eb6f412ab19b9dee9fccbc6e == 0)
									{
										num8 = 5;
									}
									continue;
								case 1:
									break;
								case 9:
									goto IL_07cf;
								case 0:
									return;
									end_IL_058c:
									break;
								}
								break;
							}
						}
					}
					finally
					{
						int num11;
						if (enumerator == null)
						{
							num11 = 1;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_45ca2c3eaeb74f8484cde152ae6e3eef == 0)
							{
								num11 = 1;
							}
							goto IL_0811;
						}
						goto IL_083a;
						IL_0811:
						switch (num11)
						{
						default:
							goto end_IL_07ec;
						case 1:
							goto end_IL_07ec;
						case 2:
							break;
						case 0:
							goto end_IL_07ec;
						}
						goto IL_083a;
						IL_083a:
						gf0Rq0cgyWhibXFkBEM(enumerator, dsixtSPyYHjKiE5k7WK.VtIPNcTU9E);
						num11 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_2b51064baece4c398bf908a830a54cbf == 0)
						{
							num11 = 0;
						}
						goto IL_0811;
						end_IL_07ec:;
					}
				case 7:
					return;
				case 16:
				case 17:
				{
					object obj = yTaeC9c7k2aDsaY5WXu(bridgeHttpResult.Json, HBbvWMcE0vjQIqqqYSs(0x53DF8D7A ^ 0x53DFD51C), YNuvA8QCYEcgOkEROsU.YOmQH5bNe7);
					val = (JArray)((obj is JArray) ? obj : null);
					num2 = 15;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1ea7d0fa701d41e395143e480836ec6b == 0)
					{
						num2 = 14;
					}
					continue;
				}
				case 9:
					flag = false;
					num2 = 14;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_cc95e336569e4097a8a610a6e511075d != 0)
					{
						num2 = 5;
					}
					continue;
				case 11:
					gate = ((BridgeSession)session).Gate;
					num2 = 4;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_9e74138bb8d34abb8ed7790887860771 == 0)
					{
						num2 = 3;
					}
					continue;
				case 0:
					return;
				case 2:
					enumerator = gcr8nnhm2CfUEhgCYDN.jnq8ilIcK(val, gcr8nnhm2CfUEhgCYDN.qAThIS7U5q);
					num2 = 13;
					continue;
				case 15:
					if (val == null)
					{
						return;
					}
					num2 = 2;
					continue;
				case 3:
					if (N1tlkEcoZnwFWqSBqQf(text2, lRNPJoPvsXOpQ7Ucdon.VmKEqmiqNI))
					{
						num2 = 17;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_84f159339c0c4d909a5aafadeb20c94b == 0)
						{
							num2 = 1;
						}
						continue;
					}
					goto case 20;
				case 1:
					if (pF95dJcwP9HICQvK3w2(bridgeHttpResult) == 409)
					{
						num2 = 19;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_5a84d0ea826a44b299199b540d6e6fc8 == 0)
						{
							num2 = 13;
						}
						continue;
					}
					if (!bridgeHttpResult.Ok)
					{
						num = 5;
						break;
					}
					goto case 6;
				case 18:
					return;
				}
				break;
			}
		}
	}

	public static void OnTcpJson(object session, object json)
	{
		int num = 4;
		JObject val2 = default(JObject);
		JObject val = default(JObject);
		string text = default(string);
		object gate = default(object);
		bool flag = default(bool);
		while (true)
		{
			int num2 = num;
			while (true)
			{
				switch (num2)
				{
				default:
					if (((BridgeSession)session).Closed != null)
					{
						num2 = 13;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_9e74138bb8d34abb8ed7790887860771 == 0)
						{
							num2 = 7;
						}
						continue;
					}
					break;
				case 1:
					if (KciCTkcmfLJ00wco8W6(val2))
					{
						return;
					}
					goto end_IL_0012;
				case 9:
					return;
				case 14:
					MxoMeicuaYHy1qsmXMv(((BridgeSession)session).ClientId, val);
					num2 = 11;
					continue;
				case 3:
					return;
				case 13:
					return;
				case 16:
					return;
				case 8:
					num2 = 12;
					continue;
				case 6:
					text = ((JToken)val2).Value<string>((object)z2jc63fLkugS1X8Q9N.tE1kD1vbB(-1778800683 ^ -1778778207));
					num2 = 9;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_9b487277fbad4617828ccf4adb74f260 == 0)
					{
						num2 = 20;
					}
					continue;
				case 17:
					gate = ((BridgeSession)session).Gate;
					num2 = 22;
					continue;
				case 12:
					try
					{
						val2 = (JObject)PQlnPRc8aiFLYTynHIF(json, Jd2FevQG6huGknEZHcv.OABQ6b4sTo);
						int num5 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_7f545199031c4e3289afcf313932f580 == 0)
						{
							num5 = 0;
						}
						switch (num5)
						{
						case 0:
							break;
						}
					}
					catch
					{
						int num6 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1090bd5856ad49da8290b4ab724faee3 == 0)
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
							lymWLNceeC5sWAA6hPE(XnUyVEc5Tkm1KK2H1pc(HBbvWMcE0vjQIqqqYSs(0x2F53E4C2 ^ 0x2F53BC6E), gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref ((BridgeSession)session).ClientId, gGrLFhEdofbUymLMOb3.RREELfe2hN), RxUSOmPDJGsMmCCGPPg.hbkPt9hhua));
							num6 = 1;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_76fed529d5134afd95ce65e7c04235fc == 0)
							{
								num6 = 1;
							}
						}
					}
					goto case 1;
				case 15:
					return;
				case 19:
					if (NgtBADEr5RrpT3l6FkM.jnq8ilIcK(((JToken)val2).Value<string>(HBbvWMcE0vjQIqqqYSs(0x31FC05B1 ^ 0x31FC3A4F)), (string)HBbvWMcE0vjQIqqqYSs(0x31FC05B1 ^ 0x31FC5D63), NgtBADEr5RrpT3l6FkM.bH7ElpvHlk))
					{
						num2 = 1;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_2879e5bb2d3341f08a6f59d9f884899e == 0)
						{
							num2 = 6;
						}
					}
					else
					{
						val = (JObject)ADYAmEcI3cfnhIKdmXm(session, val2, json);
						num2 = 21;
					}
					continue;
				case 4:
					if (session == null)
					{
						num2 = 3;
						continue;
					}
					goto default;
				case 5:
					return;
				case 22:
					flag = false;
					num2 = 7;
					continue;
				case 7:
					try
					{
						BXDm5m2zddwJCBpuXOu.jnq8ilIcK(gate, ref flag, BXDm5m2zddwJCBpuXOu.wEHUWvu4fJ);
						int num3 = 1;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_cb5bc3d191fc4bc3b6ad04816cd3def1 != 0)
						{
							num3 = 1;
						}
						while (true)
						{
							switch (num3)
							{
							case 3:
								return;
							case 2:
								break;
							case 1:
								if (!((BridgeSession)session).SeenEventIds.Add(text))
								{
									return;
								}
								num3 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_fc096a82eb6f412ab19b9dee9fccbc6e == 0)
								{
									num3 = 2;
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
						if (flag)
						{
							int num4 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_aa36ee1765db47c28f80e17ee8aff510 == 0)
							{
								num4 = 0;
							}
							while (true)
							{
								switch (num4)
								{
								default:
									Y3rNcgUPMP670sjp06c.jnq8ilIcK(gate, Y3rNcgUPMP670sjp06c.FPeUEHAdO7);
									num4 = 1;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_981aa8b195a34f3abc27084c24a88bcd == 0)
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
					}
					goto case 2;
				case 2:
				case 10:
					DispatchAccountEvent(session, val2, ((JToken)val2).Value<string>(HBbvWMcE0vjQIqqqYSs(0x44D4B4E9 ^ 0x44D4EC61)));
					num2 = 5;
					continue;
				case 20:
					if (N1tlkEcoZnwFWqSBqQf(text, lRNPJoPvsXOpQ7Ucdon.VmKEqmiqNI))
					{
						num2 = 2;
						continue;
					}
					goto case 17;
				case 11:
					return;
				case 21:
					if (val == null)
					{
						return;
					}
					num2 = 14;
					continue;
				case 18:
					break;
				}
				if (dRwJtUc9LYEoyus2XOp(((BridgeSession)session).ClientId))
				{
					return;
				}
				num2 = 8;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_b4d4669a918b4b838c27ddb986baa9b6 == 0)
				{
					num2 = 8;
				}
				continue;
				end_IL_0012:
				break;
			}
			num = 19;
		}
	}

	private static bool IsHeartbeat(object obj)
	{
		int num = 2;
		int num2 = num;
		string text = default(string);
		while (true)
		{
			switch (num2)
			{
			case 2:
			{
				string text2 = ((JToken)obj).Value<string>(HBbvWMcE0vjQIqqqYSs(-2131106415 ^ -2131109265));
				text = ((JToken)obj).Value<string>(HBbvWMcE0vjQIqqqYSs(--172341062 ^ 0xA45EFB6));
				if (!l2hSytck1xudCQ6WKdC(text2, HBbvWMcE0vjQIqqqYSs(0x3BA46780 ^ 0x3BA43E86), NgtBADEr5RrpT3l6FkM.bH7ElpvHlk))
				{
					num2 = 1;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_9f3937ff2c594a3ebca747058292ba44 == 0)
					{
						num2 = 0;
					}
					continue;
				}
				goto default;
			}
			case 1:
				if (NgtBADEr5RrpT3l6FkM.jnq8ilIcK(text, (string)HBbvWMcE0vjQIqqqYSs(-1510018411 ^ -1510036599), NgtBADEr5RrpT3l6FkM.bH7ElpvHlk))
				{
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_80394cf8f0664ed7a0669b96499a0b6e == 0)
					{
						num2 = 0;
					}
					continue;
				}
				break;
			default:
				return true;
			case 3:
				break;
			}
			if (!l2hSytck1xudCQ6WKdC(text, HBbvWMcE0vjQIqqqYSs(0x518E8229 ^ 0x518EDB1F), NgtBADEr5RrpT3l6FkM.bH7ElpvHlk))
			{
				break;
			}
			num2 = 1;
			if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_e0bd3b55ee8d41bab3f8a58185604cae == 0)
			{
				num2 = 4;
			}
		}
		return l2hSytck1xudCQ6WKdC(((JToken)obj).Value<string>((object)z2jc63fLkugS1X8Q9N.tE1kD1vbB(-1796327704 ^ -1796315200)), HBbvWMcE0vjQIqqqYSs(-896341688 ^ -896356328), NgtBADEr5RrpT3l6FkM.bH7ElpvHlk);
	}

	private static void CloseSession(object session, bool notify)
	{
		//IL_01e6: Expected O, but got I4
		int num = 7;
		string text = default(string);
		while (true)
		{
			int num2 = num;
			while (true)
			{
				BridgeSession value;
				switch (num2)
				{
				case 2:
					return;
				case 4:
					return;
				case 12:
					return;
				case 10:
					if (((BridgeSession)session).Closed == null)
					{
						num2 = 14;
						continue;
					}
					return;
				case 3:
					ByPid.TryRemove(((BridgeSession)session).Pid, out value);
					num2 = 11;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_dc0e5af6a1d1414daf07ee1c98387cef == 0)
					{
						num2 = 8;
					}
					continue;
				default:
					try
					{
						object http = ((BridgeSession)session).Http;
						int num3;
						if (http == null)
						{
							num3 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_289d7cd175a8485b8ea0e81b4750a0f3 != 0)
							{
								num3 = 1;
							}
						}
						else
						{
							((WeComBridgeHttp)http).Dispose();
							num3 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c09e27dcabbb4f6484836a4e1fd5da8e == 0)
							{
								num3 = 0;
							}
						}
						switch (num3)
						{
						case 1:
							break;
						case 0:
							break;
						}
					}
					catch
					{
						int num4 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_0e5a04c5423148499c9b265cadee0019 != 0)
						{
							num4 = 0;
						}
						switch (num4)
						{
						case 0:
							break;
						}
					}
					goto case 9;
				case 9:
					if (!notify)
					{
						num2 = 2;
						continue;
					}
					goto case 1;
				case 11:
					YovNMkc6sKgfHjsqhPH(session);
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_76a1008ec43840dda25966a6b044ac77 == 0)
					{
						num2 = 0;
					}
					continue;
				case 7:
					if (session == null)
					{
						num2 = 6;
						continue;
					}
					goto case 10;
				case 6:
					return;
				case 8:
					return;
				case 14:
					break;
				case 5:
					MxoMeicuaYHy1qsmXMv(((BridgeSession)session).ClientId, tgMoKwc1iMYDewg0p7a(session, text));
					num2 = 12;
					continue;
				case 13:
					ByClient.TryRemove(((BridgeSession)session).ClientId, out value);
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_bc19572226e743cb89917af019cec5df != 0)
					{
						num2 = 3;
					}
					continue;
				case 1:
				{
					bool loginNotified = ((BridgeSession)session).LoginNotified;
					text = (string)(N1tlkEcoZnwFWqSBqQf(((BridgeSession)session).UserId, lRNPJoPvsXOpQ7Ucdon.VmKEqmiqNI) ? ((BridgeSession)session).AccountId : ((BridgeSession)session).UserId);
					((BridgeSession)session).LoginNotified = false;
					fiPU8qcZVFeLaOV7hrL(((BridgeSession)session).ClientId);
					if (!loginNotified)
					{
						num2 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_9de6b4ecde674f909622ae29ccaad31e != 0)
						{
							num2 = 4;
						}
						continue;
					}
					goto case 5;
				}
				}
				break;
			}
			((BridgeSession)session).Closed = 1;
			num = 13;
		}
	}

	private static void DispatchAccountEvent(object session, object item, object connectionState)
	{
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Expected O, but got Unknown
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Expected O, but got Unknown
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Expected O, but got Unknown
		//IL_02c3: Expected O, but got Unknown
		//IL_03f4: Expected O, but got Unknown
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0454: Expected O, but got Unknown
		//IL_0454: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Expected O, but got Unknown
		//IL_0481: Expected O, but got Unknown
		int num = 7;
		string text = default(string);
		JObject val2 = default(JObject);
		int num3 = default(int);
		while (true)
		{
			int num2 = num;
			while (true)
			{
				int num4;
				switch (num2)
				{
				case 7:
					if (item != null)
					{
						num2 = 6;
						continue;
					}
					return;
				case 3:
					return;
				case 18:
					Bkkwbdcv6rQF0k1Yk9p(session, false);
					num2 = 2;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_45ca2c3eaeb74f8484cde152ae6e3eef == 0)
					{
						num2 = 1;
					}
					continue;
				default:
					if (!l2hSytck1xudCQ6WKdC(text, HBbvWMcE0vjQIqqqYSs(0x1C8F3B9A ^ 0x1C8F623E), NgtBADEr5RrpT3l6FkM.bH7ElpvHlk))
					{
						num2 = 3;
						continue;
					}
					goto case 12;
				case 2:
					return;
				case 5:
					dqfVK0cQl6eDUpgITv1(val2, z2jc63fLkugS1X8Q9N.tE1kD1vbB(0x46C0842F ^ 0x46C0DCA7), AJMSu3cdbMH0dIJx2DG(connectionState, b8txYwQmx8SvwU718wE.TU7QIcSjJQ), WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
					num2 = 14;
					continue;
				case 9:
					if (!NgtBADEr5RrpT3l6FkM.jnq8ilIcK(text, (string)HBbvWMcE0vjQIqqqYSs(0xA89FE75 ^ 0xA89A71F), NgtBADEr5RrpT3l6FkM.bH7ElpvHlk))
					{
						if (l2hSytck1xudCQ6WKdC(text, HBbvWMcE0vjQIqqqYSs(-1613644704 ^ -1613634280), NgtBADEr5RrpT3l6FkM.bH7ElpvHlk))
						{
							num2 = 4;
							continue;
						}
						if (NgtBADEr5RrpT3l6FkM.jnq8ilIcK(text, (string)HBbvWMcE0vjQIqqqYSs(0x3EDBC22B ^ 0x3EDB9BA3), NgtBADEr5RrpT3l6FkM.bH7ElpvHlk))
						{
							num2 = 7;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1601bb4cfcbc4e588ecdc797e83d5acc == 0)
							{
								num2 = 12;
							}
							continue;
						}
						goto default;
					}
					num = 18;
					break;
				case 19:
					return;
				case 1:
				case 14:
					WeComBridgeEvents.CopyMissing(val2, item);
					num2 = 21;
					continue;
				case 16:
					if (lRNPJoPvsXOpQ7Ucdon.jnq8ilIcK((string)connectionState, lRNPJoPvsXOpQ7Ucdon.VmKEqmiqNI))
					{
						num2 = 1;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_f9d0516f42ac4a4ca53e84f1dda1b106 == 0)
						{
							num2 = 1;
						}
						continue;
					}
					goto case 5;
				case 8:
					RR2iGecFYQ9mys4ABu0(session, item, connectionState);
					num = 13;
					break;
				case 13:
					return;
				case 15:
					num4 = 11202;
					goto IL_035a;
				case 21:
				{
					int clientId = ((BridgeSession)session).ClientId;
					JObject val3 = new JObject();
					WSJ5GQQ9myN2N0ma3xj.jnq8ilIcK((object)val3, z2jc63fLkugS1X8Q9N.tE1kD1vbB(0x3BA46780 ^ 0x3BA4587E), (JToken)KQmPiUcpmKrwNOnAA5r(num3, E9hPr6QvNKRyIQM6QVf.MB5QgmjD5J), WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
					dqfVK0cQl6eDUpgITv1((object)val3, z2jc63fLkugS1X8Q9N.tE1kD1vbB(-874623517 ^ -874612985), val2, WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
					MxoMeicuaYHy1qsmXMv(clientId, (object)val3);
					num2 = 19;
					continue;
				}
				case 4:
					if (!((BridgeSession)session).LoginNotified)
					{
						return;
					}
					num2 = 8;
					continue;
				case 12:
				case 17:
					if (NgtBADEr5RrpT3l6FkM.jnq8ilIcK(text, (string)HBbvWMcE0vjQIqqqYSs(-30755866 ^ -30741906), NgtBADEr5RrpT3l6FkM.bH7ElpvHlk))
					{
						num2 = 11;
						continue;
					}
					goto case 15;
				case 20:
					return;
				case 6:
					text = ((JToken)item).Value<string>(HBbvWMcE0vjQIqqqYSs(0x24E67878 ^ 0x24E62124));
					num2 = 9;
					continue;
				case 11:
					num4 = 11201;
					goto IL_035a;
				case 10:
					{
						JObject val = new JObject();
						dqfVK0cQl6eDUpgITv1((object)val, HBbvWMcE0vjQIqqqYSs(-396535719 ^ -396558075), AJMSu3cdbMH0dIJx2DG(text, b8txYwQmx8SvwU718wE.TU7QIcSjJQ), WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
						dqfVK0cQl6eDUpgITv1((object)val, HBbvWMcE0vjQIqqqYSs(0x3B865138 ^ 0x3B8616CC), b8txYwQmx8SvwU718wE.jnq8ilIcK((string)(N1tlkEcoZnwFWqSBqQf(((BridgeSession)session).UserId, lRNPJoPvsXOpQ7Ucdon.VmKEqmiqNI) ? (((BridgeSession)session).AccountId ?? "") : ((BridgeSession)session).UserId), b8txYwQmx8SvwU718wE.TU7QIcSjJQ), WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
						dqfVK0cQl6eDUpgITv1((object)val, HBbvWMcE0vjQIqqqYSs(0x47C9E567 ^ 0x47C9B3B1), b8txYwQmx8SvwU718wE.jnq8ilIcK((string)(((JToken)item).Value<string>(HBbvWMcE0vjQIqqqYSs(-233972548 ^ -233993622)) ?? ((BridgeSession)session).AccountId ?? ""), b8txYwQmx8SvwU718wE.TU7QIcSjJQ), WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
						WSJ5GQQ9myN2N0ma3xj.jnq8ilIcK((object)val, (string)HBbvWMcE0vjQIqqqYSs(0x29DAFBE2 ^ 0x29DAB3AA), (JToken)KQmPiUcpmKrwNOnAA5r(((BridgeSession)session).Pid, E9hPr6QvNKRyIQM6QVf.MB5QgmjD5J), WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
						val2 = val;
						num2 = 16;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_80394cf8f0664ed7a0669b96499a0b6e != 0)
						{
							num2 = 4;
						}
						continue;
					}
					IL_035a:
					num3 = num4;
					num2 = 10;
					continue;
				}
				break;
			}
		}
	}

	private static void EmitAccountLogout(object session)
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
				RR2iGecFYQ9mys4ABu0(session, null, null);
				num2 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d08eb354b58c49268ddfa8be7c8ef2b8 == 0)
				{
					num2 = 0;
				}
				break;
			}
		}
	}

	private static void EmitAccountLogout(object session, object source, object connectionState)
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected O, but got Unknown
		int num = 7;
		JObject val2 = default(JObject);
		JObject val = default(JObject);
		string text = default(string);
		while (true)
		{
			int num2 = num;
			while (true)
			{
				object obj;
				switch (num2)
				{
				case 3:
					WSJ5GQQ9myN2N0ma3xj.jnq8ilIcK(val2, (string)HBbvWMcE0vjQIqqqYSs(0x3EDBC22B ^ 0x3EDB9AA3), b8txYwQmx8SvwU718wE.jnq8ilIcK((string)connectionState, b8txYwQmx8SvwU718wE.TU7QIcSjJQ), WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
					num2 = 5;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_a34de5cebe4f4eeda550f236a75b91de != 0)
					{
						num2 = 5;
					}
					continue;
				default:
					val2 = (JObject)yTaeC9c7k2aDsaY5WXu(val, HBbvWMcE0vjQIqqqYSs(0x18907F3 ^ 0x1893D17), YNuvA8QCYEcgOkEROsU.YOmQH5bNe7);
					num2 = 11;
					continue;
				case 2:
				case 5:
					CE9J7scLSNqIvXO9WKK(val2, source);
					num2 = 9;
					continue;
				case 10:
					val = (JObject)tgMoKwc1iMYDewg0p7a(session, text);
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1090bd5856ad49da8290b4ab724faee3 != 0)
					{
						num2 = 0;
					}
					continue;
				case 8:
					XYXl5qcJAs4HRg0NXOM(((BridgeSession)session).ClientId);
					num2 = 12;
					continue;
				case 9:
					MxoMeicuaYHy1qsmXMv(((BridgeSession)session).ClientId, val);
					num2 = 8;
					continue;
				case 7:
					if (!N1tlkEcoZnwFWqSBqQf(((BridgeSession)session).UserId, lRNPJoPvsXOpQ7Ucdon.VmKEqmiqNI))
					{
						num2 = 6;
						continue;
					}
					obj = ((BridgeSession)session).AccountId;
					break;
				case 6:
					obj = ((BridgeSession)session).UserId;
					break;
				case 1:
					((BridgeSession)session).LoginNotified = false;
					num2 = 8;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_76a1008ec43840dda25966a6b044ac77 == 0)
					{
						num2 = 10;
					}
					continue;
				case 4:
					((BridgeSession)session).LoginState = HBbvWMcE0vjQIqqqYSs(-1356900703 ^ -1356922459);
					num2 = 1;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_305aaddad70a4f7fae7b0b1dbdd6ed1f != 0)
					{
						num2 = 1;
					}
					continue;
				case 11:
					if (N1tlkEcoZnwFWqSBqQf(connectionState, lRNPJoPvsXOpQ7Ucdon.VmKEqmiqNI))
					{
						goto end_IL_0012;
					}
					goto case 3;
				case 12:
					return;
				}
				text = (string)obj;
				num2 = 1;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_cbbf75a1fc4b4ad1ae851089027cdc57 != 0)
				{
					num2 = 4;
				}
				continue;
				end_IL_0012:
				break;
			}
			num = 2;
		}
	}

	private static JObject LogoutMessage(object session, object userId)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Expected O, but got Unknown
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Expected O, but got Unknown
		//IL_0113: Expected O, but got Unknown
		//IL_0113: Expected O, but got Unknown
		//IL_0114: Expected O, but got Unknown
		JObject val = new JObject();
		WSJ5GQQ9myN2N0ma3xj.jnq8ilIcK((object)val, (string)HBbvWMcE0vjQIqqqYSs(0x2F53E4C2 ^ 0x2F53DB3C), (JToken)KQmPiUcpmKrwNOnAA5r(11027, E9hPr6QvNKRyIQM6QVf.MB5QgmjD5J), WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
		object obj = HBbvWMcE0vjQIqqqYSs(-1309619823 ^ -1309609099);
		JObject val2 = new JObject();
		WSJ5GQQ9myN2N0ma3xj.jnq8ilIcK((object)val2, (string)HBbvWMcE0vjQIqqqYSs(-1191340342 ^ -1191325378), b8txYwQmx8SvwU718wE.jnq8ilIcK((string)(userId ?? ""), b8txYwQmx8SvwU718wE.TU7QIcSjJQ), WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
		WSJ5GQQ9myN2N0ma3xj.jnq8ilIcK((object)val2, (string)HBbvWMcE0vjQIqqqYSs(0x5FF7E885 ^ 0x5FF7BE53), (JToken)AJMSu3cdbMH0dIJx2DG(((BridgeSession)session).AccountId ?? "", b8txYwQmx8SvwU718wE.TU7QIcSjJQ), WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
		dqfVK0cQl6eDUpgITv1((object)val2, HBbvWMcE0vjQIqqqYSs(-592631919 ^ -592609745), KQmPiUcpmKrwNOnAA5r(((BridgeSession)session).ClientId, E9hPr6QvNKRyIQM6QVf.MB5QgmjD5J), WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
		WSJ5GQQ9myN2N0ma3xj.jnq8ilIcK((object)val2, z2jc63fLkugS1X8Q9N.tE1kD1vbB(-1429904013 ^ -1429918405), (JToken)KQmPiUcpmKrwNOnAA5r(((BridgeSession)session).Pid, E9hPr6QvNKRyIQM6QVf.MB5QgmjD5J), WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
		dqfVK0cQl6eDUpgITv1((object)val, obj, (object)val2, WSJ5GQQ9myN2N0ma3xj.b15Q811fY5);
		return val;
	}

	private static void AcceptLoop(object session)
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
			{
				<>c__DisplayClass35_0 <>c__DisplayClass35_ = new <>c__DisplayClass35_0();
				<>c__DisplayClass35_.session = (BridgeSession)session;
				X4U9kVhZ0DxxCJRjVmi.jnq8ilIcK(delegate
				{
					int num3 = 3;
					int num4 = num3;
					<>c__DisplayClass35_1 <>c__DisplayClass35_2 = default(<>c__DisplayClass35_1);
					while (true)
					{
						switch (num4)
						{
						case 4:
							return;
						case 6:
							<>c__DisplayClass35_0.ojygHBrUNJ5nrB2vG5C(new WaitCallback(<>c__DisplayClass35_2.<AcceptLoop>b__1), X4U9kVhZ0DxxCJRjVmi.Tm8h1Vtbj3);
							num4 = 1;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_7dea470b4c15478bbfe1539429f98b80 == 0)
							{
								num4 = 1;
							}
							break;
						case 1:
						case 3:
							if (<>c__DisplayClass35_.session.Closed != null)
							{
								num4 = 4;
								break;
							}
							goto case 2;
						case 2:
							<>c__DisplayClass35_2 = new <>c__DisplayClass35_1();
							num4 = 1;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_a8ee04dc577b44a4b973e7119af8874c != 0)
							{
								num4 = 5;
							}
							break;
						case 5:
							<>c__DisplayClass35_2.CS$<>8__locals1 = <>c__DisplayClass35_;
							num4 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_291da815e7d24e5fad1c5f19ff1c2dd7 == 0)
							{
								num4 = 0;
							}
							break;
						default:
							try
							{
								<>c__DisplayClass35_2.client = (TcpClient)<>c__DisplayClass35_0.L6dj2Ur2tL81o7XFx9L(<>c__DisplayClass35_.session.Listener, Fu4m99YAmtAMDTqt5CV.aQrYXIJ4B6);
								int num5 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_23f2079970f0474d9a38aefb1de2f52d == 0)
								{
									num5 = 0;
								}
								switch (num5)
								{
								case 0:
									break;
								}
							}
							catch
							{
								int num6 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_23f2079970f0474d9a38aefb1de2f52d == 0)
								{
									num6 = 0;
								}
								switch (num6)
								{
								case 0:
									break;
								}
								return;
							}
							goto case 6;
						}
					}
				}, X4U9kVhZ0DxxCJRjVmi.Tm8h1Vtbj3);
				num2 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_f9d0516f42ac4a4ca53e84f1dda1b106 == 0)
				{
					num2 = 0;
				}
				break;
			}
			case 0:
				return;
			}
		}
	}

	private static void Serve(object session, object client)
	{
		int num = 3;
		int num2 = num;
		TcpClient tcpClient = default(TcpClient);
		<>c__DisplayClass36_0 <>c__DisplayClass36_ = default(<>c__DisplayClass36_0);
		Timer timer = default(Timer);
		byte[] array2 = default(byte[]);
		int num6 = default(int);
		while (true)
		{
			switch (num2)
			{
			case 1:
				tcpClient = <>c__DisplayClass36_.client;
				num2 = 6;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_9e74138bb8d34abb8ed7790887860771 == 0)
				{
					num2 = 2;
				}
				break;
			case 3:
				<>c__DisplayClass36_ = new <>c__DisplayClass36_0();
				num2 = 1;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_db887f4a099249249dfc8abe84bd3ac9 == 0)
				{
					num2 = 2;
				}
				break;
			case 2:
				<>c__DisplayClass36_.client = (TcpClient)client;
				num2 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_0a8dc1e2655145c5af97263c14d91cce == 0)
				{
					num2 = 0;
				}
				break;
			default:
				lZ1quOcOaw0vKJ2VYCl(<>c__DisplayClass36_.client, true, y8iVbRET6QxQsChlnBy.VRshFCTLah);
				num2 = 4;
				break;
			case 6:
				try
				{
					<>c__DisplayClass36_.stream = hedHuWhpCO5iKc5VV8Y.jnq8ilIcK(<>c__DisplayClass36_.client, hedHuWhpCO5iKc5VV8Y.L7WhJeBS9W);
					int num3 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_76fed529d5134afd95ce65e7c04235fc == 0)
					{
						num3 = 0;
					}
					switch (num3)
					{
					default:
						try
						{
							<>c__DisplayClass36_.writeLock = new object();
							int num4 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_03aacce0025f45d8a46eb3d2a6e41868 != 0)
							{
								num4 = 0;
							}
							while (true)
							{
								switch (num4)
								{
								default:
									timer = new Timer(<>c__DisplayClass36_.<Serve>b__0, null, IDUgaBbPEduTX7nPOi8.jnq8ilIcK(1.0, IDUgaBbPEduTX7nPOi8.h4TxUx4lyo), IDUgaBbPEduTX7nPOi8.jnq8ilIcK(30.0, IDUgaBbPEduTX7nPOi8.h4TxUx4lyo));
									num4 = 0;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_291da815e7d24e5fad1c5f19ff1c2dd7 == 0)
									{
										num4 = 1;
									}
									break;
								case 1:
									try
									{
										byte[] array = new byte[4];
										int num5 = 9;
										while (true)
										{
											int num7;
											switch (num5)
											{
											case 11:
												if (SActFGcR8wnYja3eVgV(<>c__DisplayClass36_.stream, array2, num6))
												{
													num7 = 3;
													goto IL_01a4;
												}
												return;
											case 1:
												array2 = new byte[num6];
												num5 = 7;
												if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_45ca2c3eaeb74f8484cde152ae6e3eef != 0)
												{
													num5 = 11;
												}
												break;
											case 14:
												lymWLNceeC5sWAA6hPE(gFPhhUcNZJrKvR117L5(HBbvWMcE0vjQIqqqYSs(-1229729227 ^ -1229748167), gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref ((BridgeSession)session).ClientId, gGrLFhEdofbUymLMOb3.RREELfe2hN), z2jc63fLkugS1X8Q9N.tE1kD1vbB(0x65DCA45F ^ 0x65DCFE71), gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref num6, gGrLFhEdofbUymLMOb3.RREELfe2hN), m4SFBaEZmre5qY3dKfT.hGFE1HacV8));
												num5 = 12;
												break;
											case 5:
												num6 = (array[0] << 24) | (array[1] << 16) | (array[2] << 8) | array[3];
												num7 = 8;
												goto IL_01a4;
											case 2:
												return;
											case 8:
												if (num6 > 0)
												{
													num5 = 4;
													break;
												}
												goto case 14;
											case 4:
												if (num6 > 20971520)
												{
													num5 = 14;
													if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_bc19572226e743cb89917af019cec5df == 0)
													{
														num5 = 4;
													}
													break;
												}
												goto case 1;
											case 3:
											case 6:
												Wahs7ac3d83REL0NFkT(session, vphlSihOuKhKKC13xr8.jnq8ilIcK(LCpK8kxFhGIbWVyMacY.jnq8ilIcK(LCpK8kxFhGIbWVyMacY.PGUxp2INU4), array2, vphlSihOuKhKKC13xr8.KMUhNeLKev));
												num5 = 0;
												if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1601bb4cfcbc4e588ecdc797e83d5acc == 0)
												{
													num5 = 0;
												}
												break;
											case 7:
												return;
											case 13:
												if (!ReadExact(<>c__DisplayClass36_.stream, array, 4))
												{
													num5 = 2;
													if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_a152e1a10f374e159091a60e0cb4b256 != 0)
													{
														num5 = 0;
													}
													break;
												}
												goto case 5;
											default:
												if (((BridgeSession)session).Closed != null)
												{
													num5 = 10;
													break;
												}
												goto case 13;
											case 12:
												return;
											case 10:
												return;
												IL_01a4:
												num5 = num7;
												break;
											}
										}
									}
									finally
									{
										nVdXdwhub0GVhDyNlIF.jnq8ilIcK(timer, nVdXdwhub0GVhDyNlIF.g2IhRed5cI);
										int num8 = 0;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_97420ada251246e9bdb76f42699c0610 != 0)
										{
											num8 = 0;
										}
										switch (num8)
										{
										case 0:
											break;
										}
									}
								}
							}
						}
						finally
						{
							if (<>c__DisplayClass36_.stream != null)
							{
								int num9 = 1;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_cbbf75a1fc4b4ad1ae851089027cdc57 == 0)
								{
									num9 = 1;
								}
								while (true)
								{
									switch (num9)
									{
									case 1:
										gf0Rq0cgyWhibXFkBEM(<>c__DisplayClass36_.stream, dsixtSPyYHjKiE5k7WK.VtIPNcTU9E);
										num9 = 0;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c1db6875e3a1467ca73ac64ce11e3ed4 != 0)
										{
											num9 = 0;
										}
										continue;
									case 0:
										break;
									}
									break;
								}
							}
						}
					}
				}
				finally
				{
					int num10;
					if (tcpClient == null)
					{
						num10 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_529db4a9032c417dbe74a080d04d36c2 == 0)
						{
							num10 = 1;
						}
						goto IL_04c9;
					}
					goto IL_04df;
					IL_04c9:
					switch (num10)
					{
					default:
						goto end_IL_04a4;
					case 2:
						break;
					case 1:
						goto end_IL_04a4;
					case 0:
						goto end_IL_04a4;
					}
					goto IL_04df;
					IL_04df:
					gf0Rq0cgyWhibXFkBEM(tcpClient, dsixtSPyYHjKiE5k7WK.VtIPNcTU9E);
					num10 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_99797e4f3bab41baabcb75f7a30397b7 == 0)
					{
						num10 = 0;
					}
					goto IL_04c9;
					end_IL_04a4:;
				}
			case 5:
				return;
			case 4:
				Logger.Info((string)XnUyVEc5Tkm1KK2H1pc(HBbvWMcE0vjQIqqqYSs(0x3B5F8A40 ^ 0x3B5FD392), gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref ((BridgeSession)session).ClientId, gGrLFhEdofbUymLMOb3.RREELfe2hN), RxUSOmPDJGsMmCCGPPg.hbkPt9hhua));
				num2 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_f9d0516f42ac4a4ca53e84f1dda1b106 != 0)
				{
					num2 = 1;
				}
				break;
			}
		}
	}

	private static bool ReadExact(object stream, object buffer, int count)
	{
		int num = 3;
		int num2 = num;
		int num3 = default(int);
		int num4 = default(int);
		bool result = default(bool);
		while (true)
		{
			switch (num2)
			{
			case 2:
			case 4:
				if (num3 >= count)
				{
					num2 = 1;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_529db4a9032c417dbe74a080d04d36c2 == 0)
					{
						num2 = 6;
					}
					break;
				}
				goto case 8;
			case 1:
				try
				{
					num4 = YKylLocsKkA8dJhI21A(stream, buffer, num3, count - num3, dwdfmjPJFfMGpR80D2F.D8LPOGgPLO);
					int num5 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_e3198421401646eabf32dc544445bbbe == 0)
					{
						num5 = 0;
					}
					switch (num5)
					{
					case 0:
						break;
					}
				}
				catch
				{
					int num6 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_37f38109cd8f46f5b0100026922644f5 == 0)
					{
						num6 = 0;
					}
					while (true)
					{
						switch (num6)
						{
						case 1:
							return result;
						}
						result = false;
						num6 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c2d1dda68e3b4e11855617e481626480 == 0)
						{
							num6 = 1;
						}
					}
				}
				goto case 7;
			case 5:
				return false;
			default:
				num3 += num4;
				num2 = 4;
				break;
			case 8:
				num2 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_ebde985f65fe4000b6ba504548c2703b == 0)
				{
					num2 = 1;
				}
				break;
			case 7:
				if (num4 > 0)
				{
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_dd83a9ff75d94001819d80b24ef9a897 != 0)
					{
						num2 = 0;
					}
					break;
				}
				goto case 5;
			case 6:
				return true;
			case 3:
				num3 = 0;
				num2 = 2;
				break;
			}
		}
	}

	private static byte[] Frame(object json)
	{
		int num = 3;
		byte[] array2 = default(byte[]);
		byte[] array = default(byte[]);
		while (true)
		{
			int num2 = num;
			while (true)
			{
				switch (num2)
				{
				case 6:
					array2[3] = (byte)(array.Length & 0xFF);
					num2 = 4;
					continue;
				case 5:
					array2[2] = (byte)((array.Length >> 8) & 0xFF);
					num2 = 6;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_84f159339c0c4d909a5aafadeb20c94b == 0)
					{
						num2 = 2;
					}
					continue;
				case 1:
					array2[0] = (byte)((array.Length >> 24) & 0xFF);
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_93e9b0bbc1fe4ae1a0fca3879b36d9a6 == 0)
					{
						num2 = 0;
					}
					continue;
				case 4:
					U4AdlgkWn7A1aFpOI21(array, 0, array2, 4, array.Length, nfXjtjh3scWXW11JavL.psshsZ8Vew);
					num = 7;
					break;
				default:
					array2[1] = (byte)((array.Length >> 16) & 0xFF);
					num2 = 5;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_f8d69a0d83e2420b83127060d822cafd == 0)
					{
						num2 = 5;
					}
					continue;
				case 2:
					array2 = new byte[4 + array.Length];
					num2 = 1;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_289d7cd175a8485b8ea0e81b4750a0f3 != 0)
					{
						num2 = 1;
					}
					continue;
				case 7:
					return array2;
				case 3:
					array = lR4b30UdWuRLsJ5bAv8.jnq8ilIcK(ekWklhczmvHysJVmCg6(LCpK8kxFhGIbWVyMacY.PGUxp2INU4), (string)json, lR4b30UdWuRLsJ5bAv8.SibULVjBTZ);
					num = 2;
					break;
				}
				break;
			}
		}
	}

	private static void ReleaseListener(object session)
	{
		int num = 7;
		TcpListener listener = default(TcpListener);
		while (true)
		{
			int num2 = num;
			while (true)
			{
				switch (num2)
				{
				default:
					return;
				case 0:
					return;
				case 2:
					if (listener != null)
					{
						num2 = 1;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_93e9b0bbc1fe4ae1a0fca3879b36d9a6 == 0)
						{
							num2 = 1;
						}
					}
					else
					{
						num2 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_2cca3d059d854b57aca52e0b80a8eb7d != 0)
						{
							num2 = 0;
						}
					}
					continue;
				case 1:
					try
					{
						kixbTec00AbaPALMjAN(vKZ94XkPsKYno1ZA1at(listener, uxLtJMhzyjrn9e7Dy1C.bRrYWcPoFt), nVdXdwhub0GVhDyNlIF.N9rYPpIsWI);
						int num5 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_5138927ba3c1408e9ba282c6f1787fc1 != 0)
						{
							num5 = 0;
						}
						switch (num5)
						{
						case 0:
							break;
						}
					}
					catch
					{
						int num6 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d34bf7edba6543039b52c92f6f825304 != 0)
						{
							num6 = 0;
						}
						switch (num6)
						{
						case 0:
							break;
						}
					}
					break;
				case 6:
					((BridgeSession)session).Listener = null;
					num2 = 2;
					continue;
				case 7:
					listener = (TcpListener)((BridgeSession)session).Listener;
					num2 = 6;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d08eb354b58c49268ddfa8be7c8ef2b8 == 0)
					{
						num2 = 6;
					}
					continue;
				case 5:
					return;
				case 4:
					try
					{
						kixbTec00AbaPALMjAN(listener, nVdXdwhub0GVhDyNlIF.OlTh9S5DxM);
						int num3 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_fc096a82eb6f412ab19b9dee9fccbc6e == 0)
						{
							num3 = 0;
						}
						switch (num3)
						{
						case 0:
							break;
						}
						return;
					}
					catch
					{
						int num4 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1fdc4536a75247b19a0801dd70bf34e9 == 0)
						{
							num4 = 0;
						}
						switch (num4)
						{
						case 0:
							break;
						}
						return;
					}
				case 3:
					break;
				}
				break;
			}
			num = 4;
		}
	}

	private static TcpListener StartListenerExact(int port)
	{
		TcpListener result = default(TcpListener);
		switch (1)
		{
		case 1:
			try
			{
				TcpListener tcpListener = new TcpListener(IPAddress.Loopback, port);
				kixbTec00AbaPALMjAN(tcpListener, nVdXdwhub0GVhDyNlIF.gUnYEygRX2);
				result = tcpListener;
				int num = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_0444cf56387747139b0edc577cc00a72 == 0)
				{
					num = 0;
				}
				switch (num)
				{
				case 0:
					break;
				}
			}
			catch (Exception ex)
			{
				int num2 = 2;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_2879e5bb2d3341f08a6f59d9f884899e == 0)
				{
					num2 = 2;
				}
				while (true)
				{
					switch (num2)
					{
					case 2:
						lymWLNceeC5sWAA6hPE(gFPhhUcNZJrKvR117L5(HBbvWMcE0vjQIqqqYSs(-32776863 ^ -32798941), gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref port, gGrLFhEdofbUymLMOb3.RREELfe2hN), z2jc63fLkugS1X8Q9N.tE1kD1vbB(-216118172 ^ -216132092), nuJndEkE3v9PTORZQy5(ex, gjqRKtPV17ZUEQVAWxv.AdKPK0ve7w), m4SFBaEZmre5qY3dKfT.hGFE1HacV8));
						num2 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1bbe7758123e4bc485e91d98227e25e4 == 0)
						{
							num2 = 1;
						}
						continue;
					case 1:
						result = null;
						num2 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_9a207314288741c0b10cb80afbe48750 != 0)
						{
							num2 = 0;
						}
						continue;
					case 0:
						break;
					}
					break;
				}
			}
			break;
		}
		return result;
	}

	private static TcpListener StartListener(ref int port)
	{
		int num = 4;
		int num2 = num;
		int num3 = default(int);
		int num4 = default(int);
		while (true)
		{
			switch (num2)
			{
			case 5:
			{
				TcpListener result;
				try
				{
					TcpListener tcpListener = new TcpListener(IPAddress.Loopback, num3);
					nVdXdwhub0GVhDyNlIF.jnq8ilIcK(tcpListener, nVdXdwhub0GVhDyNlIF.gUnYEygRX2);
					port = num3;
					if (num3 != num4)
					{
						k0NxU4cte3COFcR674G(gFPhhUcNZJrKvR117L5(z2jc63fLkugS1X8Q9N.tE1kD1vbB(0x1A739410 ^ 0x1A73CE7E), gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref num4, gGrLFhEdofbUymLMOb3.RREELfe2hN), z2jc63fLkugS1X8Q9N.tE1kD1vbB(-879603300 ^ -879621348), gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref num3, gGrLFhEdofbUymLMOb3.RREELfe2hN), m4SFBaEZmre5qY3dKfT.hGFE1HacV8));
					}
					result = tcpListener;
					int num5 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_529db4a9032c417dbe74a080d04d36c2 == 0)
					{
						num5 = 0;
					}
					switch (num5)
					{
					case 0:
						break;
					}
				}
				catch (Exception ex)
				{
					int num6 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d604b1379fb540e5b6d79e791c2f5762 == 0)
					{
						num6 = 0;
					}
					while (true)
					{
						switch (num6)
						{
						default:
							lymWLNceeC5sWAA6hPE(gFPhhUcNZJrKvR117L5(HBbvWMcE0vjQIqqqYSs(0x1F6D7205 ^ 0x1F6D2847), gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref num3, gGrLFhEdofbUymLMOb3.RREELfe2hN), z2jc63fLkugS1X8Q9N.tE1kD1vbB(-554298198 ^ -554283318), gjqRKtPV17ZUEQVAWxv.jnq8ilIcK(ex, gjqRKtPV17ZUEQVAWxv.AdKPK0ve7w), m4SFBaEZmre5qY3dKfT.hGFE1HacV8));
							num6 = 1;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d34bf7edba6543039b52c92f6f825304 != 0)
							{
								num6 = 0;
							}
							continue;
						case 1:
							break;
						}
						break;
					}
					break;
				}
				return result;
			}
			case 3:
				num3 = num4;
				num2 = 3;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_db887f4a099249249dfc8abe84bd3ac9 == 0)
				{
					num2 = 7;
				}
				continue;
			default:
				num2 = 5;
				continue;
			case 4:
				num4 = port;
				num2 = 3;
				continue;
			case 6:
				return null;
			case 2:
			case 7:
				if (num3 >= num4 + 20)
				{
					num2 = 6;
					continue;
				}
				goto default;
			case 1:
				break;
			}
			num3++;
			num2 = 2;
		}
	}

	private static int RunInject(object arguments, out string error)
	{
		int num = 5;
		int num2 = num;
		string bridgeDirectory = default(string);
		string text2 = default(string);
		ProcessStartInfo processStartInfo = default(ProcessStartInfo);
		int result = default(int);
		string text = default(string);
		while (true)
		{
			switch (num2)
			{
			case 4:
				bridgeDirectory = WeComVersionCatalog.GetBridgeDirectory();
				num2 = 2;
				continue;
			case 2:
				text2 = (string)XnUyVEc5Tkm1KK2H1pc(bridgeDirectory, HBbvWMcE0vjQIqqqYSs(-1857040513 ^ -1857028169), RxUSOmPDJGsMmCCGPPg.rZAPwta2Dr);
				num2 = 3;
				continue;
			case 3:
			{
				ProcessStartInfo processStartInfo2 = new ProcessStartInfo();
				BCwKFvkbBi5RA6AixkB(processStartInfo2, text2, VqsM3KbUVrAvZwhr05A.uMnxgOpiZS);
				VqsM3KbUVrAvZwhr05A.jnq8ilIcK(processStartInfo2, (string)arguments, VqsM3KbUVrAvZwhr05A.iSux9MgeDw);
				BCwKFvkbBi5RA6AixkB(processStartInfo2, bridgeDirectory, VqsM3KbUVrAvZwhr05A.sjNx8k8amx);
				lZ1quOcOaw0vKJ2VYCl(processStartInfo2, false, y8iVbRET6QxQsChlnBy.qvBxm5aBGQ);
				lZ1quOcOaw0vKJ2VYCl(processStartInfo2, true, y8iVbRET6QxQsChlnBy.VqKxIUXbQC);
				lZ1quOcOaw0vKJ2VYCl(processStartInfo2, true, y8iVbRET6QxQsChlnBy.UKkxZkK1OO);
				lZ1quOcOaw0vKJ2VYCl(processStartInfo2, true, y8iVbRET6QxQsChlnBy.D7Qx1uJmwR);
				processStartInfo = processStartInfo2;
				num2 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_f8d69a0d83e2420b83127060d822cafd == 0)
				{
					num2 = 1;
				}
				continue;
			}
			case 5:
				error = null;
				num2 = 4;
				continue;
			case 1:
				try
				{
					Process process = JghH6fUm67WnY0KFBLu.jnq8ilIcK(processStartInfo, JghH6fUm67WnY0KFBLu.RfFUI0COCF);
					int num3 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_90f58c7ffb21444ca88319aa26ca6aa0 != 0)
					{
						num3 = 0;
					}
					switch (num3)
					{
					default:
						try
						{
							<>c__DisplayClass42_0 CS$<>8__locals11 = new <>c__DisplayClass42_0();
							int num4 = 19;
							while (true)
							{
								object obj4;
								switch (num4)
								{
								case 1:
								case 3:
									if (CJTUy1k2GLLSsExIbVp(CS$<>8__locals11.err, kfHGSgPnOQlgJ5xXCSg.PsfUH20CW9) > 0)
									{
										num4 = 5;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_2b51064baece4c398bf908a830a54cbf != 0)
										{
											num4 = 2;
										}
										continue;
									}
									goto case 17;
								case 4:
									dsixtSPyYHjKiE5k7WK.jnq8ilIcK(process, dsixtSPyYHjKiE5k7WK.TBhyEaTcVD);
									num4 = 13;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_5a84d0ea826a44b299199b540d6e6fc8 == 0)
									{
										num4 = 9;
									}
									continue;
								case 27:
									R6XcJNkxLjp9cmZM6cX(process, (DataReceivedEventHandler)delegate(object sender, DataReceivedEventArgs args)
									{
										int num15 = 2;
										int num16 = num15;
										while (true)
										{
											switch (num16)
											{
											default:
												return;
											case 0:
												return;
											case 2:
												if (<>c__DisplayClass42_0.YPtZcsr6yTQXITHHCFd(args, gjqRKtPV17ZUEQVAWxv.yb02YGrn01) == null)
												{
													return;
												}
												num16 = 1;
												if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1090bd5856ad49da8290b4ab724faee3 != 0)
												{
													num16 = 1;
												}
												break;
											case 1:
												<>c__DisplayClass42_0.e69lJRrCPyvxxrBqYGx(CS$<>8__locals11.err, gjqRKtPV17ZUEQVAWxv.jnq8ilIcK(args, gjqRKtPV17ZUEQVAWxv.yb02YGrn01), qtMqbjU60YAngWAOYRT.cF1UR4sLY5);
												num16 = 0;
												if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_83095058e27b497188a2c88050ba8f5f == 0)
												{
													num16 = 0;
												}
												break;
											}
										}
									}, WmWFPUxRQM3xYTWTkqj.o0YxslDGsI);
									num4 = 4;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_9b487277fbad4617828ccf4adb74f260 != 0)
									{
										num4 = 2;
									}
									continue;
								case 19:
									CS$<>8__locals11.output = new StringBuilder();
									num4 = 7;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_bd908099f891496e8274f46975b80fa0 != 0)
									{
										num4 = 14;
									}
									continue;
								default:
									result = CJTUy1k2GLLSsExIbVp(process, kfHGSgPnOQlgJ5xXCSg.V86UNvWWAu);
									num4 = 24;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_0e5a04c5423148499c9b265cadee0019 == 0)
									{
										num4 = 22;
									}
									continue;
								case 13:
									gf0Rq0cgyWhibXFkBEM(process, dsixtSPyYHjKiE5k7WK.wwIybjAYn1);
									num4 = 6;
									continue;
								case 23:
									break;
								case 9:
									try
									{
										JXKG3akyhPMFt3e69K9(process, 3000, p2FBj0y2KBDqZid0RZA.F1jyUbLTj1);
										int num6 = 0;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_ebde985f65fe4000b6ba504548c2703b != 0)
										{
											num6 = 0;
										}
										switch (num6)
										{
										case 0:
											break;
										}
									}
									catch
									{
										int num7 = 0;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_5bb1f583c3f24addaf7d69d4facec67c == 0)
										{
											num7 = 0;
										}
										switch (num7)
										{
										case 0:
											break;
										}
									}
									goto case 11;
								case 8:
									if (CJTUy1k2GLLSsExIbVp(CS$<>8__locals11.output, kfHGSgPnOQlgJ5xXCSg.PsfUH20CW9) <= 0)
									{
										num4 = 3;
										continue;
									}
									goto case 18;
								case 15:
									try
									{
										gf0Rq0cgyWhibXFkBEM(process, dsixtSPyYHjKiE5k7WK.BcZyypE5Ge);
										int num11 = 0;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_289d7cd175a8485b8ea0e81b4750a0f3 == 0)
										{
											num11 = 0;
										}
										switch (num11)
										{
										case 0:
											break;
										}
									}
									catch
									{
										int num12 = 0;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_080a2cdddf2144a182f32bab096ad248 != 0)
										{
											num12 = 0;
										}
										switch (num12)
										{
										case 0:
											break;
										}
									}
									goto case 20;
								case 18:
									Logger.Info((string)nuJndEkE3v9PTORZQy5(nuJndEkE3v9PTORZQy5(CS$<>8__locals11.output, gjqRKtPV17ZUEQVAWxv.w5Wxn64Klt), gjqRKtPV17ZUEQVAWxv.XjSbMbEMZX));
									num4 = 1;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_e0bd3b55ee8d41bab3f8a58185604cae != 0)
									{
										num4 = 1;
									}
									continue;
								case 7:
									obj4 = nuJndEkE3v9PTORZQy5(nuJndEkE3v9PTORZQy5(CS$<>8__locals11.output, gjqRKtPV17ZUEQVAWxv.w5Wxn64Klt), gjqRKtPV17ZUEQVAWxv.XjSbMbEMZX);
									goto IL_064d;
								case 5:
									lymWLNceeC5sWAA6hPE(gjqRKtPV17ZUEQVAWxv.jnq8ilIcK(nuJndEkE3v9PTORZQy5(CS$<>8__locals11.err, gjqRKtPV17ZUEQVAWxv.w5Wxn64Klt), gjqRKtPV17ZUEQVAWxv.XjSbMbEMZX));
									num4 = 17;
									continue;
								case 12:
									if (CJTUy1k2GLLSsExIbVp(process, kfHGSgPnOQlgJ5xXCSg.V86UNvWWAu) != 1306)
									{
										num4 = 10;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_7dea470b4c15478bbfe1539429f98b80 == 0)
										{
											num4 = 6;
										}
										continue;
									}
									goto default;
								case 11:
									error = z2jc63fLkugS1X8Q9N.tE1kD1vbB(0x64FE6D0E ^ 0x64FE3798);
									num4 = 0;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_93e9b0bbc1fe4ae1a0fca3879b36d9a6 != 0)
									{
										num4 = 2;
									}
									continue;
								case 10:
									if (CJTUy1k2GLLSsExIbVp(CS$<>8__locals11.err, kfHGSgPnOQlgJ5xXCSg.PsfUH20CW9) > 0)
									{
										num4 = 26;
										continue;
									}
									goto case 7;
								case 22:
									R6XcJNkxLjp9cmZM6cX(process, (DataReceivedEventHandler)delegate(object sender, DataReceivedEventArgs args)
									{
										int num15 = 1;
										int num16 = num15;
										while (true)
										{
											switch (num16)
											{
											default:
												return;
											case 0:
												return;
											case 3:
												return;
											case 1:
												if (<>c__DisplayClass42_0.YPtZcsr6yTQXITHHCFd(args, gjqRKtPV17ZUEQVAWxv.yb02YGrn01) == null)
												{
													num16 = 0;
													if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_12acf0ef11b346c9ab1a5afc262afd8b != 0)
													{
														num16 = 0;
													}
													continue;
												}
												break;
											case 2:
												break;
											}
											<>c__DisplayClass42_0.e69lJRrCPyvxxrBqYGx(CS$<>8__locals11.output, <>c__DisplayClass42_0.YPtZcsr6yTQXITHHCFd(args, gjqRKtPV17ZUEQVAWxv.yb02YGrn01), qtMqbjU60YAngWAOYRT.cF1UR4sLY5);
											num16 = 0;
											if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_f8d69a0d83e2420b83127060d822cafd == 0)
											{
												num16 = 3;
											}
										}
									}, WmWFPUxRQM3xYTWTkqj.jVkx3BLPDx);
									num4 = 27;
									continue;
								case 14:
									CS$<>8__locals11.err = new StringBuilder();
									num4 = 22;
									continue;
								case 20:
									num4 = 9;
									continue;
								case 16:
									try
									{
										JXKG3akyhPMFt3e69K9(process, 3000, p2FBj0y2KBDqZid0RZA.F1jyUbLTj1);
										int num9 = 0;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_cb5bc3d191fc4bc3b6ad04816cd3def1 == 0)
										{
											num9 = 0;
										}
										switch (num9)
										{
										case 0:
											break;
										}
									}
									catch
									{
										int num10 = 0;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_32c4deae05384207a567e3dea0ef9d3f != 0)
										{
											num10 = 0;
										}
										switch (num10)
										{
										case 0:
											break;
										}
									}
									goto case 8;
								case 6:
									if (!JXKG3akyhPMFt3e69K9(process, 60000, p2FBj0y2KBDqZid0RZA.F1jyUbLTj1))
									{
										int num8 = 15;
										num4 = num8;
										continue;
									}
									goto case 25;
								case 25:
									num4 = 16;
									continue;
								case 17:
									if (kfHGSgPnOQlgJ5xXCSg.jnq8ilIcK(process, kfHGSgPnOQlgJ5xXCSg.V86UNvWWAu) != 0)
									{
										num4 = 12;
										continue;
									}
									goto default;
								case 2:
									result = -1;
									num4 = 23;
									continue;
								case 26:
									obj4 = nuJndEkE3v9PTORZQy5(nuJndEkE3v9PTORZQy5(CS$<>8__locals11.err, gjqRKtPV17ZUEQVAWxv.w5Wxn64Klt), gjqRKtPV17ZUEQVAWxv.XjSbMbEMZX);
									goto IL_064d;
								case 21:
								{
									object obj;
									if (!N1tlkEcoZnwFWqSBqQf(text, lRNPJoPvsXOpQ7Ucdon.VmKEqmiqNI))
									{
										obj = text;
									}
									else
									{
										object obj2 = HBbvWMcE0vjQIqqqYSs(0x53DF8D7A ^ 0x53DFD7D8);
										int num5 = CJTUy1k2GLLSsExIbVp(process, kfHGSgPnOQlgJ5xXCSg.V86UNvWWAu);
										obj = XnUyVEc5Tkm1KK2H1pc(obj2, gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref num5, gGrLFhEdofbUymLMOb3.RREELfe2hN), RxUSOmPDJGsMmCCGPPg.hbkPt9hhua);
									}
									error = (string)obj;
									num4 = 0;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d3b039c30242456eb8cdd20df225be7a != 0)
									{
										num4 = 0;
									}
									continue;
								}
								case 24:
									break;
									IL_064d:
									text = (string)obj4;
									num4 = 21;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d91865d38b6c449295c8f801546a735d != 0)
									{
										num4 = 1;
									}
									continue;
								}
								break;
							}
						}
						finally
						{
							int num13;
							if (process == null)
							{
								num13 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_a7a51f657ad14a8fa2f52f4d42963791 != 0)
								{
									num13 = 0;
								}
								goto IL_06fb;
							}
							goto IL_0711;
							IL_06fb:
							switch (num13)
							{
							default:
								goto end_IL_06d6;
							case 2:
								break;
							case 0:
								goto end_IL_06d6;
							case 1:
								goto end_IL_06d6;
							}
							goto IL_0711;
							IL_0711:
							gf0Rq0cgyWhibXFkBEM(process, dsixtSPyYHjKiE5k7WK.VtIPNcTU9E);
							num13 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_e3198421401646eabf32dc544445bbbe == 0)
							{
								num13 = 1;
							}
							goto IL_06fb;
							end_IL_06d6:;
						}
						break;
					}
				}
				catch (Exception ex)
				{
					int num14 = 2;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_7f8cf60e2e7b454b95cc4d13162f5088 != 0)
					{
						num14 = 1;
					}
					while (true)
					{
						switch (num14)
						{
						case 1:
							result = -1;
							num14 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_45ac76a6368943c6981dcfd9e66428ae != 0)
							{
								num14 = 0;
							}
							continue;
						case 2:
							error = (string)XnUyVEc5Tkm1KK2H1pc(HBbvWMcE0vjQIqqqYSs(--1250240617 ^ 0x4A8572D3), nuJndEkE3v9PTORZQy5(ex, gjqRKtPV17ZUEQVAWxv.AdKPK0ve7w), RxUSOmPDJGsMmCCGPPg.hbkPt9hhua);
							num14 = 1;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d34bf7edba6543039b52c92f6f825304 != 0)
							{
								num14 = 1;
							}
							continue;
						case 0:
							break;
						}
						break;
					}
				}
				break;
			}
			break;
		}
		return result;
	}

	private static bool WaitHealth(int port, int pid, out string error)
	{
		int num = 4;
		int pid2 = default(int);
		string version = default(string);
		DateTime dateTime = default(DateTime);
		DateTime dateTime2 = default(DateTime);
		while (true)
		{
			int num2 = num;
			while (true)
			{
				switch (num2)
				{
				case 12:
				case 14:
					if (TryReadHealth(port, out pid2, out version))
					{
						num = 5;
						break;
					}
					goto IL_00ae;
				case 5:
					if (pid2 == pid)
					{
						num2 = 9;
						continue;
					}
					goto IL_00ae;
				case 13:
					return false;
				case 1:
				case 2:
					return true;
				case 4:
					error = null;
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_dd83a9ff75d94001819d80b24ef9a897 == 0)
					{
						num2 = 3;
					}
					continue;
				default:
					if (gqHO2VkQtO2WPBl4eN0(Or2RQ3bh3dyXsRwDb52.jnq8ilIcK(Or2RQ3bh3dyXsRwDb52.KoIbYNfyXF), dateTime, hFANSiycpt2XpYYe2h8.yqKykUvbgx))
					{
						num2 = 14;
						continue;
					}
					goto case 7;
				case 8:
					error = (string)uhAnXmcBtDKqQjGYOsb(HBbvWMcE0vjQIqqqYSs(0x2BF9058 ^ 0x2BFC60E), version, HBbvWMcE0vjQIqqqYSs(-909674956 ^ -909696420), dDG4K3E4O2Ahv334CHT.mOZEwuXRLl);
					num2 = 8;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d34bf7edba6543039b52c92f6f825304 == 0)
					{
						num2 = 13;
					}
					continue;
				case 6:
					return false;
				case 3:
					dateTime2 = Or2RQ3bh3dyXsRwDb52.jnq8ilIcK(Or2RQ3bh3dyXsRwDb52.KoIbYNfyXF);
					num2 = 7;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_db887f4a099249249dfc8abe84bd3ac9 == 0)
					{
						num2 = 11;
					}
					continue;
				case 10:
					if (!l2hSytck1xudCQ6WKdC(version, HBbvWMcE0vjQIqqqYSs(-1164549976 ^ -1164563306), NgtBADEr5RrpT3l6FkM.YGgE7TJZkD))
					{
						num = 2;
						break;
					}
					goto case 8;
				case 11:
					dateTime = rT7A8eUeTkEL4NogJZw.jnq8ilIcK(ref dateTime2, 20.0, rT7A8eUeTkEL4NogJZw.MWEQyQDa8m);
					num2 = 15;
					continue;
				case 9:
					if (N1tlkEcoZnwFWqSBqQf(version, lRNPJoPvsXOpQ7Ucdon.VmKEqmiqNI))
					{
						num2 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_dd83a9ff75d94001819d80b24ef9a897 == 0)
						{
							num2 = 1;
						}
						continue;
					}
					goto case 10;
				case 7:
					{
						error = (string)smJ0NkcDnTFTfrSLPGO(new string[5]
						{
							z2jc63fLkugS1X8Q9N.tE1kD1vbB(0x1DB17C88 ^ 0x1DB12F94),
							gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref pid, gGrLFhEdofbUymLMOb3.RREELfe2hN),
							(string)HBbvWMcE0vjQIqqqYSs(-1356900703 ^ -1356919729),
							gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref port, gGrLFhEdofbUymLMOb3.RREELfe2hN),
							z2jc63fLkugS1X8Q9N.tE1kD1vbB(0x3EDBC22B ^ 0x3EDB948F)
						}, dMZjkAEvscZOQ4dkL9w.GZiEg2xxMP);
						num2 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_6d61bc3aa64a47bfb8a09d13c4deb53b != 0)
						{
							num2 = 6;
						}
						continue;
					}
					IL_00ae:
					Qtjn2JkUGZ8ttqL2vU8(400, iNxACn2fCKfnjj1l3cF.zH92o9KkAB);
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c33375e472634b5ebc4d636397603446 == 0)
					{
						num2 = 0;
					}
					continue;
				}
				break;
			}
		}
	}

	private static bool TryReadHealth(int port, out int pid, out string version)
	{
		int num = 5;
		int num2 = num;
		bool result = default(bool);
		while (true)
		{
			switch (num2)
			{
			case 3:
				if (IsLoopbackListening(port))
				{
					num2 = 2;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_cbea79c7647f4fe9ae446d7b78f25d28 == 0)
					{
						num2 = 0;
					}
					continue;
				}
				goto default;
			case 1:
				try
				{
					WeComBridgeHttp weComBridgeHttp = new WeComBridgeHttp(port, null, 2);
					int num3 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_fe6655b3fe1741f7b687606f3c613956 == 0)
					{
						num3 = 0;
					}
					switch (num3)
					{
					default:
						try
						{
							BridgeHttpResult bridgeHttpResult = (BridgeHttpResult)CFrTnvcniRQQXXSdR96(weComBridgeHttp, HBbvWMcE0vjQIqqqYSs(0x7A7F3915 ^ 0x7A7F620B));
							int num4 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_84f159339c0c4d909a5aafadeb20c94b != 0)
							{
								num4 = 0;
							}
							while (true)
							{
								switch (num4)
								{
								case 1:
									break;
								case 2:
								case 4:
									pid = ((JToken)efjQ7ccc17w24KpXKpj(bridgeHttpResult)).Value<int?>(HBbvWMcE0vjQIqqqYSs(-1309619823 ^ -1309638183)).GetValueOrDefault();
									num4 = 7;
									continue;
								case 7:
									version = ((JToken)efjQ7ccc17w24KpXKpj(bridgeHttpResult)).Value<string>((object)z2jc63fLkugS1X8Q9N.tE1kD1vbB(-1947700697 ^ -1947719401));
									num4 = 8;
									continue;
								case 8:
									result = pid > 0;
									num4 = 5;
									continue;
								default:
									if (bridgeHttpResult.Ok)
									{
										num4 = 6;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_bd908099f891496e8274f46975b80fa0 == 0)
										{
											num4 = 2;
										}
										continue;
									}
									goto case 3;
								case 3:
									result = false;
									num4 = 0;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_a4eb1fc2846b481197982b0faf1ae1eb == 0)
									{
										num4 = 1;
									}
									continue;
								case 6:
									if (efjQ7ccc17w24KpXKpj(bridgeHttpResult) != null)
									{
										num4 = 1;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d604b1379fb540e5b6d79e791c2f5762 != 0)
										{
											num4 = 2;
										}
										continue;
									}
									goto case 3;
								case 5:
									break;
								}
								break;
							}
						}
						finally
						{
							if (weComBridgeHttp != null)
							{
								int num5 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_aaf9213cc3c34825a5d331618f885f65 != 0)
								{
									num5 = 1;
								}
								while (true)
								{
									switch (num5)
									{
									case 1:
										gf0Rq0cgyWhibXFkBEM(weComBridgeHttp, dsixtSPyYHjKiE5k7WK.VtIPNcTU9E);
										num5 = 0;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_32c4deae05384207a567e3dea0ef9d3f == 0)
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
						break;
					}
				}
				catch
				{
					int num6 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c1db6875e3a1467ca73ac64ce11e3ed4 == 0)
					{
						num6 = 0;
					}
					while (true)
					{
						switch (num6)
						{
						default:
							result = false;
							num6 = 1;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_dd83a9ff75d94001819d80b24ef9a897 != 0)
							{
								num6 = 0;
							}
							continue;
						case 1:
							break;
						}
						break;
					}
				}
				break;
			case 5:
				pid = 0;
				num2 = 4;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d08eb354b58c49268ddfa8be7c8ef2b8 == 0)
				{
					num2 = 1;
				}
				continue;
			case 4:
				version = null;
				num2 = 3;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_2d7366bbeab1424ea4ee5fb2951fe83a == 0)
				{
					num2 = 1;
				}
				continue;
			default:
				return false;
			case 2:
				num2 = 1;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_eb326c992e4b4c9f9cd930152533b185 != 0)
				{
					num2 = 0;
				}
				continue;
			case 6:
				break;
			}
			break;
		}
		return result;
	}

	private static string WaitToken(int pid, out string error)
	{
		int num = 1;
		int num2 = num;
		string text = default(string);
		string result = default(string);
		string text2 = default(string);
		DateTime dateTime2 = default(DateTime);
		DateTime dateTime = default(DateTime);
		while (true)
		{
			switch (num2)
			{
			case 3:
				error = (string)uhAnXmcBtDKqQjGYOsb(HBbvWMcE0vjQIqqqYSs(0x1DB17C88 ^ 0x1DB12734), gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref pid, gGrLFhEdofbUymLMOb3.RREELfe2hN), HBbvWMcE0vjQIqqqYSs(0x789D7F68 ^ 0x789D24A4), dDG4K3E4O2Ahv334CHT.mOZEwuXRLl);
				num2 = 9;
				break;
			case 1:
				error = null;
				num2 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_45ac76a6368943c6981dcfd9e66428ae == 0)
				{
					num2 = 0;
				}
				break;
			case 8:
				try
				{
					int num3;
					if (!N1tlkEcoZnwFWqSBqQf(text, lRNPJoPvsXOpQ7Ucdon.i4TPgmP1uC))
					{
						num3 = 2;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_0a8dc1e2655145c5af97263c14d91cce != 0)
						{
							num3 = 3;
						}
						goto IL_00f6;
					}
					goto IL_0127;
					IL_00f6:
					while (true)
					{
						switch (num3)
						{
						case 1:
							break;
						case 3:
							goto end_IL_00f6;
						default:
							result = text2;
							num3 = 5;
							continue;
						case 2:
							if (!lRNPJoPvsXOpQ7Ucdon.jnq8ilIcK(text2, lRNPJoPvsXOpQ7Ucdon.yHSEjK7hI0))
							{
								num3 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_2cca3d059d854b57aca52e0b80a8eb7d != 0)
								{
									num3 = 0;
								}
								continue;
							}
							goto end_IL_00f6;
						case 4:
							goto end_IL_00f6;
						case 5:
							return result;
						}
						goto IL_0127;
						continue;
						end_IL_00f6:
						break;
					}
					goto end_IL_00c7;
					IL_0127:
					text2 = ((JToken)PQlnPRc8aiFLYTynHIF(qvg6uAkh9MNMmfEmnpA(text, ekWklhczmvHysJVmCg6(LCpK8kxFhGIbWVyMacY.PGUxp2INU4), zjw262Yb5R8TAJnhPc1.VW4YxvQqAD), Jd2FevQG6huGknEZHcv.OABQ6b4sTo)).Value<string>(HBbvWMcE0vjQIqqqYSs(0x1BD51FB4 ^ 0x1BD54422));
					num3 = 2;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_f0c3ce86ec4a4bf2b2132a6d66c41033 != 0)
					{
						num3 = 1;
					}
					goto IL_00f6;
					end_IL_00c7:;
				}
				catch (Exception ex)
				{
					int num4 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_bc19572226e743cb89917af019cec5df != 0)
					{
						num4 = 0;
					}
					while (true)
					{
						switch (num4)
						{
						default:
							Logger.Warning((string)XnUyVEc5Tkm1KK2H1pc(z2jc63fLkugS1X8Q9N.tE1kD1vbB(-896341688 ^ -896355604), gjqRKtPV17ZUEQVAWxv.jnq8ilIcK(ex, gjqRKtPV17ZUEQVAWxv.AdKPK0ve7w), RxUSOmPDJGsMmCCGPPg.hbkPt9hhua));
							num4 = 1;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_5a84d0ea826a44b299199b540d6e6fc8 == 0)
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
				goto case 5;
			case 6:
				dateTime2 = rT7A8eUeTkEL4NogJZw.jnq8ilIcK(ref dateTime, 20.0, rT7A8eUeTkEL4NogJZw.MWEQyQDa8m);
				num2 = 10;
				break;
			case 4:
			case 10:
				if (gqHO2VkQtO2WPBl4eN0(YxhfgMcSVWyErXdREjP(Or2RQ3bh3dyXsRwDb52.KoIbYNfyXF), dateTime2, hFANSiycpt2XpYYe2h8.yqKykUvbgx))
				{
					num2 = 11;
					break;
				}
				goto case 3;
			case 2:
			case 11:
				num2 = 8;
				break;
			case 7:
				dateTime = YxhfgMcSVWyErXdREjP(Or2RQ3bh3dyXsRwDb52.KoIbYNfyXF);
				num2 = 6;
				break;
			case 5:
				iNxACn2fCKfnjj1l3cF.jnq8ilIcK(400, iNxACn2fCKfnjj1l3cF.zH92o9KkAB);
				num2 = 2;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_fe6655b3fe1741f7b687606f3c613956 == 0)
				{
					num2 = 4;
				}
				break;
			case 9:
				return null;
			default:
				text = (string)gFPhhUcNZJrKvR117L5(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), z2jc63fLkugS1X8Q9N.tE1kD1vbB(-1164549976 ^ -1164559368), HBbvWMcE0vjQIqqqYSs(-183227658 ^ -183209584), RxUSOmPDJGsMmCCGPPg.jnq8ilIcK(gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref pid, gGrLFhEdofbUymLMOb3.RREELfe2hN), z2jc63fLkugS1X8Q9N.tE1kD1vbB(-1121507562 ^ -1121485714), RxUSOmPDJGsMmCCGPPg.hbkPt9hhua), m4SFBaEZmre5qY3dKfT.KU9bgnuG97);
				num2 = 2;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_080a2cdddf2144a182f32bab096ad248 == 0)
				{
					num2 = 7;
				}
				break;
			}
		}
	}

	private static int[] SnapshotWxWorkPids()
	{
		int num = 2;
		int num2 = num;
		int num7 = default(int);
		Process process = default(Process);
		List<int> list = default(List<int>);
		while (true)
		{
			switch (num2)
			{
			case 1:
				try
				{
					Process[] array = (Process[])TA3a7bkY7cSmHyDJp2t(HBbvWMcE0vjQIqqqYSs(0x17D0B783 ^ 0x17D0AE51), vCD6HcPaOyR6Koj64nc.UfHPSCYhs5);
					int num3 = 3;
					int num4 = num3;
					while (true)
					{
						switch (num4)
						{
						case 2:
						case 4:
							if (num7 >= array.Length)
							{
								num4 = 5;
								continue;
							}
							goto case 6;
						case 3:
							num7 = 0;
							num4 = 2;
							continue;
						case 6:
							process = array[num7];
							num4 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_b4d4669a918b4b838c27ddb986baa9b6 == 0)
							{
								num4 = 0;
							}
							continue;
						case 1:
							break;
						default:
							try
							{
								list.Add(CJTUy1k2GLLSsExIbVp(process, kfHGSgPnOQlgJ5xXCSg.iYgPcMfokN));
								int num5 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d34bf7edba6543039b52c92f6f825304 == 0)
								{
									num5 = 0;
								}
								switch (num5)
								{
								case 0:
									break;
								}
							}
							finally
							{
								dsixtSPyYHjKiE5k7WK.jnq8ilIcK(process, dsixtSPyYHjKiE5k7WK.cBDyQCeBth);
								int num6 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_529db4a9032c417dbe74a080d04d36c2 != 0)
								{
									num6 = 0;
								}
								switch (num6)
								{
								case 0:
									break;
								}
							}
							break;
						case 5:
							goto end_IL_005d;
						}
						num7++;
						num4 = 4;
						continue;
						end_IL_005d:
						break;
					}
				}
				catch (Exception ex)
				{
					int num8 = 1;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_9de6b4ecde674f909622ae29ccaad31e == 0)
					{
						num8 = 0;
					}
					while (true)
					{
						switch (num8)
						{
						case 1:
							lymWLNceeC5sWAA6hPE(RxUSOmPDJGsMmCCGPPg.jnq8ilIcK((string)HBbvWMcE0vjQIqqqYSs(0x7AFEF0A3 ^ 0x7AFEBDB7), gjqRKtPV17ZUEQVAWxv.jnq8ilIcK(ex, gjqRKtPV17ZUEQVAWxv.AdKPK0ve7w), RxUSOmPDJGsMmCCGPPg.hbkPt9hhua));
							num8 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_99797e4f3bab41baabcb75f7a30397b7 != 0)
							{
								num8 = 0;
							}
							continue;
						case 0:
							break;
						}
						break;
					}
				}
				break;
			case 2:
				list = new List<int>();
				num2 = 1;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_a8ee04dc577b44a4b973e7119af8874c == 0)
				{
					num2 = 1;
				}
				continue;
			}
			break;
		}
		return (int[])W6NiWnkepoKPvbLyOy0(list);
	}

	private static int WaitNewWxWorkPid(HashSet<int> known, int timeoutMs, out string error)
	{
		error = null;
		DateTime dateTime = Or2RQ3bh3dyXsRwDb52.jnq8ilIcK(Or2RQ3bh3dyXsRwDb52.KoIbYNfyXF);
		DateTime dateTime2 = rT7A8eUeTkEL4NogJZw.jnq8ilIcK(ref dateTime, timeoutMs, rT7A8eUeTkEL4NogJZw.MsQhHScQnp);
		while (hFANSiycpt2XpYYe2h8.jnq8ilIcK(Or2RQ3bh3dyXsRwDb52.jnq8ilIcK(Or2RQ3bh3dyXsRwDb52.KoIbYNfyXF), dateTime2, hFANSiycpt2XpYYe2h8.yqKykUvbgx))
		{
			int num = 0;
			Process[] array = vCD6HcPaOyR6Koj64nc.jnq8ilIcK(z2jc63fLkugS1X8Q9N.tE1kD1vbB(0x745A8A68 ^ 0x745A93BA), vCD6HcPaOyR6Koj64nc.UfHPSCYhs5);
			foreach (Process process in array)
			{
				try
				{
					if (!known.Contains(kfHGSgPnOQlgJ5xXCSg.jnq8ilIcK(process, kfHGSgPnOQlgJ5xXCSg.iYgPcMfokN)))
					{
						num = kfHGSgPnOQlgJ5xXCSg.jnq8ilIcK(process, kfHGSgPnOQlgJ5xXCSg.iYgPcMfokN);
					}
				}
				finally
				{
					dsixtSPyYHjKiE5k7WK.jnq8ilIcK(process, dsixtSPyYHjKiE5k7WK.cBDyQCeBth);
				}
			}
			if (num > 0)
			{
				iNxACn2fCKfnjj1l3cF.jnq8ilIcK(800, iNxACn2fCKfnjj1l3cF.zH92o9KkAB);
				try
				{
					Process process2 = aMU2i5y03HSWa4qUbCd.jnq8ilIcK(num, aMU2i5y03HSWa4qUbCd.gVwyAsDh1k);
					try
					{
						return num;
					}
					finally
					{
						if (process2 != null)
						{
							dsixtSPyYHjKiE5k7WK.jnq8ilIcK(process2, dsixtSPyYHjKiE5k7WK.VtIPNcTU9E);
						}
					}
				}
				catch
				{
					known.Add(num);
				}
			}
			else
			{
				iNxACn2fCKfnjj1l3cF.jnq8ilIcK(400, iNxACn2fCKfnjj1l3cF.zH92o9KkAB);
			}
		}
		error = z2jc63fLkugS1X8Q9N.tE1kD1vbB(-2134178576 ^ -2134155476);
		return 0;
	}

	private static int ReleaseWeComInstanceMutexes()
	{
		int num = 4;
		int num2 = num;
		int[] array = default(int[]);
		int result = default(int);
		IntPtr intPtr = default(IntPtr);
		int num7 = default(int);
		int num8 = default(int);
		int[] array2 = default(int[]);
		int pid = default(int);
		while (true)
		{
			switch (num2)
			{
			case 5:
				return 0;
			case 1:
				num2 = 2;
				break;
			case 4:
				array = (int[])t8DAwfcVwC66MrmQD5d();
				num2 = 3;
				break;
			default:
				return result;
			case 2:
				try
				{
					WMMqnDkfwKr8Tvs6Tcc();
					int num3 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_83095058e27b497188a2c88050ba8f5f == 0)
					{
						num3 = 0;
					}
					while (true)
					{
						switch (num3)
						{
						case 3:
							break;
						case 1:
						case 5:
							num3 = 2;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1bbe7758123e4bc485e91d98227e25e4 == 0)
							{
								num3 = 4;
							}
							continue;
						default:
							intPtr = CreateMutexW(IntPtr.Zero, initialOwner: false, z2jc63fLkugS1X8Q9N.tE1kD1vbB(0x64FE6D0E ^ 0x64FE36F6));
							num3 = 6;
							continue;
						case 2:
							result = 0;
							num3 = 3;
							continue;
						case 6:
							if (!E6GRu8P7P9F6qI8rcqV.jnq8ilIcK(intPtr, IntPtr.Zero, E6GRu8P7P9F6qI8rcqV.ajPPdAfJO9))
							{
								num3 = 1;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_aa36ee1765db47c28f80e17ee8aff510 != 0)
								{
									num3 = 0;
								}
								continue;
							}
							goto case 7;
						case 7:
							lymWLNceeC5sWAA6hPE(HBbvWMcE0vjQIqqqYSs(-1161790814 ^ -1161812328));
							num3 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d34bf7edba6543039b52c92f6f825304 == 0)
							{
								num3 = 2;
							}
							continue;
						case 4:
							try
							{
								int num4;
								if (QuerySystemHandles(out var buffer, out var count))
								{
									num4 = 0;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_79220d9f20444b6c8b0df2558a43de3e == 0)
									{
										num4 = 0;
									}
									goto IL_01b2;
								}
								goto IL_021a;
								IL_01b2:
								while (true)
								{
									switch (num4)
									{
									default:
										num4 = 4;
										continue;
									case 2:
										result = 0;
										num4 = 0;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_99797e4f3bab41baabcb75f7a30397b7 != 0)
										{
											num4 = 1;
										}
										continue;
									case 1:
										goto end_IL_01b2;
									case 3:
										break;
									case 4:
										try
										{
											int num5 = NXy8JNkoTrwV0Zkbw28(buffer, count, intPtr);
											int num6 = 12;
											while (true)
											{
												switch (num6)
												{
												case 2:
													result = num7;
													num6 = 10;
													if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c09e27dcabbb4f6484836a4e1fd5da8e == 0)
													{
														num6 = 2;
													}
													continue;
												case 6:
													num7 = 0;
													num6 = 11;
													continue;
												case 1:
												case 7:
													if (num8 < array2.Length)
													{
														num6 = 5;
														if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c33375e472634b5ebc4d636397603446 != 0)
														{
															num6 = 9;
														}
														continue;
													}
													goto case 2;
												case 4:
													break;
												case 3:
												case 9:
													pid = array2[num8];
													num6 = 5;
													if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_dd83a9ff75d94001819d80b24ef9a897 == 0)
													{
														num6 = 13;
													}
													continue;
												case 12:
													if (num5 < 0)
													{
														num6 = 0;
														if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_76fed529d5134afd95ce65e7c04235fc == 0)
														{
															num6 = 0;
														}
														continue;
													}
													goto case 6;
												case 5:
												{
													result = 0;
													int num9 = 4;
													num6 = num9;
													continue;
												}
												case 8:
													num8 = 0;
													num6 = 1;
													if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_03aacce0025f45d8a46eb3d2a6e41868 == 0)
													{
														num6 = 1;
													}
													continue;
												case 11:
													array2 = array;
													num6 = 8;
													if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_2cca3d059d854b57aca52e0b80a8eb7d == 0)
													{
														num6 = 5;
													}
													continue;
												default:
													lymWLNceeC5sWAA6hPE(z2jc63fLkugS1X8Q9N.tE1kD1vbB(--445132931 ^ 0x1A886CF9));
													num6 = 5;
													continue;
												case 14:
													num8++;
													num6 = 7;
													continue;
												case 13:
													num7 += FweDZUkie1RK0oEB0V3(buffer, count, pid, num5);
													num6 = 10;
													if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_7f8cf60e2e7b454b95cc4d13162f5088 == 0)
													{
														num6 = 14;
													}
													continue;
												case 10:
													break;
												}
												break;
											}
										}
										finally
										{
											vJEaAvkV2N9DXL7sK3i(buffer, u4dP1aYym5FsbWYG27R.w1VY2IUOek);
											int num10 = 0;
											if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1fdc4536a75247b19a0801dd70bf34e9 != 0)
											{
												num10 = 0;
											}
											switch (num10)
											{
											case 0:
												break;
											}
										}
										goto end_IL_01b2;
									}
									goto IL_021a;
									continue;
									end_IL_01b2:
									break;
								}
								goto end_IL_0186;
								IL_021a:
								Logger.Warning((string)HBbvWMcE0vjQIqqqYSs(--1250240617 ^ 0x4A85743F));
								num4 = 2;
								goto IL_01b2;
								end_IL_0186:;
							}
							finally
							{
								CloseHandle(intPtr);
								int num11 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_4067485f69dd42b795335d8574adc3dd != 0)
								{
									num11 = 0;
								}
								switch (num11)
								{
								case 0:
									break;
								}
							}
							break;
						}
						break;
					}
				}
				catch (Exception ex)
				{
					int num12 = 1;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_981aa8b195a34f3abc27084c24a88bcd == 0)
					{
						num12 = 1;
					}
					while (true)
					{
						switch (num12)
						{
						default:
							result = 0;
							num12 = 2;
							continue;
						case 1:
							lymWLNceeC5sWAA6hPE(XnUyVEc5Tkm1KK2H1pc(HBbvWMcE0vjQIqqqYSs(0x1F6D7205 ^ 0x1F6D2EA5), gjqRKtPV17ZUEQVAWxv.jnq8ilIcK(ex, gjqRKtPV17ZUEQVAWxv.AdKPK0ve7w), RxUSOmPDJGsMmCCGPPg.hbkPt9hhua));
							num12 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_b3eccb7902c54a0db09f3abb83d000b4 != 0)
							{
								num12 = 0;
							}
							continue;
						case 2:
							break;
						}
						break;
					}
				}
				goto default;
			case 3:
				if (array.Length != 0)
				{
					num2 = 1;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1fdc4536a75247b19a0801dd70bf34e9 != 0)
					{
						num2 = 0;
					}
					break;
				}
				goto case 5;
			}
		}
	}

	private static int FindMutexType(IntPtr buffer, int count, IntPtr probe)
	{
		int num = 10;
		long num4 = default(long);
		int num3 = default(int);
		long handle = default(long);
		int currentProcessId = default(int);
		int type = default(int);
		long pid = default(long);
		while (true)
		{
			int num2 = num;
			while (true)
			{
				switch (num2)
				{
				case 9:
					num4 = fwcjZYQJaurakSkLuhh.jnq8ilIcK(ref probe, fwcjZYQJaurakSkLuhh.scAQOTqMva);
					num2 = 8;
					break;
				case 8:
					num3 = 0;
					num2 = 5;
					break;
				case 2:
					return -1;
				case 1:
					if (handle == num4)
					{
						num2 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_bc19572226e743cb89917af019cec5df != 0)
						{
							num2 = 4;
						}
						break;
					}
					goto IL_00d4;
				case 10:
					currentProcessId = GetCurrentProcessId();
					num2 = 9;
					break;
				case 4:
					return type;
				case 5:
				case 6:
					if (num3 < count)
					{
						goto end_IL_0012;
					}
					goto case 2;
				default:
					if (pid == currentProcessId)
					{
						num2 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_47f3813d118644b6b4369b1318743526 != 0)
						{
							num2 = 1;
						}
						break;
					}
					goto IL_00d4;
				case 3:
				case 7:
					{
						if (ReadHandleEntry(buffer, num3, out pid, out handle, out type))
						{
							num2 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_b3eccb7902c54a0db09f3abb83d000b4 != 0)
							{
								num2 = 0;
							}
							break;
						}
						goto IL_00d4;
					}
					IL_00d4:
					num3++;
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_fd96f1535e384acb8fa11da99670a759 == 0)
					{
						num2 = 6;
					}
					break;
				}
				continue;
				end_IL_0012:
				break;
			}
			num = 7;
		}
	}

	private static int CloseInstanceMutexes(IntPtr buffer, int count, int pid, int mutexType)
	{
		int num = 4;
		int num2 = num;
		int num6 = default(int);
		IntPtr intPtr = default(IntPtr);
		long handle = default(long);
		long pid2 = default(long);
		int type = default(int);
		IntPtr targetHandle = default(IntPtr);
		IntPtr targetHandle2 = default(IntPtr);
		while (true)
		{
			switch (num2)
			{
			case 2:
				return 0;
			case 6:
				num6 = 0;
				num2 = 5;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_bd908099f891496e8274f46975b80fa0 == 0)
				{
					num2 = 1;
				}
				break;
			case 4:
				intPtr = OpenProcess(64u, inherit: false, pid);
				num2 = 3;
				break;
			default:
				return num6;
			case 1:
				Logger.Warning((string)uhAnXmcBtDKqQjGYOsb(HBbvWMcE0vjQIqqqYSs(0x3B865138 ^ 0x3B860DF8), gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref pid, gGrLFhEdofbUymLMOb3.RREELfe2hN), HBbvWMcE0vjQIqqqYSs(0x1BD51FB4 ^ 0x1BD5436E), dDG4K3E4O2Ahv334CHT.mOZEwuXRLl));
				num2 = 2;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_cb5bc3d191fc4bc3b6ad04816cd3def1 != 0)
				{
					num2 = 2;
				}
				break;
			case 3:
				if (!l0YY3FkKD1vbivauv0X(intPtr, IntPtr.Zero, E6GRu8P7P9F6qI8rcqV.ajPPdAfJO9))
				{
					num2 = 6;
					break;
				}
				goto case 1;
			case 5:
				try
				{
					int num3 = 0;
					int num4 = 3;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_b4d4669a918b4b838c27ddb986baa9b6 != 0)
					{
						num4 = 1;
					}
					while (true)
					{
						int num8;
						switch (num4)
						{
						case 5:
							if (handle == 0L)
							{
								num4 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_bf7dadb9c4914436a2302bdcb2bac558 == 0)
								{
									num4 = 0;
								}
								continue;
							}
							goto case 10;
						default:
							num3++;
							num8 = 12;
							goto IL_0122;
						case 9:
							if (pid2 == pid)
							{
								num4 = 6;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d604b1379fb540e5b6d79e791c2f5762 != 0)
								{
									num4 = 7;
								}
								continue;
							}
							goto default;
						case 3:
						case 12:
							if (num3 >= count)
							{
								num4 = 1;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_2d8e9ecb9a5b407690948cd8111b2980 != 0)
								{
									num4 = 1;
								}
								continue;
							}
							goto case 4;
						case 4:
							if (ReadHandleEntry(buffer, num3, out pid2, out handle, out type))
							{
								num4 = 9;
								continue;
							}
							goto default;
						case 6:
							if (l0YY3FkKD1vbivauv0X(targetHandle, IntPtr.Zero, E6GRu8P7P9F6qI8rcqV.ajPPdAfJO9))
							{
								num4 = 2;
								continue;
							}
							goto case 8;
						case 10:
							if (DuplicateHandle(intPtr, new IntPtr(handle), GetCurrentProcess(), out targetHandle, 0u, inherit: false, 2u))
							{
								num4 = 6;
								continue;
							}
							goto default;
						case 8:
							num8 = 11;
							goto IL_0122;
						case 7:
							if (type == mutexType)
							{
								num4 = 4;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_83095058e27b497188a2c88050ba8f5f == 0)
								{
									num4 = 5;
								}
								continue;
							}
							goto default;
						case 11:
							try
							{
								int num5;
								if (!IarTu3ktpriqDE4Y5uK(O7NSbRkDxroQSuMCcxd(targetHandle)))
								{
									num5 = 1;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1fdc4536a75247b19a0801dd70bf34e9 != 0)
									{
										num5 = 1;
									}
									goto IL_02e8;
								}
								goto IL_033c;
								IL_02e8:
								while (true)
								{
									switch (num5)
									{
									default:
										goto end_IL_02e8;
									case 4:
										CloseHandle(targetHandle2);
										num5 = 2;
										continue;
									case 2:
										num6++;
										num5 = 0;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_bd908099f891496e8274f46975b80fa0 != 0)
										{
											num5 = 0;
										}
										continue;
									case 5:
										break;
									case 1:
										goto end_IL_02e8;
									case 3:
										if (E6GRu8P7P9F6qI8rcqV.jnq8ilIcK(targetHandle2, IntPtr.Zero, E6GRu8P7P9F6qI8rcqV.qwMQxr2Hqc))
										{
											num5 = 4;
											continue;
										}
										goto case 2;
									case 0:
										goto end_IL_02e8;
									}
									goto IL_033c;
									continue;
									end_IL_02e8:
									break;
								}
								goto end_IL_02b9;
								IL_033c:
								if (DuplicateHandle(intPtr, new IntPtr(handle), GetCurrentProcess(), out targetHandle2, 0u, inherit: false, 1u))
								{
									num5 = 3;
									goto IL_02e8;
								}
								end_IL_02b9:;
							}
							finally
							{
								CloseHandle(targetHandle);
								int num7 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_4819ae0b3e934df4906fdac118b2ddf1 == 0)
								{
									num7 = 0;
								}
								switch (num7)
								{
								case 0:
									break;
								}
							}
							goto default;
						case 1:
							break;
							IL_0122:
							num4 = num8;
							continue;
						}
						break;
					}
				}
				finally
				{
					CloseHandle(intPtr);
					int num9 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_a34de5cebe4f4eeda550f236a75b91de == 0)
					{
						num9 = 0;
					}
					switch (num9)
					{
					case 0:
						break;
					}
				}
				goto default;
			}
		}
	}

	private static bool IsInstanceMutex(object name)
	{
		int num = 2;
		int num3 = default(int);
		while (true)
		{
			int num2 = num;
			while (true)
			{
				switch (num2)
				{
				case 6:
					return false;
				case 1:
					num3 = 0;
					num2 = 4;
					break;
				case 2:
					if (!N1tlkEcoZnwFWqSBqQf(name, lRNPJoPvsXOpQ7Ucdon.VmKEqmiqNI))
					{
						num2 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_90f58c7ffb21444ca88319aa26ca6aa0 != 0)
						{
							num2 = 1;
						}
						break;
					}
					goto case 6;
				default:
					return false;
				case 7:
					return true;
				case 4:
				case 8:
					if (num3 < ((Array)InstanceMutexSuffixes).Length)
					{
						goto end_IL_0012;
					}
					goto default;
				case 3:
				case 5:
					if (!wj7LywYU4eY9QWnywwq.jnq8ilIcK(name, (string)((object[])InstanceMutexSuffixes)[num3], StringComparison.Ordinal, wj7LywYU4eY9QWnywwq.FlCYQxeL96))
					{
						num3++;
						num2 = 7;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c09e27dcabbb4f6484836a4e1fd5da8e != 0)
						{
							num2 = 8;
						}
					}
					else
					{
						num2 = 7;
					}
					break;
				}
				continue;
				end_IL_0012:
				break;
			}
			num = 3;
		}
	}

	private static bool ReadHandleEntry(IntPtr buffer, int index, out long pid, out long handle, out int type)
	{
		int num = 1;
		HandleEntry32 handleEntry = default(HandleEntry32);
		int num4 = default(int);
		HandleEntry64 handleEntry2 = default(HandleEntry64);
		int num3 = default(int);
		while (true)
		{
			int num2 = num;
			while (true)
			{
				switch (num2)
				{
				case 4:
					handleEntry = (HandleEntry32)a5dtFWYhISuNlG0dGAm.jnq8ilIcK(new IntPtr(fwcjZYQJaurakSkLuhh.jnq8ilIcK(ref buffer, fwcjZYQJaurakSkLuhh.scAQOTqMva) + 12 + (long)index * (long)num4), sqr80GkBoUKbvZgUk9n(typeof(HandleEntry32).TypeHandle, THNTxYEVPVbLybtAqi1.PLCEKy6KBP), a5dtFWYhISuNlG0dGAm.FBqYYL47jj);
					num2 = 10;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_5a84d0ea826a44b299199b540d6e6fc8 == 0)
					{
						num2 = 10;
					}
					continue;
				case 11:
					return true;
				case 2:
					pid = handleEntry2.UniqueProcessId;
					num2 = 9;
					continue;
				case 14:
					type = handleEntry2.ObjectTypeIndex;
					num2 = 5;
					continue;
				case 5:
					return true;
				default:
					handle = 0L;
					num = 13;
					break;
				case 8:
					type = handleEntry.ObjectTypeIndex;
					num2 = 3;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_a4eb1fc2846b481197982b0faf1ae1eb == 0)
					{
						num2 = 11;
					}
					continue;
				case 6:
					num4 = srK3uwk0r0xfuiPw4Iu(sqr80GkBoUKbvZgUk9n(typeof(HandleEntry32).TypeHandle, THNTxYEVPVbLybtAqi1.PLCEKy6KBP), RNf552EDJrbs9F9iSGt.niNEts2C7C);
					num2 = 4;
					continue;
				case 12:
					handleEntry2 = (HandleEntry64)rvZouCkA9Oi6tC2r7n5(new IntPtr(fwcjZYQJaurakSkLuhh.jnq8ilIcK(ref buffer, fwcjZYQJaurakSkLuhh.scAQOTqMva) + 16 + (long)index * (long)num3), sqr80GkBoUKbvZgUk9n(typeof(HandleEntry64).TypeHandle, THNTxYEVPVbLybtAqi1.PLCEKy6KBP), a5dtFWYhISuNlG0dGAm.FBqYYL47jj);
					num2 = 2;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_265198d303964f58a1121245a89716c5 != 0)
					{
						num2 = 1;
					}
					continue;
				case 9:
					handle = handleEntry2.HandleValue;
					num = 14;
					break;
				case 1:
					pid = 0L;
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_93e9b0bbc1fe4ae1a0fca3879b36d9a6 == 0)
					{
						num2 = 0;
					}
					continue;
				case 13:
					type = 0;
					num = 3;
					break;
				case 7:
					handle = handleEntry.HandleValue;
					num2 = 8;
					continue;
				case 3:
					if (zPTqJrk5Q7UReKPJ31x(wxuXMNEPBwAMyhQ4pNJ.TV1EEDlsPj) != 4)
					{
						num3 = srK3uwk0r0xfuiPw4Iu(sqr80GkBoUKbvZgUk9n(typeof(HandleEntry64).TypeHandle, THNTxYEVPVbLybtAqi1.PLCEKy6KBP), RNf552EDJrbs9F9iSGt.niNEts2C7C);
						num2 = 9;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_76a1008ec43840dda25966a6b044ac77 == 0)
						{
							num2 = 12;
						}
					}
					else
					{
						num2 = 6;
					}
					continue;
				case 10:
					pid = handleEntry.UniqueProcessId;
					num2 = 7;
					continue;
				}
				break;
			}
		}
	}

	private static bool QuerySystemHandles(out IntPtr buffer, out int count)
	{
		int num = 13;
		int num2 = num;
		int num5 = default(int);
		int num3 = default(int);
		int num4 = default(int);
		int returnLength = default(int);
		IntPtr intPtr = default(IntPtr);
		while (true)
		{
			switch (num2)
			{
			case 6:
				if (count <= 0)
				{
					num2 = 16;
					break;
				}
				goto case 8;
			case 18:
				return false;
			case 3:
			case 10:
				if (num5 < 8)
				{
					num2 = 10;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_080a2cdddf2144a182f32bab096ad248 == 0)
					{
						num2 = 14;
					}
					break;
				}
				goto case 17;
			case 13:
				buffer = IntPtr.Zero;
				num2 = 12;
				break;
			case 2:
				num5 = 0;
				num2 = 10;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_db887f4a099249249dfc8abe84bd3ac9 != 0)
				{
					num2 = 1;
				}
				break;
			case 4:
				if (num3 == StatusInfoLengthMismatch)
				{
					num4 = lWsOq8kX8XQXBEhlZap(num4 * 2, returnLength, yjijbDUp9NiSjcZo47Q.LUbUJ2XBBB);
					num2 = 7;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_fe6655b3fe1741f7b687606f3c613956 != 0)
					{
						num2 = 2;
					}
				}
				else
				{
					num2 = 18;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c1db6875e3a1467ca73ac64ce11e3ed4 == 0)
					{
						num2 = 9;
					}
				}
				break;
			case 1:
				if (num3 == 0)
				{
					num2 = 11;
					break;
				}
				u4dP1aYym5FsbWYG27R.jnq8ilIcK(intPtr, u4dP1aYym5FsbWYG27R.w1VY2IUOek);
				num2 = 4;
				break;
			case 17:
				return false;
			case 9:
			case 14:
				intPtr = cUVhFMYeWAFhw40H9Cw.jnq8ilIcK(num4, cUVhFMYeWAFhw40H9Cw.s5hYfEFphr);
				num2 = 5;
				break;
			case 5:
				num3 = NtQuerySystemInformation(64, intPtr, num4, out returnLength);
				num2 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_043809f5a9d2487fae267fd26a3fe35d == 0)
				{
					num2 = 1;
				}
				break;
			case 11:
				count = (int)u6pMRnYouUI4JNdZuqV.jnq8ilIcK(intPtr, u6pMRnYouUI4JNdZuqV.IrwYiArx9C);
				num2 = 13;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d86987deeedc4dab9e9f358fe117cd1f != 0)
				{
					num2 = 15;
				}
				break;
			case 8:
				return count < 5000000;
			case 16:
				return false;
			case 7:
				num5++;
				num2 = 3;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d91865d38b6c449295c8f801546a735d != 0)
				{
					num2 = 0;
				}
				break;
			case 15:
				buffer = intPtr;
				num2 = 2;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_23f2079970f0474d9a38aefb1de2f52d == 0)
				{
					num2 = 6;
				}
				break;
			default:
				num4 = 1048576;
				num2 = 2;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_f8d69a0d83e2420b83127060d822cafd == 0)
				{
					num2 = 2;
				}
				break;
			case 12:
				count = 0;
				num2 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_a34de5cebe4f4eeda550f236a75b91de != 0)
				{
					num2 = 0;
				}
				break;
			}
		}
	}

	private static string QueryObjectName(IntPtr handle)
	{
		int num = 3;
		int num2 = num;
		IntPtr intPtr = default(IntPtr);
		int num3 = default(int);
		ObjectName objectName = default(ObjectName);
		string result = default(string);
		while (true)
		{
			switch (num2)
			{
			case 1:
				try
				{
					int returnLength;
					int num4 = NtQueryObject(handle, 1, intPtr, num3, out returnLength);
					int num5 = 1;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_043809f5a9d2487fae267fd26a3fe35d != 0)
					{
						num5 = 0;
					}
					while (true)
					{
						switch (num5)
						{
						case 15:
							if (returnLength <= 0)
							{
								num5 = 13;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_e3198421401646eabf32dc544445bbbe == 0)
								{
									num5 = 18;
								}
								continue;
							}
							goto case 8;
						case 12:
							vJEaAvkV2N9DXL7sK3i(intPtr, u4dP1aYym5FsbWYG27R.w1VY2IUOek);
							num5 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_45ac76a6368943c6981dcfd9e66428ae != 0)
							{
								num5 = 13;
							}
							continue;
						case 11:
							num3 = returnLength;
							num5 = 3;
							continue;
						case 10:
							break;
						case 17:
						{
							objectName = (ObjectName)rvZouCkA9Oi6tC2r7n5(intPtr, sqr80GkBoUKbvZgUk9n(typeof(ObjectName).TypeHandle, THNTxYEVPVbLybtAqi1.PLCEKy6KBP), a5dtFWYhISuNlG0dGAm.FBqYYL47jj);
							int num6 = 2;
							num5 = num6;
							continue;
						}
						case 9:
						case 14:
							result = null;
							num5 = 6;
							continue;
						case 1:
							if (num4 == StatusInfoLengthMismatch)
							{
								num5 = 15;
								continue;
							}
							goto case 5;
						case 8:
							if (returnLength >= 65536)
							{
								num5 = 16;
								continue;
							}
							goto case 12;
						case 6:
							break;
						case 3:
							num4 = NtQueryObject(handle, 1, intPtr, num3, out returnLength);
							num5 = 5;
							continue;
						default:
							result = ny1OfeYVrw9Ko9faNTv.jnq8ilIcK(objectName.Buffer, objectName.Length / 2, ny1OfeYVrw9Ko9faNTv.ROXYKbVMMJ);
							num5 = 15;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_7f545199031c4e3289afcf313932f580 == 0)
							{
								num5 = 19;
							}
							continue;
						case 7:
							result = null;
							num5 = 10;
							continue;
						case 4:
							if (E6GRu8P7P9F6qI8rcqV.jnq8ilIcK(objectName.Buffer, IntPtr.Zero, E6GRu8P7P9F6qI8rcqV.ajPPdAfJO9))
							{
								num5 = 14;
								continue;
							}
							goto default;
						case 5:
						case 16:
						case 18:
							if (num4 != 0)
							{
								num5 = 7;
								continue;
							}
							goto case 17;
						case 13:
							intPtr = G4QvsckTkyIXSaZMROr(returnLength, cUVhFMYeWAFhw40H9Cw.s5hYfEFphr);
							num5 = 11;
							continue;
						case 2:
							if (objectName.Length == 0)
							{
								num5 = 2;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_bf7dadb9c4914436a2302bdcb2bac558 == 0)
								{
									num5 = 9;
								}
								continue;
							}
							goto case 4;
						case 19:
							break;
						}
						break;
					}
				}
				finally
				{
					u4dP1aYym5FsbWYG27R.jnq8ilIcK(intPtr, u4dP1aYym5FsbWYG27R.w1VY2IUOek);
					int num7 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_5bb1f583c3f24addaf7d69d4facec67c == 0)
					{
						num7 = 0;
					}
					switch (num7)
					{
					case 0:
						break;
					}
				}
				break;
			case 3:
				num3 = 512;
				num2 = 2;
				continue;
			case 2:
				intPtr = cUVhFMYeWAFhw40H9Cw.jnq8ilIcK(num3, cUVhFMYeWAFhw40H9Cw.s5hYfEFphr);
				num2 = 1;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_9a207314288741c0b10cb80afbe48750 == 0)
				{
					num2 = 1;
				}
				continue;
			}
			break;
		}
		return result;
	}

	private static void EnableDebugPrivilege()
	{
		int num = 1;
		int num2 = num;
		IntPtr token = default(IntPtr);
		TokenPrivileges newState = default(TokenPrivileges);
		TokenPrivileges tokenPrivileges = default(TokenPrivileges);
		while (true)
		{
			switch (num2)
			{
			default:
				return;
			case 2:
				try
				{
					if (!LookupPrivilegeValue(null, (string)HBbvWMcE0vjQIqqqYSs(-608990028 ^ -608972710), out var luid))
					{
						return;
					}
					int num3 = 6;
					while (true)
					{
						switch (num3)
						{
						case 3:
							AdjustTokenPrivileges(token, disableAll: false, ref newState, 0, IntPtr.Zero, IntPtr.Zero);
							num3 = 1;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_e3198421401646eabf32dc544445bbbe == 0)
							{
								num3 = 1;
							}
							break;
						case 5:
							tokenPrivileges.Count = 1;
							num3 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_3c2786a3c96a4c30b2aea580e623804b != 0)
							{
								num3 = 4;
							}
							break;
						case 7:
							return;
						case 4:
							tokenPrivileges.Luid = luid;
							num3 = 8;
							break;
						case 8:
							tokenPrivileges.Attributes = 2;
							num3 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_a1f12b82c6a64d0b9c37bec032cf2c41 == 0)
							{
								num3 = 0;
							}
							break;
						default:
							newState = tokenPrivileges;
							num3 = 3;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_cc95e336569e4097a8a610a6e511075d == 0)
							{
								num3 = 3;
							}
							break;
						case 2:
						case 6:
							tokenPrivileges = default(TokenPrivileges);
							num3 = 5;
							break;
						case 1:
							return;
						}
					}
				}
				finally
				{
					CloseHandle(token);
					int num4 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c2d1dda68e3b4e11855617e481626480 == 0)
					{
						num4 = 0;
					}
					switch (num4)
					{
					case 0:
						break;
					}
				}
			case 3:
				return;
			case 1:
				if (!OpenProcessToken(GetCurrentProcess(), 40u, out token))
				{
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_2d7366bbeab1424ea4ee5fb2951fe83a == 0)
					{
						num2 = 0;
					}
				}
				else
				{
					num2 = 1;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_eb326c992e4b4c9f9cd930152533b185 == 0)
					{
						num2 = 2;
					}
				}
				break;
			case 0:
				return;
			}
		}
	}

	[DllImport("iphlpapi.dll", SetLastError = true)]
	private static extern uint GetExtendedTcpTable(IntPtr table, ref int size, bool order, int addressFamily, int tableClass, uint reserved);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr CreateToolhelp32Snapshot(uint flags, uint processId);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern bool Module32FirstW(IntPtr snapshot, ref ModuleEntry32 entry);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern bool Module32NextW(IntPtr snapshot, ref ModuleEntry32 entry);

	[DllImport("ntdll.dll")]
	private static extern int NtQuerySystemInformation(int infoClass, IntPtr buffer, int length, out int returnLength);

	[DllImport("ntdll.dll")]
	private static extern int NtQueryObject(IntPtr handle, int infoClass, IntPtr buffer, int length, out int returnLength);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr OpenProcess(uint access, bool inherit, int pid);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool CloseHandle(IntPtr handle);

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetCurrentProcess();

	[DllImport("kernel32.dll")]
	private static extern int GetCurrentProcessId();

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern IntPtr CreateMutexW(IntPtr attributes, bool initialOwner, string name);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool DuplicateHandle(IntPtr sourceProcess, IntPtr sourceHandle, IntPtr targetProcess, out IntPtr targetHandle, uint access, bool inherit, uint options);

	[DllImport("advapi32.dll", SetLastError = true)]
	private static extern bool OpenProcessToken(IntPtr process, uint access, out IntPtr token);

	[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern bool LookupPrivilegeValue(string system, string name, out long luid);

	[DllImport("advapi32.dll", SetLastError = true)]
	private static extern bool AdjustTokenPrivileges(IntPtr token, bool disableAll, ref TokenPrivileges newState, int length, IntPtr previous, IntPtr returnLength);

	private static bool BridgeAlreadyLoaded(int pid)
	{
		int num = 2;
		IntPtr intPtr = default(IntPtr);
		bool result = default(bool);
		while (true)
		{
			int num2 = num;
			while (true)
			{
				switch (num2)
				{
				case 5:
					try
					{
						ModuleEntry32 entry = default(ModuleEntry32);
						int num3 = 6;
						int num4 = num3;
						while (true)
						{
							switch (num4)
							{
							default:
								if (!Fiyx2ykG27NHWOv9aWV(entry.szModule, HBbvWMcE0vjQIqqqYSs(-879246903 ^ -879234779), StringComparison.OrdinalIgnoreCase, zGiJK0EFxYWyQDBCddu.OaTEpqoroE))
								{
									num4 = 1;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_79220d9f20444b6c8b0df2558a43de3e == 0)
									{
										num4 = 0;
									}
									continue;
								}
								goto case 3;
							case 9:
								break;
							case 5:
								break;
							case 1:
							case 4:
								if (!Module32NextW(intPtr, ref entry))
								{
									num4 = 7;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1601bb4cfcbc4e588ecdc797e83d5acc != 0)
									{
										num4 = 5;
									}
									continue;
								}
								goto default;
							case 2:
								if (!Module32FirstW(intPtr, ref entry))
								{
									num4 = 8;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_bf7dadb9c4914436a2302bdcb2bac558 != 0)
									{
										num4 = 2;
									}
									continue;
								}
								goto default;
							case 3:
								result = true;
								num4 = 9;
								continue;
							case 6:
								entry.dwSize = (uint)srK3uwk0r0xfuiPw4Iu(sqr80GkBoUKbvZgUk9n(typeof(ModuleEntry32).TypeHandle, THNTxYEVPVbLybtAqi1.PLCEKy6KBP), RNf552EDJrbs9F9iSGt.niNEts2C7C);
								num4 = 2;
								continue;
							case 8:
								result = false;
								num4 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c2d1dda68e3b4e11855617e481626480 == 0)
								{
									num4 = 5;
								}
								continue;
							case 7:
								goto IL_0259;
							}
							break;
						}
					}
					finally
					{
						CloseHandle(intPtr);
						int num5 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c1db6875e3a1467ca73ac64ce11e3ed4 == 0)
						{
							num5 = 0;
						}
						switch (num5)
						{
						case 0:
							break;
						}
					}
					return result;
				case 4:
					return false;
				case 1:
					if (!l0YY3FkKD1vbivauv0X(intPtr, IntPtr.Zero, E6GRu8P7P9F6qI8rcqV.ajPPdAfJO9))
					{
						num2 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_2879e5bb2d3341f08a6f59d9f884899e != 0)
						{
							num2 = 0;
						}
						break;
					}
					goto case 4;
				default:
					if (!E6GRu8P7P9F6qI8rcqV.jnq8ilIcK(intPtr, new IntPtr(-1), E6GRu8P7P9F6qI8rcqV.ajPPdAfJO9))
					{
						num2 = 5;
						break;
					}
					goto end_IL_0012;
				case 3:
					goto IL_0259;
				case 2:
					{
						intPtr = CreateToolhelp32Snapshot(24u, (uint)pid);
						num2 = 1;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_90f58c7ffb21444ca88319aa26ca6aa0 == 0)
						{
							num2 = 0;
						}
						break;
					}
					IL_0259:
					return false;
				}
				continue;
				end_IL_0012:
				break;
			}
			num = 4;
		}
	}

	private static string RuntimePath(int pid, object suffix)
	{
		return (string)gFPhhUcNZJrKvR117L5(nI0MGGk6tFthx4sQmRJ(Environment.SpecialFolder.LocalApplicationData), HBbvWMcE0vjQIqqqYSs(--445132931 ^ 0x1A886BD3), HBbvWMcE0vjQIqqqYSs(0x3B0AFC8F ^ 0x3B0AA7E9), XnUyVEc5Tkm1KK2H1pc(gGrLFhEdofbUymLMOb3.jnq8ilIcK(ref pid, gGrLFhEdofbUymLMOb3.RREELfe2hN), suffix, RxUSOmPDJGsMmCCGPPg.hbkPt9hhua), m4SFBaEZmre5qY3dKfT.KU9bgnuG97);
	}

	private static int ReadSessionHttpPort(int pid)
	{
		int num = 2;
		JObject val = default(JObject);
		int? num3 = default(int?);
		while (true)
		{
			int num2 = num;
			while (true)
			{
				switch (num2)
				{
				case 1:
					if (val == null)
					{
						goto end_IL_0012;
					}
					goto default;
				default:
					num3 = ((JToken)val).Value<int?>(HBbvWMcE0vjQIqqqYSs(0x647ABFB ^ 0x647F6E9));
					num2 = 3;
					break;
				case 2:
					val = (JObject)rOkf8okH16cGSnmkPow(zblxvckCXb3WYi2n2TH(pid, z2jc63fLkugS1X8Q9N.tE1kD1vbB(-608990028 ^ -608970804)));
					num2 = 1;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_9de6b4ecde674f909622ae29ccaad31e == 0)
					{
						num2 = 0;
					}
					break;
				case 3:
					return num3.GetValueOrDefault();
				case 4:
					return 0;
				}
				continue;
				end_IL_0012:
				break;
			}
			num = 4;
		}
	}

	private static int ReadStatusTcpPort(int pid)
	{
		int num = 5;
		int num2 = num;
		int? num3 = default(int?);
		JObject val2 = default(JObject);
		JObject val = default(JObject);
		while (true)
		{
			object obj;
			switch (num2)
			{
			case 3:
				return num3.GetValueOrDefault();
			case 4:
				if (val2 == null)
				{
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_0e5a04c5423148499c9b265cadee0019 != 0)
					{
						num2 = 0;
					}
					continue;
				}
				goto case 2;
			case 6:
				if (val == null)
				{
					return 0;
				}
				num2 = 1;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_2d8e9ecb9a5b407690948cd8111b2980 != 0)
				{
					num2 = 1;
				}
				continue;
			case 5:
				val2 = (JObject)rOkf8okH16cGSnmkPow(RuntimePath(pid, HBbvWMcE0vjQIqqqYSs(-896341688 ^ -896355242)));
				num2 = 4;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_7f8cf60e2e7b454b95cc4d13162f5088 == 0)
				{
					num2 = 4;
				}
				continue;
			case 1:
				num3 = ((JToken)val).Value<int?>(HBbvWMcE0vjQIqqqYSs(0x47C9E567 ^ 0x47C9B875));
				num2 = 3;
				continue;
			case 2:
			{
				JToken obj2 = YNuvA8QCYEcgOkEROsU.jnq8ilIcK(val2, (string)HBbvWMcE0vjQIqqqYSs(0x46C0842F ^ 0x46C0D915), YNuvA8QCYEcgOkEROsU.YOmQH5bNe7);
				obj = ((obj2 is JObject) ? obj2 : null);
				break;
			}
			default:
				obj = null;
				break;
			}
			val = (JObject)obj;
			num2 = 6;
			if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_fd96f1535e384acb8fa11da99670a759 != 0)
			{
				num2 = 1;
			}
		}
	}

	private static JObject ReadRuntimeJson(object path)
	{
		JObject result = default(JObject);
		switch (1)
		{
		case 1:
			try
			{
				int num;
				if (!N1tlkEcoZnwFWqSBqQf(path, lRNPJoPvsXOpQ7Ucdon.i4TPgmP1uC))
				{
					num = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_aaf9213cc3c34825a5d331618f885f65 != 0)
					{
						num = 3;
					}
					goto IL_0053;
				}
				goto IL_00a9;
				IL_0053:
				FileStream fileStream = default(FileStream);
				while (true)
				{
					switch (num)
					{
					case 3:
						result = null;
						num = 1;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_dd83a9ff75d94001819d80b24ef9a897 == 0)
						{
							num = 1;
						}
						continue;
					case 1:
						goto end_IL_0053;
					case 2:
						try
						{
							StreamReader streamReader = new StreamReader(fileStream, (Encoding)ekWklhczmvHysJVmCg6(LCpK8kxFhGIbWVyMacY.PGUxp2INU4));
							int num2 = 0;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_b45f0fd64f6448d2958872ecffbb53cd != 0)
							{
								num2 = 0;
							}
							switch (num2)
							{
							default:
								try
								{
									result = (JObject)PQlnPRc8aiFLYTynHIF(nuJndEkE3v9PTORZQy5(streamReader, gjqRKtPV17ZUEQVAWxv.gcGYDZeqWG), Jd2FevQG6huGknEZHcv.OABQ6b4sTo);
									int num3 = 0;
									if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c09e27dcabbb4f6484836a4e1fd5da8e != 0)
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
									if (streamReader != null)
									{
										int num4 = 0;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_fe6655b3fe1741f7b687606f3c613956 != 0)
										{
											num4 = 0;
										}
										while (true)
										{
											switch (num4)
											{
											default:
												gf0Rq0cgyWhibXFkBEM(streamReader, dsixtSPyYHjKiE5k7WK.VtIPNcTU9E);
												num4 = 0;
												if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_f8d69a0d83e2420b83127060d822cafd == 0)
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
								}
								break;
							}
						}
						finally
						{
							if (fileStream != null)
							{
								int num5 = 1;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_d604b1379fb540e5b6d79e791c2f5762 != 0)
								{
									num5 = 1;
								}
								while (true)
								{
									switch (num5)
									{
									case 1:
										gf0Rq0cgyWhibXFkBEM(fileStream, dsixtSPyYHjKiE5k7WK.VtIPNcTU9E);
										num5 = 0;
										if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_32c4deae05384207a567e3dea0ef9d3f == 0)
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
						goto end_IL_0053;
					}
					goto IL_00a9;
					continue;
					end_IL_0053:
					break;
				}
				goto end_IL_0025;
				IL_00a9:
				fileStream = new FileStream((string)path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				num = 2;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_84f159339c0c4d909a5aafadeb20c94b == 0)
				{
					num = 0;
				}
				goto IL_0053;
				end_IL_0025:;
			}
			catch (Exception ex)
			{
				int num6 = 1;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_043809f5a9d2487fae267fd26a3fe35d != 0)
				{
					num6 = 1;
				}
				while (true)
				{
					switch (num6)
					{
					case 2:
						result = null;
						num6 = 0;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c33375e472634b5ebc4d636397603446 == 0)
						{
							num6 = 0;
						}
						continue;
					case 1:
						lymWLNceeC5sWAA6hPE(gFPhhUcNZJrKvR117L5(z2jc63fLkugS1X8Q9N.tE1kD1vbB(0x4A43C73F ^ 0x4A439A7B), path, HBbvWMcE0vjQIqqqYSs(0x856E40B ^ 0x856BE6B), nuJndEkE3v9PTORZQy5(ex, gjqRKtPV17ZUEQVAWxv.AdKPK0ve7w), m4SFBaEZmre5qY3dKfT.hGFE1HacV8));
						num6 = 2;
						continue;
					case 0:
						break;
					}
					break;
				}
			}
			break;
		}
		return result;
	}

	private static bool IsLoopbackListening(int port)
	{
		int num = 5;
		int num2 = num;
		IntPtr intPtr = default(IntPtr);
		int size = default(int);
		bool result = default(bool);
		int num5 = default(int);
		int num7 = default(int);
		uint num4 = default(uint);
		uint num6 = default(uint);
		int num8 = default(int);
		while (true)
		{
			switch (num2)
			{
			case 3:
				try
				{
					int num3;
					if (GetExtendedTcpTable(intPtr, ref size, order: false, 2, 3, 0u) == 0)
					{
						num3 = 15;
						goto IL_0055;
					}
					goto IL_0139;
					IL_0139:
					result = false;
					num3 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_c09e27dcabbb4f6484836a4e1fd5da8e == 0)
					{
						num3 = 0;
					}
					goto IL_0055;
					IL_0055:
					while (true)
					{
						switch (num3)
						{
						default:
							goto end_IL_0055;
						case 18:
							num5++;
							num3 = 8;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_4067485f69dd42b795335d8574adc3dd != 0)
							{
								num3 = 14;
							}
							continue;
						case 2:
						{
							num7 += 24;
							int num9 = 18;
							num3 = num9;
							continue;
						}
						case 17:
							num4 = (uint)JkCVcakSG0H5IigZTlh(intPtr, num7 + 4, t9Mq6xYBpWWwIO12tZ9.eWSY0VsqAf);
							num3 = 7;
							continue;
						case 11:
							goto end_IL_0055;
						case 16:
							break;
						case 0:
							goto end_IL_0055;
						case 8:
							if (num4 == 16777343)
							{
								num3 = 5;
								continue;
							}
							goto case 2;
						case 13:
							if ((((num6 & 0xFF) << 8) | ((num6 >> 8) & 0xFF)) == (uint)port)
							{
								num3 = 3;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_7dea470b4c15478bbfe1539429f98b80 != 0)
								{
									num3 = 3;
								}
								continue;
							}
							goto case 2;
						case 6:
						case 14:
							if (num5 >= num8)
							{
								num3 = 8;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_83095058e27b497188a2c88050ba8f5f == 0)
								{
									num3 = 9;
								}
								continue;
							}
							goto case 17;
						case 7:
							num6 = (uint)JkCVcakSG0H5IigZTlh(intPtr, num7 + 8, t9Mq6xYBpWWwIO12tZ9.eWSY0VsqAf);
							num3 = 13;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_311d03ca4084414a9ea0c8d2c1bd343d != 0)
							{
								num3 = 1;
							}
							continue;
						case 12:
						case 15:
							num8 = rkQ0h4kaXrWGXfqWTMa(intPtr, hAOoAsYtN4LbcEHxf9a.IuWY5QvDbp);
							num3 = 4;
							continue;
						case 4:
							num7 = 4;
							num3 = 1;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_0a8dc1e2655145c5af97263c14d91cce == 0)
							{
								num3 = 1;
							}
							continue;
						case 5:
						case 10:
							result = true;
							num3 = 11;
							if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_45ac76a6368943c6981dcfd9e66428ae == 0)
							{
								num3 = 5;
							}
							continue;
						case 1:
							num5 = 0;
							num3 = 6;
							continue;
						case 3:
							if (num4 == 0)
							{
								num3 = 10;
								continue;
							}
							goto case 8;
						case 9:
							goto IL_0319;
						}
						goto IL_0139;
						continue;
						end_IL_0055:
						break;
					}
				}
				finally
				{
					vJEaAvkV2N9DXL7sK3i(intPtr, u4dP1aYym5FsbWYG27R.w1VY2IUOek);
					int num10 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_fe6655b3fe1741f7b687606f3c613956 != 0)
					{
						num10 = 0;
					}
					switch (num10)
					{
					case 0:
						break;
					}
				}
				return result;
			case 5:
				size = 0;
				num2 = 1;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1601bb4cfcbc4e588ecdc797e83d5acc == 0)
				{
					num2 = 4;
				}
				break;
			case 2:
				if (size <= 0)
				{
					num2 = 1;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_529db4a9032c417dbe74a080d04d36c2 == 0)
					{
						num2 = 1;
					}
					break;
				}
				intPtr = G4QvsckTkyIXSaZMROr(size, cUVhFMYeWAFhw40H9Cw.s5hYfEFphr);
				num2 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_37f38109cd8f46f5b0100026922644f5 != 0)
				{
					num2 = 3;
				}
				break;
			default:
				goto IL_0319;
			case 1:
				return false;
			case 4:
				{
					GetExtendedTcpTable(IntPtr.Zero, ref size, order: false, 2, 3, 0u);
					num2 = 2;
					break;
				}
				IL_0319:
				return false;
			}
		}
	}

	private static int AllocatePort(int start, bool http)
	{
		int num = 9;
		int num2 = num;
		IEnumerator<BridgeSession> enumerator = default(IEnumerator<BridgeSession>);
		HashSet<int> hashSet = default(HashSet<int>);
		BridgeSession current = default(BridgeSession);
		int num3 = default(int);
		while (true)
		{
			switch (num2)
			{
			case 10:
				try
				{
					while (true)
					{
						IL_00a9:
						int num4;
						if (!nsJ9LebXYwUJAHxJYqI.jnq8ilIcK(enumerator, nsJ9LebXYwUJAHxJYqI.tcqbTnwQjH))
						{
							num4 = 2;
							goto IL_006c;
						}
						goto IL_0086;
						IL_006c:
						while (true)
						{
							switch (num4)
							{
							case 3:
								goto IL_00a9;
							case 1:
								hashSet.Add(http ? current.HttpPort : current.TcpPort);
								num4 = 3;
								continue;
							case 2:
								goto end_IL_00a9;
							}
							break;
						}
						goto IL_0086;
						IL_0086:
						current = enumerator.Current;
						num4 = 1;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_99797e4f3bab41baabcb75f7a30397b7 == 0)
						{
							num4 = 1;
						}
						goto IL_006c;
						continue;
						end_IL_00a9:
						break;
					}
				}
				finally
				{
					if (enumerator != null)
					{
						int num5 = 1;
						if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1bbe7758123e4bc485e91d98227e25e4 != 0)
						{
							num5 = 1;
						}
						while (true)
						{
							switch (num5)
							{
							case 1:
								gf0Rq0cgyWhibXFkBEM(enumerator, dsixtSPyYHjKiE5k7WK.VtIPNcTU9E);
								num5 = 0;
								if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_79220d9f20444b6c8b0df2558a43de3e != 0)
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
				goto case 5;
			case 9:
				hashSet = new HashSet<int>();
				num2 = 8;
				break;
			case 8:
				enumerator = ByClient.Values.GetEnumerator();
				num2 = 10;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_1bbe7758123e4bc485e91d98227e25e4 == 0)
				{
					num2 = 10;
				}
				break;
			case 3:
				return num3;
			case 6:
				num3++;
				num2 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_03aacce0025f45d8a46eb3d2a6e41868 == 0)
				{
					num2 = 0;
				}
				break;
			case 5:
				num3 = start;
				num2 = 1;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_fa47eaa7900849fc99eccdf087262b8e != 0)
				{
					num2 = 1;
				}
				break;
			default:
				if (num3 < start + 20)
				{
					num2 = 2;
					break;
				}
				goto case 7;
			case 7:
				return start;
			case 2:
			case 4:
				if (hashSet.Contains(num3))
				{
					num2 = 0;
					if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_291da815e7d24e5fad1c5f19ff1c2dd7 == 0)
					{
						num2 = 6;
					}
					break;
				}
				goto case 3;
			}
		}
	}

	static WeComBridgeRuntime()
	{
		int num = 4;
		int num2 = num;
		while (true)
		{
			switch (num2)
			{
			case 1:
				InstanceMutexSuffixes = new string[3]
				{
					(string)HBbvWMcE0vjQIqqqYSs(-1857040513 ^ -1857056207),
					(string)HBbvWMcE0vjQIqqqYSs(0x31FC05B1 ^ 0x31FC5821),
					(string)HBbvWMcE0vjQIqqqYSs(0x7AFEF0A3 ^ 0x7AFEAD47)
				};
				num2 = 2;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_9de6b4ecde674f909622ae29ccaad31e != 0)
				{
					num2 = 6;
				}
				break;
			case 4:
				WtWcN0knQdNvxVJPetr();
				num2 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_03aacce0025f45d8a46eb3d2a6e41868 == 0)
				{
					num2 = 3;
				}
				break;
			case 7:
				ByPid = new ConcurrentDictionary<int, BridgeSession>();
				num2 = 2;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_37f38109cd8f46f5b0100026922644f5 == 0)
				{
					num2 = 1;
				}
				break;
			case 2:
				LaunchLock = new object();
				num2 = 1;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_080a2cdddf2144a182f32bab096ad248 == 0)
				{
					num2 = 1;
				}
				break;
			case 5:
				return;
			case 6:
				StatusInfoLengthMismatch = -1073741820;
				num2 = 5;
				break;
			default:
				ByClient = new ConcurrentDictionary<int, BridgeSession>();
				num2 = 7;
				break;
			case 3:
				mQH1aHkc9yWB4gAxuNU();
				num2 = 0;
				if (<Module>{5c5516b6-08de-4b99-b275-4ac3e38a22c5}.m_0a8dc1e2655145c5af97263c14d91cce == 0)
				{
					num2 = 0;
				}
				break;
			}
		}
	}

	internal static bool IakXI6cWqZ0266MFRo7()
	{
		return HuT1VInzTSJ4ICh5NiA == null;
	}

	internal static WeComBridgeRuntime k6ps2QcPvU80wiJPxMq()
	{
		return (WeComBridgeRuntime)HuT1VInzTSJ4ICh5NiA;
	}

	internal static object HBbvWMcE0vjQIqqqYSs(int P_0)
	{
		return z2jc63fLkugS1X8Q9N.tE1kD1vbB(P_0);
	}

	internal static void JCaPQRcbbBeraAkb8OS()
	{
		EnsureWatchTimer();
	}

	internal static void oL8rrXcxFgFNZZs4QDf(object P_0, bool notify)
	{
		CloseSession(P_0, notify);
	}

	internal static bool UYtwpfcyKsZ8PpyNh8D(object P_0, object P_1)
	{
		return nsJ9LebXYwUJAHxJYqI.jnq8ilIcK(P_0, (nsJ9LebXYwUJAHxJYqI)P_1);
	}

	internal static void LQTslmc2PAU8SkX837l(object P_0)
	{
		((ConcurrentDictionary<int, BridgeSession>)P_0).Clear();
	}

	internal static object Otg8HQcUiCoxCZe7p39(bool P_0, object P_1)
	{
		return DL0i8Bh4Ywk5x07Pweq.jnq8ilIcK(P_0, (DL0i8Bh4Ywk5x07Pweq)P_1);
	}

	internal static void dqfVK0cQl6eDUpgITv1(object P_0, object P_1, object P_2, object P_3)
	{
		WSJ5GQQ9myN2N0ma3xj.jnq8ilIcK(P_0, (string)P_1, (JToken)P_2, (WSJ5GQQ9myN2N0ma3xj)P_3);
	}

	internal static bool U7ArJXchIZR71xbpxuE(object P_0)
	{
		return ((BridgeHttpResult)P_0).Ok;
	}

	internal static object CJXtFXcYuiyaN1xAjkp(object P_0, object P_1)
	{
		return WeComBridgeHttp.Describe(P_0, P_1);
	}

	internal static void lymWLNceeC5sWAA6hPE(object P_0)
	{
		Logger.Warning((string)P_0);
	}

	internal static bool NwDJBjcf7WuuFEeb4MV(object P_0, object P_1)
	{
		return UXlitYhvu2h0jmfceHJ.jnq8ilIcK(P_0, (UXlitYhvu2h0jmfceHJ)P_1);
	}

	internal static bool N1tlkEcoZnwFWqSBqQf(object P_0, object P_1)
	{
		return lRNPJoPvsXOpQ7Ucdon.jnq8ilIcK((string)P_0, (lRNPJoPvsXOpQ7Ucdon)P_1);
	}

	internal static int cOLLLAcidlrWei9XWsK(int start, bool http)
	{
		return AllocatePort(start, http);
	}

	internal static object t8DAwfcVwC66MrmQD5d()
	{
		return SnapshotWxWorkPids();
	}

	internal static int plRH4RcK7wdAxuhvKAI()
	{
		return ReleaseWeComInstanceMutexes();
	}

	internal static object smJ0NkcDnTFTfrSLPGO(object P_0, object P_1)
	{
		return dMZjkAEvscZOQ4dkL9w.jnq8ilIcK((string[])P_0, (dMZjkAEvscZOQ4dkL9w)P_1);
	}

	internal static void k0NxU4cte3COFcR674G(object P_0)
	{
		Logger.Info((string)P_0);
	}

	internal static object XnUyVEc5Tkm1KK2H1pc(object P_0, object P_1, object P_2)
	{
		return RxUSOmPDJGsMmCCGPPg.jnq8ilIcK((string)P_0, (string)P_1, (RxUSOmPDJGsMmCCGPPg)P_2);
	}

	internal static object uhAnXmcBtDKqQjGYOsb(object P_0, object P_1, object P_2, object P_3)
	{
		return dDG4K3E4O2Ahv334CHT.jnq8ilIcK((string)P_0, (string)P_1, (string)P_2, (dDG4K3E4O2Ahv334CHT)P_3);
	}

	internal static void kixbTec00AbaPALMjAN(object P_0, object P_1)
	{
		nVdXdwhub0GVhDyNlIF.jnq8ilIcK(P_0, (nVdXdwhub0GVhDyNlIF)P_1);
	}

	internal static object Br5jxLcAl31ILg0XKsb(int pid)
	{
		return WeComVersionCatalog.ReadProcessFileVersion(pid);
	}

	internal static bool ionqQBcXf7e1A4eQlAg(object P_0, object P_1)
	{
		return WeComVersionCatalog.MatchesSelected((string)P_0, (string)P_1);
	}

	internal static int b6QaDdcT87fQt05hwSA(int pid)
	{
		return ReadStatusTcpPort(pid);
	}

	internal static bool a1KdFRcGwaMuuhJ7tvn(int pid)
	{
		return BridgeAlreadyLoaded(pid);
	}

	internal static void YovNMkc6sKgfHjsqhPH(object P_0)
	{
		ReleaseListener(P_0);
	}

	internal static object fwmFxlcCExxicVq5IYy(int port)
	{
		return StartListenerExact(port);
	}

	internal static void fbYNxjcH1DFr9g4cFNN(object P_0)
	{
		AcceptLoop(P_0);
	}

	internal static object dnGbEsca1E35BhtvuvG(object P_0, object P_1, object P_2)
	{
		return ((WeComBridgeHttp)P_0).Post((string)P_1, (JObject)P_2);
	}

	internal static DateTime YxhfgMcSVWyErXdREjP(object P_0)
	{
		return Or2RQ3bh3dyXsRwDb52.jnq8ilIcK((Or2RQ3bh3dyXsRwDb52)P_0);
	}

	internal static object CFrTnvcniRQQXXSdR96(object P_0, object P_1)
	{
		return ((WeComBridgeHttp)P_0).Get((string)P_1);
	}

	internal static object efjQ7ccc17w24KpXKpj(object P_0)
	{
		return ((BridgeHttpResult)P_0).Json;
	}

	internal static bool l2hSytck1xudCQ6WKdC(object P_0, object P_1, object P_2)
	{
		return NgtBADEr5RrpT3l6FkM.jnq8ilIcK((string)P_0, (string)P_1, (NgtBADEr5RrpT3l6FkM)P_2);
	}

	internal static void oTNs1WcrNa24La2ssRx(object P_0)
	{
		EmitAccountLogout(P_0);
	}

	internal static object yTaeC9c7k2aDsaY5WXu(object P_0, object P_1, object P_2)
	{
		return YNuvA8QCYEcgOkEROsU.jnq8ilIcK(P_0, (string)P_1, (YNuvA8QCYEcgOkEROsU)P_2);
	}

	internal static object AJMSu3cdbMH0dIJx2DG(object P_0, object P_1)
	{
		return b8txYwQmx8SvwU718wE.jnq8ilIcK((string)P_0, (b8txYwQmx8SvwU718wE)P_1);
	}

	internal static void CE9J7scLSNqIvXO9WKK(object P_0, object P_1)
	{
		WeComBridgeEvents.CopyMissing(P_0, P_1);
	}

	internal static bool KqwxwacqR9oQoIEkWSs(object P_0, object P_1, object P_2)
	{
		return EnngxCbLyiM9g6bVrXJ.jnq8ilIcK(P_0, (string)P_1, (EnngxCbLyiM9g6bVrXJ)P_2);
	}

	internal static void dD6Uu9cM0cWoIyKJtb6(int clientId, int type, object P_2)
	{
		PublishLoginEvent(clientId, type, P_2);
	}

	internal static void MxoMeicuaYHy1qsmXMv(int clientId, object P_1)
	{
		WeComHelper.PublishBridgeCallback(clientId, (JObject)P_1);
	}

	internal static int XXbh4Wcj22P27vOKVyf(object P_0)
	{
		return ((List<string>)P_0).Count;
	}

	internal static void LEQw5hclW0ZORePYTDQ(object P_0, object P_1)
	{
		Y3rNcgUPMP670sjp06c.jnq8ilIcK(P_0, (Y3rNcgUPMP670sjp06c)P_1);
	}

	internal static object sGWPE2c4NepDsZccdh9(object P_0, object P_1)
	{
		return JjMeHVb3TPv6k2AB0Oq.jnq8ilIcK((string)P_0, (JjMeHVb3TPv6k2AB0Oq)P_1);
	}

	internal static int pF95dJcwP9HICQvK3w2(object P_0)
	{
		return ((BridgeHttpResult)P_0).Status;
	}

	internal static void Bkkwbdcv6rQF0k1Yk9p(object P_0, bool initial)
	{
		RefreshLogin(P_0, initial);
	}

	internal static void gf0Rq0cgyWhibXFkBEM(object P_0, object P_1)
	{
		dsixtSPyYHjKiE5k7WK.jnq8ilIcK(P_0, (dsixtSPyYHjKiE5k7WK)P_1);
	}

	internal static bool dRwJtUc9LYEoyus2XOp(int clientId)
	{
		return WeComHelper.IsDetached(clientId);
	}

	internal static object PQlnPRc8aiFLYTynHIF(object P_0, object P_1)
	{
		return Jd2FevQG6huGknEZHcv.jnq8ilIcK((string)P_0, (Jd2FevQG6huGknEZHcv)P_1);
	}

	internal static bool KciCTkcmfLJ00wco8W6(object P_0)
	{
		return IsHeartbeat(P_0);
	}

	internal static object ADYAmEcI3cfnhIKdmXm(object P_0, object P_1, object P_2)
	{
		return WeComBridgeEvents.ToLegacy(P_0, P_1, P_2);
	}

	internal static void fiPU8qcZVFeLaOV7hrL(int clientId)
	{
		WeComHelper.PublishBridgeOffline(clientId);
	}

	internal static object tgMoKwc1iMYDewg0p7a(object P_0, object P_1)
	{
		return LogoutMessage(P_0, P_1);
	}

	internal static void RR2iGecFYQ9mys4ABu0(object P_0, object P_1, object P_2)
	{
		EmitAccountLogout(P_0, P_1, P_2);
	}

	internal static object KQmPiUcpmKrwNOnAA5r(int P_0, object P_1)
	{
		return E9hPr6QvNKRyIQM6QVf.jnq8ilIcK(P_0, (E9hPr6QvNKRyIQM6QVf)P_1);
	}

	internal static void XYXl5qcJAs4HRg0NXOM(int clientId)
	{
		WeComHelper.PublishBridgeWireOffline(clientId);
	}

	internal static void lZ1quOcOaw0vKJ2VYCl(object P_0, bool P_1, object P_2)
	{
		y8iVbRET6QxQsChlnBy.jnq8ilIcK(P_0, P_1, (y8iVbRET6QxQsChlnBy)P_2);
	}

	internal static object gFPhhUcNZJrKvR117L5(object P_0, object P_1, object P_2, object P_3, object P_4)
	{
		return m4SFBaEZmre5qY3dKfT.jnq8ilIcK((string)P_0, (string)P_1, (string)P_2, (string)P_3, (m4SFBaEZmre5qY3dKfT)P_4);
	}

	internal static bool SActFGcR8wnYja3eVgV(object P_0, object P_1, int count)
	{
		return ReadExact(P_0, P_1, count);
	}

	internal static void Wahs7ac3d83REL0NFkT(object P_0, object P_1)
	{
		OnTcpJson(P_0, P_1);
	}

	internal static int YKylLocsKkA8dJhI21A(object P_0, object P_1, int P_2, int P_3, object P_4)
	{
		return dwdfmjPJFfMGpR80D2F.jnq8ilIcK(P_0, (byte[])P_1, P_2, P_3, (dwdfmjPJFfMGpR80D2F)P_4);
	}

	internal static object ekWklhczmvHysJVmCg6(object P_0)
	{
		return LCpK8kxFhGIbWVyMacY.jnq8ilIcK((LCpK8kxFhGIbWVyMacY)P_0);
	}

	internal static void U4AdlgkWn7A1aFpOI21(object P_0, int P_1, object P_2, int P_3, int P_4, object P_5)
	{
		nfXjtjh3scWXW11JavL.jnq8ilIcK((Array)P_0, P_1, (Array)P_2, P_3, P_4, (nfXjtjh3scWXW11JavL)P_5);
	}

	internal static object vKZ94XkPsKYno1ZA1at(object P_0, object P_1)
	{
		return uxLtJMhzyjrn9e7Dy1C.jnq8ilIcK(P_0, (uxLtJMhzyjrn9e7Dy1C)P_1);
	}

	internal static object nuJndEkE3v9PTORZQy5(object P_0, object P_1)
	{
		return gjqRKtPV17ZUEQVAWxv.jnq8ilIcK(P_0, (gjqRKtPV17ZUEQVAWxv)P_1);
	}

	internal static void BCwKFvkbBi5RA6AixkB(object P_0, object P_1, object P_2)
	{
		VqsM3KbUVrAvZwhr05A.jnq8ilIcK(P_0, (string)P_1, (VqsM3KbUVrAvZwhr05A)P_2);
	}

	internal static void R6XcJNkxLjp9cmZM6cX(object P_0, object P_1, object P_2)
	{
		WmWFPUxRQM3xYTWTkqj.jnq8ilIcK(P_0, (DataReceivedEventHandler)P_1, (WmWFPUxRQM3xYTWTkqj)P_2);
	}

	internal static bool JXKG3akyhPMFt3e69K9(object P_0, int P_1, object P_2)
	{
		return p2FBj0y2KBDqZid0RZA.jnq8ilIcK(P_0, P_1, (p2FBj0y2KBDqZid0RZA)P_2);
	}

	internal static int CJTUy1k2GLLSsExIbVp(object P_0, object P_1)
	{
		return kfHGSgPnOQlgJ5xXCSg.jnq8ilIcK(P_0, (kfHGSgPnOQlgJ5xXCSg)P_1);
	}

	internal static void Qtjn2JkUGZ8ttqL2vU8(int P_0, object P_1)
	{
		iNxACn2fCKfnjj1l3cF.jnq8ilIcK(P_0, (iNxACn2fCKfnjj1l3cF)P_1);
	}

	internal static bool gqHO2VkQtO2WPBl4eN0(DateTime P_0, DateTime P_1, object P_2)
	{
		return hFANSiycpt2XpYYe2h8.jnq8ilIcK(P_0, P_1, (hFANSiycpt2XpYYe2h8)P_2);
	}

	internal static object qvg6uAkh9MNMmfEmnpA(object P_0, object P_1, object P_2)
	{
		return zjw262Yb5R8TAJnhPc1.jnq8ilIcK((string)P_0, (Encoding)P_1, (zjw262Yb5R8TAJnhPc1)P_2);
	}

	internal static object TA3a7bkY7cSmHyDJp2t(object P_0, object P_1)
	{
		return vCD6HcPaOyR6Koj64nc.jnq8ilIcK((string)P_0, (vCD6HcPaOyR6Koj64nc)P_1);
	}

	internal static object W6NiWnkepoKPvbLyOy0(object P_0)
	{
		return ((List<int>)P_0).ToArray();
	}

	internal static void WMMqnDkfwKr8Tvs6Tcc()
	{
		EnableDebugPrivilege();
	}

	internal static int NXy8JNkoTrwV0Zkbw28(IntPtr buffer, int count, IntPtr probe)
	{
		return FindMutexType(buffer, count, probe);
	}

	internal static int FweDZUkie1RK0oEB0V3(IntPtr buffer, int count, int pid, int mutexType)
	{
		return CloseInstanceMutexes(buffer, count, pid, mutexType);
	}

	internal static void vJEaAvkV2N9DXL7sK3i(IntPtr P_0, object P_1)
	{
		u4dP1aYym5FsbWYG27R.jnq8ilIcK(P_0, (u4dP1aYym5FsbWYG27R)P_1);
	}

	internal static bool l0YY3FkKD1vbivauv0X(IntPtr P_0, IntPtr P_1, object P_2)
	{
		return E6GRu8P7P9F6qI8rcqV.jnq8ilIcK(P_0, P_1, (E6GRu8P7P9F6qI8rcqV)P_2);
	}

	internal static object O7NSbRkDxroQSuMCcxd(IntPtr handle)
	{
		return QueryObjectName(handle);
	}

	internal static bool IarTu3ktpriqDE4Y5uK(object P_0)
	{
		return IsInstanceMutex(P_0);
	}

	internal static int zPTqJrk5Q7UReKPJ31x(object P_0)
	{
		return wxuXMNEPBwAMyhQ4pNJ.jnq8ilIcK((wxuXMNEPBwAMyhQ4pNJ)P_0);
	}

	internal static Type sqr80GkBoUKbvZgUk9n(RuntimeTypeHandle P_0, object P_1)
	{
		return THNTxYEVPVbLybtAqi1.jnq8ilIcK(P_0, (THNTxYEVPVbLybtAqi1)P_1);
	}

	internal static int srK3uwk0r0xfuiPw4Iu(Type P_0, object P_1)
	{
		return RNf552EDJrbs9F9iSGt.jnq8ilIcK(P_0, (RNf552EDJrbs9F9iSGt)P_1);
	}

	internal static object rvZouCkA9Oi6tC2r7n5(IntPtr P_0, Type P_1, object P_2)
	{
		return a5dtFWYhISuNlG0dGAm.jnq8ilIcK(P_0, P_1, (a5dtFWYhISuNlG0dGAm)P_2);
	}

	internal static int lWsOq8kX8XQXBEhlZap(int P_0, int P_1, object P_2)
	{
		return yjijbDUp9NiSjcZo47Q.jnq8ilIcK(P_0, P_1, (yjijbDUp9NiSjcZo47Q)P_2);
	}

	internal static IntPtr G4QvsckTkyIXSaZMROr(int P_0, object P_1)
	{
		return cUVhFMYeWAFhw40H9Cw.jnq8ilIcK(P_0, (cUVhFMYeWAFhw40H9Cw)P_1);
	}

	internal static bool Fiyx2ykG27NHWOv9aWV(object P_0, object P_1, StringComparison P_2, object P_3)
	{
		return zGiJK0EFxYWyQDBCddu.jnq8ilIcK((string)P_0, (string)P_1, P_2, (zGiJK0EFxYWyQDBCddu)P_3);
	}

	internal static object nI0MGGk6tFthx4sQmRJ(Environment.SpecialFolder P_0)
	{
		return Environment.GetFolderPath(P_0);
	}

	internal static object zblxvckCXb3WYi2n2TH(int pid, object P_1)
	{
		return RuntimePath(pid, P_1);
	}

	internal static object rOkf8okH16cGSnmkPow(object P_0)
	{
		return ReadRuntimeJson(P_0);
	}

	internal static int rkQ0h4kaXrWGXfqWTMa(IntPtr P_0, object P_1)
	{
		return hAOoAsYtN4LbcEHxf9a.jnq8ilIcK(P_0, (hAOoAsYtN4LbcEHxf9a)P_1);
	}

	internal static int JkCVcakSG0H5IigZTlh(IntPtr P_0, int P_1, object P_2)
	{
		return t9Mq6xYBpWWwIO12tZ9.jnq8ilIcK(P_0, P_1, (t9Mq6xYBpWWwIO12tZ9)P_2);
	}

	internal static void WtWcN0knQdNvxVJPetr()
	{
		z2jc63fLkugS1X8Q9N.TrhALsn43();
	}

	internal static void mQH1aHkc9yWB4gAxuNU()
	{
		z2jc63fLkugS1X8Q9N.rQ19jmJab();
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.2.0.10050' (yours is '9.1.0.7988')
