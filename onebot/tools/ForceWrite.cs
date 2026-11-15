using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

class ForceWrite
{
    static IEnumerable<TypeDefinition> Walk(TypeDefinition t)
    {
        yield return t;
        foreach (TypeDefinition n in t.NestedTypes) foreach (TypeDefinition x in Walk(n)) yield return x;
    }
    static IEnumerable<TypeDefinition> AllTypes(ModuleDefinition mod)
    {
        foreach (TypeDefinition t in mod.Types) foreach (TypeDefinition x in Walk(t)) yield return x;
    }

    static bool NeedsOperand(OperandType ot)
    {
        if (ot == OperandType.InlineNone) return false;
        return true;
    }

    static void Main(string[] args)
    {
        AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(args[0]);
        ModuleDefinition mod = asm.MainModule;
        int methods = 0, bodies = 0, instrs = 0, nullOp = 0;
        Dictionary<string, int> nullByOp = new Dictionary<string, int>();
        List<string> nullSamples = new List<string>();
        foreach (TypeDefinition t in AllTypes(mod))
        {
            foreach (MethodDefinition m in t.Methods)
            {
                methods++;
                Mono.Cecil.Cil.MethodBody b = m.Body;
                if (b == null) continue;
                bodies++;
                foreach (Instruction ins in b.Instructions)
                {
                    instrs++;
                    if (!NeedsOperand(ins.OpCode.OperandType)) continue;
                    if (ins.Operand != null) continue;
                    nullOp++;
                    string k = ins.OpCode.OperandType + ":" + ins.OpCode.Name;
                    if (!nullByOp.ContainsKey(k)) nullByOp[k] = 0;
                    nullByOp[k]++;
                    if (nullSamples.Count < 60)
                        nullSamples.Add(t.FullName + "::" + m.Name + " IL_" + ins.Offset.ToString("X4") + " " + ins.OpCode.Name + " type=" + ins.OpCode.OperandType);
                }
            }
        }
        Console.WriteLine("methods=" + methods + " bodies=" + bodies + " instrs=" + instrs + " nullOperands=" + nullOp);
        foreach (KeyValuePair<string, int> kv in nullByOp) Console.WriteLine("  " + kv.Key + " x" + kv.Value);
        foreach (string s in nullSamples) Console.WriteLine("  NULL " + s);

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[1])));
        try
        {
            mod.Write(args[1]);
            Console.WriteLine("write ok len=" + new FileInfo(args[1]).Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine("WRITE FAIL type=" + ex.GetType().FullName);
            try { Console.WriteLine("msg=" + ex.Message); } catch (Exception) { }
            try { Console.WriteLine("stack=" + ex.StackTrace); } catch (Exception) { }
        }
    }
}
