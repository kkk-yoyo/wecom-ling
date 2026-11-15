using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

class Dump3
{
    static void Main(string[] args)
    {
        StringBuilder sb = new StringBuilder();
        try
        {
            AssemblyDefinition shim = AssemblyDefinition.ReadAssembly(args[0]);
            ModuleDefinition smod = shim.MainModule;
            TypeDefinition st = null;
            foreach (TypeDefinition t in smod.Types) if (t.FullName == "OneBotShim.WxSocketClient") st = t;
            sb.AppendLine("cctor present=" + (st.Methods.Count > 0));
            foreach (MethodDefinition m in st.Methods) if (m.Name == ".cctor") sb.AppendLine("SHIM HAS CCTOR");
            // operand census
            Dictionary<string, int> ops = new Dictionary<string, int>();
            HashSet<string> argSample = new HashSet<string>();
            foreach (MethodDefinition m in st.Methods)
            {
                if (!m.HasBody) continue;
                foreach (Instruction ins in m.Body.Instructions)
                {
                    string k = ins.OpCode.OperandType.ToString();
                    if (!ops.ContainsKey(k)) ops[k] = 0;
                    ops[k] = ops[k] + 1;
                    if (ins.OpCode.OperandType == OperandType.InlineArg || ins.OpCode.OperandType == OperandType.ShortInlineArg)
                    {
                        ParameterDefinition p = (ParameterDefinition)ins.Operand;
                        bool isThis = object.ReferenceEquals(p, m.Body.ThisParameter);
                        string key = m.Name + " arg idx=" + p.Index + " name=" + (p.Name == null ? "<null>" : p.Name) + " isThis=" + isThis + " inParams=" + (m.Parameters.IndexOf(p) >= 0);
                        if (!argSample.Contains(key)) argSample.Add(key);
                    }
                }
            }
            foreach (KeyValuePair<string, int> kv in ops) sb.AppendLine("OPT " + kv.Key + " x" + kv.Value);
            sb.AppendLine("-- arg samples --");
            List<string> ks = new List<string>(argSample); ks.Sort();
            foreach (string k in ks) sb.AppendLine("ARG " + k);
            // InlineVar check
            int varCnt = 0; foreach (MethodDefinition m in st.Methods) { if (!m.HasBody) continue; foreach (Instruction ins in m.Body.Instructions) if (ins.OpCode.OperandType == OperandType.InlineVar || ins.OpCode.OperandType == OperandType.ShortInlineVar) { varCnt++; if (varCnt < 6) sb.AppendLine("VAROP " + m.Name + " " + ((VariableDefinition)ins.Operand).Index); } }
            sb.AppendLine("varOps=" + varCnt);
            // private method samples
            foreach (MethodDefinition m in st.Methods)
            {
                if (m.Name == "ParseOutgoingSegments" || m.Name == "add_StatusChanged" || m.Name == "InvokeCommand")
                {
                    sb.AppendLine("-- IL " + m.Name + " --");
                    if (m.HasBody) foreach (Instruction ins in m.Body.Instructions) sb.AppendLine("   " + ins.OpCode + " " + Fmt(ins));
                }
            }
        }
        catch (Exception ex) { sb.AppendLine("FATAL " + ex.GetType().FullName + " / " + ex.Message); }
        Console.WriteLine(sb.ToString());
    }

    static string Fmt(Instruction ins)
    {
        object o = ins.Operand;
        if (o == null) return "";
        if (o is string) return "\"" + o + "\"";
        if (o is Instruction) return "IL_" + ((Instruction)o).Offset;
        if (o is Instruction[]) return "switch[]";
        return o.ToString();
    }
}
