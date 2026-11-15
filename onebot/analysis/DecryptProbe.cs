using System;
using System.IO;
using System.Reflection;
using System.Text;

internal static class DecryptProbe
{
    private static readonly StringBuilder Sb = new StringBuilder();

    private static void W(string s)
    {
        Sb.AppendLine(s);
        Console.Out.Flush();
    }

    private static string Describe(Exception ex)
    {
        StringBuilder b = new StringBuilder();
        try
        {
            b.Append(ex.GetType().FullName);
            b.Append(": ");
            b.Append(ex.Message);
            if (ex.InnerException != null)
            {
                b.Append(" | inner: ");
                b.Append(ex.InnerException.GetType().FullName);
                b.Append(": ");
                b.Append(ex.InnerException.Message);
            }
        }
        catch (Exception)
        {
            b.Append("<describe failed>");
        }
        return b.ToString();
    }

    private static void Main(string[] args)
    {
        string outPath = Path.Combine(Path.GetDirectoryName(typeof(DecryptProbe).Assembly.Location), "decrypt.out.txt");
        try
        {
            Console.OutputEncoding = Encoding.UTF8;
        }
        catch (Exception)
        {
        }
        try
        {
            W("args=" + args.Length);
            if (args.Length < 2)
            {
                W("usage: DecryptProbe <exe> <index> [index...]");
                return;
            }
            W("loading: " + args[0]);
            Assembly asm;
            try
            {
                asm = Assembly.LoadFrom(args[0]);
            }
            catch (Exception ex)
            {
                W("LoadFrom failed: " + Describe(ex));
                try
                {
                    asm = Assembly.Load(File.ReadAllBytes(args[0]));
                    W("byte-load ok");
                }
                catch (Exception ex2)
                {
                    W("byte-load failed: " + Describe(ex2));
                    return;
                }
            }
            W("loaded: " + asm.FullName);
            Type helper = null;
            try
            {
                helper = asm.GetType("SG9KiyIbtdgGDf12qr.z2jc63fLkugS1X8Q9N");
            }
            catch (Exception ex)
            {
                W("GetType failed: " + Describe(ex));
            }
            if (helper == null)
            {
                W("helper type not found");
                return;
            }
            W("helper: " + helper.FullName);
            MethodInfo target = null;
            foreach (MethodInfo mi in helper.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (mi.Name == "tE1kD1vbB")
                {
                    ParameterInfo[] ps = mi.GetParameters();
                    if (ps.Length == 1 && ps[0].ParameterType == typeof(int))
                    {
                        target = mi;
                        break;
                    }
                }
            }
            if (target == null)
            {
                W("decrypt method not found; methods: ");
                foreach (MethodInfo mi in helper.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    W("  " + mi.Name);
                }
                return;
            }
            W("decrypt method: " + target.Name);
            for (int i = 1; i < args.Length; i++)
            {
                int idx;
                if (!int.TryParse(args[i], out idx))
                {
                    W(args[i] + " => <bad index>");
                    continue;
                }
                try
                {
                    object value = target.Invoke(null, new object[] { idx });
                    string s = value as string;
                    W(idx + " => [" + (s == null ? "<null>" : s) + "]");
                }
                catch (Exception ex)
                {
                    W(idx + " => <error: " + Describe(ex) + ">");
                }
            }
        }
        catch (Exception ex)
        {
            W("FATAL: " + Describe(ex));
        }
        finally
        {
            try
            {
                File.WriteAllText(outPath, Sb.ToString(), new UTF8Encoding(false));
            }
            catch (Exception)
            {
            }
        }
    }
}
