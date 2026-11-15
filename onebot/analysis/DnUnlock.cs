using System;
using System.IO;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;

internal static class DnUnlock
{
    private static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("usage: DnUnlock <src> <dst>");
            return;
        }
        ModuleDefMD mod = ModuleDefMD.Load(args[0]);
        TypeDef type = mod.Find("SG9KiyIbtdgGDf12qr.z2jc63fLkugS1X8Q9N", false);
        if (type == null)
        {
            Console.WriteLine("helper type missing");
            return;
        }
        MethodDef target = null;
        foreach (MethodDef m in type.Methods)
        {
            if (m.Name != "tE1kD1vbB" || !m.HasParamDefs || m.ParamDefs.Count != 1)
            {
                continue;
            }
            if (m.MethodSig != null && m.MethodSig.Params.Count == 1 && m.MethodSig.Params[0].ElementType == ElementType.I4)
            {
                target = m;
                break;
            }
        }
        if (target == null || target.Body == null)
        {
            Console.WriteLine("decrypt method missing");
            return;
        }
        Console.WriteLine("method " + target.FullName + " instrs=" + target.Body.Instructions.Count);
        int patched = 0;
        for (int i = 0; i + 1 < target.Body.Instructions.Count; i++)
        {
            Instruction cur = target.Body.Instructions[i];
            if (cur.OpCode.Code != Code.Ldsfld)
            {
                continue;
            }
            IField fr = cur.Operand as IField;
            if (fr == null || fr.Name != "Pgs1HOBEQA")
            {
                continue;
            }
            Instruction nxt = target.Body.Instructions[i + 1];
            if (nxt.OpCode.Code == Code.Ldc_I4_S && Convert.ToInt32(nxt.Operand) == 75)
            {
                nxt.Operand = (sbyte)0;
                patched++;
                Console.WriteLine("patched anti-tamper cmp at instr " + (i + 1));
                break;
            }
        }
        ModuleWriterOptions opts = new ModuleWriterOptions(mod);
        opts.Logger = DummyLogger.NoThrowInstance;
        mod.Write(args[1], opts);
        Console.WriteLine("written " + args[1] + " (patched=" + patched + ")");
    }
}
