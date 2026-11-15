using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using cnrhLs8n43tu50PqDX;
using j3AmrhgkCleVTGdEwA;

namespace SG9KiyIbtdgGDf12qr;

internal class z2jc63fLkugS1X8Q9N
{
	private delegate void uKtvKdkBjv4t3TFQBe(object o);

	internal class NifvdpthXyZS3j8XxE : Attribute
	{
		internal class jsT8Vi1nqW2nM6FKKCJ<T>
		{
			private static object Dtj2UIZdjRpTTQ9g1r9;

			public jsT8Vi1nqW2nM6FKKCJ()
			{
				rQ19jmJab();
				base..ctor();
				int num = 0;
				if (1 == 0)
				{
					num = 0;
				}
				switch (num)
				{
				case 0:
					break;
				}
			}

			static jsT8Vi1nqW2nM6FKKCJ()
			{
				TrhALsn43();
			}

			internal static bool gGIxWCZLB3P77t4O1hX()
			{
				return Dtj2UIZdjRpTTQ9g1r9 == null;
			}

			internal static object QLxF9RZqf7tHg9hj2kg()
			{
				return Dtj2UIZdjRpTTQ9g1r9;
			}
		}

		public NifvdpthXyZS3j8XxE(object P_0)
		{
		}
	}

	internal class CIKBY5Ztq9Gg4oE5VU
	{
		internal static string u4iI94Dy8g(object P_0, object P_1)
		{
			byte[] array = (byte[])SdwcOiZ4MnapSoG7wYT(qdC4iPZlkkExNHyFAGJ(), P_0);
			byte[] array2 = new byte[32];
			sY25LnZwXM49BXTJ318(array2, (RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
			byte[] key = array2;
			byte[] array3 = erhwkCleV(SdwcOiZ4MnapSoG7wYT(qdC4iPZlkkExNHyFAGJ(), P_1));
			MemoryStream memoryStream = new MemoryStream();
			SymmetricAlgorithm symmetricAlgorithm = Q3ljUyvXm();
			symmetricAlgorithm.Key = key;
			I2BBnnZvSogPiPFtKGU(symmetricAlgorithm, array3);
			CryptoStream cryptoStream = new CryptoStream(memoryStream, (ICryptoTransform)cTLT4jZgHymYp0730aL(symmetricAlgorithm), CryptoStreamMode.Write);
			EehC4YZ9US7sllHrdA6(cryptoStream, array, 0, array.Length);
			KBjI2sZ8JHlu6BPV23l(cryptoStream);
			return (string)obWmmxZmCvSh3kFk1w1(memoryStream.ToArray());
		}

		internal static object qdC4iPZlkkExNHyFAGJ()
		{
			return Encoding.Unicode;
		}

		internal static object SdwcOiZ4MnapSoG7wYT(object P_0, object P_1)
		{
			return ((Encoding)P_0).GetBytes((string)P_1);
		}

		internal static void sY25LnZwXM49BXTJ318(object P_0, RuntimeFieldHandle P_1)
		{
			RuntimeHelpers.InitializeArray((Array)P_0, P_1);
		}

		internal static void I2BBnnZvSogPiPFtKGU(object P_0, object P_1)
		{
			((SymmetricAlgorithm)P_0).IV = (byte[])P_1;
		}

		internal static object cTLT4jZgHymYp0730aL(object P_0)
		{
			return ((SymmetricAlgorithm)P_0).CreateEncryptor();
		}

		internal static void EehC4YZ9US7sllHrdA6(object P_0, object P_1, int P_2, int P_3)
		{
			((Stream)P_0).Write((byte[])P_1, P_2, P_3);
		}

		internal static void KBjI2sZ8JHlu6BPV23l(object P_0)
		{
			((Stream)P_0).Close();
		}

		internal static object obWmmxZmCvSh3kFk1w1(object P_0)
		{
			return Convert.ToBase64String((byte[])P_0);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate uint vbhv6YuBLBnZE0HBZU(IntPtr classthis, IntPtr comp, IntPtr info, [MarshalAs(UnmanagedType.U4)] uint flags, IntPtr nativeEntry, ref uint nativeSizeOfCode);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate IntPtr XCB0s87BDH0iQKgJ6X();

	internal struct Hmvyw6OJ2VPZUCVIlU
	{
		internal bool MoXISnUino;

		internal byte[] SvYIkqaRc9;
	}

	internal class fjvIDXoHxLTGxlALSK
	{
		private object PTIIvZJmfC;

		public fjvIDXoHxLTGxlALSK(Stream P_0)
		{
			PTIIvZJmfC = new BinaryReader(P_0);
		}

		[SpecialName]
		internal Stream m9OIO8Q0EK()
		{
			return ((BinaryReader)PTIIvZJmfC).BaseStream;
		}

		internal byte[] z47It19xek(int P_0)
		{
			return (byte[])tpVceyZOgbCov416p72(PTIIvZJmfC, P_0);
		}

		internal int NQ1IZyT0jI(byte[] P_0, int P_1, int P_2)
		{
			return midubFZN9hMlCXQI8xT(PTIIvZJmfC, P_0, P_1, P_2);
		}

		internal int wgZIumIPsF()
		{
			return ud8hRqZRggLpg5RRDGT(PTIIvZJmfC);
		}

		internal void V26I7M2UXj()
		{
			Cp8gPTZ3Wf8kTQAPHwD(PTIIvZJmfC);
		}

		internal static object tpVceyZOgbCov416p72(object P_0, int P_1)
		{
			return ((BinaryReader)P_0).ReadBytes(P_1);
		}

		internal static int midubFZN9hMlCXQI8xT(object P_0, object P_1, int P_2, int P_3)
		{
			return ((BinaryReader)P_0).Read((byte[])P_1, P_2, P_3);
		}

		internal static int ud8hRqZRggLpg5RRDGT(object P_0)
		{
			return ((BinaryReader)P_0).ReadInt32();
		}

		internal static void Cp8gPTZ3Wf8kTQAPHwD(object P_0)
		{
			((BinaryReader)P_0).Close();
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	private delegate IntPtr ioC1YcvbZ2yRGgsOBE(IntPtr hModule, string lpName, uint lpType);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate IntPtr qAS8QiRs8nbFaA3e0c(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate int XNV0uBNIf0hRGLlTer(IntPtr hProcess, IntPtr lpBaseAddress, [In][Out] byte[] buffer, uint size, out IntPtr lpNumberOfBytesWritten);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate int QRRKfBrNJhQuGAsIhZ(IntPtr lpAddress, int dwSize, int flNewProtect, ref int lpflOldProtect);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate IntPtr IQBZqwbrNiAH0AoMWy(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate int BJSCyWPgKUiJQGYihL(IntPtr ptr);

	[Flags]
	private enum v6InYtDFWZApx1JyNL
	{

	}

	private static bool uU3JCB0s8;

	internal static object QDHV0iQKg;

	private static object CHf1uRY5iT;

	private static object PrY1OKksdd;

	private static object HqA1otFDcJ;

	private static object J8t1vEcUBI;

	private static object lRG1gLlTer;

	private static int Pgs1HOBEQA;

	private static int RYG1kecuXB;

	private static IntPtr hyO1NyyabH;

	private static object WDJ1r9tJFl;

	private static bool VIl2UgjvI;

	private static object bYc11bZ2yR;

	internal static object iBN1U8RgNG;

	private static object QRR1YKfBNJ;

	private static object U6XE6mvyw;

	internal static object XBV1llcJHV;

	private static bool atZ1SSejVu;

	private static object D081RjBZ7X;

	[NifvdpthXyZS3j8XxE(typeof(NifvdpthXyZS3j8XxE.jsT8Vi1nqW2nM6FKKCJ<object>[]))]
	private static bool firstrundone;

	private static long X4h1A928yN;

	private static bool GJ23VPZUC;

	private static List<string> RaA1I3e0cE;

	private static long CoD1QZV1a1;

	private static bool QxN19ECe8h;

	private static int SDV1d6LAgj;

	private static object Syr1aJSCyW;

	private static bool mhL1hZ6InY;

	private static List<int> BV01muBIf0;

	private static IntPtr ueQ1CBZqwr;

	private static IntPtr lQu18GAsIh;

	private static int fKU1wiJQGY;

	private static IntPtr WB11tRIZxP;

	private static Dictionary<int, int> pAL10SKCoC;

	private static object uFW1iZApx1;

	private static int VyN15LqlEh;

	internal static object EXHzxLTGx;

	private static object BiA1jH0AoM;

	internal static object BSI1ZLPHr8;

	private static object t8Q1fis8nb;

	private static object XJ517srtuJ;

	static z2jc63fLkugS1X8Q9N()
	{
		uU3JCB0s8 = false;
		QDHV0iQKg = Type.GetTypeFromHandle(qsV27iYB524t2usC5L.Sea13WHiH5(33554606)).Assembly;
		U6XE6mvyw = new uint[64]
		{
			3614090360u, 3905402710u, 606105819u, 3250441966u, 4118548399u, 1200080426u, 2821735955u, 4249261313u, 1770035416u, 2336552879u,
			4294925233u, 2304563134u, 1804603682u, 4254626195u, 2792965006u, 1236535329u, 4129170786u, 3225465664u, 643717713u, 3921069994u,
			3593408605u, 38016083u, 3634488961u, 3889429448u, 568446438u, 3275163606u, 4107603335u, 1163531501u, 2850285829u, 4243563512u,
			1735328473u, 2368359562u, 4294588738u, 2272392833u, 1839030562u, 4259657740u, 2763975236u, 1272893353u, 4139469664u, 3200236656u,
			681279174u, 3936430074u, 3572445317u, 76029189u, 3654602809u, 3873151461u, 530742520u, 3299628645u, 4096336452u, 1126891415u,
			2878612391u, 4237533241u, 1700485571u, 2399980690u, 4293915773u, 2240044497u, 1873313359u, 4264355552u, 2734768916u, 1309151649u,
			4149444226u, 3174756917u, 718787259u, 3951481745u
		};
		GJ23VPZUC = false;
		VIl2UgjvI = false;
		EXHzxLTGx = null;
		pAL10SKCoC = null;
		bYc11bZ2yR = new object();
		Pgs1HOBEQA = 0;
		t8Q1fis8nb = new object();
		RaA1I3e0cE = null;
		BV01muBIf0 = null;
		lRG1gLlTer = new byte[0];
		QRR1YKfBNJ = new byte[0];
		lQu18GAsIh = IntPtr.Zero;
		ueQ1CBZqwr = IntPtr.Zero;
		BiA1jH0AoM = new string[0];
		Syr1aJSCyW = new int[0];
		fKU1wiJQGY = 1;
		mhL1hZ6InY = false;
		uFW1iZApx1 = new SortedList();
		VyN15LqlEh = 0;
		X4h1A928yN = 0L;
		XBV1llcJHV = null;
		iBN1U8RgNG = null;
		CoD1QZV1a1 = 0L;
		SDV1d6LAgj = 0;
		QxN19ECe8h = false;
		atZ1SSejVu = false;
		RYG1kecuXB = 0;
		WB11tRIZxP = IntPtr.Zero;
		firstrundone = false;
		BSI1ZLPHr8 = new Hashtable();
		CHf1uRY5iT = null;
		XJ517srtuJ = null;
		PrY1OKksdd = null;
		HqA1otFDcJ = null;
		J8t1vEcUBI = null;
		D081RjBZ7X = null;
		hyO1NyyabH = IntPtr.Zero;
		WDJ1r9tJFl = Encoding.Unicode.GetString(new byte[8] { 134, 123, 241, 8, 24, 98, 77, 199 });
		try
		{
			RSACryptoServiceProvider.UseMachineKeyStore = true;
		}
		catch
		{
		}
	}

	private void bN71RFmnmF()
	{
	}

	internal static byte[] HImHehMQs(object P_0)
	{
		uint[] array = new uint[16];
		uint num = (uint)((448 - ((Array)P_0).Length * 8 % 512 + 512) % 512);
		if (num == 0)
		{
			num = 512u;
		}
		uint num2 = (uint)(((Array)P_0).Length + num / 8 + 8);
		ulong num3 = (ulong)((Array)P_0).Length * 8uL;
		byte[] array2 = new byte[num2];
		for (int i = 0; i < ((Array)P_0).Length; i++)
		{
			array2[i] = ((byte[])P_0)[i];
		}
		array2[((Array)P_0).Length] |= 128;
		for (int num4 = 8; num4 > 0; num4--)
		{
			array2[num2 - num4] = (byte)((num3 >> (8 - num4) * 8) & 0xFF);
		}
		uint num5 = (uint)(array2.Length * 8) / 32u;
		uint num6 = 1732584193u;
		uint num7 = 4023233417u;
		uint num8 = 2562383102u;
		uint num9 = 271733878u;
		for (uint num10 = 0u; num10 < num5 / 16; num10++)
		{
			uint num11 = num10 << 6;
			for (uint num12 = 0u; num12 < 61; num12 += 4)
			{
				array[num12 >> 2] = (uint)((array2[num11 + (num12 + 3)] << 24) | (array2[num11 + (num12 + 2)] << 16) | (array2[num11 + (num12 + 1)] << 8) | array2[num11 + num12]);
			}
			uint num13 = num6;
			uint num14 = num7;
			uint num15 = num8;
			uint num16 = num9;
			OdTftVXgR(ref num6, num7, num8, num9, 0u, 7, 1u, array);
			OdTftVXgR(ref num9, num6, num7, num8, 1u, 12, 2u, array);
			OdTftVXgR(ref num8, num9, num6, num7, 2u, 17, 3u, array);
			OdTftVXgR(ref num7, num8, num9, num6, 3u, 22, 4u, array);
			OdTftVXgR(ref num6, num7, num8, num9, 4u, 7, 5u, array);
			OdTftVXgR(ref num9, num6, num7, num8, 5u, 12, 6u, array);
			OdTftVXgR(ref num8, num9, num6, num7, 6u, 17, 7u, array);
			OdTftVXgR(ref num7, num8, num9, num6, 7u, 22, 8u, array);
			OdTftVXgR(ref num6, num7, num8, num9, 8u, 7, 9u, array);
			OdTftVXgR(ref num9, num6, num7, num8, 9u, 12, 10u, array);
			OdTftVXgR(ref num8, num9, num6, num7, 10u, 17, 11u, array);
			OdTftVXgR(ref num7, num8, num9, num6, 11u, 22, 12u, array);
			OdTftVXgR(ref num6, num7, num8, num9, 12u, 7, 13u, array);
			OdTftVXgR(ref num9, num6, num7, num8, 13u, 12, 14u, array);
			OdTftVXgR(ref num8, num9, num6, num7, 14u, 17, 15u, array);
			OdTftVXgR(ref num7, num8, num9, num6, 15u, 22, 16u, array);
			fBSIsFavs(ref num6, num7, num8, num9, 1u, 5, 17u, array);
			fBSIsFavs(ref num9, num6, num7, num8, 6u, 9, 18u, array);
			fBSIsFavs(ref num8, num9, num6, num7, 11u, 14, 19u, array);
			fBSIsFavs(ref num7, num8, num9, num6, 0u, 20, 20u, array);
			fBSIsFavs(ref num6, num7, num8, num9, 5u, 5, 21u, array);
			fBSIsFavs(ref num9, num6, num7, num8, 10u, 9, 22u, array);
			fBSIsFavs(ref num8, num9, num6, num7, 15u, 14, 23u, array);
			fBSIsFavs(ref num7, num8, num9, num6, 4u, 20, 24u, array);
			fBSIsFavs(ref num6, num7, num8, num9, 9u, 5, 25u, array);
			fBSIsFavs(ref num9, num6, num7, num8, 14u, 9, 26u, array);
			fBSIsFavs(ref num8, num9, num6, num7, 3u, 14, 27u, array);
			fBSIsFavs(ref num7, num8, num9, num6, 8u, 20, 28u, array);
			fBSIsFavs(ref num6, num7, num8, num9, 13u, 5, 29u, array);
			fBSIsFavs(ref num9, num6, num7, num8, 2u, 9, 30u, array);
			fBSIsFavs(ref num8, num9, num6, num7, 7u, 14, 31u, array);
			fBSIsFavs(ref num7, num8, num9, num6, 12u, 20, 32u, array);
			lVvm2jc63(ref num6, num7, num8, num9, 5u, 4, 33u, array);
			lVvm2jc63(ref num9, num6, num7, num8, 8u, 11, 34u, array);
			lVvm2jc63(ref num8, num9, num6, num7, 11u, 16, 35u, array);
			lVvm2jc63(ref num7, num8, num9, num6, 14u, 23, 36u, array);
			lVvm2jc63(ref num6, num7, num8, num9, 1u, 4, 37u, array);
			lVvm2jc63(ref num9, num6, num7, num8, 4u, 11, 38u, array);
			lVvm2jc63(ref num8, num9, num6, num7, 7u, 16, 39u, array);
			lVvm2jc63(ref num7, num8, num9, num6, 10u, 23, 40u, array);
			lVvm2jc63(ref num6, num7, num8, num9, 13u, 4, 41u, array);
			lVvm2jc63(ref num9, num6, num7, num8, 0u, 11, 42u, array);
			lVvm2jc63(ref num8, num9, num6, num7, 3u, 16, 43u, array);
			lVvm2jc63(ref num7, num8, num9, num6, 6u, 23, 44u, array);
			lVvm2jc63(ref num6, num7, num8, num9, 9u, 4, 45u, array);
			lVvm2jc63(ref num9, num6, num7, num8, 12u, 11, 46u, array);
			lVvm2jc63(ref num8, num9, num6, num7, 15u, 16, 47u, array);
			lVvm2jc63(ref num7, num8, num9, num6, 2u, 23, 48u, array);
			QkuggS1X8(ref num6, num7, num8, num9, 0u, 6, 49u, array);
			QkuggS1X8(ref num9, num6, num7, num8, 7u, 10, 50u, array);
			QkuggS1X8(ref num8, num9, num6, num7, 14u, 15, 51u, array);
			QkuggS1X8(ref num7, num8, num9, num6, 5u, 21, 52u, array);
			QkuggS1X8(ref num6, num7, num8, num9, 12u, 6, 53u, array);
			QkuggS1X8(ref num9, num6, num7, num8, 3u, 10, 54u, array);
			QkuggS1X8(ref num8, num9, num6, num7, 10u, 15, 55u, array);
			QkuggS1X8(ref num7, num8, num9, num6, 1u, 21, 56u, array);
			QkuggS1X8(ref num6, num7, num8, num9, 8u, 6, 57u, array);
			QkuggS1X8(ref num9, num6, num7, num8, 15u, 10, 58u, array);
			QkuggS1X8(ref num8, num9, num6, num7, 6u, 15, 59u, array);
			QkuggS1X8(ref num7, num8, num9, num6, 13u, 21, 60u, array);
			QkuggS1X8(ref num6, num7, num8, num9, 4u, 6, 61u, array);
			QkuggS1X8(ref num9, num6, num7, num8, 11u, 10, 62u, array);
			QkuggS1X8(ref num8, num9, num6, num7, 2u, 15, 63u, array);
			QkuggS1X8(ref num7, num8, num9, num6, 9u, 21, 64u, array);
			num6 += num13;
			num7 += num14;
			num8 += num15;
			num9 += num16;
		}
		byte[] array3 = new byte[16];
		Array.Copy(BitConverter.GetBytes(num6), 0, array3, 0, 4);
		Array.Copy(BitConverter.GetBytes(num7), 0, array3, 4, 4);
		Array.Copy(BitConverter.GetBytes(num8), 0, array3, 8, 4);
		Array.Copy(BitConverter.GetBytes(num9), 0, array3, 12, 4);
		return array3;
	}

	private static void OdTftVXgR(ref uint P_0, uint P_1, uint P_2, uint P_3, uint P_4, ushort P_5, uint P_6, object P_7)
	{
		P_0 = P_1 + q9NYFG9Ki(P_0 + ((P_1 & P_2) | (~P_1 & P_3)) + ((uint[])P_7)[P_4] + ((uint[])U6XE6mvyw)[P_6 - 1], P_5);
	}

	private static void fBSIsFavs(ref uint P_0, uint P_1, uint P_2, uint P_3, uint P_4, ushort P_5, uint P_6, object P_7)
	{
		P_0 = P_1 + q9NYFG9Ki(P_0 + ((P_1 & P_3) | (P_2 & ~P_3)) + ((uint[])P_7)[P_4] + ((uint[])U6XE6mvyw)[P_6 - 1], P_5);
	}

	private static void lVvm2jc63(ref uint P_0, uint P_1, uint P_2, uint P_3, uint P_4, ushort P_5, uint P_6, object P_7)
	{
		P_0 = P_1 + q9NYFG9Ki(P_0 + (P_1 ^ P_2 ^ P_3) + ((uint[])P_7)[P_4] + ((uint[])U6XE6mvyw)[P_6 - 1], P_5);
	}

	private static void QkuggS1X8(ref uint P_0, uint P_1, uint P_2, uint P_3, uint P_4, ushort P_5, uint P_6, object P_7)
	{
		P_0 = P_1 + q9NYFG9Ki(P_0 + (P_2 ^ (P_1 | ~P_3)) + ((uint[])P_7)[P_4] + ((uint[])U6XE6mvyw)[P_6 - 1], P_5);
	}

	private static uint q9NYFG9Ki(uint P_0, ushort P_1)
	{
		return (P_0 >> 32 - P_1) | (P_0 << (int)P_1);
	}

	internal static bool Obt8dgGDf()
	{
		if (!GJ23VPZUC)
		{
			f29ajAj3A();
			GJ23VPZUC = true;
		}
		return VIl2UgjvI;
	}

	internal z2jc63fLkugS1X8Q9N()
	{
	}

	private void b2qCrnKW1(byte[] P_0, byte[] P_1, byte[] P_2)
	{
		int num = P_2.Length % 4;
		int num2 = P_2.Length / 4;
		byte[] array = new byte[P_2.Length];
		int num3 = P_0.Length / 4;
		uint num4 = 0u;
		uint num5 = 0u;
		uint num6 = 0u;
		if (num > 0)
		{
			num2++;
		}
		uint num7 = 0u;
		for (int i = 0; i < num2; i++)
		{
			int num8 = i % num3;
			int num9 = i * 4;
			num7 = (uint)(num8 * 4);
			num5 = (uint)((P_0[num7 + 3] << 24) | (P_0[num7 + 2] << 16) | (P_0[num7 + 1] << 8) | P_0[num7]);
			uint num10 = 255u;
			int num11 = 0;
			if (i == num2 - 1 && num > 0)
			{
				num6 = 0u;
				num4 += num5;
				for (int j = 0; j < num; j++)
				{
					if (j > 0)
					{
						num6 <<= 8;
					}
					num6 |= P_2[P_2.Length - (1 + j)];
				}
			}
			else
			{
				num4 += num5;
				num7 = (uint)num9;
				num6 = (uint)((P_2[num7 + 3] << 24) | (P_2[num7 + 2] << 16) | (P_2[num7 + 1] << 8) | P_2[num7]);
			}
			uint num12 = num4;
			num4 = 0u;
			uint num13 = 981178211u;
			uint num14 = num12;
			uint num15 = 1764373615u;
			uint num16 = 863553077u;
			uint num17 = 1654995901u;
			uint num18 = 2139915629u;
			num13 += num17;
			num17 = 111337 * (num17 & 0x7FFF) + (num17 >> 15);
			num13 = 120764 * (num13 & 0x7FFF) + (num13 >> 15);
			num16 = 51441 * num16 + num15;
			uint num19 = ((num16 << 5) | (num16 >> 27)) ^ num13;
			uint num20 = num19 & 0xFF00FF;
			num19 &= 0xFF00FF00u;
			num16 = (num19 >> 8) | (num20 << 8);
			uint num21 = num17 & 0x55555555;
			uint num22 = num17 & 0xAAAAAAAAu;
			num21 = ((num21 >> 1) | (num22 << 1)) ^ num13;
			num17 = (num17 << 15) | (num17 >> 17);
			if ((double)num18 == 0.0)
			{
				num18--;
			}
			uint num23 = (uint)(998503209.0 / (double)num18 + (double)num18);
			if ((short)num13 == 0)
			{
				num13--;
			}
			num18 = (uint)((double)((uint)((double)num13 / (double)(short)num13) ^ num23) + (double)num13);
			num14 ^= num14 >> 2;
			num14 += num16;
			num14 ^= num14 >> 7;
			num14 += num17;
			num14 ^= num14 << 7;
			num14 += num18;
			num14 = (((num13 << 20) - num17) ^ num16) - num14;
			num4 = num12 + (uint)(double)num14;
			if (i == num2 - 1 && num > 0)
			{
				uint num24 = num4 ^ num6;
				for (int k = 0; k < num; k++)
				{
					if (k > 0)
					{
						num10 <<= 8;
						num11 += 8;
					}
					array[num9 + k] = (byte)((num24 & num10) >> num11);
				}
			}
			else
			{
				uint num25 = num4 ^ num6;
				array[num9] = (byte)(num25 & 0xFF);
				array[num9 + 1] = (byte)((num25 & 0xFF00) >> 8);
				array[num9 + 2] = (byte)((num25 & 0xFF0000) >> 16);
				array[num9 + 3] = (byte)((num25 & 0xFF000000u) >> 24);
			}
		}
		lRG1gLlTer = array;
	}

	internal static SymmetricAlgorithm Q3ljUyvXm()
	{
		SymmetricAlgorithm symmetricAlgorithm = null;
		if (Obt8dgGDf())
		{
			return new AesCryptoServiceProvider();
		}
		try
		{
			return new RijndaelManaged();
		}
		catch
		{
			return (SymmetricAlgorithm)Activator.CreateInstance("System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "System.Security.Cryptography.AesCryptoServiceProvider").Unwrap();
		}
	}

	internal static void f29ajAj3A()
	{
		try
		{
			VIl2UgjvI = CryptoConfig.AllowOnlyFipsAlgorithms;
		}
		catch
		{
		}
	}

	internal static byte[] erhwkCleV(object P_0)
	{
		if (!Obt8dgGDf())
		{
			return new MD5CryptoServiceProvider().ComputeHash((byte[])P_0);
		}
		return HImHehMQs(P_0);
	}

	internal static void IGdhEwAds(object P_0, object P_1, uint P_2, object P_3)
	{
		while (P_2 != 0)
		{
			int num = ((P_2 > (uint)((Array)P_3).Length) ? ((Array)P_3).Length : ((int)P_2));
			((Stream)P_1).Read((byte[])P_3, 0, num);
			W27iiB524(P_0, P_3, 0, num);
			P_2 -= (uint)num;
		}
	}

	internal static void W27iiB524(object P_0, object P_1, int P_2, int P_3)
	{
		((HashAlgorithm)P_0).TransformBlock((byte[])P_1, P_2, P_3, (byte[])P_1, P_2);
	}

	internal static uint u2u5sC5LH(uint P_0, int P_1, long P_2, object P_3)
	{
		for (int i = 0; i < P_1; i++)
		{
			((BinaryReader)P_3).BaseStream.Position = P_2 + (i * 40 + 8);
			uint num = ((BinaryReader)P_3).ReadUInt32();
			uint num2 = ((BinaryReader)P_3).ReadUInt32();
			((BinaryReader)P_3).ReadUInt32();
			uint num3 = ((BinaryReader)P_3).ReadUInt32();
			if (num2 <= P_0 && P_0 < num2 + num)
			{
				return num3 + P_0 - num2;
			}
		}
		return 0u;
	}

	internal static void TrhALsn43()
	{
	}

	public static void uu5l0PqDX(RuntimeTypeHandle P_0)
	{
		try
		{
			Type typeFromHandle = Type.GetTypeFromHandle(P_0);
			if (pAL10SKCoC == null)
			{
				lock (bYc11bZ2yR)
				{
					Dictionary<int, int> dictionary = new Dictionary<int, int>();
					BinaryReader binaryReader = new BinaryReader(Type.GetTypeFromHandle(qsV27iYB524t2usC5L.Sea13WHiH5(33554606)).Assembly.GetManifestResourceStream("cSDKENemDwhxmHBtcK.B5B03efYDED1hvX8TY"));
					binaryReader.BaseStream.Position = 0L;
					byte[] array = binaryReader.ReadBytes((int)binaryReader.BaseStream.Length);
					binaryReader.Close();
					if (array.Length != 0)
					{
						int num = array.Length % 4;
						int num2 = array.Length / 4;
						byte[] array2 = new byte[array.Length];
						uint num3 = 0u;
						uint num4 = 0u;
						if (num > 0)
						{
							num2++;
						}
						uint num5 = 0u;
						for (int i = 0; i < num2; i++)
						{
							int num6 = i * 4;
							uint num7 = 255u;
							int num8 = 0;
							if (i == num2 - 1 && num > 0)
							{
								num4 = 0u;
								for (int j = 0; j < num; j++)
								{
									if (j > 0)
									{
										num4 <<= 8;
									}
									num4 |= array[array.Length - (1 + j)];
								}
							}
							else
							{
								num5 = (uint)num6;
								num4 = (uint)((array[num5 + 3] << 24) | (array[num5 + 2] << 16) | (array[num5 + 1] << 8) | array[num5]);
							}
							num3 = num3;
							uint num9 = num3;
							uint num10 = num3;
							uint num11 = 981178211u;
							uint num12 = num10;
							uint num13 = 1764373615u;
							uint num14 = 863553077u;
							uint num15 = 1654995901u;
							uint num16 = 2139915629u;
							num11 += num15;
							num15 = 111337 * (num15 & 0x7FFF) + (num15 >> 15);
							num11 = 120764 * (num11 & 0x7FFF) + (num11 >> 15);
							num14 = 51441 * num14 + num13;
							uint num17 = ((num14 << 5) | (num14 >> 27)) ^ num11;
							uint num18 = num17 & 0xFF00FF;
							num17 &= 0xFF00FF00u;
							num14 = (num17 >> 8) | (num18 << 8);
							uint num19 = num15 & 0x55555555;
							uint num20 = num15 & 0xAAAAAAAAu;
							num19 = ((num19 >> 1) | (num20 << 1)) ^ num11;
							num15 = (num15 << 15) | (num15 >> 17);
							if ((double)num16 == 0.0)
							{
								num16--;
							}
							uint num21 = (uint)(998503209.0 / (double)num16 + (double)num16);
							if ((short)num11 == 0)
							{
								num11--;
							}
							num16 = (uint)((double)((uint)((double)num11 / (double)(short)num11) ^ num21) + (double)num11);
							num12 ^= num12 >> 2;
							num12 += num14;
							num12 ^= num12 >> 7;
							num12 += num15;
							num12 ^= num12 << 7;
							num12 += num16;
							num12 = (((num11 << 20) - num15) ^ num14) - num12;
							num3 = num9 + (uint)(double)num12;
							if (i == num2 - 1 && num > 0)
							{
								uint num22 = num3 ^ num4;
								for (int k = 0; k < num; k++)
								{
									if (k > 0)
									{
										num7 <<= 8;
										num8 += 8;
									}
									array2[num6 + k] = (byte)((num22 & num7) >> num8);
								}
							}
							else
							{
								uint num23 = num3 ^ num4;
								array2[num6] = (byte)(num23 & 0xFF);
								array2[num6 + 1] = (byte)((num23 & 0xFF00) >> 8);
								array2[num6 + 2] = (byte)((num23 & 0xFF0000) >> 16);
								array2[num6 + 3] = (byte)((num23 & 0xFF000000u) >> 24);
							}
						}
						array = array2;
						array2 = null;
						int num24 = array.Length / 8;
						fjvIDXoHxLTGxlALSK fjvIDXoHxLTGxlALSK = new fjvIDXoHxLTGxlALSK(new MemoryStream(array));
						for (int l = 0; l < num24; l++)
						{
							int key = fjvIDXoHxLTGxlALSK.wgZIumIPsF();
							int value = fjvIDXoHxLTGxlALSK.wgZIumIPsF();
							dictionary.Add(key, value);
						}
						fjvIDXoHxLTGxlALSK.V26I7M2UXj();
					}
					pAL10SKCoC = dictionary;
				}
			}
			FieldInfo[] fields = typeFromHandle.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetField);
			for (int m = 0; m < fields.Length; m++)
			{
				try
				{
					FieldInfo fieldInfo = fields[m];
					int metadataToken = fieldInfo.MetadataToken;
					int num25 = pAL10SKCoC[metadataToken];
					bool flag = (num25 & 0x40000000) > 0;
					num25 &= 0x3FFFFFFF;
					MethodInfo methodInfo = (MethodInfo)Type.GetTypeFromHandle(qsV27iYB524t2usC5L.Sea13WHiH5(33554606)).Module.ResolveMethod(num25, typeFromHandle.GetGenericArguments(), new Type[0]);
					if (methodInfo.IsStatic)
					{
						fieldInfo.SetValue(null, Delegate.CreateDelegate(fieldInfo.FieldType, methodInfo));
						continue;
					}
					ParameterInfo[] parameters = methodInfo.GetParameters();
					int num26 = parameters.Length + 1;
					Type[] array3 = new Type[num26];
					if (methodInfo.DeclaringType.IsValueType)
					{
						array3[0] = methodInfo.DeclaringType.MakeByRefType();
					}
					else
					{
						array3[0] = Type.GetTypeFromHandle(qsV27iYB524t2usC5L.Sea13WHiH5(16777240));
					}
					for (int n = 0; n < parameters.Length; n++)
					{
						array3[n + 1] = parameters[n].ParameterType;
					}
					DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, methodInfo.ReturnType, array3, typeFromHandle, skipVisibility: true);
					ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
					for (int num27 = 0; num27 < num26; num27++)
					{
						switch (num27)
						{
						case 0:
							iLGenerator.Emit(OpCodes.Ldarg_0);
							break;
						case 1:
							iLGenerator.Emit(OpCodes.Ldarg_1);
							break;
						case 2:
							iLGenerator.Emit(OpCodes.Ldarg_2);
							break;
						case 3:
							iLGenerator.Emit(OpCodes.Ldarg_3);
							break;
						default:
							iLGenerator.Emit(OpCodes.Ldarg_S, num27);
							break;
						}
					}
					iLGenerator.Emit(OpCodes.Tailcall);
					iLGenerator.Emit(flag ? OpCodes.Callvirt : OpCodes.Call, methodInfo);
					iLGenerator.Emit(OpCodes.Ret);
					fieldInfo.SetValue(null, dynamicMethod.CreateDelegate(typeFromHandle));
				}
				catch
				{
				}
			}
		}
		catch (Exception)
		{
		}
	}

	private static uint sFXQYxgj0(uint P_0)
	{
		return (uint)"{11111-22222-10009-11112}".Length;
	}

	internal static void rQ19jmJab()
	{
		if (Debugger.IsAttached)
		{
			throw new Exception("Debugger Detected");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void dr5SSqLSS(object P_0, int P_1)
	{
		int num = 155;
		byte[] array = default(byte[]);
		int num4 = default(int);
		int num3 = default(int);
		byte[] array2 = default(byte[]);
		byte[] array5 = default(byte[]);
		byte[] array3 = default(byte[]);
		int num5 = default(int);
		byte[] array4 = default(byte[]);
		Stream stream = default(Stream);
		ICryptoTransform transform = default(ICryptoTransform);
		byte[] array6 = default(byte[]);
		while (true)
		{
			int num2 = num;
			while (true)
			{
				switch (num2)
				{
				case 217:
					array[20] = 108;
					num2 = 280;
					continue;
				case 106:
					num4 = 118 + 81;
					num2 = 36;
					continue;
				case 68:
					num3 = 106 + 65;
					num2 = 151;
					continue;
				case 84:
					array[7] = 108;
					num2 = 54;
					continue;
				case 185:
					array2[2] = (byte)num4;
					num2 = 187;
					continue;
				case 337:
					array5[5] = array3[2];
					num2 = 20;
					continue;
				case 299:
					array[2] = 122;
					num2 = 294;
					continue;
				case 275:
					array[6] = 117;
					num2 = 283;
					continue;
				case 66:
					array2[12] = 159;
					num2 = 291;
					if (c5UQYoHrwPy4s3HFqO())
					{
						num2 = 338;
					}
					continue;
				case 220:
					array2[1] = (byte)num4;
					num2 = 249;
					continue;
				case 111:
					array[28] = 24;
					num2 = 137;
					continue;
				case 291:
					array2[7] = 96;
					num2 = 106;
					continue;
				case 219:
					array[2] = (byte)num3;
					num2 = 81;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 8;
					}
					continue;
				case 51:
					num4 = 36 - 16;
					num2 = 158;
					continue;
				case 178:
					num3 = 86 + 42;
					num2 = 219;
					continue;
				case 67:
				case 150:
					num5 = 0;
					num2 = 295;
					continue;
				case 31:
					num4 = 236 - 78;
					num2 = 164;
					continue;
				case 227:
					array[17] = 76;
					num2 = 72;
					continue;
				case 123:
					array2[5] = (byte)num4;
					num2 = 129;
					continue;
				case 161:
					array[4] = (byte)num3;
					num2 = 304;
					continue;
				case 8:
					num3 = 219 - 73;
					num2 = 312;
					continue;
				case 100:
					num3 = 54 + 15;
					num2 = 57;
					continue;
				case 22:
					array4 = (byte[])lRG1gLlTer;
					num2 = 300;
					continue;
				case 182:
					array[27] = 108;
					num2 = 1;
					if (c5UQYoHrwPy4s3HFqO())
					{
						num2 = 27;
					}
					continue;
				case 162:
					array2[6] = (byte)num4;
					num2 = 276;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 173;
					}
					continue;
				case 327:
					num4 = 115 + 42;
					num2 = 117;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 251;
					}
					continue;
				case 343:
					array[21] = (byte)num3;
					num2 = 94;
					continue;
				case 342:
					num4 = 64 + 85;
					num2 = 163;
					continue;
				case 54:
					num3 = 19 + 64;
					num = 21;
					break;
				case 307:
					array[29] = (byte)num3;
					num = 231;
					break;
				case 205:
					array[20] = 108;
					num2 = 110;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 21;
					}
					continue;
				case 171:
					array[23] = (byte)num3;
					num = 254;
					break;
				case 285:
					array[6] = 71;
					num2 = 85;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 3;
					}
					continue;
				case 266:
					array[17] = 114;
					num2 = 250;
					continue;
				case 118:
					num3 = 55 + 52;
					num2 = 35;
					continue;
				case 336:
					array2[5] = (byte)num4;
					num2 = 117;
					continue;
				case 80:
					array2[12] = (byte)num4;
					num2 = 319;
					continue;
				case 117:
					num4 = 229 - 76;
					num2 = 123;
					continue;
				case 138:
					array2[9] = (byte)num4;
					num2 = 51;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 39;
					}
					continue;
				case 168:
					num3 = 128 - 42;
					num2 = 92;
					continue;
				case 222:
					array[11] = (byte)num3;
					num2 = 328;
					continue;
				case 269:
					num3 = 145 - 23;
					num2 = 226;
					continue;
				case 322:
					num3 = 84 + 49;
					num2 = 108;
					continue;
				case 312:
					array[7] = (byte)num3;
					num2 = 118;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 12;
					}
					continue;
				case 104:
					array2[9] = 39;
					num2 = 228;
					continue;
				case 16:
					array[12] = 89;
					num2 = 177;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 122;
					}
					continue;
				case 1:
					num3 = 73 + 34;
					num2 = 72;
					if (c5UQYoHrwPy4s3HFqO())
					{
						num2 = 131;
					}
					continue;
				case 209:
					array2[7] = 231;
					num2 = 98;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 78;
					}
					continue;
				case 218:
					array[20] = 101;
					num2 = 205;
					continue;
				case 228:
					array2[9] = 78;
					num2 = 42;
					continue;
				case 32:
					array2[1] = 80;
					num2 = 296;
					continue;
				case 75:
					array[5] = 157;
					num2 = 201;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 188;
					}
					continue;
				case 145:
				{
					CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Write);
					OTDXFol2E58TQbw1tN(cryptoStream, array4, 0, array4.Length);
					UiiKqs4hQIhbxK1y2E(cryptoStream);
					lRG1gLlTer = tGXtlywuDIF2NsH9F8(stream);
					fvtyTJvLd4ZNqSjenZ(stream);
					fvtyTJvLd4ZNqSjenZ(cryptoStream);
					num2 = 22;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 0;
					}
					continue;
				}
				case 236:
					array[11] = (byte)num3;
					num2 = 189;
					continue;
				case 341:
					num4 = 235 - 78;
					num2 = 80;
					continue;
				case 59:
					array2[6] = 160;
					num = 196;
					break;
				case 71:
					array[21] = (byte)num3;
					num2 = 225;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 36;
					}
					continue;
				case 33:
					array2[4] = 88;
					num2 = 195;
					if (c5UQYoHrwPy4s3HFqO())
					{
						num2 = 327;
					}
					continue;
				case 55:
					array[22] = 102;
					num2 = 136;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 127;
					}
					continue;
				case 202:
					array[5] = (byte)num3;
					num2 = 275;
					continue;
				case 142:
					num3 = 147 - 49;
					num2 = 125;
					continue;
				case 263:
					array[10] = (byte)num3;
					num2 = 176;
					continue;
				case 192:
					new z2jc63fLkugS1X8Q9N().b2qCrnKW1(array6, array5, array4);
					num2 = 282;
					continue;
				case 250:
					array[17] = 137;
					num2 = 60;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 227;
					}
					continue;
				case 315:
					array2[0] = 126;
					num2 = 288;
					continue;
				case 140:
					array2[12] = (byte)num4;
					num2 = 66;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 44;
					}
					continue;
				case 12:
					array6 = array;
					num2 = 303;
					continue;
				case 325:
					array[31] = 140;
					num2 = 12;
					continue;
				case 65:
					num3 = 211 - 70;
					num2 = 308;
					continue;
				case 97:
					array[27] = 20;
					num2 = 19;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 24;
					}
					continue;
				case 146:
					Pgs1HOBEQA = 80;
					num2 = 192;
					continue;
				case 229:
					num3 = 176 - 58;
					num2 = 96;
					continue;
				case 76:
					array[31] = 113;
					num2 = 292;
					continue;
				case 216:
					array5[3] = array3[1];
					num2 = 337;
					continue;
				case 108:
					array[14] = (byte)num3;
					num2 = 103;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 237;
					}
					continue;
				case 23:
					num3 = 70 + 97;
					num2 = 190;
					continue;
				case 155:
				{
					fjvIDXoHxLTGxlALSK fjvIDXoHxLTGxlALSK = new fjvIDXoHxLTGxlALSK((Stream)P_0);
					d4U2henhDNYMZudxS2(fgxItVSmoyf2UdaZW9(fjvIDXoHxLTGxlALSK), 0L);
					array4 = (byte[])oBNoiTkRXSasLXWU43(fjvIDXoHxLTGxlALSK, (int)BB1E7tce89vnIVZBQZ(fgxItVSmoyf2UdaZW9(fjvIDXoHxLTGxlALSK)));
					T1dOflrluaTmBibP4T(fjvIDXoHxLTGxlALSK);
					num2 = 154;
					continue;
				}
				case 183:
					num3 = 108 + 53;
					num2 = 103;
					continue;
				case 78:
					array2[7] = 125;
					num2 = 38;
					continue;
				case 286:
					array2[13] = 31;
					num2 = 238;
					continue;
				case 215:
					array2[2] = (byte)num4;
					num2 = 6;
					continue;
				case 292:
					num3 = 157 - 52;
					num2 = 0;
					if (c5UQYoHrwPy4s3HFqO())
					{
						num2 = 0;
					}
					continue;
				case 323:
					array2[3] = 46;
					num2 = 15;
					if (c5UQYoHrwPy4s3HFqO())
					{
						num2 = 33;
					}
					continue;
				case 311:
					array[18] = 178;
					num2 = 114;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 203;
					}
					continue;
				case 339:
					array2[0] = (byte)num4;
					num2 = 204;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 134;
					}
					continue;
				case 58:
					num3 = 22 + 54;
					num = 50;
					break;
				case 131:
					array[2] = (byte)num3;
					num2 = 17;
					if (c5UQYoHrwPy4s3HFqO())
					{
						num2 = 73;
					}
					continue;
				case 156:
					array[16] = 181;
					num2 = 229;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 110;
					}
					continue;
				case 338:
					num4 = 161 - 63;
					num2 = 56;
					continue;
				case 253:
					array[10] = 58;
					num2 = 65;
					continue;
				case 320:
					array[5] = 96;
					num2 = 37;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 75;
					}
					continue;
				case 203:
					array[18] = 99;
					num2 = 245;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 168;
					}
					continue;
				case 231:
					array[29] = 147;
					num2 = 143;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 113;
					}
					continue;
				case 92:
					array[0] = (byte)num3;
					num2 = 132;
					continue;
				case 331:
					array2[7] = 119;
					num2 = 291;
					continue;
				case 26:
					array[15] = 108;
					num2 = 93;
					if (c5UQYoHrwPy4s3HFqO())
					{
						num2 = 239;
					}
					continue;
				case 283:
					array[6] = 142;
					num2 = 285;
					continue;
				case 238:
					num4 = 160 + 43;
					num2 = 79;
					continue;
				case 265:
					array2[5] = 187;
					num2 = 53;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 49;
					}
					continue;
				case 157:
					array2[1] = 158;
					num2 = 32;
					continue;
				case 81:
					num3 = 119 + 53;
					num2 = 221;
					continue;
				case 326:
					num3 = 221 - 102;
					num = 235;
					break;
				case 13:
					num3 = 142 - 47;
					num2 = 199;
					continue;
				case 4:
					array[25] = 19;
					num2 = 8;
					if (c5UQYoHrwPy4s3HFqO())
					{
						num2 = 120;
					}
					continue;
				case 207:
					array2[11] = 100;
					num2 = 121;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 184;
					}
					continue;
				case 213:
					array[30] = 122;
					num2 = 41;
					continue;
				case 29:
					num4 = 98 + 7;
					num2 = 148;
					continue;
				case 273:
					array[13] = 134;
					num2 = 86;
					continue;
				case 20:
					array5[7] = array3[3];
					num2 = 309;
					continue;
				case 15:
					num3 = 166 - 93;
					num2 = 333;
					continue;
				case 258:
					array[11] = 238;
					num2 = 172;
					continue;
				case 260:
					array[12] = (byte)num3;
					num2 = 302;
					continue;
				case 122:
					array[29] = (byte)num3;
					num2 = 188;
					continue;
				case 184:
					num4 = 29 + 99;
					num2 = 160;
					continue;
				case 308:
					array[11] = (byte)num3;
					num2 = 63;
					continue;
				case 105:
					array2[1] = (byte)num4;
					num2 = 242;
					continue;
				case 149:
					array2[15] = (byte)num4;
					num2 = 46;
					continue;
				case 98:
					array2[8] = 180;
					num2 = 97;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 223;
					}
					continue;
				case 187:
					num4 = 46 + 117;
					num2 = 215;
					continue;
				case 148:
					array2[3] = (byte)num4;
					num2 = 323;
					continue;
				case 264:
					FIpLtC7yY1sNprMD81(array5);
					num2 = 185;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 191;
					}
					continue;
				case 289:
					array[15] = (byte)num3;
					num2 = 95;
					continue;
				case 128:
					array[22] = (byte)num3;
					num = 90;
					break;
				case 297:
					array[15] = (byte)num3;
					num2 = 26;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 1;
					}
					continue;
				case 329:
					array2[10] = (byte)num4;
					num2 = 139;
					continue;
				case 139:
					array2[10] = 105;
					num2 = 60;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 40;
					}
					continue;
				case 169:
					array[28] = 141;
					num2 = 111;
					continue;
				case 276:
					array2[6] = 117;
					num = 59;
					break;
				case 5:
					array2[11] = 227;
					num2 = 111;
					if (c5UQYoHrwPy4s3HFqO())
					{
						num2 = 341;
					}
					continue;
				case 189:
					array[11] = 133;
					num2 = 83;
					continue;
				case 48:
					array2[15] = 92;
					num2 = 186;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 206;
					}
					continue;
				case 230:
					array[14] = 94;
					num2 = 39;
					continue;
				case 72:
					array[17] = 167;
					num2 = 7;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 147;
					}
					continue;
				case 191:
					array3 = (byte[])EhXxM2LIcXbUlHJ1DI(aL3tDUdTHHmu4KmSvX(QDHV0iQKg));
					num = 82;
					break;
				case 103:
					array[21] = (byte)num3;
					num2 = 77;
					continue;
				case 91:
					array2[8] = 84;
					num2 = 306;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 135;
					}
					continue;
				case 21:
					array[7] = (byte)num3;
					num2 = 10;
					continue;
				case 46:
					array5 = array2;
					num2 = 264;
					continue;
				case 114:
					array[23] = 122;
					num2 = 193;
					continue;
				case 3:
					array2[2] = 83;
					num2 = 89;
					continue;
				case 188:
					array[29] = 207;
					num2 = 93;
					continue;
				case 24:
					array[28] = 154;
					num2 = 233;
					continue;
				case 281:
					array2[13] = 141;
					num2 = 170;
					continue;
				case 163:
					array2[10] = (byte)num4;
					num = 107;
					break;
				case 243:
					array[13] = (byte)num3;
					num2 = 235;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 269;
					}
					continue;
				case 249:
					array2[2] = 119;
					num2 = 3;
					continue;
				case 44:
					array[9] = 124;
					num = 326;
					break;
				case 173:
					array[7] = (byte)num3;
					num2 = 8;
					continue;
				case 208:
					array[19] = 93;
					num2 = 310;
					continue;
				case 175:
					array2[10] = (byte)num4;
					num2 = 0;
					if (c5UQYoHrwPy4s3HFqO())
					{
						num2 = 167;
					}
					continue;
				case 280:
					array[20] = 153;
					num2 = 174;
					continue;
				case 121:
					array2[2] = (byte)num4;
					num2 = 45;
					continue;
				case 158:
					array2[9] = (byte)num4;
					num2 = 342;
					continue;
				case 35:
					array[7] = (byte)num3;
					num2 = 25;
					continue;
				case 211:
					array2[14] = 108;
					num2 = 340;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 215;
					}
					continue;
				case 89:
					array2[2] = 116;
					num2 = 287;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 127;
					}
					continue;
				case 241:
					num4 = 26 + 30;
					num2 = 15;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 105;
					}
					continue;
				case 37:
					array[26] = (byte)num3;
					num2 = 200;
					continue;
				case 7:
					num3 = 119 + 99;
					num2 = 305;
					continue;
				case 60:
					num4 = 130 - 43;
					num2 = 270;
					continue;
				case 333:
					array[24] = (byte)num3;
					num2 = 180;
					continue;
				case 19:
					num4 = 0 + 14;
					num2 = 336;
					continue;
				case 309:
					array5[9] = array3[4];
					num2 = 153;
					continue;
				case 279:
					array[1] = 98;
					num2 = 210;
					continue;
				case 330:
					array2[13] = 139;
					num2 = 267;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 46;
					}
					continue;
				case 34:
					array[10] = (byte)num3;
					num2 = 232;
					continue;
				case 177:
					num3 = 240 - 80;
					num2 = 260;
					continue;
				case 201:
					array[5] = 110;
					num2 = 269;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 332;
					}
					continue;
				case 52:
					num3 = 54 + 54;
					num2 = 297;
					continue;
				case 119:
					array[13] = 130;
					num2 = 273;
					continue;
				case 49:
					array[0] = 188;
					num2 = 43;
					continue;
				case 198:
					array[10] = (byte)num3;
					num2 = 250;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 253;
					}
					continue;
				case 244:
					array5[1] = array3[0];
					num2 = 216;
					continue;
				case 328:
					num3 = 244 - 81;
					num2 = 236;
					continue;
				case 293:
					num3 = 222 - 74;
					num2 = 11;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 161;
					}
					continue;
				case 6:
					num4 = 148 - 49;
					num2 = 61;
					continue;
				case 272:
					num3 = 32 + 119;
					num2 = 298;
					continue;
				case 25:
					array[7] = 36;
					num2 = 54;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 124;
					}
					continue;
				case 172:
					num3 = 254 - 84;
					num2 = 28;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 40;
					}
					continue;
				case 186:
					if (P_1 == -1)
					{
						num2 = 318;
						continue;
					}
					goto case 300;
				case 319:
					num4 = 116 + 74;
					num2 = 140;
					continue;
				case 257:
					array[20] = 71;
					num2 = 218;
					continue;
				case 86:
					num3 = 106 + 97;
					num2 = 243;
					continue;
				default:
					array[31] = (byte)num3;
					num2 = 20;
					if (c5UQYoHrwPy4s3HFqO())
					{
						num2 = 28;
					}
					continue;
				case 255:
					array[18] = 154;
					num2 = 141;
					continue;
				case 247:
					array[25] = (byte)num3;
					num2 = 4;
					continue;
				case 246:
					array2[6] = 216;
					num2 = 78;
					continue;
				case 38:
					array2[7] = 98;
					num2 = 331;
					continue;
				case 277:
					stream = (Stream)zZLAOCjiCoRIvFbxtl();
					num2 = 145;
					continue;
				case 313:
					array[19] = 8;
					num2 = 257;
					continue;
				case 290:
					array[13] = 121;
					num2 = 119;
					continue;
				case 176:
					num3 = 57 + 106;
					num2 = 198;
					continue;
				case 305:
					array[23] = (byte)num3;
					num2 = 99;
					continue;
				case 133:
					array2[1] = 160;
					num2 = 157;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 82;
					}
					continue;
				case 96:
					array[16] = (byte)num3;
					num2 = 259;
					continue;
				case 50:
					array[1] = (byte)num3;
					num2 = 279;
					continue;
				case 254:
					array[23] = 120;
					num2 = 102;
					if (c5UQYoHrwPy4s3HFqO())
					{
						num2 = 159;
					}
					continue;
				case 316:
					if (array3.Length != 0)
					{
						num2 = 47;
						if (NQR33GaYr4l24BZ5uG() == null)
						{
							num2 = 244;
						}
						continue;
					}
					goto case 67;
				case 179:
					array2[14] = (byte)num4;
					num2 = 252;
					continue;
				case 304:
					array[4] = 112;
					num2 = 272;
					continue;
				case 136:
					num3 = 212 - 70;
					num2 = 50;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 128;
					}
					continue;
				case 141:
					num3 = 122 + 53;
					num2 = 9;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 6;
					}
					continue;
				case 221:
					array[2] = (byte)num3;
					num2 = 0;
					if (c5UQYoHrwPy4s3HFqO())
					{
						num2 = 1;
					}
					continue;
				case 340:
					array2[14] = 205;
					num = 30;
					break;
				case 70:
					array2[13] = (byte)num4;
					num2 = 286;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 32;
					}
					continue;
				case 9:
					array[18] = (byte)num3;
					num = 311;
					break;
				case 143:
					num3 = 24 + 109;
					num2 = 122;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 17;
					}
					continue;
				case 303:
					array2 = new byte[16];
					num2 = 315;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 43;
					}
					continue;
				case 11:
					num3 = 122 + 114;
					num2 = 171;
					continue;
				case 259:
					array[16] = 130;
					num2 = 194;
					continue;
				case 17:
					array[30] = (byte)num3;
					num2 = 69;
					continue;
				case 210:
					array[1] = 175;
					num2 = 178;
					continue;
				case 28:
					array[31] = 112;
					num2 = 325;
					continue;
				case 239:
					num3 = 232 - 77;
					num2 = 101;
					continue;
				case 302:
					array[12] = 98;
					num2 = 68;
					continue;
				case 334:
					array2[14] = 175;
					num2 = 74;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 51;
					}
					continue;
				case 113:
					array[21] = (byte)num3;
					num2 = 55;
					continue;
				case 294:
					array[3] = 115;
					num2 = 262;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 261;
					}
					continue;
				case 242:
					num4 = 157 - 52;
					num2 = 224;
					continue;
				case 267:
					num4 = 120 + 64;
					num2 = 70;
					continue;
				case 314:
					array[16] = (byte)num3;
					num = 266;
					break;
				case 235:
					array[9] = (byte)num3;
					num2 = 170;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 181;
					}
					continue;
				case 27:
					array[27] = 113;
					num2 = 62;
					continue;
				case 47:
					array[22] = (byte)num3;
					num2 = 114;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 32;
					}
					continue;
				case 53:
					num4 = 207 - 69;
					num2 = 154;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 162;
					}
					continue;
				case 79:
					array2[13] = (byte)num4;
					num2 = 334;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 306;
					}
					continue;
				case 271:
					array[1] = 57;
					num2 = 58;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 50;
					}
					continue;
				case 45:
					num4 = 186 - 62;
					num2 = 185;
					continue;
				case 154:
					array = new byte[32];
					num2 = 168;
					continue;
				case 204:
					array2[0] = 4;
					num2 = 118;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 241;
					}
					continue;
				case 56:
					array2[12] = (byte)num4;
					num2 = 281;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 261;
					}
					continue;
				case 240:
					array2[15] = 84;
					num2 = 48;
					continue;
				case 284:
					num5++;
					num = 87;
					break;
				case 318:
				{
					object obj = lbnlflqLmSXPZKcQTX();
					VYbUXLMxKNPiwoIkhq(obj, CipherMode.CBC);
					transform = (ICryptoTransform)vhUXeKugHAdyDYuHqb(obj, array6, array5);
					num2 = 277;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 114;
					}
					continue;
				}
				case 245:
					array[18] = 104;
					num2 = 142;
					continue;
				case 2:
					array[21] = (byte)num3;
					num2 = 77;
					if (c5UQYoHrwPy4s3HFqO())
					{
						num2 = 126;
					}
					continue;
				case 270:
					array2[11] = (byte)num4;
					num2 = 27;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 31;
					}
					continue;
				case 124:
					array[8] = 61;
					num2 = 112;
					continue;
				case 233:
					array[28] = 140;
					num2 = 169;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 138;
					}
					continue;
				case 193:
					array[23] = 146;
					num2 = 7;
					continue;
				case 107:
					num4 = 1 + 2;
					num2 = 134;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 35;
					}
					continue;
				case 268:
					array5[13] = array3[6];
					num2 = 214;
					continue;
				case 212:
					num3 = 133 + 88;
					num2 = 19;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 37;
					}
					continue;
				case 298:
					array[4] = (byte)num3;
					num2 = 234;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 112;
					}
					continue;
				case 237:
					num3 = 217 - 72;
					num2 = 324;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 158;
					}
					continue;
				case 165:
					num3 = 155 - 51;
					num2 = 289;
					continue;
				case 85:
					array[6] = 202;
					num2 = 18;
					continue;
				case 196:
					array2[6] = 90;
					num2 = 248;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 86;
					}
					continue;
				case 93:
					num3 = 248 - 82;
					num2 = 13;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 17;
					}
					continue;
				case 77:
					num3 = 159 - 53;
					num2 = 343;
					continue;
				case 301:
					num4 = 204 - 68;
					num2 = 64;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 54;
					}
					continue;
				case 224:
					array2[1] = (byte)num4;
					num2 = 133;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 69;
					}
					continue;
				case 43:
					array[1] = 109;
					num2 = 271;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 91;
					}
					continue;
				case 234:
					array[4] = 56;
					num2 = 115;
					continue;
				case 36:
					array2[7] = (byte)num4;
					num2 = 209;
					continue;
				case 40:
					array[12] = (byte)num3;
					num2 = 127;
					continue;
				case 135:
					num3 = 117 + 11;
					num2 = 152;
					continue;
				case 194:
					num3 = 74 - 35;
					num2 = 314;
					continue;
				case 159:
					array[24] = 174;
					num2 = 0;
					if (c5UQYoHrwPy4s3HFqO())
					{
						num2 = 109;
					}
					continue;
				case 126:
					num3 = 193 - 64;
					num2 = 71;
					continue;
				case 164:
					array2[11] = (byte)num4;
					num2 = 207;
					continue;
				case 248:
					array2[6] = 124;
					num2 = 246;
					continue;
				case 232:
					num3 = 18 + 30;
					num2 = 263;
					continue;
				case 110:
					array[20] = 162;
					num2 = 217;
					continue;
				case 321:
					array[26] = 143;
					num2 = 177;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 212;
					}
					continue;
				case 206:
					num4 = 99 + 88;
					num2 = 149;
					continue;
				case 160:
					array2[11] = (byte)num4;
					num2 = 301;
					continue;
				case 332:
					num3 = 151 + 9;
					num2 = 202;
					continue;
				case 324:
					array[15] = (byte)num3;
					num2 = 52;
					continue;
				case 87:
				case 295:
					if (num5 >= array5.Length)
					{
						num2 = 186;
						continue;
					}
					goto case 102;
				case 166:
					array[9] = 138;
					num2 = 44;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 8;
					}
					continue;
				case 62:
					array[27] = 122;
					num2 = 97;
					continue;
				case 223:
					array2[8] = 157;
					num2 = 91;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 68;
					}
					continue;
				case 132:
					array[0] = 144;
					num2 = 49;
					continue;
				case 214:
					array5[15] = array3[7];
					num = 150;
					break;
				case 115:
					array[5] = 106;
					num2 = 335;
					continue;
				case 74:
					num4 = 240 - 80;
					num2 = 179;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 120;
					}
					continue;
				case 30:
					array2[15] = 107;
					num2 = 240;
					continue;
				case 10:
					num3 = 77 + 77;
					num2 = 150;
					if (c5UQYoHrwPy4s3HFqO())
					{
						num2 = 173;
					}
					continue;
				case 153:
					array5[11] = array3[5];
					num2 = 268;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 23;
					}
					continue;
				case 39:
					num3 = 222 - 74;
					num2 = 105;
					if (c5UQYoHrwPy4s3HFqO())
					{
						num2 = 116;
					}
					continue;
				case 151:
					array[13] = (byte)num3;
					num2 = 290;
					continue;
				case 261:
					num4 = 3 + 19;
					num2 = 175;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 45;
					}
					continue;
				case 282:
					return;
				case 125:
					array[19] = (byte)num3;
					num2 = 208;
					continue;
				case 69:
					array[30] = 90;
					num2 = 213;
					continue;
				case 310:
					num3 = 19 + 6;
					num = 274;
					break;
				case 129:
					array2[5] = 140;
					num2 = 40;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 265;
					}
					continue;
				case 99:
					array[23] = 98;
					num2 = 11;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 3;
					}
					continue;
				case 134:
					array2[10] = (byte)num4;
					num2 = 261;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 203;
					}
					continue;
				case 251:
					array2[4] = (byte)num4;
					num2 = 14;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 1;
					}
					continue;
				case 57:
					array[26] = (byte)num3;
					num2 = 321;
					continue;
				case 42:
					num4 = 113 + 88;
					num2 = 138;
					continue;
				case 296:
					num4 = 146 + 53;
					num2 = 185;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 220;
					}
					continue;
				case 41:
					array[30] = 190;
					num2 = 63;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 76;
					}
					continue;
				case 181:
					num3 = 78 + 65;
					num2 = 34;
					continue;
				case 14:
					array2[4] = 19;
					num2 = 12;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 19;
					}
					continue;
				case 101:
					array[15] = (byte)num3;
					num2 = 66;
					if (c5UQYoHrwPy4s3HFqO())
					{
						num2 = 165;
					}
					continue;
				case 278:
					num3 = 196 + 8;
					num2 = 256;
					continue;
				case 199:
					array[26] = (byte)num3;
					num2 = 100;
					continue;
				case 197:
					array[2] = 162;
					num2 = 299;
					continue;
				case 90:
					num3 = 117 + 115;
					num2 = 47;
					continue;
				case 152:
					array[9] = (byte)num3;
					num2 = 166;
					continue;
				case 61:
					array2[3] = (byte)num4;
					num2 = 29;
					continue;
				case 306:
					array2[8] = 56;
					num2 = 104;
					continue;
				case 274:
					array[19] = (byte)num3;
					num = 313;
					break;
				case 144:
					num3 = 111 + 20;
					num2 = 247;
					continue;
				case 94:
					num3 = 102 + 41;
					num2 = 113;
					continue;
				case 102:
					array6[num5] ^= array5[num5];
					num2 = 284;
					continue;
				case 127:
					array[12] = 166;
					num2 = 16;
					continue;
				case 73:
					array[2] = 121;
					num2 = 197;
					continue;
				case 317:
					array[3] = 126;
					num2 = 293;
					continue;
				case 120:
					array[26] = 111;
					num2 = 13;
					continue;
				case 226:
					array[13] = (byte)num3;
					num2 = 230;
					continue;
				case 190:
					array[9] = (byte)num3;
					num2 = 135;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 30;
					}
					continue;
				case 262:
					array[3] = 132;
					num2 = 317;
					continue;
				case 64:
					array2[11] = (byte)num4;
					num2 = 5;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 5;
					}
					continue;
				case 167:
					num4 = 131 - 43;
					num2 = 329;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 89;
					}
					continue;
				case 252:
					array2[14] = 125;
					num = 211;
					break;
				case 200:
					array[27] = 172;
					num2 = 182;
					continue;
				case 288:
					num4 = 236 - 78;
					num2 = 339;
					continue;
				case 300:
					if (FAOp5P9EMskJNR6hK3(hFCFP5g3c0I3cSdUlo(QDHV0iQKg), null))
					{
						num2 = 141;
						if (c5UQYoHrwPy4s3HFqO())
						{
							num2 = 146;
						}
						continue;
					}
					goto case 192;
				case 82:
					if (array3 == null)
					{
						num = 67;
						break;
					}
					goto case 316;
				case 335:
					array[5] = 133;
					num2 = 320;
					continue;
				case 18:
					num3 = 152 + 9;
					num = 195;
					break;
				case 180:
					array[25] = 107;
					num2 = 144;
					if (NQR33GaYr4l24BZ5uG() != null)
					{
						num2 = 17;
					}
					continue;
				case 109:
					array[24] = 131;
					num2 = 15;
					continue;
				case 256:
					array[8] = (byte)num3;
					num2 = 130;
					continue;
				case 88:
					array[11] = (byte)num3;
					num = 258;
					break;
				case 170:
					array2[13] = 125;
					num = 330;
					break;
				case 195:
					array[6] = (byte)num3;
					num2 = 84;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 78;
					}
					continue;
				case 83:
					num3 = 223 - 74;
					num2 = 88;
					if (!c5UQYoHrwPy4s3HFqO())
					{
						num2 = 6;
					}
					continue;
				case 130:
					array[9] = 59;
					num2 = 23;
					continue;
				case 287:
					num4 = 253 - 84;
					num2 = 121;
					continue;
				case 116:
					array[14] = (byte)num3;
					num2 = 239;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 322;
					}
					continue;
				case 112:
					array[8] = 79;
					num2 = 278;
					continue;
				case 95:
					array[15] = 18;
					num2 = 156;
					continue;
				case 147:
					array[17] = 118;
					num2 = 255;
					continue;
				case 137:
					num3 = 53 + 101;
					num2 = 307;
					continue;
				case 225:
					array[21] = 162;
					num2 = 183;
					continue;
				case 174:
					num3 = 182 - 60;
					num2 = 2;
					continue;
				case 63:
					num3 = 32 + 9;
					num2 = 56;
					if (NQR33GaYr4l24BZ5uG() == null)
					{
						num2 = 222;
					}
					continue;
				}
				break;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal static string tE1kD1vbB(int P_0)
	{
		if (((Array)lRG1gLlTer).Length == 0)
		{
			RaA1I3e0cE = new List<string>();
			BV01muBIf0 = new List<int>();
			dr5SSqLSS(((Assembly)QDHV0iQKg).GetManifestResourceStream("Vddcg0PBc9s1Y40Ht5.F0ANIOEy9fAwW7eIKk"), P_0);
		}
		if (Pgs1HOBEQA < 75)
		{
			if ((Assembly)QDHV0iQKg != new StackFrame(1).GetMethod().DeclaringType.Assembly)
			{
				throw new Exception();
			}
			Pgs1HOBEQA++;
		}
		lock (t8Q1fis8nb)
		{
			int num = BitConverter.ToInt32((byte[])lRG1gLlTer, P_0);
			if (num < BV01muBIf0.Count && BV01muBIf0[num] == P_0)
			{
				return RaA1I3e0cE[num];
			}
			try
			{
				TKW1L3mlUyvXmg29jA.SLV0fFIsptsZtjvFft17();
				byte[] array = new byte[num];
				Array.Copy((Array)lRG1gLlTer, P_0 + 4, array, 0, num);
				string text = Encoding.Unicode.GetString(array, 0, array.Length);
				RaA1I3e0cE.Add(text);
				BV01muBIf0.Add(P_0);
				Array.Copy(BitConverter.GetBytes(RaA1I3e0cE.Count - 1), 0, (Array)lRG1gLlTer, P_0, 4);
				return text;
			}
			catch
			{
			}
		}
		return "";
	}

	internal static string kHVtMWjbo(object P_0)
	{
		"{11111-22222-50001-00000}".Trim();
		byte[] array = Convert.FromBase64String((string)P_0);
		return Encoding.Unicode.GetString(array, 0, array.Length);
	}

	private static int SmeZwTKOr()
	{
		return 5;
	}

	private static void G7pusoAAO()
	{
		try
		{
			RSACryptoServiceProvider.UseMachineKeyStore = true;
		}
		catch
		{
		}
	}

	private static Delegate WNn7VtyuI(IntPtr P_0, Type P_1)
	{
		return (Delegate)Type.GetTypeFromHandle(qsV27iYB524t2usC5L.Sea13WHiH5(16777473)).GetMethod("GetDelegateForFunctionPointer", new Type[2]
		{
			Type.GetTypeFromHandle(qsV27iYB524t2usC5L.Sea13WHiH5(16777260)),
			Type.GetTypeFromHandle(qsV27iYB524t2usC5L.Sea13WHiH5(16777252))
		}).Invoke(null, new object[2] { P_0, P_1 });
	}

	internal static object oGhoVyLwi(object P_0)
	{
		try
		{
			if (File.Exists(((Assembly)P_0).Location))
			{
				return ((Assembly)P_0).Location;
			}
		}
		catch
		{
		}
		try
		{
			if (File.Exists(((Assembly)P_0).GetName().CodeBase.ToString().Replace("file:///", "")))
			{
				return ((Assembly)P_0).GetName().CodeBase.ToString().Replace("file:///", "");
			}
		}
		catch
		{
		}
		try
		{
			if (File.Exists(P_0.GetType().GetProperty("Location").GetValue(P_0, new object[0])
				.ToString()))
			{
				return P_0.GetType().GetProperty("Location").GetValue(P_0, new object[0])
					.ToString();
			}
		}
		catch
		{
		}
		return "";
	}

	[DllImport("kernel32", EntryPoint = "LoadLibrary")]
	public static extern IntPtr grYvFMse6(string P_0);

	[DllImport("kernel32", CharSet = CharSet.Ansi, EntryPoint = "GetProcAddress")]
	public static extern IntPtr r76RP97uO(IntPtr P_0, string P_1);

	private static IntPtr A8TNGW3oN(IntPtr P_0, object P_1, uint P_2)
	{
		if (CHf1uRY5iT == null)
		{
			CHf1uRY5iT = (ioC1YcvbZ2yRGgsOBE)Marshal.GetDelegateForFunctionPointer(r76RP97uO(pZbnhv6YB(), "Find ".Trim() + "ResourceA"), Type.GetTypeFromHandle(qsV27iYB524t2usC5L.Sea13WHiH5(33554615)));
		}
		return CHf1uRY5iT(P_0, (string)P_1, P_2);
	}

	private static IntPtr hrKrktdEC(IntPtr P_0, uint P_1, uint P_2, uint P_3)
	{
		if (XJ517srtuJ == null)
		{
			XJ517srtuJ = (qAS8QiRs8nbFaA3e0c)Marshal.GetDelegateForFunctionPointer(r76RP97uO(pZbnhv6YB(), "Virtual ".Trim() + "Alloc"), Type.GetTypeFromHandle(qsV27iYB524t2usC5L.Sea13WHiH5(33554616)));
		}
		return XJ517srtuJ(P_0, P_1, P_2, P_3);
	}

	private static int tdobkFCxq(IntPtr P_0, IntPtr P_1, [In][Out] byte[] P_2, uint P_3, out IntPtr P_4)
	{
		if (PrY1OKksdd == null)
		{
			PrY1OKksdd = (XNV0uBNIf0hRGLlTer)Marshal.GetDelegateForFunctionPointer(r76RP97uO(pZbnhv6YB(), "Write ".Trim() + "Process ".Trim() + "Memory"), Type.GetTypeFromHandle(qsV27iYB524t2usC5L.Sea13WHiH5(33554617)));
		}
		return PrY1OKksdd(P_0, P_1, P_2, P_3, out P_4);
	}

	private static int sHBPFakju(IntPtr P_0, int P_1, int P_2, ref int P_3)
	{
		if (HqA1otFDcJ == null)
		{
			HqA1otFDcJ = (QRRKfBrNJhQuGAsIhZ)Marshal.GetDelegateForFunctionPointer(r76RP97uO(pZbnhv6YB(), "Virtual ".Trim() + "Protect"), Type.GetTypeFromHandle(qsV27iYB524t2usC5L.Sea13WHiH5(33554618)));
		}
		return HqA1otFDcJ(P_0, P_1, P_2, ref P_3);
	}

	private static IntPtr wOCDlEV0B(uint P_0, int P_1, uint P_2)
	{
		if (J8t1vEcUBI == null)
		{
			J8t1vEcUBI = (IQBZqwbrNiAH0AoMWy)Marshal.GetDelegateForFunctionPointer(r76RP97uO(pZbnhv6YB(), "Open ".Trim() + "Process"), Type.GetTypeFromHandle(qsV27iYB524t2usC5L.Sea13WHiH5(33554619)));
		}
		return J8t1vEcUBI(P_0, P_1, P_2);
	}

	private static int npu4Nxkth(IntPtr P_0)
	{
		if (D081RjBZ7X == null)
		{
			D081RjBZ7X = (BJSCyWPgKUiJQGYihL)Marshal.GetDelegateForFunctionPointer(r76RP97uO(pZbnhv6YB(), "Close ".Trim() + "Handle"), Type.GetTypeFromHandle(qsV27iYB524t2usC5L.Sea13WHiH5(33554620)));
		}
		return D081RjBZ7X(P_0);
	}

	[SpecialName]
	private static IntPtr pZbnhv6YB()
	{
		if (hyO1NyyabH == IntPtr.Zero)
		{
			hyO1NyyabH = grYvFMse6("kernel ".Trim() + "32.dll");
		}
		return hyO1NyyabH;
	}

	private static byte[] wvdMNOpNF(object P_0)
	{
		using FileStream fileStream = new FileStream((string)P_0, FileMode.Open, FileAccess.Read, FileShare.Read);
		int num = 0;
		int num2 = (int)fileStream.Length;
		byte[] array = new byte[num2];
		while (num2 > 0)
		{
			int num3 = fileStream.Read(array, num, num2);
			num += num3;
			num2 -= num3;
		}
		return array;
	}

	internal static Stream lkp69qZGc()
	{
		return new MemoryStream();
	}

	internal static byte[] NiXTA2HX7(object P_0)
	{
		return ((MemoryStream)P_0).ToArray();
	}

	private static byte[] s1FeCITDg(object P_0)
	{
		Stream stream = lkp69qZGc();
		SymmetricAlgorithm symmetricAlgorithm = Q3ljUyvXm();
		symmetricAlgorithm.Key = new byte[32]
		{
			164, 230, 197, 107, 242, 72, 232, 250, 190, 57,
			239, 167, 191, 33, 120, 196, 158, 95, 15, 28,
			68, 128, 96, 116, 137, 66, 96, 33, 196, 42,
			145, 61
		};
		symmetricAlgorithm.IV = new byte[16]
		{
			51, 103, 220, 178, 226, 6, 243, 31, 216, 129,
			47, 51, 7, 121, 113, 225
		};
		CryptoStream cryptoStream = new CryptoStream(stream, symmetricAlgorithm.CreateDecryptor(), CryptoStreamMode.Write);
		cryptoStream.Write((byte[])P_0, 0, ((Array)P_0).Length);
		cryptoStream.Close();
		byte[] result = NiXTA2HX7(stream);
		TKW1L3mlUyvXmg29jA.SLV0fFIsptsZtjvFft17();
		return result;
	}

	private unsafe static int zlXXocCli(object P_0)
	{
		fixed (char* ptr = P_0)
		{
			int num = 5381;
			int num2 = num;
			char* ptr2 = ptr;
			int num3;
			while ((num3 = *ptr2) != 0)
			{
				num = ((num << 5) + num) ^ num3;
				num3 = ptr2[1];
				if (num3 == 0)
				{
					break;
				}
				num2 = ((num2 << 5) + num2) ^ num3;
				ptr2 += 2;
			}
			return num + num2 * 1566083941;
		}
	}

	internal static bool wN1cdRyTS(object P_0, object P_1)
	{
		if ((string)P_0 == (string)P_1)
		{
			return true;
		}
		if (P_0 == null || P_1 == null)
		{
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		int num = 0;
		int num2 = 0;
		if (((string)P_0).StartsWith((string)WDJ1r9tJFl))
		{
			flag = true;
			num = (int)(((string)P_0)[4] | ((uint)((string)P_0)[5] << 8) | ((uint)((string)P_0)[6] << 16) | ((uint)((string)P_0)[7] << 24));
		}
		if (((string)P_1).StartsWith((string)WDJ1r9tJFl))
		{
			flag2 = true;
			num2 = (int)(((string)P_1)[4] | ((uint)((string)P_1)[5] << 8) | ((uint)((string)P_1)[6] << 16) | ((uint)((string)P_1)[7] << 24));
		}
		if (!flag && !flag2)
		{
			return false;
		}
		if (!flag)
		{
			num = zlXXocCli(P_0);
		}
		if (!flag2)
		{
			num2 = zlXXocCli(P_1);
		}
		return num == num2;
	}

	private byte[] MqUKUgjbE()
	{
		return null;
	}

	private byte[] rt6sXXhe1()
	{
		return null;
	}

	private byte[] aDKqxYqZo()
	{
		return null;
	}

	private byte[] K4yxLJrtK()
	{
		return null;
	}

	private byte[] uvKydBjv4()
	{
		_ = "{11111-22222-30001-00001}".Length;
		_ = 0;
		return new byte[2] { 1, 2 };
	}

	private byte[] u3TGFQBex()
	{
		_ = "{11111-22222-30001-00002}".Length;
		_ = 0;
		return new byte[2] { 1, 2 };
	}

	internal byte[] mfvBdphXy()
	{
		_ = "{11111-22222-40001-00001}".Length;
		_ = 0;
		return new byte[2] { 1, 2 };
	}

	internal byte[] vS3Lj8XxE()
	{
		_ = "{11111-22222-40001-00002}".Length;
		_ = 0;
		return new byte[2] { 1, 2 };
	}

	internal byte[] CIKFBY5tq()
	{
		return null;
	}

	internal byte[] rGgW4oE5V()
	{
		return null;
	}

	internal static object fgxItVSmoyf2UdaZW9(object P_0)
	{
		return ((fjvIDXoHxLTGxlALSK)P_0).m9OIO8Q0EK();
	}

	internal static void d4U2henhDNYMZudxS2(object P_0, long P_1)
	{
		((Stream)P_0).Position = P_1;
	}

	internal static long BB1E7tce89vnIVZBQZ(object P_0)
	{
		return ((Stream)P_0).Length;
	}

	internal static object oBNoiTkRXSasLXWU43(object P_0, int P_1)
	{
		return ((fjvIDXoHxLTGxlALSK)P_0).z47It19xek(P_1);
	}

	internal static void T1dOflrluaTmBibP4T(object P_0)
	{
		((fjvIDXoHxLTGxlALSK)P_0).V26I7M2UXj();
	}

	internal static void FIpLtC7yY1sNprMD81(object P_0)
	{
		Array.Reverse((Array)P_0);
	}

	internal static object aL3tDUdTHHmu4KmSvX(object P_0)
	{
		return ((Assembly)P_0).GetName();
	}

	internal static object EhXxM2LIcXbUlHJ1DI(object P_0)
	{
		return ((AssemblyName)P_0).GetPublicKeyToken();
	}

	internal static object lbnlflqLmSXPZKcQTX()
	{
		return Q3ljUyvXm();
	}

	internal static void VYbUXLMxKNPiwoIkhq(object P_0, CipherMode P_1)
	{
		((SymmetricAlgorithm)P_0).Mode = P_1;
	}

	internal static object vhUXeKugHAdyDYuHqb(object P_0, object P_1, object P_2)
	{
		return ((SymmetricAlgorithm)P_0).CreateDecryptor((byte[])P_1, (byte[])P_2);
	}

	internal static object zZLAOCjiCoRIvFbxtl()
	{
		return lkp69qZGc();
	}

	internal static void OTDXFol2E58TQbw1tN(object P_0, object P_1, int P_2, int P_3)
	{
		((Stream)P_0).Write((byte[])P_1, P_2, P_3);
	}

	internal static void UiiKqs4hQIhbxK1y2E(object P_0)
	{
		((CryptoStream)P_0).FlushFinalBlock();
	}

	internal static object tGXtlywuDIF2NsH9F8(object P_0)
	{
		return NiXTA2HX7(P_0);
	}

	internal static void fvtyTJvLd4ZNqSjenZ(object P_0)
	{
		((Stream)P_0).Close();
	}

	internal static object hFCFP5g3c0I3cSdUlo(object P_0)
	{
		return ((Assembly)P_0).EntryPoint;
	}

	internal static bool FAOp5P9EMskJNR6hK3(object P_0, object P_1)
	{
		return (MethodInfo)P_0 == (MethodInfo)P_1;
	}

	internal static bool c5UQYoHrwPy4s3HFqO()
	{
		return (object)null == null;
	}

	internal static object NQR33GaYr4l24BZ5uG()
	{
		return null;
	}

	internal static void RsC3LEVTdskpdN0chxE()
	{
		TKW1L3mlUyvXmg29jA.SLV0fFIsptsZtjvFft17();
	}

	internal static void i7YjldVGDlk1CjhU5Ls(bool P_0)
	{
		RSACryptoServiceProvider.UseMachineKeyStore = P_0;
	}

	internal static Type uvNn4CV6Nx8G5sppy1F(RuntimeTypeHandle P_0)
	{
		return Type.GetTypeFromHandle(P_0);
	}

	internal static object lxFjn2VCgikddsiisyy(object P_0)
	{
		return ((Assembly)P_0).Location;
	}

	internal static int pHE5tLVHpCC28yf28ms(object P_0)
	{
		return ((string)P_0).Length;
	}

	internal static object GOSw76VaLaqcplZLxIV()
	{
		return SHA1.Create();
	}

	internal static object r1Zo7oVSfD9sUlE2Oer(object P_0)
	{
		return CryptoConfig.MapNameToOID((string)P_0);
	}

	internal static bool xuHXidVn92aickIC37X(object P_0)
	{
		return File.Exists((string)P_0);
	}

	internal static object hXxG1gVcLQ0qt6NnAiU(object P_0, object P_1)
	{
		return ((Assembly)P_0).GetManifestResourceStream((string)P_1);
	}

	internal static object dskYhlVk06eKCCTFM2T(object P_0)
	{
		return ((fjvIDXoHxLTGxlALSK)P_0).m9OIO8Q0EK();
	}

	internal static void km4h2rVrpmeiGDc0co1(object P_0, long P_1)
	{
		((Stream)P_0).Position = P_1;
	}

	internal static long PLlbrxV78RGsEOWgN1B(object P_0)
	{
		return ((Stream)P_0).Length;
	}

	internal static object dgcjBXVdfhRDPk3eS0v(object P_0, int P_1)
	{
		return ((fjvIDXoHxLTGxlALSK)P_0).z47It19xek(P_1);
	}

	internal static object cB7VR7VLen67vc8MSpR()
	{
		return Q3ljUyvXm();
	}

	internal static void RODiIJVq9eRU8xajRaU(object P_0, CipherMode P_1)
	{
		((SymmetricAlgorithm)P_0).Mode = P_1;
	}

	internal static object SGeZbvVMVGTEoM3pprS(object P_0, object P_1, object P_2)
	{
		return ((SymmetricAlgorithm)P_0).CreateDecryptor((byte[])P_1, (byte[])P_2);
	}

	internal static object ThpAbsVuMayUpcuD2lt()
	{
		return lkp69qZGc();
	}

	internal static void fukh2VVjX5ZsEFBhG64(object P_0, object P_1, int P_2, int P_3)
	{
		((Stream)P_0).Write((byte[])P_1, P_2, P_3);
	}

	internal static void I3xulHVlDcUxGmHijF4(object P_0)
	{
		((CryptoStream)P_0).FlushFinalBlock();
	}

	internal static object VpA2ZNV4Tx2P5yHjmal()
	{
		return Encoding.UTF8;
	}

	internal static object X8xKEYVwV313fZv2yHZ(object P_0)
	{
		return NiXTA2HX7(P_0);
	}

	internal static object z4itM6VvPYu9ERDWcWn(object P_0, object P_1)
	{
		return ((Encoding)P_0).GetString((byte[])P_1);
	}

	internal static void DqoBcDVgqxMS4cXL2ea(object P_0, object P_1)
	{
		((AsymmetricAlgorithm)P_0).FromXmlString((string)P_1);
	}

	internal static void mTwZ02V9WbKVqUTZIXf(object P_0)
	{
		((Stream)P_0).Close();
	}

	internal static void OL4QkgV878SlOj3GQHL(object P_0)
	{
		((fjvIDXoHxLTGxlALSK)P_0).V26I7M2UXj();
	}

	internal static void oHD0ACVmylJBnLG4w48(object P_0, object P_1, uint P_2, object P_3)
	{
		IGdhEwAds(P_0, P_1, P_2, P_3);
	}

	internal static ushort Tea9bJVIa63UvV4QdQY(object P_0)
	{
		return ((BinaryReader)P_0).ReadUInt16();
	}

	internal static int BwEdSeVZtSQXwcbI9Km(object P_0, object P_1, int P_2, int P_3)
	{
		return ((Stream)P_0).Read((byte[])P_1, P_2, P_3);
	}

	internal static void EcrUCFV13N7yIOykUSA(object P_0, object P_1, int P_2, int P_3)
	{
		W27iiB524(P_0, P_1, P_2, P_3);
	}

	internal static long rxiv8RVFOgEVUmVYXbi(object P_0)
	{
		return ((Stream)P_0).Position;
	}

	internal static uint IOIijsVp2CKAGvhxiSp(object P_0)
	{
		return ((BinaryReader)P_0).ReadUInt32();
	}

	internal static uint dRInLkVJVsMY4cC5jXJ(uint P_0, int P_1, long P_2, object P_3)
	{
		return u2u5sC5LH(P_0, P_1, P_2, P_3);
	}

	internal static long prcAyNVOsjDIQUjBxjZ(long P_0, long P_1)
	{
		return Math.Min(P_0, P_1);
	}

	internal static object QDL7cMVNZ6qgnGEJW2a(object P_0, object P_1, int P_2, int P_3)
	{
		return ((HashAlgorithm)P_0).TransformFinalBlock((byte[])P_1, P_2, P_3);
	}

	internal static object uZfoA3VRwAZC9Zqa12x(object P_0, int P_1)
	{
		return ((BinaryReader)P_0).ReadBytes(P_1);
	}

	internal static void XgbLNoV3rVV6mWQcOxb(object P_0)
	{
		Array.Reverse((Array)P_0);
	}

	internal static object vYusHvVsk5cfNrv4QHR(object P_0)
	{
		return ((HashAlgorithm)P_0).Hash;
	}

	internal static bool F9JtBoVz0adHkaQWRGA(object P_0, object P_1, object P_2, object P_3)
	{
		return ((RSACryptoServiceProvider)P_0).VerifyHash((byte[])P_1, (string)P_2, (byte[])P_3);
	}

	internal static void nWEgMlKWBZji3Mh9kwD(object P_0)
	{
		((BinaryReader)P_0).Close();
	}

	internal static object dceoAiKP26ayP4CixmC(object P_0)
	{
		return ((Assembly)P_0).GetName();
	}

	internal static object AcUe9sKEvJGxOsPGsZ8(object P_0)
	{
		return ((AssemblyName)P_0).Name;
	}

	internal static object ufg6RbKbKaQPexZctpg(object P_0, object P_1)
	{
		return (string)P_0 + (string)P_1;
	}

	internal static bool i49iQrVAbQJjTAmDg5C()
	{
		return (object)null == null;
	}

	internal static object AXIGxCVXqrTRTX5negD()
	{
		return null;
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.2.0.10050' (yours is '9.1.0.7988')
