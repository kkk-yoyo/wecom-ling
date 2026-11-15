using System;
using System.Reflection;
internal static class R3
{
    private static void Main()
    {
        Assembly asm = typeof(dnlib.DotNet.ModuleDefMD).Assembly;
        Type[] ts = asm.GetTypes();
        for (int i = 0; i < ts.Length; i++)
        {
            string n = ts[i].FullName == null ? "" : ts[i].FullName;
            if (n.IndexOf("Logger", StringComparison.OrdinalIgnoreCase) >= 0) Console.WriteLine(ts[i].FullName + "  interface=" + ts[i].IsInterface);
        }
        Type l = asm.GetType("dnlib.DotNet.ILogger");
        if (l != null) foreach (MemberInfo m in l.GetMembers()) Console.WriteLine("ILogger: " + m.MemberType + " " + m);
        Type le = asm.GetType("dnlib.DotNet.LoggerEvent");
        if (le != null) Console.WriteLine("LoggerEvent values: " + string.Join(", ", Enum.GetNames(le)));
    }
}
