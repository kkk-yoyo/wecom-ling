using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

class Dump
{
    static string Attrs(Mono.Cecil.MethodAttributes a) { return a.ToString(); }
    static string FAttrs(Mono.Cecil.FieldAttributes a) { return a.ToString(); }

    static string T(TypeReference t)
    {
        if (t == null) return "<null>";
        return t.FullName;
    }

    static void DumpTypeDetails(TypeDefinition t, StringBuilder sb, bool fullBodies)
    {
        sb.AppendLine("== TYPE " + t.FullName + " attrs=" + t.Attributes + " base=" + T(t.BaseType) + " ifaces=" + t.Interfaces.Count);
        sb.AppendLine("   props=" + t.Properties.Count + " events=" + t.Events.Count + " nested=" + t.NestedTypes.Count + " fields=" + t.Fields.Count + " methods=" + t.Methods.Count);
        foreach (FieldDefinition f in t.Fields)
        {
            string extra = "";
            if (f.HasConstant) extra = " const=" + (f.Constant == null ? "<null>" : f.Constant.ToString());
            sb.AppendLine("   F " + f.Name + " : " + T(f.FieldType) + " attrs=" + FAttrs(f.Attributes) + extra);
        }
        foreach (PropertyDefinition p in t.Properties)
        {
            sb.AppendLine("   P " + p.Name + " : " + T(p.PropertyType) + " get=" + (p.GetMethod == null ? "-" : p.GetMethod.Name) + " set=" + (p.SetMethod == null ? "-" : p.SetMethod.Name));
        }
        foreach (EventDefinition e in t.Events)
        {
            sb.AppendLine("   E " + e.Name + " : " + T(e.EventType) + " add=" + (e.AddMethod == null ? "-" : e.AddMethod.Name) + " remove=" + (e.RemoveMethod == null ? "-" : e.RemoveMethod.Name));
        }
        foreach (MethodDefinition m in t.Methods)
        {
            StringBuilder sig = new StringBuilder();
            sig.Append("   M ").Append(m.Name).Append(" static=").Append(m.IsStatic).Append(" attrs=").Append(m.Attributes);
            sig.Append(" ret=").Append(T(m.ReturnType)).Append(" params=");
            for (int i = 0; i < m.Parameters.Count; i++)
            {
                if (i > 0) sig.Append(", ");
                sig.Append(T(m.Parameters[i].ParameterType));
            }
            if (m.HasBody)
            {
                sig.Append(" [IL maxstack=").Append(m.Body.MaxStackSize).Append(" initlocals=").Append(m.Body.InitLocals).Append(" vars=").Append(m.Body.Variables.Count).Append(" instrs=").Append(m.Body.Instructions.Count).Append(" eh=").Append(m.Body.ExceptionHandlers.Count).Append("]");
            }
            else
            {
                sig.Append(" [no body]");
            }
            sb.AppendLine(sig.ToString());
            if (fullBodies && m.HasBody)
            {
                foreach (VariableDefinition v in m.Body.Variables) sb.AppendLine("      V " + v.Index + " : " + T(v.VariableType));
                foreach (ExceptionHandler h in m.Body.ExceptionHandlers)
                {
                    sb.AppendLine("      EH " + h.HandlerType + " catch=" + T(h.CatchType) + " try=" + (h.TryStart == null ? -1 : h.TryStart.Offset) + "-" + (h.TryEnd == null ? -1 : h.TryEnd.Offset));
                }
            }
        }
        if (t.HasNestedTypes)
        {
            foreach (TypeDefinition n in t.NestedTypes) DumpTypeDetails(n, sb, false);
        }
    }

    static void Main(string[] args) { StringBuilder sb = new StringBuilder(); try {
        try { Console.OutputEncoding = Encoding.UTF8; } catch (Exception) { }
        sb.AppendLine("### TARGET " + args[0]);
        AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(args[0]);
        ModuleDefinition mod = asm.MainModule;
        sb.AppendLine("asm=" + asm.Name.FullName);
        sb.AppendLine("module=" + mod.Name + " attrs=" + mod.Attributes + " types=" + mod.Types.Count + " resources=" + mod.Resources.Count);
        foreach (Resource r in mod.Resources) sb.AppendLine("RES " + r.Name + " type=" + r.ResourceType + " attrs=" + r.Attributes);
        foreach (AssemblyNameReference a in mod.AssemblyReferences) sb.AppendLine("AREF " + a.FullName);
        foreach (ModuleReference mr in mod.ModuleReferences) sb.AppendLine("MREF " + mr.Name);
        TypeDefinition target = null;
        foreach (TypeDefinition t in mod.Types) if (t.FullName == "WeComConsoleServer.Core.WxWebSocketClient") target = t;
        if (target == null) sb.AppendLine("TARGET TYPE NOT FOUND");
        else DumpTypeDetails(target, sb, true);

        sb.AppendLine();
        sb.AppendLine("### SHIM " + args[1]);
        AssemblyDefinition shim = AssemblyDefinition.ReadAssembly(args[1]);
        ModuleDefinition smod = shim.MainModule;
        sb.AppendLine("asm=" + shim.Name.FullName);
        sb.AppendLine("module=" + smod.Name + " attrs=" + smod.Attributes + " types=" + smod.Types.Count + " resources=" + smod.Resources.Count);
        foreach (Resource r in smod.Resources) sb.AppendLine("RES " + r.Name + " type=" + r.ResourceType);
        foreach (AssemblyNameReference a in smod.AssemblyReferences) sb.AppendLine("AREF " + a.FullName);
        sb.AppendLine("-- top level types --");
        foreach (TypeDefinition t in smod.Types)
        {
            sb.AppendLine("TOP " + t.FullName + " attrs=" + t.Attributes + " m=" + t.Methods.Count + " f=" + t.Fields.Count + " n=" + t.NestedTypes.Count);
        }
        TypeDefinition shimType = null;
        foreach (TypeDefinition t in smod.Types) if (t.FullName == "OneBotShim.WxSocketClient") shimType = t;
        if (shimType == null) sb.AppendLine("SHIM TYPE NOT FOUND");
        else DumpTypeDetails(shimType, sb, true);

        sb.AppendLine("-- shim distinct TypeRefs --");
        Dictionary<string, string> trs = new Dictionary<string, string>();
        foreach (TypeReference t in smod.GetTypeReferences())
        {
            string k = t.FullName + " || scope=" + (t.Scope == null ? "<null>" : t.Scope.Name) + " type=" + (t.Scope == null ? "?" : t.Scope.GetType().Name);
            if (!trs.ContainsKey(k)) trs[k] = k;
        }
        List<string> ks = new List<string>(trs.Keys);
        ks.Sort();
        foreach (string k in ks) sb.AppendLine("TR " + k);
        sb.AppendLine("-- shim distinct MemberRef declaring types --");
        Dictionary<string, string> mrs = new Dictionary<string, string>();
        foreach (MemberReference mr in smod.GetMemberReferences())
        {
            string k = (mr.DeclaringType == null ? "<null>" : mr.DeclaringType.FullName) + " :: " + mr.Name;
            if (!mrs.ContainsKey(k)) mrs[k] = k;
        }
        List<string> mks = new List<string>(mrs.Keys);
        mks.Sort();
        foreach (string k in mks) sb.AppendLine("MR " + k);
        sb.AppendLine("-- shim MethodSpecs --");
        foreach (MethodReference ms in smod.GetMemberReferences())
        {
            GenericInstanceMethod gim = ms as GenericInstanceMethod;
            if (gim != null) sb.AppendLine("GIM " + gim.FullName);
        }

        } catch (Exception ex) { sb.AppendLine("FATAL " + ex.GetType().FullName + " / " + ex.Message); }
        Console.WriteLine(sb.ToString());
    }
}

