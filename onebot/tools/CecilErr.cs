using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Mono.Cecil;

class CecilErr
{
    static void Dump(Exception ex, string tag)
    {
        Console.WriteLine("EX[" + tag + "] type=" + ex.GetType().FullName);
        try { Console.WriteLine("  msg=" + ex.Message); } catch (Exception e2) { Console.WriteLine("  msg-fail " + e2.GetType().FullName); }
        try { Console.WriteLine("  stack=" + ex.StackTrace); } catch (Exception e3) { Console.WriteLine("  stack-fail"); }
        Exception inner = ex.InnerException;
        int depth = 0;
        while (inner != null && depth < 5)
        {
            Console.WriteLine("  inner type=" + inner.GetType().FullName);
            try { Console.WriteLine("  inner msg=" + inner.Message); } catch (Exception) { Console.WriteLine("  inner msg-fail"); }
            try { Console.WriteLine("  inner stack=" + inner.StackTrace); } catch (Exception) { Console.WriteLine("  inner stack-fail"); }
            inner = inner.InnerException;
            depth++;
        }
    }

    static void TryRT(string src, string dst)
    {
        Console.WriteLine("== RT " + src + " -> " + dst + " ==");
        try
        {
            AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(src);
            Console.WriteLine("read ok types=" + asm.MainModule.Types.Count);
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(dst)));
            asm.MainModule.Write(dst);
            Console.WriteLine("write ok len=" + new FileInfo(dst).Length);
        }
        catch (Exception ex) { Dump(ex, src); }
    }

    static void Main(string[] args)
    {
        TryRT(args[0], args[1]);
        if (args.Length > 2) TryRT(args[2], args[3]);
    }
}
