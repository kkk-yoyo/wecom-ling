using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

internal static class ProbeReach
{
    private static void Main(string[] args)
    {
        string path = args.Length > 0 ? args[0] : @"D:\wecom-ling\微灵1.11.0_免卡密.exe";
        ModuleDefMD mod = ModuleDefMD.Load(path);
        int total = 0, unreachable = 0, reachable = 0, ehBoundary = 0;
        List<string> reachableList = new List<string>();
        foreach (TypeDef t in mod.GetTypes())
        {
            foreach (MethodDef m in t.Methods)
            {
                if (!m.HasBody || m.Body == null) continue;
                CilBody body = m.Body;
                List<Instruction> instrs = new List<Instruction>(body.Instructions);
                List<Instruction> nulls = new List<Instruction>();
                for (int i = 0; i < instrs.Count; i++)
                {
                    Instruction ins = instrs[i];
                    if (ins.Operand == null && ins.OpCode.OperandType != OperandType.InlineNone) nulls.Add(ins);
                }
                if (nulls.Count == 0) continue;
                HashSet<Instruction> roots = new HashSet<Instruction>();
                roots.Add(instrs[0]);
                foreach (ExceptionHandler eh in body.ExceptionHandlers)
                {
                    if (eh.TryStart != null) roots.Add(eh.TryStart);
                    if (eh.HandlerStart != null) roots.Add(eh.HandlerStart);
                    if (eh.FilterStart != null) roots.Add(eh.FilterStart);
                }
                HashSet<Instruction> reach = new HashSet<Instruction>();
                Queue<Instruction> q = new Queue<Instruction>();
                foreach (Instruction r in roots) { if (reach.Add(r)) q.Enqueue(r); }
                while (q.Count > 0)
                {
                    Instruction ins = q.Dequeue();
                    foreach (Instruction succ in Successors(ins, instrs))
                        if (succ != null && reach.Add(succ)) q.Enqueue(succ);
                }
                foreach (Instruction ins in nulls)
                {
                    total++;
                    if (reach.Contains(ins)) { reachable++; reachableList.Add(m.FullName + " IL_" + ins.Offset.ToString("X4") + " " + ins.OpCode.Name); }
                    else unreachable++;
                }
                // EH 边界直接指向 null 指令也算危险
                foreach (ExceptionHandler eh in body.ExceptionHandlers)
                {
                    if (nulls.Contains(eh.TryStart) || nulls.Contains(eh.TryEnd) || nulls.Contains(eh.HandlerStart) || nulls.Contains(eh.HandlerEnd) || nulls.Contains(eh.FilterStart)) ehBoundary++;
                }
            }
        }
        Console.WriteLine("nullOperands=" + total + " unreachable=" + unreachable + " reachable=" + reachable + " ehBoundaryRefs=" + ehBoundary);
        for (int i = 0; i < reachableList.Count && i < 20; i++) Console.WriteLine("REACHABLE " + reachableList[i]);
    }

    private static IEnumerable<Instruction> Successors(Instruction ins, List<Instruction> instrs)
    {
        OpCode op = ins.OpCode;
        List<Instruction> result = new List<Instruction>();
        bool isBranch = op.FlowControl == FlowControl.Branch || op.FlowControl == FlowControl.Cond_Branch;
        if (op.Code == Code.Switch)
        {
            Instruction[] targets = ins.Operand as Instruction[];
            if (targets != null) foreach (Instruction t in targets) result.Add(t);
            result.Add(Next(ins, instrs));
            return result;
        }
        if (isBranch)
        {
            result.Add(ins.Operand as Instruction);
            if (op.FlowControl == FlowControl.Cond_Branch) result.Add(Next(ins, instrs));
            return result;
        }
        if (op.FlowControl == FlowControl.Return || op.FlowControl == FlowControl.Throw || op.FlowControl == FlowControl.Break) return result;
        if (op.Code == Code.Endfinally || op.Code == Code.Endfilter) return result;
        if (op.Code == Code.Jmp) return result;
        result.Add(Next(ins, instrs));
        return result;
    }

    private static Instruction Next(Instruction ins, List<Instruction> instrs)
    {
        int idx = instrs.IndexOf(ins);
        if (idx < 0 || idx + 1 >= instrs.Count) return null;
        return instrs[idx + 1];
    }
}

