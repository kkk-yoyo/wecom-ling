using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

class BodyProbe
{
    static void Main(string[] args)
    {
        Console.WriteLine("== ReadingMode values ==");
        foreach (string n in Enum.GetNames(typeof(ReadingMode))) Console.WriteLine("  " + n + "=" + (int)Enum.Parse(typeof(ReadingMode), n));
        Console.WriteLine("== MethodBody nonpublic fields ==");
        foreach (FieldInfo fi in typeof(Mono.Cecil.Cil.MethodBody).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            Console.WriteLine("  " + fi.FieldType.Name + " " + fi.Name);
        Console.WriteLine("== MethodBody public props ==");
        foreach (PropertyInfo pi in typeof(Mono.Cecil.Cil.MethodBody).GetProperties())
            Console.WriteLine("  " + pi.PropertyType.Name + " " + pi.Name);

        AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(args[0]);
        ModuleDefinition mod = asm.MainModule;
        Console.WriteLine("module attrs=" + mod.Attributes + " kind? " + mod.Kind);
        int total = 0, bodyNull = 0, instrNull = 0, zeroInstr = 0;
        Dictionary<string, int> implHist = new Dictionary<string, int>();
        List<string> suspicious = new List<string>();
        foreach (TypeDefinition t in AllTypes(mod))
        {
            foreach (MethodDefinition m in t.Methods)
            {
                total++;
                Mono.Cecil.MethodImplAttributes ia = m.ImplAttributes;
                string key = ia.ToString();
                if (!implHist.ContainsKey(key)) implHist[key] = 0;
                implHist[key]++;
                Mono.Cecil.Cil.MethodBody b = m.Body;
                if (b == null) { bodyNull++; if (suspicious.Count < 80) suspicious.Add("NULLBODY " + t.FullName + "::" + m.Name + " impl=" + ia + " attrs=" + m.Attributes); continue; }
                if (b.Instructions == null) { instrNull++; if (suspicious.Count < 80) suspicious.Add("NULLINSTR " + t.FullName + "::" + m.Name + " impl=" + ia); continue; }
                if (b.Instructions.Count == 0) { zeroInstr++; if (suspicious.Count < 80) suspicious.Add("ZERO " + t.FullName + "::" + m.Name + " impl=" + ia + " attrs=" + m.Attributes + " eh=" + b.ExceptionHandlers.Count); }
            }
        }
        Console.WriteLine("methods=" + total + " bodyNull=" + bodyNull + " instrNull=" + instrNull + " zeroInstr=" + zeroInstr);
        Console.WriteLine("-- ImplAttributes histogram --");
        foreach (KeyValuePair<string, int> kv in implHist) Console.WriteLine("  " + kv.Key + " x" + kv.Value);
        Console.WriteLine("-- suspicious --");
        foreach (string s in suspicious) Console.WriteLine("  " + s);
    }

    static IEnumerable<TypeDefinition> AllTypes(ModuleDefinition mod)
    {
        foreach (TypeDefinition t in mod.Types) foreach (TypeDefinition x in Walk(t)) yield return x;
    }
    static IEnumerable<TypeDefinition> Walk(TypeDefinition t)
    {
        yield return t;
        foreach (TypeDefinition n in t.NestedTypes) foreach (TypeDefinition x in Walk(n)) yield return x;
    }
}
