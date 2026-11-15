using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Mono.Cecil;

class RoundTrip
{
    static void Main(string[] args)
    {
        string src = args[0];
        string dst = args[1];
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(dst)));

        Console.WriteLine("== API probe ==");
        Console.WriteLine("MethodBody ctors:");
        foreach (ConstructorInfo ci in typeof(MethodBody).GetConstructors())
            Console.WriteLine("  " + ci);
        Console.WriteLine("ModuleDefinition resource/image props:");
        foreach (PropertyInfo pi in typeof(ModuleDefinition).GetProperties())
            if (pi.Name.IndexOf("Resource", StringComparison.OrdinalIgnoreCase) >= 0 ||
                pi.Name.IndexOf("Image", StringComparison.OrdinalIgnoreCase) >= 0 ||
                pi.Name.IndexOf("Mvid", StringComparison.OrdinalIgnoreCase) >= 0)
                Console.WriteLine("  " + pi.PropertyType.Name + " " + pi.Name);
        Console.WriteLine("ParameterDefinition props:");
        foreach (PropertyInfo pi in typeof(ParameterDefinition).GetProperties())
            Console.WriteLine("  " + pi.PropertyType.Name + " " + pi.Name);
        Console.WriteLine("ParameterDefinition ctors:");
        foreach (ConstructorInfo ci in typeof(ParameterDefinition).GetConstructors())
            Console.WriteLine("  " + ci);
        Console.WriteLine("TypeDefinition.Fields type: " + typeof(TypeDefinition).GetProperty("Fields").PropertyType);
        Console.WriteLine("Instruction props:");
        foreach (PropertyInfo pi in typeof(Mono.Cecil.Cil.Instruction).GetProperties())
            Console.WriteLine("  " + pi.PropertyType.Name + " " + pi.Name);

        Console.WriteLine("== roundtrip ==");
        AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(src);
        ModuleDefinition mod = asm.MainModule;
        Console.WriteLine("read types=" + mod.Types.Count + " res=" + mod.Resources.Count + " mvid=" + mod.Mvid);
        foreach (Resource r in mod.Resources)
        {
            EmbeddedResource er = r as EmbeddedResource;
            long len = -1;
            if (er != null) { using (Stream s = er.GetResourceStream()) { len = s.Length; } }
            Console.WriteLine("  res " + r.Name + " len=" + len);
        }
        mod.Write(dst);
        Console.WriteLine("wrote " + dst + " len=" + new FileInfo(dst).Length);
        AssemblyDefinition asm2 = AssemblyDefinition.ReadAssembly(dst);
        ModuleDefinition mod2 = asm2.MainModule;
        Console.WriteLine("reread types=" + mod2.Types.Count + " res=" + mod2.Resources.Count + " mvid=" + mod2.Mvid);
        foreach (Resource r in mod2.Resources)
        {
            EmbeddedResource er = r as EmbeddedResource;
            long len = -1;
            if (er != null) { using (Stream s = er.GetResourceStream()) { len = s.Length; } }
            Console.WriteLine("  res " + r.Name + " len=" + len);
        }
    }
}
