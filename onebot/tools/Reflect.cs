using System;
using System.Reflection;
internal static class R
{
    private static void Main()
    {
        Type t = typeof(dnlib.PE.RVA);
        Console.WriteLine(t.FullName);
        MemberInfo[] ms = t.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
        foreach (MemberInfo m in ms) Console.WriteLine(m.MemberType + "  " + m);
        Console.WriteLine("--- Instruction ---");
        Type it = typeof(dnlib.DotNet.Emit.Instruction);
        foreach (MemberInfo m in it.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)) Console.WriteLine(m.MemberType + "  " + m);
        Console.WriteLine("--- ExceptionHandler ---");
        Type eh = typeof(dnlib.DotNet.Emit.ExceptionHandler);
        foreach (MemberInfo m in eh.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)) Console.WriteLine(m.MemberType + "  " + m);
    }
}
