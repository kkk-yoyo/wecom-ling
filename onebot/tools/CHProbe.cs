using System;
using dnlib.DotNet;
internal static class CHProbe
{
    private static void Main(string[] args)
    {
        ModuleDefMD mod = ModuleDefMD.Load(args[0]);
        TypeDef t = mod.Find("WeComConsoleServer.Core.WxWebSocketClient", false);
        TypeDef ch = null;
        for (int i = 0; i < t.NestedTypes.Count; i++)
            if (t.NestedTypes[i].Name == "CommandHandler") ch = t.NestedTypes[i];
        Console.WriteLine("type=" + (ch == null ? "null" : ch.FullName) + " methods=" + (ch == null ? 0 : ch.Methods.Count));
        if (ch != null)
            for (int i = 0; i < ch.Methods.Count; i++)
                Console.WriteLine("  " + ch.Methods[i].FullName + " attrs=" + ch.Methods[i].Attributes + " impl=" + ch.Methods[i].ImplAttributes + " rva=0x" + ((uint)ch.Methods[i].RVA).ToString("X8"));
    }
}
