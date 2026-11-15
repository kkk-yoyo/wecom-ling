using System;
using System.IO;
using dnlib.DotNet;
using dnlib.DotNet.Writer;

internal static class DnRoundTrip
{
    private static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("usage: DnRoundTrip <src> <dst>");
            return;
        }
        ModuleDefMD mod = ModuleDefMD.Load(args[0]);
        Console.WriteLine("types=" + mod.Types.Count + " resources=" + mod.Resources.Count + " asm=" + mod.Assembly.FullName);
        ModuleWriterOptions opts = new ModuleWriterOptions(mod);
        opts.Logger = DummyLogger.NoThrowInstance;
        mod.Write(args[1], opts);
        Console.WriteLine("written " + args[1] + " size=" + new FileInfo(args[1]).Length);
        ModuleDefMD mod2 = ModuleDefMD.Load(args[1]);
        Console.WriteLine("reread types=" + mod2.Types.Count + " resources=" + mod2.Resources.Count);
    }
}
