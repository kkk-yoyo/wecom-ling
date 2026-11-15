using System;
using System.Reflection;
internal static class R4
{
    private static void Main()
    {
        Type t = typeof(dnlib.DotNet.DummyLogger);
        foreach (MemberInfo m in t.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            Console.WriteLine("  " + (m is FieldInfo ? ((FieldInfo)m).IsStatic + " field " : "member ") + m);
    }
}
