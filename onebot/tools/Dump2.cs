using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

class Dump2
{
    static void Main(string[] args)
    {
        StringBuilder sb = new StringBuilder();
        try
        {
            AssemblyDefinition tasm = AssemblyDefinition.ReadAssembly(args[0]);
            ModuleDefinition tmod = tasm.MainModule;
            string targetName = "WeComConsoleServer.Core.WxWebSocketClient";
            TypeDefinition target = null;
            foreach (TypeDefinition t in tmod.Types) if (t.FullName == targetName) target = t;
            sb.AppendLine("TARGET found=" + (target != null));
            if (target != null)
            {
                sb.AppendLine("-- interfaces --");
                foreach (InterfaceImplementation ii in target.Interfaces) sb.AppendLine("II " + ii.InterfaceType.FullName + " scope=" + (ii.InterfaceType.Scope == null ? "?" : ii.InterfaceType.Scope.Name));
                sb.AppendLine("-- nested types --");
                foreach (TypeDefinition n in target.NestedTypes) sb.AppendLine("NT " + n.FullName);
            }
            sb.AppendLine("-- required target types by fullname --");
            string[] names = new string[] {
                "Robot.Common.CommandExecResult",
                "WeComConsoleServer.Core.WxWsSendPriority",
                "Robot.Common.CommandExecResult",
            };
            for (int i = 0; i < names.Length; i++)
            {
                TypeDefinition ft = null;
                foreach (TypeDefinition t in tmod.Types) if (t.FullName == names[i]) ft = t;
                sb.AppendLine("FIND " + names[i] + " => " + (ft == null ? "MISSING" : ("found attrs=" + ft.Attributes)));
            }
            sb.AppendLine("-- external refs to target-type members (outside target type) --");
            int ext = 0;
            HashSet<string> extNames = new HashSet<string>();
            if (target != null)
            {
                foreach (TypeDefinition t in tmod.Types)
                {
                    if (t == target) continue;
                    List<TypeDefinition> all = new List<TypeDefinition>();
                    all.Add(t);
                    Collect(t, all);
                    foreach (TypeDefinition tt in all)
                    {
                        foreach (MethodDefinition m in tt.Methods)
                        {
                            if (!m.HasBody) continue;
                            foreach (Instruction ins in m.Body.Instructions)
                            {
                                MemberReference mr = ins.Operand as MemberReference;
                                if (mr == null) continue;
                                TypeReference dt = mr.DeclaringType;
                                if (dt != null && dt.FullName == targetName)
                                {
                                    ext++;
                                    string nm = mr.GetType().Name + " " + mr.Name;
                                    if (!extNames.Contains(nm)) { extNames.Add(nm); sb.AppendLine("EXT " + tt.FullName + "::" + m.Name + " -> " + nm); }
                                }
                            }
                        }
                    }
                }
            }
            sb.AppendLine("EXT total refs=" + ext);

            AssemblyDefinition sasm = AssemblyDefinition.ReadAssembly(args[1]);
            ModuleDefinition smod = sasm.MainModule;
            sb.AppendLine("-- shim PID type --");
            TypeDefinition pid = null;
            foreach (TypeDefinition t in smod.Types) if (t.FullName.StartsWith("<PrivateImplementationDetails>")) pid = t;
            sb.AppendLine("PID type=" + (pid == null ? "MISSING" : pid.FullName));
            if (pid != null)
            {
                foreach (FieldDefinition f in pid.Fields)
                {
                    sb.AppendLine("PIDF " + f.Name + " : " + f.FieldType.FullName + " attrs=" + f.Attributes + " hasRVA=" + f.HasFieldRVA + " initLen=" + (f.InitialValue == null ? -1 : f.InitialValue.Length));
                }
                TypeDefinition shimType = null;
                foreach (TypeDefinition t in smod.Types) if (t.FullName == "OneBotShim.WxSocketClient") shimType = t;
                if (shimType != null)
                {
                    foreach (MethodDefinition m in shimType.Methods)
                    {
                        if (!m.HasBody) continue;
                        foreach (Instruction ins in m.Body.Instructions)
                        {
                            FieldReference fr = ins.Operand as FieldReference;
                            if (fr == null) continue;
                            if (fr.DeclaringType != null && fr.DeclaringType.FullName == pid.FullName)
                            {
                                sb.AppendLine("PIDUSE " + m.Name + " " + ins.OpCode + " " + fr.Name);
                            }
                        }
                    }
                }
            }
            sb.AppendLine("-- shim MethodSpecs --");
            foreach (MethodDefinition m in smod.Types.Count > 0 ? GetShimMethods(smod) : new List<MethodDefinition>())
            {
                if (!m.HasBody) continue;
                foreach (Instruction ins in m.Body.Instructions)
                {
                    MethodReference mr = ins.Operand as MethodReference;
                    if (mr == null) continue;
                    GenericInstanceMethod gim = mr as GenericInstanceMethod;
                    if (gim != null)
                    {
                        StringBuilder g = new StringBuilder();
                        foreach (TypeReference ta in gim.GenericArguments) { if (g.Length > 0) g.Append(","); g.Append(ta.FullName); }
                        sb.AppendLine("GIM " + m.Name + " :: " + gim.DeclaringType.FullName + "::" + gim.Name + "<" + g + ">");
                    }
                }
            }
            sb.AppendLine("-- shim field types of StatusChanged --");
            TypeDefinition st = null;
            foreach (TypeDefinition t in smod.Types) if (t.FullName == "OneBotShim.WxSocketClient") st = t;
            if (st != null)
            {
                foreach (FieldDefinition f in st.Fields) if (f.Name == "StatusChanged") sb.AppendLine("SC " + f.FieldType.FullName + " attrs=" + f.Attributes);
                foreach (EventDefinition e in st.Events) sb.AppendLine("EV " + e.Name + " " + e.EventType.FullName);
            }
        }
        catch (Exception ex)
        {
            sb.AppendLine("FATAL " + ex.GetType().FullName + " / " + ex.Message + " @ " + (ex.StackTrace == null ? "" : ex.StackTrace));
        }
        Console.WriteLine(sb.ToString());
    }

    static List<MethodDefinition> GetShimMethods(ModuleDefinition smod)
    {
        List<MethodDefinition> r = new List<MethodDefinition>();
        foreach (TypeDefinition t in smod.Types)
        {
            if (t.FullName == "OneBotShim.WxSocketClient" || t.FullName == "<PrivateImplementationDetails>{71645B54-9410-4E88-A7A3-8362F69B36CE}")
            {
                foreach (MethodDefinition m in t.Methods) r.Add(m);
            }
        }
        return r;
    }

    static void Collect(TypeDefinition t, List<TypeDefinition> all)
    {
        foreach (TypeDefinition n in t.NestedTypes) { all.Add(n); Collect(n, all); }
    }
}
