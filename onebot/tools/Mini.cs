using System;
class Mini {
  static void Main(string[] args) {
    try {
      var asm = Mono.Cecil.AssemblyDefinition.ReadAssembly(args[0]);
      Console.WriteLine("asm=" + asm.Name.Name);
    } catch (Exception ex) { Console.WriteLine("ERR " + ex.GetType().FullName); }
  }
}