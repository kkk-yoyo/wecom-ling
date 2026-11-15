using System;
using System.Reflection;
internal static class R5
{
    private static void Main()
    {
        Type t = typeof(dnlib.DotNet.Emit.CilBody).GetProperty("Instructions").PropertyType;
        foreach (MemberInfo m in t.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            Console.WriteLine(m.MemberType + " " + m);
        Type c = typeof(dnlib.DotNet.Emit.CilBody);
        foreach (MemberInfo m in c.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            Console.WriteLine("CilBody: " + m.MemberType + " " + m);
    }
}

