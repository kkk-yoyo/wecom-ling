using System;
using System.Collections.Generic;
using Mono.Cecil;
class R {
  static void Main(string[] a) {
    var asm = AssemblyDefinition.ReadAssembly(a[0]);
    var m = asm.MainModule;
    Console.WriteLine("mvid=" + m.Mvid);
    foreach (var r in m.Resources) {
      var er = r as EmbeddedResource;
      long len = -1;
      if (er != null) { var s = er.GetResourceStream(); len = s.Length; }
      Console.WriteLine("RES " + r.Name + " attrs=" + r.Attributes + " len=" + len);
    }
    Console.WriteLine("win32res=" + (m.Win32Resources == null ? "null" : ("count=" + m.Win32Resources.Resources.Count)));
    Console.WriteLine("attrs module=" + m.Attributes + " arch=" + m.Architecture);
    Console.WriteLine("entrypoint=" + (asm.EntryPoint == null ? "null" : asm.EntryPoint.FullName));
  }
}
