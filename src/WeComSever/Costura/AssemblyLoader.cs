using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Costura;

[CompilerGenerated]
internal static class AssemblyLoader
{
	private static object nullCacheLock = new object();

	private static Dictionary<string, bool> nullCache = new Dictionary<string, bool>();

	private static Dictionary<string, string> assemblyNames = new Dictionary<string, string>();

	private static Dictionary<string, string> symbolNames = new Dictionary<string, string>();

	private static int isAttached;

	private static string CultureToString(CultureInfo culture)
	{
		if (culture == null)
		{
			return string.Empty;
		}
		return culture.Name;
	}

	private static Assembly ReadExistingAssembly(AssemblyName name)
	{
		AppDomain currentDomain = AppDomain.CurrentDomain;
		Assembly[] assemblies = currentDomain.GetAssemblies();
		Assembly[] array = assemblies;
		foreach (Assembly assembly in array)
		{
			AssemblyName name2 = assembly.GetName();
			if (string.Equals(name2.Name, name.Name, StringComparison.InvariantCultureIgnoreCase) && string.Equals(CultureToString(name2.CultureInfo), CultureToString(name.CultureInfo), StringComparison.InvariantCultureIgnoreCase))
			{
				return assembly;
			}
		}
		return null;
	}

	private static string GetAssemblyResourceName(AssemblyName requestedAssemblyName)
	{
		string text = requestedAssemblyName.Name.ToLowerInvariant();
		if (requestedAssemblyName.CultureInfo != null && !string.IsNullOrEmpty(requestedAssemblyName.CultureInfo.Name))
		{
			text = (CultureToString(requestedAssemblyName.CultureInfo) + "." + text).ToLowerInvariant();
		}
		return text;
	}

	private static void CopyTo(Stream source, Stream destination)
	{
		byte[] array = new byte[81920];
		int count;
		while ((count = source.Read(array, 0, array.Length)) != 0)
		{
			destination.Write(array, 0, count);
		}
	}

	private static Stream LoadStream(string fullName)
	{
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		if (fullName.EndsWith(".compressed"))
		{
			using (Stream stream = executingAssembly.GetManifestResourceStream(fullName))
			{
				using DeflateStream source = new DeflateStream(stream, CompressionMode.Decompress);
				MemoryStream memoryStream = new MemoryStream();
				CopyTo(source, memoryStream);
				memoryStream.Position = 0L;
				return memoryStream;
			}
		}
		return executingAssembly.GetManifestResourceStream(fullName);
	}

	private static Stream LoadStream(Dictionary<string, string> resourceNames, string name)
	{
		if (resourceNames.TryGetValue(name, out var value))
		{
			return LoadStream(value);
		}
		return null;
	}

	private static byte[] ReadStream(Stream stream)
	{
		byte[] array = new byte[stream.Length];
		stream.Read(array, 0, array.Length);
		return array;
	}

	private static Assembly ReadFromEmbeddedResources(Dictionary<string, string> assemblyNames, Dictionary<string, string> symbolNames, AssemblyName requestedAssemblyName)
	{
		string assemblyResourceName = GetAssemblyResourceName(requestedAssemblyName);
		byte[] rawAssembly;
		using (Stream stream = LoadStream(assemblyNames, assemblyResourceName))
		{
			if (stream == null)
			{
				return null;
			}
			rawAssembly = ReadStream(stream);
		}
		using (Stream stream2 = LoadStream(symbolNames, assemblyResourceName))
		{
			if (stream2 != null)
			{
				byte[] rawSymbolStore = ReadStream(stream2);
				return Assembly.Load(rawAssembly, rawSymbolStore);
			}
		}
		return Assembly.Load(rawAssembly);
	}

	public static Assembly ResolveAssembly(object sender, ResolveEventArgs e)
	{
		string name = e.Name;
		AssemblyName assemblyName = new AssemblyName(name);
		lock (nullCacheLock)
		{
			if (nullCache.ContainsKey(name))
			{
				return null;
			}
		}
		Assembly assembly = ReadExistingAssembly(assemblyName);
		if ((object)assembly != null)
		{
			return assembly;
		}
		assembly = ReadFromEmbeddedResources(assemblyNames, symbolNames, assemblyName);
		if ((object)assembly == null)
		{
			lock (nullCacheLock)
			{
				nullCache[name] = true;
			}
			if ((assemblyName.Flags & AssemblyNameFlags.Retargetable) != AssemblyNameFlags.None)
			{
				assembly = Assembly.Load(assemblyName);
			}
		}
		return assembly;
	}

	static AssemblyLoader()
	{
		assemblyNames.Add("costura", "costura.costura.dll.compressed");
		symbolNames.Add("costura", "costura.costura.pdb.compressed");
		assemblyNames.Add("de.spectre.console.cli.resources", "costura.de.spectre.console.cli.resources.dll.compressed");
		assemblyNames.Add("es.spectre.console.cli.resources", "costura.es.spectre.console.cli.resources.dll.compressed");
		assemblyNames.Add("fr.spectre.console.cli.resources", "costura.fr.spectre.console.cli.resources.dll.compressed");
		assemblyNames.Add("it.spectre.console.cli.resources", "costura.it.spectre.console.cli.resources.dll.compressed");
		assemblyNames.Add("ja.spectre.console.cli.resources", "costura.ja.spectre.console.cli.resources.dll.compressed");
		assemblyNames.Add("ko.spectre.console.cli.resources", "costura.ko.spectre.console.cli.resources.dll.compressed");
		assemblyNames.Add("newtonsoft.json.bson", "costura.newtonsoft.json.bson.dll.compressed");
		symbolNames.Add("newtonsoft.json.bson", "costura.newtonsoft.json.bson.pdb.compressed");
		assemblyNames.Add("newtonsoft.json", "costura.newtonsoft.json.dll.compressed");
		assemblyNames.Add("pt.spectre.console.cli.resources", "costura.pt.spectre.console.cli.resources.dll.compressed");
		assemblyNames.Add("ru.spectre.console.cli.resources", "costura.ru.spectre.console.cli.resources.dll.compressed");
		assemblyNames.Add("spectre.console.cli", "costura.spectre.console.cli.dll.compressed");
		assemblyNames.Add("spectre.console", "costura.spectre.console.dll.compressed");
		assemblyNames.Add("spectre.console.json", "costura.spectre.console.json.dll.compressed");
		assemblyNames.Add("sv.spectre.console.cli.resources", "costura.sv.spectre.console.cli.resources.dll.compressed");
		assemblyNames.Add("system.buffers", "costura.system.buffers.dll.compressed");
		assemblyNames.Add("system.memory", "costura.system.memory.dll.compressed");
		assemblyNames.Add("system.net.http.formatting", "costura.system.net.http.formatting.dll.compressed");
		assemblyNames.Add("system.numerics.vectors", "costura.system.numerics.vectors.dll.compressed");
		assemblyNames.Add("system.runtime.compilerservices.unsafe", "costura.system.runtime.compilerservices.unsafe.dll.compressed");
		assemblyNames.Add("system.threading.tasks.extensions", "costura.system.threading.tasks.extensions.dll.compressed");
		assemblyNames.Add("system.web.http", "costura.system.web.http.dll.compressed");
		assemblyNames.Add("system.web.http.selfhost", "costura.system.web.http.selfhost.dll.compressed");
		assemblyNames.Add("zh-hans.spectre.console.cli.resources", "costura.zh-hans.spectre.console.cli.resources.dll.compressed");
	}

	public static void Attach(bool subscribe)
	{
		if (Interlocked.Exchange(ref isAttached, 1) == 1 || !subscribe)
		{
			return;
		}
		AppDomain currentDomain = AppDomain.CurrentDomain;
		currentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs e)
		{
			string name = e.Name;
			AssemblyName assemblyName = new AssemblyName(name);
			lock (nullCacheLock)
			{
				if (nullCache.ContainsKey(name))
				{
					return (Assembly)null;
				}
			}
			Assembly assembly = ReadExistingAssembly(assemblyName);
			if ((object)assembly != null)
			{
				return assembly;
			}
			assembly = ReadFromEmbeddedResources(assemblyNames, symbolNames, assemblyName);
			if ((object)assembly == null)
			{
				lock (nullCacheLock)
				{
					nullCache[name] = true;
				}
				if ((assemblyName.Flags & AssemblyNameFlags.Retargetable) != AssemblyNameFlags.None)
				{
					assembly = Assembly.Load(assemblyName);
				}
			}
			return assembly;
		};
	}
}
