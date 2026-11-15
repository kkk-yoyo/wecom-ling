using System;
using System.Collections.Generic;
using System.Text;
using dnlib.DotNet;

internal static class RidProbe
{
    private static int Main(string[] args)
    {
        try
        {
            ModuleDefMD m = ModuleDefMD.Load(args[0], new ModuleContext());
            int typeCount = 0;
            List<TypeDef> all = new List<TypeDef>();
            Collect(m.Types, all, ref typeCount);
            Console.WriteLine("module=" + m.Assembly.FullName);
            Console.WriteLine("types(total)=" + all.Count + " methodRows=" + m.TablesStream.MethodTable.Rows +
                              " fieldRows=" + m.TablesStream.FieldTable.Rows +
                              " typeRows=" + m.TablesStream.TypeDefTable.Rows);
            TypeDef t = m.Find("WeComConsoleServer.Core.WxWebSocketClient", false);
            if (t == null) { Console.WriteLine("TARGET NULL"); return 1; }
            Console.WriteLine("target rid=" + t.Rid + " methods=" + t.Methods.Count + " fields=" + t.Fields.Count +
                              " nested=" + t.NestedTypes.Count + " props=" + t.Properties.Count + " events=" + t.Events.Count);
            if (t.Methods.Count > 0)
            {
                Console.WriteLine("  firstMethod=" + t.Methods[0].Name + " rid=" + t.Methods[0].Rid);
                Console.WriteLine("  lastMethod =" + t.Methods[t.Methods.Count - 1].Name + " rid=" + t.Methods[t.Methods.Count - 1].Rid);
            }
            if (t.Fields.Count > 0)
            {
                Console.WriteLine("  firstField=" + t.Fields[0].Name + " rid=" + t.Fields[0].Rid);
                Console.WriteLine("  lastField =" + t.Fields[t.Fields.Count - 1].Name + " rid=" + t.Fields[t.Fields.Count - 1].Rid);
            }
            // 打印 RID 相邻的 TypeDef
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].Rid == t.Rid - 1) Console.WriteLine("prevType rid=" + all[i].Rid + " " + all[i].FullName);
                if (all[i].Rid == t.Rid + 1) Console.WriteLine("nextType rid=" + all[i].Rid + " " + all[i].FullName + " methods=" + all[i].Methods.Count);
            }
            // 查找 ob_ 成员
            int obF = 0, obM = 0;
            for (int i = 0; i < t.Fields.Count; i++) if (t.Fields[i].Name.ToString().StartsWith("ob_")) obF++;
            for (int i = 0; i < t.Methods.Count; i++) if (t.Methods[i].Name.ToString().StartsWith("ob_")) obM++;
            Console.WriteLine("ob_fields=" + obF + " ob_methods=" + obM);
            Console.WriteLine("has ob_probeMethod=" + (t.FindMethod("ob_probeMethod") != null));
            // 全模块方法排序检查：每个 TypeDef 的 methods 是否在 RID 上连续
            int nonContig = 0;
            for (int i = 0; i < all.Count; i++)
            {
                IList<MethodDef> ms = all[i].Methods;
                for (int j = 1; j < ms.Count; j++)
                    if (ms[j].Rid != ms[j - 1].Rid + 1) { nonContig++; if (nonContig <= 5) Console.WriteLine("NONCONTIG type=" + all[i].FullName); break; }
            }
            Console.WriteLine("nonContiguousTypeMethodRuns=" + nonContig);
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("PROBE_FAILED: " + ex.GetType().FullName + ": " + ex.Message);
            return 2;
        }
    }

    private static void Collect(IList<TypeDef> types, List<TypeDef> list, ref int n)
    {
        for (int i = 0; i < types.Count; i++)
        {
            TypeDef t = types[i];
            list.Add(t);
            n++;
            Collect(t.NestedTypes, list, ref n);
        }
    }
}
