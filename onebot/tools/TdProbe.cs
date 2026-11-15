using System;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.MD;

internal static class TdProbe
{
    private static int Main(string[] args)
    {
        try
        {
            ModuleDefMD m = ModuleDefMD.Load(args[0], new ModuleContext());
            TypeDefTable tt = m.TablesStream.TypeDefTable;
            Console.WriteLine("rows=" + tt.Rows + " methodRows=" + m.TablesStream.MethodTable.Rows + " fieldRows=" + m.TablesStream.FieldTable.Rows);
            uint[] rids = new uint[] { 1, 106, 107, 108, 109, 110, 111, 536, 537 };
            for (int i = 0; i < rids.Length; i++)
            {
                uint rid = rids[i];
                if (rid > tt.Rows) continue;
                TypeDefRow row;
                if (!tt.TryRead(rid, out row)) { Console.WriteLine("rid " + rid + " read fail"); continue; }
                Console.WriteLine("TypeDef rid=" + rid + " methodList=" + row.MethodList + " fieldList=" + row.FieldList + " nameOff=" + row.Name);
            }
            return 0;
        }
        catch (Exception ex) { Console.WriteLine("FAILED " + ex.GetType().Name + ": " + ex.Message); return 2; }
    }
}