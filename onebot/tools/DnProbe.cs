using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

class DnProbe
{
    static void P(string s) { Console.WriteLine(s); }

    static void Ctors(Type t)
    {
        P("-- ctors " + t.FullName);
        foreach (ConstructorInfo ci in t.GetConstructors())
            P("   " + ci);
    }

    static void Props(Type t, string filter)
    {
        P("-- props " + t.FullName + (filter == null ? "" : " [" + filter + "]"));
        foreach (PropertyInfo pi in t.GetProperties())
            if (filter == null || pi.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                P("   " + pi.PropertyType.FullName + " " + pi.Name + (pi.CanWrite ? " {get;set;}" : " {get;}"));
    }

    static void Mems(Type t, string filter)
    {
        P("-- methods " + t.FullName + " [" + filter + "]");
        foreach (MethodInfo mi in t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            if (filter == null || mi.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                P("   " + mi);
    }

    static void Fields(Type t)
    {
        P("-- fields " + t.FullName);
        foreach (FieldInfo fi in t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            P("   " + fi.FieldType.FullName + " " + fi.Name);
    }

    static void Main(string[] args)
    {
        Assembly asm = typeof(ModuleDefMD).Assembly;
        P("dnlib asm: " + asm.FullName);
        P("dnlib loc: " + asm.Location);

        P("== ModuleDefMD static loads ==");
        foreach (MethodInfo mi in typeof(ModuleDefMD).GetMethods(BindingFlags.Public | BindingFlags.Static))
            if (mi.Name == "Load") P("   " + mi);
        P("== ModuleDefMD instance Find/GetType ==");
        foreach (MethodInfo mi in typeof(ModuleDefMD).GetMethods(BindingFlags.Public | BindingFlags.Instance))
            if (mi.Name.IndexOf("Find", StringComparison.OrdinalIgnoreCase) >= 0 || mi.Name.IndexOf("GetType", StringComparison.OrdinalIgnoreCase) >= 0 || mi.Name.IndexOf("GetTypes", StringComparison.OrdinalIgnoreCase) >= 0)
                P("   " + mi);
        Props(typeof(ModuleDef), "Type");
        Props(typeof(TypeDef), null);
        Props(typeof(MethodDef), null);
        Props(typeof(FieldDef), null);
        Props(typeof(MemberRef), null);
        Props(typeof(TypeRef), null);
        Ctors(typeof(TypeRefUser));
        Ctors(typeof(MemberRefUser));
        Ctors(typeof(TypeSpecUser));
        Ctors(typeof(MethodDefUser));
        Ctors(typeof(FieldDefUser));
        Ctors(typeof(GenericInstMethodSig));
        Ctors(typeof(MethodSpec));
        Props(typeof(MethodSpec), null);
        Ctors(typeof(MethodSig));
        Mems(typeof(MethodSig), "Create");
        P("MethodSig CreateInstance variants:");
        foreach (MethodInfo mi in typeof(MethodSig).GetMethods()) if (mi.Name == "CreateInstance" || mi.Name == "CreateStatic") P("   " + mi);
        Ctors(typeof(GenericInstSig));
        Ctors(typeof(SZArraySig));
        Ctors(typeof(ArraySig));
        Ctors(typeof(ByRefSig));
        Ctors(typeof(PtrSig));
        Ctors(typeof(CModReqdSig));
        Ctors(typeof(CModOptSig));
        Ctors(typeof(PinnedSig));
        Ctors(typeof(GenericVar));
        Ctors(typeof(GenericMVar));
        P("== Importer ==");
        foreach (ConstructorInfo ci in typeof(Importer).GetConstructors()) P("   " + ci);
        foreach (MethodInfo mi in typeof(Importer).GetMethods(BindingFlags.Public | BindingFlags.Instance))
            if (mi.Name == "Import") P("   " + mi);
        P("== CilBody ==");
        Ctors(typeof(CilBody));
        Props(typeof(CilBody), null);
        P("== Instruction ==");
        foreach (MethodInfo mi in typeof(Instruction).GetMethods(BindingFlags.Public | BindingFlags.Static))
            if (mi.Name == "Create") P("   " + mi);
        Props(typeof(Instruction), null);
        P("== Local ==");
        Ctors(typeof(Local));
        Props(typeof(Local), null);
        P("== ExceptionHandler ==");
        Ctors(typeof(ExceptionHandler));
        Props(typeof(ExceptionHandler), null);
        P("== Parameter ==");
        Ctors(typeof(Parameter));
        Props(typeof(Parameter), null);
        P("== MethodDef params ==");
        Props(typeof(MethodDef), "Param");
        P("== Constant ==");
        Ctors(typeof(Constant));
        Props(typeof(Constant), null);
        Ctors(typeof(FieldDef));
        P("== extensions GetTypes ==");
        foreach (Type t in asm.GetTypes())
        {
            if (!t.IsAbstract || !t.IsSealed) continue;
            foreach (MethodInfo mi in t.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (mi.Name == "GetTypes" && mi.GetParameters().Length == 1 && mi.GetParameters()[0].ParameterType == typeof(ModuleDef))
                    P("   " + t.FullName + "." + mi);
            }
        }
        P("== ModuleDefMD.GetTypes? / GetType ==");
    }
}
