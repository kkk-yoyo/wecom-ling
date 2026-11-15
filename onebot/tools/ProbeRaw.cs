using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

internal static class ProbeRaw
{
    private static List<long[]> _sections = new List<long[]>();
    private static long _sizeOfHeaders;
    private static string _path;
    private static StringBuilder _log = new StringBuilder();

    private static void Log(string s)
    {
        Console.WriteLine(s);
        _log.AppendLine(s);
    }

    private static void Main(string[] args)
    {
        string path = args.Length > 0 ? args[0] : @"D:\wecom-ling\微灵1.11.0_免卡密.exe";
        _path = path;
        try
        {
            Run();
        }
        catch (Exception ex)
        {
            Log("EXCEPTION " + ex.GetType().FullName);
            try { Log("MESSAGE " + ex.Message); } catch { Log("MESSAGE <unprintable>"); }
            try { Log("STACK " + ex.StackTrace); } catch { Log("STACK <unprintable>"); }
            try
            {
                Exception inner = ex.InnerException;
                while (inner != null)
                {
                    Log("INNER " + inner.GetType().FullName + " : " + inner.Message);
                    inner = inner.InnerException;
                }
            }
            catch { }
        }
        finally
        {
            try { File.WriteAllText(Path.Combine(Path.GetDirectoryName(_path), "probe_raw.log"), _log.ToString(), Encoding.UTF8); } catch { }
        }
    }

    private static void Run()
    {
        Log("file = " + _path);
        ParsePe(_path);
        ModuleDefMD mod = ModuleDefMD.Load(_path);
        Log("types(top)=" + mod.Types.Count + " asm=" + mod.Assembly.FullName);

        int nullInstrs = 0;
        List<string> methodsWithNull = new List<string>();
        List<TypeDef> types = new List<TypeDef>(mod.GetTypes());
        Log("types(all)=" + types.Count);
        for (int ti = 0; ti < types.Count; ti++)
        {
            TypeDef t = types[ti];
            List<MethodDef> methods;
            try { methods = new List<MethodDef>(t.Methods); }
            catch (Exception ex) { Log("ERR type " + SafeName(t) + ": " + ex.GetType().Name + " " + ex.Message); continue; }
            for (int i = 0; i < methods.Count; i++)
            {
                MethodDef m = methods[i];
                try
                {
                    if (!m.HasBody || m.Body == null) continue;
                    int n = 0;
                    for (int j = 0; j < m.Body.Instructions.Count; j++)
                    {
                        Instruction instr = m.Body.Instructions[j];
                        if (instr.Operand == null && instr.OpCode.OperandType != OperandType.InlineNone) n++;
                    }
                    if (n > 0)
                    {
                        nullInstrs += n;
                        methodsWithNull.Add(m.FullName + "  nullOp=" + n + "  RVA=0x" + ((uint)m.RVA).ToString("X8"));
                    }
                }
                catch (Exception ex)
                {
                    Log("ERR body " + SafeName(t) + "::" + ((object)m.Name) + " " + ex.GetType().Name + " " + ex.Message);
                }
            }
        }
        Log("nullOperandInstructions=" + nullInstrs + " in " + methodsWithNull.Count + " methods");
        for (int i = 0; i < methodsWithNull.Count && i < 40; i++) Log("  " + methodsWithNull[i]);

        TypeDef global = mod.GlobalType;
        Log("GlobalType=" + (global == null ? "<null>" : SafeName(global)) +
            " fields=" + (global == null ? 0 : global.Fields.Count) +
            " methods=" + (global == null ? 0 : global.Methods.Count));
        if (global != null)
        {
            for (int i = 0; i < global.Methods.Count; i++)
            {
                MethodDef m = global.Methods[i];
                try
                {
                    Log("METHOD " + m.FullName + " RVA=0x" + ((uint)m.RVA).ToString("X8") + " attrs=" + m.Attributes +
                        " impl=" + m.ImplAttributes + " body=" + (m.Body == null ? "null" : m.Body.Instructions.Count + " instrs"));
                    if (m.Body == null) continue;
                    for (int j = 0; j < m.Body.Instructions.Count; j++)
                    {
                        Instruction instr = m.Body.Instructions[j];
                        string opnd = instr.Operand == null ? "<null>" : instr.Operand.ToString();
                        string raw = "";
                        if (instr.Operand == null && instr.OpCode.OperandType != OperandType.InlineNone)
                            raw = " rawToken=" + ReadRawToken((uint)m.RVA, instr);
                        Log("  IL_" + instr.Offset.ToString("X4") + " " + instr.OpCode.Name + " operand=" + opnd + raw);
                    }
                }
                catch (Exception ex)
                {
                    Log("ERR global method " + SafeName(global) + "::" + ((object)m.Name) + " " + ex.GetType().Name + " " + ex.Message);
                }
            }
        }
    }

    private static string SafeName(TypeDef t)
    {
        try { return t.FullName; } catch { return "<bad type>"; }
    }

    private static void ParsePe(string path)
    {
        using (BinaryReader br = new BinaryReader(File.OpenRead(path)))
        {
            br.BaseStream.Position = 0x3C;
            int lfanew = br.ReadInt32();
            br.BaseStream.Position = lfanew;
            br.ReadUInt32();
            br.ReadUInt16();
            int nsec = br.ReadUInt16();
            br.ReadUInt32(); br.ReadUInt32(); br.ReadUInt32();
            int optSize = br.ReadUInt16();
            br.ReadUInt16();
            long optStart = br.BaseStream.Position;
            br.BaseStream.Position = optStart + 0x3C;
            _sizeOfHeaders = br.ReadUInt32();
            br.BaseStream.Position = optStart + optSize;
            _sections = new List<long[]>();
            for (int i = 0; i < nsec; i++)
            {
                byte[] name = br.ReadBytes(8);
                uint vsize = br.ReadUInt32();
                uint vaddr = br.ReadUInt32();
                uint rawsize = br.ReadUInt32();
                uint rawptr = br.ReadUInt32();
                br.ReadBytes(16);
                _sections.Add(new long[] { vaddr, rawptr, rawsize, vsize });
                Log("section " + Encoding.ASCII.GetString(name).TrimEnd('\0') +
                    " vaddr=0x" + vaddr.ToString("X8") + " vsize=" + vsize +
                    " rawptr=0x" + rawptr.ToString("X8") + " rawsize=" + rawsize);
            }
        }
    }

    private static long RvaToOffset(uint rva)
    {
        if (rva < _sizeOfHeaders) return rva;
        for (int i = 0; i < _sections.Count; i++)
        {
            long[] s = _sections[i];
            long end = s[0] + Math.Max(s[2], s[3]);
            if (rva >= s[0] && rva < end) return s[1] + (rva - s[0]);
        }
        return -1;
    }

    private static string ReadRawToken(uint methodRva, Instruction instr)
    {
        try
        {
            long bodyOff = RvaToOffset(methodRva);
            if (bodyOff < 0) return "RVA->off failed";
            using (FileStream fs = File.OpenRead(_path))
            using (BinaryReader br = new BinaryReader(fs))
            {
                br.BaseStream.Position = bodyOff;
                byte b = br.ReadByte();
                long codeStart;
                if ((b & 3) == 2) codeStart = bodyOff + 1;
                else if ((b & 3) == 3) codeStart = bodyOff + 12;
                else return "bad header 0x" + b.ToString("X2");
                long pos = codeStart + instr.Offset;
                br.BaseStream.Position = pos;
                byte op = br.ReadByte();
                int opLen = 1;
                if (op == 0xFE) { br.ReadByte(); opLen = 2; }
                br.BaseStream.Position = pos + opLen;
                uint token = br.ReadUInt32();
                return "0x" + token.ToString("X8");
            }
        }
        catch (Exception ex)
        {
            return "err " + ex.GetType().Name + " " + ex.Message;
        }
    }
}
