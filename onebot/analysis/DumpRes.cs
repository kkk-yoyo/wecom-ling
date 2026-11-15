using System;
using System.IO;
using System.Reflection;
using Mono.Cecil;

internal static class DumpRes
{
    private static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("usage: DumpRes <assembly> <outDir>");
            return;
        }
        string outDir = args[1];
        Directory.CreateDirectory(outDir);
        AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(args[0]);
        Console.WriteLine("module: " + asm.MainModule.Name);
        foreach (Resource r in asm.MainModule.Resources)
        {
            Console.WriteLine("res: " + r.Name + " type=" + r.ResourceType);
            EmbeddedResource er = r as EmbeddedResource;
            if (er == null)
            {
                continue;
            }
            try
            {
                using (Stream s = er.GetResourceStream())
                {
                    string safe = r.Name;
                    foreach (char c in Path.GetInvalidFileNameChars())
                    {
                        safe = safe.Replace(c, '_');
                    }
                    string path = Path.Combine(outDir, safe + ".bin");
                    using (FileStream fs = File.Create(path))
                    {
                        s.CopyTo(fs);
                    }
                    Console.WriteLine("  saved " + path + " size=" + new FileInfo(path).Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("  error: " + ex.GetType().Name + ": " + ex.Message);
            }
        }
    }
}
