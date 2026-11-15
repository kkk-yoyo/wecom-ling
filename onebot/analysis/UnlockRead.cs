using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

internal static class UnlockRead
{
    private static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("usage: UnlockRead <origExe> <outExe>");
            return;
        }
        string orig = args[0];
        string outp = args[1];
        AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(orig);
        TypeDefinition t = asm.MainModule.GetType("SG9KiyIbtdgGDf12qr.z2jc63fLkugS1X8Q9N");
        if (t == null)
        {
            Console.WriteLine("helper type missing");
            return;
        }
        MethodDefinition m = null;
        foreach (MethodDefinition md in t.Methods)
        {
            if (md.Name == "tE1kD1vbB" && md.HasParameters && md.Parameters.Count == 1 && md.Parameters[0].ParameterType.MetadataType == MetadataType.Int32)
            {
                m = md;
                break;
            }
        }
        if (m == null)
        {
            Console.WriteLine("decrypt method missing");
            return;
        }
        Console.WriteLine("patching " + m.FullName + " bodyLen=" + m.Body.Instructions.Count);
        int patched = 0;
        bool skipPatch = args.Length > 2 && args[2] == "nopatch";
        var il = m.Body.GetILProcessor();
        var insns = m.Body.Instructions;
        if (skipPatch)
        {
            Console.WriteLine("nopatch mode: round-trip only");
        }
        else
        {
            for (int i = 0; i + 2 < insns.Count; i++)
            {
            Instruction cur = insns[i];
            if (cur.OpCode != OpCodes.Ldsfld)
            {
                continue;
            }
            FieldReference fr = cur.Operand as FieldReference;
            if (fr == null || fr.Name != "Pgs1HOBEQA")
            {
                continue;
            }
            Instruction nxt = insns[i + 1];
            if (nxt.OpCode == OpCodes.Ldc_I4_S && Convert.ToInt32(nxt.Operand) == 75)
            {
                nxt.Operand = (sbyte)0;
                patched++;
                Console.WriteLine("patched anti-tamper cmp at instr " + (i + 1) + " (in-place operand)");
                break;
            }
            }
        }
        if (patched == 0)
        {
            Console.WriteLine("WARN: anti-tamper pattern not found; dumping nearby locals");
        }
        asm.Write(outp);
        Console.WriteLine("written " + outp + " (patched=" + patched + ")");
    }
}
