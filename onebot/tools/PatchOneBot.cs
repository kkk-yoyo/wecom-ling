// PatchOneBot.cs — 使用 dnlib 4.5.0 把「微灵 1.11.0」主程序中的
// WeComConsoleServer.Core.WxWebSocketClient 类型实现整体替换为
// OneBotShim.WxSocketClient（OneBot v11 shim）的实现。
//
// 编译：
//   C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /nologo /out:PatchOneBot.exe ^
//     /r:C:\Windows\Microsoft.NET\Framework\v4.0.30319\netstandard.dll ^
//     /r:dnlib.dll PatchOneBot.cs
// 运行：PATCH 工具 exe 必须与 dnlib.dll 同目录（dnlib 4.5.0 net45）。
//
// 输入：D:\wecom-ling\微灵1.11.0_免卡密.exe      （只读，绝不修改）
//       D:\wecom-ling\onebot\output\OneBotShim.dll
// 输出：D:\wecom-ling\onebot\output\微灵-OneBotV11-1.11.0.exe
//
// 第二版策略（加法式保留；第一版"删除式"产物启动即崩溃，已废弃）：
//   1. **不删除任何原有 TypeDef / Field / Method / 属性 / 事件 / 嵌套类型**。
//      PendingSend、<>c__DisplayClass*、<...>d__* 状态机等全部原样保留（死代码也无妨）。
//      原因：PreserveAll writer 需要旧 TypeSpec/签名引用仍可解析；且原 exe 的混淆器依赖
//      原始元数据布局 —— 第一版删除成员后 CLR 启动即 FailFast：
//      "Infinite recursion during resource lookup within mscorlib ... Resource name: Arg_NullReferenceException"。
//      对照实验：原 exe 原样 dnlib round-trip（PreserveAll + NoThrow）可正常启动（smoke\rt4），
//      仅保留堆偏移 flags（0x7800）的 round-trip 仍崩溃；DnAddProbe 加法加字段/方法（smoke\rt6）也可启动。
//   2. 目标类型原有 10 个公开成员（.ctor/Start/Stop/TrySendEvent/Dispose/
//      add_StatusChanged/remove_StatusChanged/get_IsConnected/get_StatusText/
//      get_StatusBackground）保留 MethodDef，仅替换 Body（从 shim 克隆）。
//   3. shim 其余 87 个私有/内部方法 → 以 `ob_` 前缀新增为方法追加到目标类型；
//      shim 新字段（含 literal const / <PrivateImplementationDetails> 静态字段）→ 以 `ob_` 前缀追加；
//      `StatusChanged` 复用目标原字段，不新建。
//   4. 操作数重映射：类型、方法、字段、分支目标、switch 目标、局部变量、参数、异常处理器。
//   5. writer：MetadataFlags.PreserveAll + DummyLogger.NoThrowInstance（首版 0x7800 堆偏移组合
//      的产物启动崩溃，PreserveAll 是被 smoke 实验证明唯一可启动的 writer 路径）。
//   6. 保留 4 个 EmbeddedResource 与其它所有类型；程序集名/版本不变。
//
// 说明：shim 的 <PrivateImplementationDetails>{...} 不克隆整个类型，而是把其静态字段
// $$method0x600000f-1 作为 `ob_` 前缀 static 字段加到目标类型并映射引用
// （shim 方法体只通过 ldsfld/stsfld 引用该字段，不引用该类型本身）。

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;

internal static class PatchOneBot
{
    private const string OriginalExe = @"D:\wecom-ling\微灵1.11.0_免卡密.exe";
    private const string ShimDll = @"D:\wecom-ling\onebot\output\OneBotShim.dll";
    private const string OutputExe = @"D:\wecom-ling\onebot\output\微灵-OneBotV11-1.11.0.exe";
    private const string RootCopyExe = @"D:\wecom-ling\微灵-OneBotV11-1.11.0.exe";
    private const string OriginalSha256 = "4F2954A7149B318B4A69F7D407302EAED67A2911F27084C289DD35BF0F5BA24A";
    private const string TargetTypeFullName = "WeComConsoleServer.Core.WxWebSocketClient";
    private const string ShimTypeFullName = "OneBotShim.WxSocketClient";
    private const string TargetAssemblyName = "WeComSever";
    private const string ShimAssemblyName = "OneBotShim";

    private static ModuleDefMD _target;
    private static ModuleDefMD _shim;
    private static TypeDef _targetType;
    private static TypeDef _shimType;
    private static Importer _importer;

    private static Dictionary<string, TypeDef> _targetTypesByFullName;
    private static Dictionary<string, TypeDef> _shimTypesByFullName;
    private static Dictionary<string, TypeDef> _mappedTypesByFullName; // shim FullName -> target TypeDef

    private static Dictionary<TypeDef, TypeDef> _typeMap;      // shim TypeDef -> target TypeDef
    private static Dictionary<FieldDef, IField> _fieldMap;     // shim FieldDef -> target FieldDef
    private static Dictionary<MethodDef, MethodDef> _methodMap; // shim MethodDef -> target MethodDef
    private static HashSet<string> _preservedMethodNames;
    private const string NewMemberPrefix = "ob_";

    // HTTP 8060 起不来：MainWindow.InitializeServer 把 WebServer.BaseAddress 设成
    // http://0.0.0.0:{Port}/，Windows HttpListener/HttpSelfHost 不接受带 0.0.0.0 的前缀
    // （The request is not supported），OpenAsync 抛 AggregateException 后被静默吞掉。
    //
    // 注意：HttpSelfHostConfiguration 构造函数会把 baseAddress 交给 new Uri(...)，
    // 而 .NET Framework 4.8 的 Uri 拒绝 "+" 与 "*" 作为主机名
    // （UriFormatException: Invalid URI: The hostname could not be parsed.），
    // 因此不能用 http://+:port/ 方案（Lead 最初建议的 "+" 已验证不可行）。
    // 实测可用且 HttpListener 可绑定的地址是回环地址 http://127.0.0.1:{Port}/。
    private const string HttpBaseAddressOldHost = "0.0.0.0";
    private const string HttpBaseAddressNewHost = "127.0.0.1";
    private static bool _httpFixApplied;

    // 诊断开关：非空时在 StartWebServer() 入口/结尾把 BaseAddress 实际值写到该文件。
    // 最终交付产物必须为空（调试已结束）。
    private const string HttpDebugLogFile = "";
    private static HashSet<string> _deletedNestedFullNames; // 第一版遗留：仅 Preflight（已不再调用）使用

    // additive 策略下用于回读校验的原目标类型快照
    private static int _origTargetMethodCount;
    private static int _origTargetFieldCount;
    private static int _origTargetPropCount;
    private static int _origTargetEventCount;
    private static List<string> _origTargetMethodKeys;
    private static List<string> _origTargetFieldKeys;
    private static List<string> _origTargetNestedNames;
    private static Dictionary<string, int> _origTargetMethodInstrCounts;

    private static Dictionary<string, uint> _origResources;
    private static int _origAllTypeCount;
    private static Version _origAsmVersion;
    private static string _origAsmFullName;
    private static List<MethodDef> _preservedMethods;
    private static List<string> _origCommandHandlerMethodKeys;
    private static int _addedFieldCount;
    private static int _addedMethodCount;
    private static Dictionary<string, int> _expectedInstrCounts;   // target method name -> instruction count
    private static Dictionary<string, string> _expectedSigKeys;    // target method name -> sig key
    private static List<string> _warnings;
    private static int _fallbackMemberRefCount;

    private static int Main(string[] args)
    {
        try
        {
            Run();
            Console.WriteLine();
            Console.WriteLine("PATCH_OK");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("PATCH_FAILED: " + ex.Message);
            Console.WriteLine(ex.ToString());
            return 1;
        }
    }

    private static void Run()
    {
        Section("1. 校验原 exe（只读，不修改）");
        if (!File.Exists(OriginalExe)) throw new Exception("原 exe 不存在: " + OriginalExe);
        FileInfo fi = new FileInfo(OriginalExe);
        string sha = Sha256File(OriginalExe);
        Console.WriteLine("  path   = " + OriginalExe);
        Console.WriteLine("  size   = " + fi.Length);
        Console.WriteLine("  sha256 = " + sha);
        if (!string.Equals(sha, OriginalSha256, StringComparison.OrdinalIgnoreCase))
            throw new Exception("原 exe SHA256 与任务给定值不一致，拒绝继续。");
        Console.WriteLine("  OK 原 exe SHA256 与任务给定值一致");
        if (!File.Exists(ShimDll)) throw new Exception("shim 不存在: " + ShimDll);

        Section("2. 加载原 exe 与 shim（dnlib " + typeof(ModuleDefMD).Assembly.GetName().Version + "）");
        _target = ModuleDefMD.Load(OriginalExe, new ModuleContext());
        _shim = ModuleDefMD.Load(ShimDll, new ModuleContext());
        if (_target.Assembly == null) throw new Exception("原 exe 无程序集清单");
        if (_shim.Assembly == null) throw new Exception("shim 无程序集清单");
        if (!string.Equals(_target.Assembly.Name.ToString(), TargetAssemblyName, StringComparison.Ordinal))
            throw new Exception("原 exe 程序集名不是 " + TargetAssemblyName + ": " + _target.Assembly.Name);
        if (!string.Equals(_shim.Assembly.Name.ToString(), ShimAssemblyName, StringComparison.Ordinal))
            throw new Exception("shim 程序集名不是 " + ShimAssemblyName + ": " + _shim.Assembly.Name);
        _origAsmFullName = _target.Assembly.FullName;
        _origAsmVersion = _target.Assembly.Version;
        Console.WriteLine("  target = " + _origAsmFullName);
        Console.WriteLine("  shim   = " + _shim.Assembly.FullName + "  (" + new FileInfo(ShimDll).Length + " bytes)");
        Console.WriteLine("  target types(all)=" + CountTypes(_target.Types) + "  shim types(all)=" + CountTypes(_shim.Types));
        _origAllTypeCount = CountTypes(_target.Types);

        _origResources = new Dictionary<string, uint>(StringComparer.Ordinal);
        foreach (Resource r in _target.Resources)
        {
            EmbeddedResource er = r as EmbeddedResource;
            if (er == null) throw new Exception("原 exe 存在非 EmbeddedResource: " + r.Name);
            _origResources[r.Name.ToString()] = er.Length;
            Console.WriteLine("  resource " + r.Name + " len=" + er.Length);
        }
        if (_origResources.Count != 4)
            throw new Exception("原 exe 应有 4 个 EmbeddedResource，实际 " + _origResources.Count);

        _importer = new Importer(_target);
        _warnings = new List<string>();
        _preservedMethodNames = new HashSet<string>(StringComparer.Ordinal);
        string[] preserved = new string[]
        {
            ".ctor", "Start", "Stop", "TrySendEvent", "Dispose",
            "add_StatusChanged", "remove_StatusChanged",
            "get_IsConnected", "get_StatusText", "get_StatusBackground"
        };
        for (int i = 0; i < preserved.Length; i++) _preservedMethodNames.Add(preserved[i]);

        Section("3. 定位目标类型与 shim 类型");
        BuildTypeIndex();
        _targetType = FindType(_target, TargetTypeFullName);
        _shimType = FindType(_shim, ShimTypeFullName);
        _typeMap = new Dictionary<TypeDef, TypeDef>();
        _mappedTypesByFullName = new Dictionary<string, TypeDef>(StringComparer.Ordinal);
        _typeMap[_shimType] = _targetType;
        _mappedTypesByFullName[_shimType.FullName] = _targetType;
        ReportTypeInfo();

        // 记录原目标类型成员快照（additive 策略：这些成员必须全部保留到输出）
        _origTargetMethodCount = _targetType.Methods.Count;
        _origTargetFieldCount = _targetType.Fields.Count;
        _origTargetPropCount = _targetType.Properties.Count;
        _origTargetEventCount = _targetType.Events.Count;
        _origTargetNestedNames = new List<string>();
        for (int i = 0; i < _targetType.NestedTypes.Count; i++)
            _origTargetNestedNames.Add(_targetType.NestedTypes[i].FullName);
        _origTargetMethodKeys = new List<string>();
        _origTargetMethodInstrCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int i = 0; i < _targetType.Methods.Count; i++)
        {
            MethodDef om = _targetType.Methods[i];
            string ok = MethodKey(om.Name.ToString(), om.MethodSig);
            _origTargetMethodKeys.Add(ok);
            _origTargetMethodInstrCounts[ok] = BodyInstrCount(om);
        }
        _origTargetMethodKeys.Sort(StringComparer.Ordinal);
        _origTargetFieldKeys = new List<string>();
        for (int i = 0; i < _targetType.Fields.Count; i++)
            _origTargetFieldKeys.Add(_targetType.Fields[i].Name.ToString());
        _origTargetFieldKeys.Sort(StringComparer.Ordinal);
        Console.WriteLine("  原目标类型快照: methods=" + _origTargetMethodCount + " fields=" + _origTargetFieldCount +
                          " props=" + _origTargetPropCount + " events=" + _origTargetEventCount +
                          " nested=" + _origTargetNestedNames.Count);

        _origCommandHandlerMethodKeys = new List<string>();
        foreach (TypeDef nt in _targetType.NestedTypes)
        {
            if (nt.Name.ToString() != "CommandHandler") continue;
            for (int i = 0; i < nt.Methods.Count; i++)
                _origCommandHandlerMethodKeys.Add(MethodKey(nt.Methods[i].Name.ToString(), nt.Methods[i].MethodSig));
        }
        _origCommandHandlerMethodKeys.Sort(StringComparer.Ordinal);
        Console.WriteLine("  CommandHandler 原方法数=" + _origCommandHandlerMethodKeys.Count + "（.ctor/Invoke/BeginInvoke/EndInvoke + 混淆器 .cctor）");

        Section("4. 保留式策略：不删除任何原有 TypeDef/Field/Method/属性/事件/嵌套类型");
        Console.WriteLine("  旧私有成员/嵌套类型全部保留（成为死代码）；仅替换 10 个公开方法的 body。");
        Console.WriteLine("  shim 其余方法与字段以 " + NewMemberPrefix + " 前缀新增，避免与保留成员重名。");

        Section("5. 收集保留的 10 个公开方法（仅替换 body）");
        KeepOriginalMembers();

        Section("6. 按 shim 追加字段（StatusChanged 复用原字段，其余 " + NewMemberPrefix + " 前缀）");
        BuildFieldMap();

        Section("7. 按 shim 追加方法（10 保留 + 其余 " + NewMemberPrefix + " 新增）");
        BuildMethodMap();

        Section("8. 克隆方法体（两阶段：先指令，后操作数）");
        CloneAllBodies();

        // [已禁用] HTTP BaseAddress 修复：经现场验证，8060 起不来是安全策略限制运行目录所致（见 REPORT §八），
        // 与 BaseAddress 无关；原版 exe 从可信目录运行时 http://0.0.0.0:{Port}/ 监听正常，故不再注入该补丁。

        Section("9. 写出输出 exe");
        if (!Directory.Exists(Path.GetDirectoryName(OutputExe)))
            throw new Exception("输出目录不存在: " + Path.GetDirectoryName(OutputExe));
        ModuleWriterOptions writerOptions = new ModuleWriterOptions(_target);
        CollectingLogger collectLogger = new CollectingLogger();
        writerOptions.Logger = collectLogger;
        // 关键：混淆器依赖原始元数据布局/堆偏移/签名尾部数据（运行期解密与反调试）。
        // 实测：原 exe 原样 round-trip + PreserveAll + NoThrow 的 smoke\rt4 可正常启动；
        // 仅保留堆偏移 flags（PreserveStrings/US/Blob/ExtraSignatureData，0x7800）的 round-trip
        // 启动崩溃；DnAddProbe（加法加字段/方法）+PreserveAll 的 smoke\rt6 也可启动。
        // 因此必须使用 PreserveAll（含 PreserveRids/保留 token 的 writer 路径）。
        writerOptions.MetadataOptions.Flags = MetadataFlags.PreserveAll;
        string tmpPath = OutputExe + ".tmp";
        if (File.Exists(tmpPath)) File.Delete(tmpPath);
        _target.Write(tmpPath, writerOptions);
        if (File.Exists(OutputExe)) File.Delete(OutputExe);
        File.Move(tmpPath, OutputExe);
        FileInfo ofi = new FileInfo(OutputExe);
        Console.WriteLine("  written = " + OutputExe);
        Console.WriteLine("  size    = " + ofi.Length + " bytes");
        Console.WriteLine("  sha256  = " + Sha256File(OutputExe));
        // root copy 放在校验之后执行：若该 exe 正在运行会被锁住，
        // 不应因此让整个 patch 失败（校验通过即视为成功，root copy 失败只记 WARNING）。
        Console.WriteLine("  writer  = 非致命事件: errors=" + collectLogger.ErrorCount +
                          " warnings=" + collectLogger.WarningCount + "（NoThrow 语义：写出未中止）");
        for (int i = 0; i < collectLogger.Messages.Count; i++)
            Console.WriteLine("  [dnlib] " + collectLogger.Messages[i]);

        Section("10. 回读校验输出 exe");
        VerifyOutput();

        Section("11. 再次校验原 exe 未被修改");
        string sha2 = Sha256File(OriginalExe);
        Console.WriteLine("  sha256 = " + sha2);
        if (!string.Equals(sha2, OriginalSha256, StringComparison.OrdinalIgnoreCase))
            throw new Exception("原 exe 在运行过程中被改动了！");
        Console.WriteLine("  OK 原 exe 未修改");

        Section("12. 复制产物到根目录 " + RootCopyExe);
        try
        {
            if (File.Exists(RootCopyExe)) File.Delete(RootCopyExe);
            File.Copy(OutputExe, RootCopyExe);
            Console.WriteLine("  root copy = " + RootCopyExe + " (" + new FileInfo(RootCopyExe).Length + " bytes)");
            Console.WriteLine("  OK 根目录副本已更新");
        }
        catch (Exception ex)
        {
            string msg = "root copy 失败（目标 exe 可能正在运行而被锁定）: " + ex.GetType().Name + ": " + ex.Message;
            _warnings.Add(msg);
            Console.WriteLine("  WARNING " + msg);
        }

        if (_warnings.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("WARNINGS (" + _warnings.Count + "):");
            for (int i = 0; i < _warnings.Count; i++) Console.WriteLine("  - " + _warnings[i]);
        }
    }

    // ------------------------------------------------------------------ index

    private static void BuildTypeIndex()
    {
        _targetTypesByFullName = new Dictionary<string, TypeDef>(StringComparer.Ordinal);
        _shimTypesByFullName = new Dictionary<string, TypeDef>(StringComparer.Ordinal);
        AddTypesRecursive(_target.Types, _targetTypesByFullName);
        AddTypesRecursive(_shim.Types, _shimTypesByFullName);
        Console.WriteLine("  target typeDef index=" + _targetTypesByFullName.Count +
                          "  shim typeDef index=" + _shimTypesByFullName.Count);
    }

    private static void AddTypesRecursive(IList<TypeDef> types, Dictionary<string, TypeDef> dict)
    {
        if (types == null) return;
        for (int i = 0; i < types.Count; i++)
        {
            TypeDef t = types[i];
            if (!dict.ContainsKey(t.FullName)) dict.Add(t.FullName, t);
            AddTypesRecursive(t.NestedTypes, dict);
        }
    }

    private static TypeDef FindType(ModuleDefMD module, string fullName)
    {
        TypeDef t = module.Find(fullName, false);
        if (t == null)
        {
            // 兜底：手动递归（Find 对某些混淆名可能不工作）
            Dictionary<string, TypeDef> idx = new Dictionary<string, TypeDef>(StringComparer.Ordinal);
            AddTypesRecursive(module.Types, idx);
            if (idx.TryGetValue(fullName, out t)) return t;
        }
        if (t == null) throw new Exception("找不到类型: " + fullName);
        return t;
    }

    private static int CountTypes(IList<TypeDef> types)
    {
        int n = 0;
        for (int i = 0; i < types.Count; i++)
        {
            n++;
            n += CountTypes(types[i].NestedTypes);
        }
        return n;
    }

    private static void ReportTypeInfo()
    {
        Console.WriteLine("  target type = " + _targetType.FullName);
        Console.WriteLine("    fields=" + _targetType.Fields.Count + " methods=" + _targetType.Methods.Count +
                          " props=" + _targetType.Properties.Count + " events=" + _targetType.Events.Count +
                          " nested=" + _targetType.NestedTypes.Count + " ifaces=" + _targetType.Interfaces.Count);
        Console.WriteLine("  shim type   = " + _shimType.FullName);
        Console.WriteLine("    fields=" + _shimType.Fields.Count + " methods=" + _shimType.Methods.Count +
                          " props=" + _shimType.Properties.Count + " events=" + _shimType.Events.Count +
                          " nested=" + _shimType.NestedTypes.Count);
        if (_shimType.Methods.Count == 0) throw new Exception("shim 类型没有方法");
    }

    // -------------------------------------------------------------- preflight

    private static void Preflight()
    {
        int bad = 0;
        int scanned = 0;
        foreach (TypeDef t in _target.GetTypes())
        {
            if (t == _targetType) continue;
            if (_deletedNestedFullNames.Contains(t.FullName)) continue;
            scanned++;
            for (int i = 0; i < t.Fields.Count; i++)
            {
                FieldDef f = t.Fields[i];
                TypeSig sig = f.FieldSig == null ? null : f.FieldSig.Type;
                if (TypeSigRefsDeleted(sig))
                {
                    ReportBad("字段签名", t.FullName + "::" + f.Name, sig == null ? "" : sig.FullName);
                    bad++;
                }
            }
            for (int i = 0; i < t.Methods.Count; i++)
            {
                MethodDef m = t.Methods[i];
                if (m.MethodSig != null)
                {
                    if (TypeSigRefsDeleted(m.MethodSig.RetType))
                    {
                        ReportBad("方法返回类型", t.FullName + "::" + m.Name, m.MethodSig.RetType.FullName);
                        bad++;
                    }
                    IList<TypeSig> ps = m.MethodSig.Params;
                    if (ps != null)
                    {
                        for (int j = 0; j < ps.Count; j++)
                        {
                            if (TypeSigRefsDeleted(ps[j]))
                            {
                                ReportBad("方法参数类型", t.FullName + "::" + m.Name, ps[j].FullName);
                                bad++;
                            }
                        }
                    }
                }
                if (!m.HasBody || m.Body == null) continue;
                IList<Instruction> instrs = m.Body.Instructions;
                for (int j = 0; j < instrs.Count; j++)
                {
                    object op = instrs[j].Operand;
                    if (op == null) continue;
                    string why = CheckOperandRef(op);
                    if (why != null)
                    {
                        ReportBad("指令操作数", t.FullName + "::" + m.Name + " @IL_" + instrs[j].Offset.ToString("X4"), why);
                        bad++;
                    }
                }
            }
            for (int i = 0; i < t.Properties.Count; i++)
            {
                PropertyDef p = t.Properties[i];
                if (p.PropertySig != null && TypeSigRefsDeleted(p.PropertySig.RetType))
                {
                    ReportBad("属性类型", t.FullName + "::" + p.Name, p.PropertySig.RetType.FullName);
                    bad++;
                }
            }
        }
        Console.WriteLine("  扫描存活类型数=" + scanned + "，问题引用数=" + bad);
        if (bad > 0)
            throw new Exception("预检失败：存在 " + bad + " 处对将被删除成员/嵌套类型的引用（见上），拒绝继续以免生成坏 exe。");
        Console.WriteLine("  OK 未发现对将被删除成员的引用");
    }

    private static void ReportBad(string kind, string where, string detail)
    {
        if (_warnings.Count > 60) return;
        _warnings.Add("PREFLIGHT " + kind + " " + where + " -> " + detail);
    }

    private static bool TypeSigRefsDeleted(TypeSig sig)
    {
        if (sig == null) return false;
        string fn;
        try { fn = sig.FullName; }
        catch (Exception) { return false; }
        if (fn == null) return false;
        foreach (string d in _deletedNestedFullNames)
        {
            if (fn.IndexOf(d, StringComparison.Ordinal) >= 0) return true;
        }
        return false;
    }

    private static string CheckOperandRef(object op)
    {
        MethodDef md = op as MethodDef;
        if (md != null)
        {
            if (md.DeclaringType == _targetType && !_preservedMethodNames.Contains(md.Name.ToString()))
                return "删除方法 " + md.FullName;
            return null;
        }
        MethodSpec ms = op as MethodSpec;
        if (ms != null)
        {
            MethodDef msd = ms.Method as MethodDef;
            if (msd != null && msd.DeclaringType == _targetType && !_preservedMethodNames.Contains(msd.Name.ToString()))
                return "删除方法(MethodSpec) " + msd.FullName;
            return null;
        }
        MemberRef mr = op as MemberRef;
        if (mr != null)
        {
            ITypeDefOrRef decl = null;
            try { decl = mr.DeclaringType; }
            catch (Exception) { return null; }
            if (decl == null) return null;
            string declFull = decl.FullName;
            if (declFull == TargetTypeFullName)
            {
                string name = mr.Name.ToString();
                if (mr.IsMethodRef && !_preservedMethodNames.Contains(name))
                    return "删除方法(MemberRef) " + TargetTypeFullName + "::" + name;
                if (mr.IsFieldRef && name != "StatusChanged")
                    return "删除字段(MemberRef) " + TargetTypeFullName + "::" + name;
                return null;
            }
            if (_deletedNestedFullNames.Contains(declFull))
                return "删除嵌套类型引用 " + declFull;
            return null;
        }
        FieldDef fd = op as FieldDef;
        if (fd != null)
        {
            if (fd.DeclaringType == _targetType && fd.Name.ToString() != "StatusChanged")
                return "删除字段 " + fd.FullName;
            return null;
        }
        TypeDef td = op as TypeDef;
        if (td != null)
        {
            if (_deletedNestedFullNames.Contains(td.FullName)) return "删除嵌套类型 " + td.FullName;
            return null;
        }
        TypeRef tr = op as TypeRef;
        if (tr != null)
        {
            string f;
            try { f = tr.FullName; }
            catch (Exception) { return null; }
            if (_deletedNestedFullNames.Contains(f)) return "删除嵌套类型 " + f;
            return null;
        }
        TypeSpec ts = op as TypeSpec;
        if (ts != null)
        {
            if (TypeSigRefsDeleted(ts.TypeSig)) return "删除类型(TypeSpec) " + ts.TypeSig.FullName;
            return null;
        }
        return null;
    }

    // --------------------------------------------------------------- deletion

    // ------------------------------------------------------------ 保留原成员（加法式）

    // additive 策略：不删除任何成员；仅收集需要替换 body 的 10 个公开方法。
    private static void KeepOriginalMembers()
    {
        _preservedMethods = new List<MethodDef>();
        for (int i = 0; i < _targetType.Methods.Count; i++)
        {
            MethodDef m = _targetType.Methods[i];
            if (_preservedMethodNames.Contains(m.Name.ToString())) _preservedMethods.Add(m);
        }
        if (_preservedMethods.Count != 10)
            throw new Exception("保留的公开方法数应为 10，实际 " + _preservedMethods.Count);
        Console.WriteLine("  methods  " + _targetType.Methods.Count + "（全部保留，其中 10 个将替换 body）");
        Console.WriteLine("  fields   " + _targetType.Fields.Count + "（全部保留）");
        Console.WriteLine("  props    " + _targetType.Properties.Count + "（全部保留）");
        Console.WriteLine("  events   " + _targetType.Events.Count + "（全部保留）");
        Console.WriteLine("  nested   " + _targetType.NestedTypes.Count + "（全部保留，包括 PendingSend/状态机等死代码）");
    }

    // ----------------------------------------------------------------- fields

    private static void BuildFieldMap()
    {
        _fieldMap = new Dictionary<FieldDef, IField>();
        FieldDef shimStatus = null;
        for (int i = 0; i < _shimType.Fields.Count; i++)
        {
            if (_shimType.Fields[i].Name.ToString() == "StatusChanged")
            {
                shimStatus = _shimType.Fields[i];
                break;
            }
        }
        if (shimStatus == null) throw new Exception("shim 类型缺少 StatusChanged 字段");
        FieldDef targetStatus = null;
        for (int i = 0; i < _targetType.Fields.Count; i++)
        {
            if (_targetType.Fields[i].Name.ToString() == "StatusChanged")
            {
                targetStatus = _targetType.Fields[i];
                break;
            }
        }
        if (targetStatus == null) throw new Exception("目标类型缺少 StatusChanged 字段");
        _fieldMap[shimStatus] = targetStatus;
        Console.WriteLine("  StatusChanged 复用目标原有字段 (" + targetStatus.FullName + ")");

        _addedFieldCount = 0;
        HashSet<string> usedFieldNames = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < _targetType.Fields.Count; i++)
            usedFieldNames.Add(_targetType.Fields[i].Name.ToString());
        for (int i = 0; i < _shimType.Fields.Count; i++)
        {
            FieldDef sf = _shimType.Fields[i];
            if (sf == shimStatus) continue;
            if (sf.HasFieldRVA)
                throw new Exception("shim 实例字段带 RVA，本工具不支持: " + sf.FullName);
            TypeSig ft = RemapTypeSig(sf.FieldSig.Type);
            // additive：新字段统一加 ob_ 前缀，避免与保留的旧字段（_channel/_socket/...）重名
            string newFieldName = NewMemberPrefix + sf.Name.ToString();
            if (!usedFieldNames.Add(newFieldName))
                throw new Exception("新增字段名与现有字段冲突: " + newFieldName);
            FieldDefUser nf = new FieldDefUser(newFieldName, new FieldSig(ft), sf.Attributes);
            if (sf.HasConstant)
            {
                if (sf.Constant == null) throw new Exception("shim 常量字段 Constant 为 null: " + sf.FullName);
                nf.Constant = new ConstantUser(sf.Constant.Value, sf.Constant.Type);
            }
            _targetType.Fields.Add(nf);
            _fieldMap[sf] = nf;
            _addedFieldCount++;
        }
        Console.WriteLine("  从 shim 新增字段=" + _addedFieldCount + "（literal const 已复制 Constant）");

        // shim 的 <PrivateImplementationDetails> 静态字段：不克隆类型，挂到目标类型上
        TypeDef pidType = null;
        for (int i = 0; i < _shim.Types.Count; i++)
        {
            if (_shim.Types[i].Name.ToString().StartsWith("<PrivateImplementationDetails>", StringComparison.Ordinal))
            {
                pidType = _shim.Types[i];
                break;
            }
        }
        if (pidType != null)
        {
            Console.WriteLine("  发现 shim <PrivateImplementationDetails> 类型: " + pidType.FullName +
                              " fields=" + pidType.Fields.Count);
            for (int i = 0; i < pidType.Fields.Count; i++)
            {
                FieldDef pf = pidType.Fields[i];
                if (!pf.IsStatic)
                    throw new Exception("shim <PrivateImplementationDetails> 存在非静态字段，需克隆类型: " + pf.FullName);
                if (pf.HasFieldRVA)
                    throw new Exception("shim <PrivateImplementationDetails> 字段带 RVA，需克隆类型: " + pf.FullName);
                TypeSig pft = RemapTypeSig(pf.FieldSig.Type);
                string newPfName = NewMemberPrefix + pf.Name.ToString();
                if (!usedFieldNames.Add(newPfName))
                    throw new Exception("新增字段名与现有字段冲突: " + newPfName);
                FieldDefUser npf = new FieldDefUser(newPfName, new FieldSig(pft), pf.Attributes);
                if (pf.HasConstant)
                    npf.Constant = new ConstantUser(pf.Constant.Value, pf.Constant.Type);
                _targetType.Fields.Add(npf);
                _fieldMap[pf] = npf;
                _addedFieldCount++;
                Console.WriteLine("    迁移静态字段 " + newPfName + " : " + pf.FieldSig.Type.FullName +
                                  (pf.HasConstant ? " (const)" : " (static)"));
            }
        }
        Console.WriteLine("  目标类型字段总数=" + _targetType.Fields.Count);
    }

    // ---------------------------------------------------------------- methods

    private static void BuildMethodMap()
    {
        _methodMap = new Dictionary<MethodDef, MethodDef>();
        _expectedInstrCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        _expectedSigKeys = new Dictionary<string, string>(StringComparer.Ordinal);
        _addedMethodCount = 0;
        int preservedMapped = 0;
        HashSet<MethodDef> usedPreserved = new HashSet<MethodDef>();

        for (int i = 0; i < _shimType.Methods.Count; i++)
        {
            MethodDef sm = _shimType.Methods[i];
            string name = sm.Name.ToString();
            MethodSig remapped = RemapMethodSig(sm.MethodSig);
            if (sm.Body == null || sm.Body.Instructions.Count == 0)
                throw new Exception("shim 方法无 IL body: " + sm.FullName);
            if (_preservedMethodNames.Contains(name))
            {
                MethodDef match = FindPreservedTargetMethod(name, remapped);
                if (match == null)
                    throw new Exception("找不到对应的保留目标方法: " + sm.FullName + " key=" + MethodSigKey(remapped));
                if (usedPreserved.Contains(match))
                    throw new Exception("保留目标方法被映射多次: " + match.FullName);
                usedPreserved.Add(match);
                _methodMap[sm] = match;
                string pkey = MethodKey(name, match.MethodSig);
                if (_expectedInstrCounts.ContainsKey(pkey))
                    throw new Exception("映射键重复: " + pkey);
                _expectedInstrCounts[pkey] = sm.Body.Instructions.Count;
                preservedMapped++;
                Console.WriteLine("  [保留] " + name + " " + ShortSig(match.MethodSig) +
                                  "  body_instr=" + BodyInstrCount(sm));
            }
            else
            {
                // additive：shim 私有/内部方法统一加 ob_ 前缀，避免与保留的旧私有方法（SendLoop 等）重名
                string newMethodName = NewMemberPrefix + name;
                MethodDefUser nm = new MethodDefUser(newMethodName, remapped, sm.ImplAttributes, sm.Attributes);
                for (int j = 0; j < sm.ParamDefs.Count; j++)
                {
                    ParamDef sp = sm.ParamDefs[j];
                    nm.ParamDefs.Add(new ParamDefUser(sp.Name, sp.Sequence, sp.Attributes));
                }
                _targetType.Methods.Add(nm);
                _methodMap[sm] = nm;
                string nkey = MethodKey(newMethodName, nm.MethodSig);
                if (_expectedInstrCounts.ContainsKey(nkey))
                    throw new Exception("映射键重复: " + nkey);
                _expectedInstrCounts[nkey] = sm.Body.Instructions.Count;
                _addedMethodCount++;
            }
        }

        if (preservedMapped != 10)
            throw new Exception("保留方法映射数应为 10，实际 " + preservedMapped);
        if (usedPreserved.Count != 10)
            throw new Exception("保留方法使用数应为 10，实际 " + usedPreserved.Count);
        Console.WriteLine("  新建方法=" + _addedMethodCount + "，保留方法=" + preservedMapped +
                          "，映射总数=" + _methodMap.Count + " / shim 方法数=" + _shimType.Methods.Count);
        if (_methodMap.Count != _shimType.Methods.Count)
            throw new Exception("方法映射不完整");
        Console.WriteLine("  目标类型方法总数=" + _targetType.Methods.Count);
    }

    private static MethodDef FindPreservedTargetMethod(string name, MethodSig remappedSig)
    {
        string wantKey = MethodSigKey(remappedSig);
        MethodDef bySig = null;
        MethodDef byNameOnly = null;
        int nameMatches = 0;
        for (int i = 0; i < _preservedMethods.Count; i++)
        {
            MethodDef tm = _preservedMethods[i];
            if (tm.Name.ToString() != name) continue;
            nameMatches++;
            byNameOnly = tm;
            if (tm.MethodSig != null && MethodSigKey(tm.MethodSig) == wantKey) bySig = tm;
        }
        if (bySig != null) return bySig;
        if (nameMatches == 1) return byNameOnly;
        return null;
    }

    private static void CloneAllBodies()
    {
        int total = 0;
        for (int i = 0; i < _shimType.Methods.Count; i++)
        {
            MethodDef sm = _shimType.Methods[i];
            MethodDef tm = _methodMap[sm];
            CloneBody(sm, tm);
            total += tm.Body.Instructions.Count;
        }
        Console.WriteLine("  已克隆方法体 " + _shimType.Methods.Count + " 个，共 " + total + " 条指令");
        // 只校验被替换/新增的 97 个方法（原保留的其它旧方法不允许改动，也不必检查）
        for (int i = 0; i < _shimType.Methods.Count; i++)
        {
            MethodDef tm = _methodMap[_shimType.Methods[i]];
            if (!tm.HasBody || tm.Body == null || tm.Body.Instructions.Count == 0)
                throw new Exception("克隆后目标方法无 body: " + tm.FullName);
        }
    }

    private static void CloneBody(MethodDef sm, MethodDef tm)
    {
        CilBody sb = sm.Body;
        if (sb == null) throw new Exception("shim 方法 body 为 null: " + sm.FullName);
        CilBody nb = new CilBody();
        nb.InitLocals = sb.InitLocals;
        nb.MaxStack = sb.MaxStack;
        // 不用 KeepOldMaxStack：rt4/rt6 的 PreserveAll writer 未使用该 flag 且启动正常

        // 局部变量按索引一一重建
        List<Local> newLocals = new List<Local>();
        for (int i = 0; i < sb.Variables.Count; i++)
        {
            Local nl = new Local(RemapTypeSig(sb.Variables[i].Type));
            newLocals.Add(nl);
            nb.Variables.Add(nl);
        }
        if (newLocals.Count != sb.Variables.Count)
            throw new Exception("局部变量数不一致: " + sm.FullName);

        // 参数按 Index 映射（dnlib Parameter.Index 含 hidden this 的原始 arg 索引）
        Dictionary<int, Parameter> paramByIndex = new Dictionary<int, Parameter>();
        foreach (Parameter p in tm.Parameters)
        {
            if (!paramByIndex.ContainsKey(p.Index)) paramByIndex.Add(p.Index, p);
        }

        // 阶段 1：顺序建新指令，建立 旧->新 字典
        Dictionary<Instruction, Instruction> instrMap = new Dictionary<Instruction, Instruction>();
        List<Instruction> newInstrs = new List<Instruction>();
        for (int i = 0; i < sb.Instructions.Count; i++)
        {
            Instruction si = sb.Instructions[i];
            // 用 public ctor 建空指令（Instruction.Create(OpCode) 只接受无操作数 opcode）
            Instruction ni = new Instruction(si.OpCode, null);
            instrMap[si] = ni;
            newInstrs.Add(ni);
            nb.Instructions.Add(ni);
        }

        // 阶段 2：填操作数
        for (int i = 0; i < sb.Instructions.Count; i++)
        {
            Instruction si = sb.Instructions[i];
            Instruction ni = newInstrs[i];
            switch (si.OpCode.OperandType)
            {
                case OperandType.InlineNone:
                    ni.Operand = null;
                    break;
                case OperandType.ShortInlineBrTarget:
                case OperandType.InlineBrTarget:
                    ni.Operand = MapInstruction(instrMap, (Instruction)si.Operand, sm);
                    break;
                case OperandType.InlineSwitch:
                    {
                        Instruction[] targets = (Instruction[])si.Operand;
                        if (targets == null) throw new Exception("switch 操作数为 null: " + sm.FullName);
                        Instruction[] ntargets = new Instruction[targets.Length];
                        for (int j = 0; j < targets.Length; j++)
                            ntargets[j] = MapInstruction(instrMap, targets[j], sm);
                        ni.Operand = ntargets;
                    }
                    break;
                case OperandType.InlineString:
                    ni.Operand = (string)si.Operand;
                    break;
                case OperandType.ShortInlineI:
                    ni.Operand = Convert.ToSByte(si.Operand);
                    break;
                case OperandType.InlineI:
                    ni.Operand = Convert.ToInt32(si.Operand);
                    break;
                case OperandType.InlineI8:
                    ni.Operand = Convert.ToInt64(si.Operand);
                    break;
                case OperandType.ShortInlineR:
                    ni.Operand = Convert.ToSingle(si.Operand);
                    break;
                case OperandType.InlineR:
                    ni.Operand = Convert.ToDouble(si.Operand);
                    break;
                case OperandType.InlineVar:
                case OperandType.ShortInlineVar:
                    {
                        // dnlib 不区分 InlineArg/InlineVar：ldarg/ldloc 共用 InlineVar，
                        // 操作数可能是 Local 或 Parameter，按对象类型分别映射。
                        Local sl = si.Operand as Local;
                        if (sl != null)
                        {
                            if (sl.Index < 0 || sl.Index >= newLocals.Count)
                                throw new Exception("局部变量索引越界: " + sm.FullName + " idx=" + sl.Index);
                            ni.Operand = newLocals[sl.Index];
                            break;
                        }
                        Parameter sp = si.Operand as Parameter;
                        if (sp != null)
                        {
                            Parameter np;
                            if (!paramByIndex.TryGetValue(sp.Index, out np))
                                throw new Exception("参数索引无法映射: " + sm.FullName + " idx=" + sp.Index);
                            ni.Operand = np;
                            break;
                        }
                        throw new Exception("InlineVar 操作数既不是 Local 也不是 Parameter: " + sm.FullName +
                                            " op=" + (si.Operand == null ? "<null>" : si.Operand.GetType().FullName));
                    }
                case OperandType.InlineMethod:
                    ni.Operand = RemapMethod((IMethod)si.Operand);
                    break;
                case OperandType.InlineField:
                    ni.Operand = RemapField((IField)si.Operand);
                    break;
                case OperandType.InlineType:
                case OperandType.InlineTok:
                    ni.Operand = RemapTokenOperand(si.Operand);
                    break;
                default:
                    throw new Exception("未支持的 OperandType " + si.OpCode.OperandType +
                                        " 在 " + sm.FullName + " IL_" + si.Offset.ToString("X4") +
                                        " opcode=" + si.OpCode);
            }
        }

        // 异常处理器
        for (int i = 0; i < sb.ExceptionHandlers.Count; i++)
        {
            ExceptionHandler seh = sb.ExceptionHandlers[i];
            ExceptionHandler neh = new ExceptionHandler(seh.HandlerType);
            neh.TryStart = MapInstrOrNull(instrMap, seh.TryStart, sm);
            neh.TryEnd = MapInstrOrNull(instrMap, seh.TryEnd, sm);
            neh.HandlerStart = MapInstrOrNull(instrMap, seh.HandlerStart, sm);
            neh.HandlerEnd = MapInstrOrNull(instrMap, seh.HandlerEnd, sm);
            neh.FilterStart = MapInstrOrNull(instrMap, seh.FilterStart, sm);
            if (seh.CatchType != null) neh.CatchType = RemapType(seh.CatchType);
            nb.ExceptionHandlers.Add(neh);
        }

        tm.Body = nb;
    }

    private static Instruction MapInstruction(Dictionary<Instruction, Instruction> map, Instruction oldInstr, MethodDef sm)
    {
        Instruction ni;
        if (oldInstr == null || !map.TryGetValue(oldInstr, out ni))
            throw new Exception("分支目标无法映射: " + sm.FullName);
        return ni;
    }

    private static Instruction MapInstrOrNull(Dictionary<Instruction, Instruction> map, Instruction oldInstr, MethodDef sm)
    {
        if (oldInstr == null) return null;
        return MapInstruction(map, oldInstr, sm);
    }

    private static int BodyInstrCount(MethodDef m)
    {
        if (m.Body == null || m.Body.Instructions == null) return 0;
        return m.Body.Instructions.Count;
    }

    // ------------------------------------------------------------- type remap

    private static ITypeDefOrRef RemapType(ITypeDefOrRef t)
    {
        if (t == null) return null;
        TypeDef td = t as TypeDef;
        if (td != null)
        {
            if (td.Module == _shim)
            {
                TypeDef mapped;
                if (_typeMap.TryGetValue(td, out mapped)) return mapped;
                throw new Exception("shim TypeDef 未映射: " + td.FullName);
            }
            return td; // 目标模块中的类型：原样
        }
        TypeRef tr = t as TypeRef;
        if (tr != null) return MapTypeRef(tr);
        TypeSpec ts = t as TypeSpec;
        if (ts != null) return new TypeSpecUser(RemapTypeSig(ts.TypeSig));
        throw new Exception("RemapType 无法处理的 ITypeDefOrRef: " + t.GetType().FullName + " " + SafeFullName(t));
    }

    private static string SafeFullName(ITypeDefOrRef t)
    {
        try { return t.FullName; }
        catch (Exception) { return "<FullName 抛出异常>"; }
    }

    private static ITypeDefOrRef MapTypeRef(TypeRef tr)
    {
        string full;
        try { full = tr.FullName; }
        catch (Exception) { full = tr.Name.ToString(); }
        string asmName = "";
        try
        {
            IAssembly defAsm = tr.DefinitionAssembly;
            if (defAsm != null) asmName = defAsm.Name.ToString();
        }
        catch (Exception) { asmName = ""; }

        if (string.Equals(asmName, TargetAssemblyName, StringComparison.Ordinal))
        {
            TypeDef td;
            if (_targetTypesByFullName.TryGetValue(full, out td)) return td;
            throw new Exception("WeComSever 程序集 TypeRef 在目标模块中找不到对应 TypeDef: " + full);
        }
        if (string.Equals(asmName, ShimAssemblyName, StringComparison.Ordinal))
        {
            TypeDef td;
            if (_mappedTypesByFullName.TryGetValue(full, out td)) return td;
            throw new Exception("OneBotShim TypeRef 无法映射: " + full);
        }
        return _importer.Import(tr);
    }

    private static TypeSig RemapTypeSig(TypeSig sig)
    {
        if (sig == null) return null;
        CorLibTypeSig crl = sig as CorLibTypeSig;
        if (crl != null) return crl;

        GenericInstSig gi = sig as GenericInstSig;
        if (gi != null)
        {
            ClassOrValueTypeSig gts = RemapTypeSig(gi.GenericType) as ClassOrValueTypeSig;
            if (gts == null) throw new Exception("GenericInstSig 的 GenericType 重映射失败");
            List<TypeSig> args = new List<TypeSig>();
            for (int i = 0; i < gi.GenericArguments.Count; i++)
                args.Add(RemapTypeSig(gi.GenericArguments[i]));
            return new GenericInstSig(gts, args);
        }
        SZArraySig sza = sig as SZArraySig;
        if (sza != null) return new SZArraySig(RemapTypeSig(sza.Next));
        ArraySig arr = sig as ArraySig;
        if (arr != null)
        {
            TypeSig elem = RemapTypeSig(arr.Next);
            if (arr.Sizes.Count > 0 || arr.LowerBounds.Count > 0)
                return new ArraySig(elem, arr.Rank, arr.Sizes, arr.LowerBounds);
            return new ArraySig(elem, arr.Rank);
        }
        ByRefSig br = sig as ByRefSig;
        if (br != null) return new ByRefSig(RemapTypeSig(br.Next));
        PtrSig ptr = sig as PtrSig;
        if (ptr != null) return new PtrSig(RemapTypeSig(ptr.Next));
        PinnedSig pin = sig as PinnedSig;
        if (pin != null) return new PinnedSig(RemapTypeSig(pin.Next));
        CModReqdSig creq = sig as CModReqdSig;
        if (creq != null) return new CModReqdSig(RemapType(creq.Modifier), RemapTypeSig(creq.Next));
        CModOptSig copt = sig as CModOptSig;
        if (copt != null) return new CModOptSig(RemapType(copt.Modifier), RemapTypeSig(copt.Next));
        GenericVar gv = sig as GenericVar;
        if (gv != null) return new GenericVar(gv.Number);
        GenericMVar gmv = sig as GenericMVar;
        if (gmv != null) return new GenericMVar(gmv.Number);
        TypeDefOrRefSig tdrs = sig as TypeDefOrRefSig;
        if (tdrs != null)
        {
            ITypeDefOrRef mapped = RemapType(tdrs.TypeDefOrRef);
            ClassSig cs = sig as ClassSig;
            if (cs != null) return new ClassSig(mapped);
            ValueTypeSig vs = sig as ValueTypeSig;
            if (vs != null) return new ValueTypeSig(mapped);
            return new ClassSig(mapped);
        }
        throw new Exception("未支持的 TypeSig: " + sig.GetType().FullName + " " + SafeSigFullName(sig));
    }

    private static string SafeSigFullName(TypeSig sig)
    {
        try { return sig.FullName; }
        catch (Exception) { return "<FullName 抛出异常>"; }
    }

    private static MethodSig RemapMethodSig(MethodSig s)
    {
        if (s == null) throw new Exception("RemapMethodSig: sig 为 null");
        List<TypeSig> ps = new List<TypeSig>();
        if (s.Params != null)
        {
            for (int i = 0; i < s.Params.Count; i++) ps.Add(RemapTypeSig(s.Params[i]));
        }
        MethodSig n = new MethodSig(s.CallingConvention, s.GenParamCount, RemapTypeSig(s.RetType), ps);
        n.HasThis = s.HasThis;
        n.ExplicitThis = s.ExplicitThis;
        return n;
    }

    private static FieldSig RemapFieldSig(FieldSig s)
    {
        if (s == null) throw new Exception("RemapFieldSig: sig 为 null");
        return new FieldSig(RemapTypeSig(s.Type));
    }

    private static string MethodSigKey(MethodSig sig)
    {
        if (sig == null) return "?";
        StringBuilder sb = new StringBuilder();
        sb.Append(sig.GenParamCount);
        sb.Append('|');
        sb.Append(sig.RetType == null ? "?" : SafeSigFullName(sig.RetType));
        sb.Append('|');
        IList<TypeSig> ps = sig.Params;
        if (ps != null)
        {
            for (int i = 0; i < ps.Count; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(ps[i] == null ? "?" : SafeSigFullName(ps[i]));
            }
        }
        return sb.ToString();
    }

    private static string MethodKey(string name, MethodSig sig)
    {
        return name + "\u0001" + MethodSigKey(sig);
    }

    private static string ShortSig(MethodSig sig)
    {
        if (sig == null) return "(null sig)";
        StringBuilder sb = new StringBuilder();
        sb.Append('(');
        IList<TypeSig> ps = sig.Params;
        if (ps != null)
        {
            for (int i = 0; i < ps.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(SafeSigFullName(ps[i]));
            }
        }
        sb.Append(") -> ");
        sb.Append(sig.RetType == null ? "?" : SafeSigFullName(sig.RetType));
        return sb.ToString();
    }

    // ---------------------------------------------------------- member remap

    private static object RemapTokenOperand(object op)
    {
        if (op == null) return null;
        TypeSpec tspec = op as TypeSpec;
        if (tspec != null) return new TypeSpecUser(RemapTypeSig(tspec.TypeSig));
        ITypeDefOrRef tdr = op as ITypeDefOrRef;
        if (tdr != null) return RemapType(tdr);
        IField f = op as IField;
        if (f != null) return RemapField(f);
        IMethod m = op as IMethod;
        if (m != null) return RemapMethod(m);
        throw new Exception("无法重映射 InlineTok/InlineType 操作数: " + op.GetType().FullName);
    }

    private static IMethod RemapMethod(IMethod method)
    {
        if (method == null) return null;
        MethodDef md = method as MethodDef;
        if (md != null)
        {
            if (md.Module == _shim)
            {
                MethodDef target;
                if (_methodMap.TryGetValue(md, out target)) return target;
                throw new Exception("shim MethodDef 未映射: " + md.FullName);
            }
            return md;
        }
        MethodSpec ms = method as MethodSpec;
        if (ms != null)
        {
            IMethodDefOrRef baseMethod = RemapMethodDefOrRef(ms.Method);
            List<TypeSig> genArgs = new List<TypeSig>();
            GenericInstMethodSig gims = ms.GenericInstMethodSig;
            if (gims != null && gims.GenericArguments != null)
            {
                for (int i = 0; i < gims.GenericArguments.Count; i++)
                    genArgs.Add(RemapTypeSig(gims.GenericArguments[i]));
            }
            return new MethodSpecUser(baseMethod, new GenericInstMethodSig(genArgs));
        }
        MemberRef mr = method as MemberRef;
        if (mr != null && mr.IsMethodRef)
        {
            IMethodDefOrRef internalMapped = MapInternalMethodRef(mr);
            if (internalMapped != null) return internalMapped;
            MethodSig sig = RemapMethodSig(mr.MethodSig);
            IMemberRefParent cls = RemapMemberRefClass(mr.Class, mr);
            _fallbackMemberRefCount++;
            if (_warnings.Count < 40)
                _warnings.Add("外部/未解析 MemberRef 重建: " + SafeFullName2(mr) + " -> " + mr.Name);
            return new MemberRefUser(_target, mr.Name, sig, cls);
        }
        throw new Exception("无法重映射方法引用: " + method.GetType().FullName + " " + SafeFullName2(method));
    }

    private static IMethodDefOrRef RemapMethodDefOrRef(IMethodDefOrRef m)
    {
        MethodDef md = m as MethodDef;
        if (md != null)
        {
            if (md.Module == _shim)
            {
                MethodDef target;
                if (_methodMap.TryGetValue(md, out target)) return target;
                throw new Exception("shim MethodDef(MethodSpec) 未映射: " + md.FullName);
            }
            return md;
        }
        MemberRef mr = m as MemberRef;
        if (mr != null)
        {
            IMethodDefOrRef internalMapped = MapInternalMethodRef(mr);
            if (internalMapped != null) return internalMapped;
            MethodSig sig = RemapMethodSig(mr.MethodSig);
            IMemberRefParent cls = RemapMemberRefClass(mr.Class, mr);
            _fallbackMemberRefCount++;
            if (_warnings.Count < 40)
                _warnings.Add("外部/未解析 MemberRef(MethodSpec) 重建: " + SafeFullName2(mr) + " -> " + mr.Name);
            return new MemberRefUser(_target, mr.Name, sig, cls);
        }
        throw new Exception("无法重映射 IMethodDefOrRef: " + m.GetType().FullName);
    }

    private static IMethodDefOrRef MapInternalMethodRef(MemberRef mr)
    {
        ITypeDefOrRef decl;
        try { decl = mr.DeclaringType; }
        catch (Exception) { return null; }
        if (decl == null) return null;
        TypeDef td = RemapInternalDeclaringType(decl);
        if (td == null) return null;
        MethodSig sig = RemapMethodSig(mr.MethodSig);
        string key = MethodSigKey(sig);
        string name = mr.Name.ToString();
        MethodDef bySig = null;
        int nameMatches = 0;
        MethodDef byName = null;
        for (int i = 0; i < td.Methods.Count; i++)
        {
            MethodDef m = td.Methods[i];
            if (m.Name.ToString() != name) continue;
            nameMatches++;
            byName = m;
            if (m.MethodSig != null && MethodSigKey(m.MethodSig) == key) bySig = m;
        }
        if (bySig != null) return bySig;
        if (nameMatches == 1) return byName;
        return null;
    }

    private static IField RemapField(IField field)
    {
        if (field == null) return null;
        FieldDef fd = field as FieldDef;
        if (fd != null)
        {
            if (fd.Module == _shim)
            {
                IField target;
                if (_fieldMap.TryGetValue(fd, out target)) return target;
                throw new Exception("shim FieldDef 未映射: " + fd.FullName);
            }
            return fd;
        }
        MemberRef mr = field as MemberRef;
        if (mr != null && mr.IsFieldRef)
        {
            IField internalMapped = MapInternalFieldRef(mr);
            if (internalMapped != null) return internalMapped;
            FieldSig fsig = RemapFieldSig(mr.FieldSig);
            IMemberRefParent cls = RemapMemberRefClass(mr.Class, mr);
            _fallbackMemberRefCount++;
            if (_warnings.Count < 40)
                _warnings.Add("外部/未解析 Field MemberRef 重建: " + SafeFullName2(mr) + " -> " + mr.Name);
            return new MemberRefUser(_target, mr.Name, fsig, cls);
        }
        throw new Exception("无法重映射字段引用: " + field.GetType().FullName + " " + SafeFullName2(field));
    }

    private static IField MapInternalFieldRef(MemberRef mr)
    {
        ITypeDefOrRef decl;
        try { decl = mr.DeclaringType; }
        catch (Exception) { return null; }
        if (decl == null) return null;
        TypeDef td = RemapInternalDeclaringType(decl);
        if (td == null) return null;
        FieldSig fsig = RemapFieldSig(mr.FieldSig);
        string wantType = SafeSigFullName(fsig.Type);
        string name = mr.Name.ToString();
        FieldDef byType = null;
        int nameMatches = 0;
        FieldDef byName = null;
        for (int i = 0; i < td.Fields.Count; i++)
        {
            FieldDef f = td.Fields[i];
            if (f.Name.ToString() != name) continue;
            nameMatches++;
            byName = f;
            if (f.FieldSig != null && SafeSigFullName(f.FieldSig.Type) == wantType) byType = f;
        }
        if (byType != null) return byType;
        if (nameMatches == 1) return byName;
        return null;
    }

    /// <summary>若 decl 属于目标程序集/shim，则返回映射后的 TypeDef；否则返回 null。</summary>
    private static TypeDef RemapInternalDeclaringType(ITypeDefOrRef decl)
    {
        TypeDef td = decl as TypeDef;
        if (td != null)
        {
            if (td.Module == _shim)
            {
                TypeDef mapped;
                if (_typeMap.TryGetValue(td, out mapped)) return mapped;
                return null;
            }
            return td;
        }
        TypeRef tr = decl as TypeRef;
        if (tr == null) return null;
        string full;
        try { full = tr.FullName; }
        catch (Exception) { return null; }
        string asmName = "";
        try
        {
            IAssembly defAsm = tr.DefinitionAssembly;
            if (defAsm != null) asmName = defAsm.Name.ToString();
        }
        catch (Exception) { asmName = ""; }
        if (string.Equals(asmName, TargetAssemblyName, StringComparison.Ordinal))
        {
            TypeDef found;
            if (_targetTypesByFullName.TryGetValue(full, out found)) return found;
            return null;
        }
        if (string.Equals(asmName, ShimAssemblyName, StringComparison.Ordinal))
        {
            TypeDef found;
            if (_mappedTypesByFullName.TryGetValue(full, out found)) return found;
            return null;
        }
        return null;
    }

    private static IMemberRefParent RemapMemberRefClass(IMemberRefParent cls, MemberRef owner)
    {
        if (cls == null) throw new Exception("MemberRef.Class 为 null: " + owner.Name);
        TypeDef td = cls as TypeDef;
        if (td != null)
        {
            if (td.Module == _shim)
            {
                TypeDef mapped;
                if (_typeMap.TryGetValue(td, out mapped)) return mapped;
                throw new Exception("MemberRef.Class 为未映射 shim TypeDef: " + td.FullName);
            }
            return td;
        }
        TypeRef tr = cls as TypeRef;
        if (tr != null) return MapTypeRef(tr);
        TypeSpec ts = cls as TypeSpec;
        if (ts != null) return new TypeSpecUser(RemapTypeSig(ts.TypeSig));
        throw new Exception("无法重映射 MemberRef.Class: " + cls.GetType().FullName);
    }

    private static string SafeFullName2(object o)
    {
        try
        {
            IMemberRef mr = o as IMemberRef;
            if (mr != null) return mr.FullName;
            IMethod m = o as IMethod;
            if (m != null) return m.FullName;
            IField f = o as IField;
            if (f != null) return f.FullName;
            return o.GetType().FullName;
        }
        catch (Exception) { return "<FullName 抛出异常>"; }
    }

    // ---------------------------------------------------------------- http fix

    /// <summary>
    /// 在 Robot.Config.WebServer.StartWebServer() 方法体首部注入：
    ///   if (BaseAddress != null) BaseAddress = BaseAddress.Replace("0.0.0.0", "+");
    /// 对应 IL：
    ///   ldarg.0; call get_BaseAddress; brfalse <原首指令>; ldarg.0; ldarg.0;
    ///   call get_BaseAddress; ldstr "0.0.0.0"; ldstr "+";
    ///   callvirt string::Replace(string,string); call set_BaseAddress
    /// 其余方法体/类型/成员/资源完全不动；maxstack 手动抬到 >= 4。
    /// </summary>
    private static void PatchHttpBaseAddress()
    {
        TypeDef ws = _target.Find("Robot.Config.WebServer", false);
        if (ws == null) throw new Exception("原 exe 中找不到 Robot.Config.WebServer 类型");

        MethodDef start = null;
        for (int i = 0; i < ws.Methods.Count; i++)
        {
            MethodDef m = ws.Methods[i];
            if (string.Equals(m.Name.ToString(), "StartWebServer", StringComparison.Ordinal) &&
                m.MethodSig.Params.Count == 0 && !m.IsStatic)
            {
                start = m;
                break;
            }
        }
        if (start == null) throw new Exception("Robot.Config.WebServer 中找不到 StartWebServer() 实例方法");

        PropertyDef prop = null;
        for (int i = 0; i < ws.Properties.Count; i++)
        {
            if (string.Equals(ws.Properties[i].Name.ToString(), "BaseAddress", StringComparison.Ordinal))
            {
                prop = ws.Properties[i];
                break;
            }
        }
        if (prop == null) throw new Exception("Robot.Config.WebServer 中找不到 BaseAddress 属性");
        MethodDef getter = prop.GetMethod;
        MethodDef setter = prop.SetMethod;
        if (getter == null || setter == null)
            throw new Exception("BaseAddress 属性缺少 getter/setter: get=" + (getter != null) + " set=" + (setter != null));
        if (!start.HasBody || start.Body == null || start.Body.Instructions.Count == 0)
            throw new Exception("StartWebServer() 没有可用方法体，无法注入");

        CilBody body = start.Body;
        Instruction firstOrig = body.Instructions[0];
        int origCount = body.Instructions.Count;

        // String::Replace(string,string)（实例方法，声明类型 System.String）
        ITypeDefOrRef strType = _target.CorLibTypes.String.TypeDefOrRef;
        MethodSig replaceSig = MethodSig.CreateInstance(_target.CorLibTypes.String,
                                                        _target.CorLibTypes.String,
                                                        _target.CorLibTypes.String);
        MemberRef replaceRef = new MemberRefUser(_target, "Replace", replaceSig, strType);

        List<Instruction> seq = new List<Instruction>();

        // ---- 诊断（正式产物请把 HttpDebugLogFile 置空）：写出 BaseAddress 实际值 ----
        if (!string.IsNullOrEmpty(HttpDebugLogFile))
        {
            ITypeDefOrRef fileType = new TypeRefUser(_target, "System.IO", "File", _target.CorLibTypes.AssemblyRef);
            MethodSig writeAllTextSig = MethodSig.CreateStatic(_target.CorLibTypes.Void,
                                                              _target.CorLibTypes.String,
                                                              _target.CorLibTypes.String);
            MemberRef writeAllTextRef = new MemberRefUser(_target, "WriteAllText", writeAllTextSig, fileType);
            MemberRef concatRef = new MemberRefUser(_target, "Concat",
                MethodSig.CreateStatic(_target.CorLibTypes.String, _target.CorLibTypes.String, _target.CorLibTypes.String),
                strType);

            // File.WriteAllText(path, "pre=" + BaseAddress)
            seq.Add(Instruction.Create(OpCodes.Ldstr, HttpDebugLogFile));
            seq.Add(Instruction.Create(OpCodes.Ldstr, "pre="));
            seq.Add(Instruction.Create(OpCodes.Ldarg_0));
            seq.Add(Instruction.Create(OpCodes.Call, getter));
            seq.Add(Instruction.Create(OpCodes.Call, concatRef));
            seq.Add(Instruction.Create(OpCodes.Call, writeAllTextRef));
        }

        // ---- 修复：BaseAddress = BaseAddress.Replace("0.0.0.0", "127.0.0.1")（带 null guard） ----
        seq.Add(Instruction.Create(OpCodes.Ldarg_0));
        seq.Add(Instruction.Create(OpCodes.Call, getter));
        seq.Add(Instruction.Create(OpCodes.Brfalse, firstOrig));
        seq.Add(Instruction.Create(OpCodes.Ldarg_0));
        seq.Add(Instruction.Create(OpCodes.Ldarg_0));
        seq.Add(Instruction.Create(OpCodes.Call, getter));
        seq.Add(Instruction.Create(OpCodes.Ldstr, HttpBaseAddressOldHost));
        seq.Add(Instruction.Create(OpCodes.Ldstr, HttpBaseAddressNewHost));
        seq.Add(Instruction.Create(OpCodes.Callvirt, replaceRef));
        seq.Add(Instruction.Create(OpCodes.Call, setter));

        // ---- 诊断：File.WriteAllText(path, "post=" + BaseAddress) ----
        if (!string.IsNullOrEmpty(HttpDebugLogFile))
        {
            ITypeDefOrRef fileType2 = new TypeRefUser(_target, "System.IO", "File", _target.CorLibTypes.AssemblyRef);
            MethodSig writeAllTextSig2 = MethodSig.CreateStatic(_target.CorLibTypes.Void,
                                                               _target.CorLibTypes.String,
                                                               _target.CorLibTypes.String);
            MemberRef writeAllTextRef2 = new MemberRefUser(_target, "WriteAllText", writeAllTextSig2, fileType2);
            MemberRef concatRef2 = new MemberRefUser(_target, "Concat",
                MethodSig.CreateStatic(_target.CorLibTypes.String, _target.CorLibTypes.String, _target.CorLibTypes.String),
                strType);

            seq.Add(Instruction.Create(OpCodes.Ldstr, HttpDebugLogFile));
            seq.Add(Instruction.Create(OpCodes.Ldstr, "post="));
            seq.Add(Instruction.Create(OpCodes.Ldarg_0));
            seq.Add(Instruction.Create(OpCodes.Call, getter));
            seq.Add(Instruction.Create(OpCodes.Call, concatRef2));
            seq.Add(Instruction.Create(OpCodes.Call, writeAllTextRef2));
        }

        for (int i = 0; i < seq.Count; i++) body.Instructions.Insert(i, seq[i]);
        if (body.MaxStack < 4) body.MaxStack = 4;
        _httpFixApplied = true;

        Console.WriteLine("  type      = " + ws.FullName + " (rid=" + ws.Rid + ")");
        Console.WriteLine("  method    = " + start.FullName + " (token=0x" + start.MDToken.Raw.ToString("X8") + ")");
        Console.WriteLine("  getter    = " + getter.Name + " / setter = " + setter.Name +
                          " / Replace parent = " + strType.FullName);
        Console.WriteLine("  injected  = " + seq.Count + " 条指令（入口 guard: BaseAddress != null）");
        Console.WriteLine("  debuglog  = " + (string.IsNullOrEmpty(HttpDebugLogFile) ? "(disabled)" : HttpDebugLogFile));
        Console.WriteLine("  body      = 原 " + origCount + " 条 -> 现 " + body.Instructions.Count +
                          " 条，MaxStack=" + body.MaxStack);
    }

    // --------------------------------------------------------------- verify

    private static void VerifyOutput()
    {
        if (!File.Exists(OutputExe)) throw new Exception("输出文件不存在: " + OutputExe);
        Console.WriteLine("  size   = " + new FileInfo(OutputExe).Length);
        ModuleDefMD outMod;
        try
        {
            outMod = ModuleDefMD.Load(OutputExe, new ModuleContext());
        }
        catch (Exception ex)
        {
            throw new Exception("dnlib 无法加载输出 exe（元数据损坏）: " + ex.Message, ex);
        }
        if (outMod.Assembly == null) throw new Exception("输出 exe 无程序集清单");
        Console.WriteLine("  assembly = " + outMod.Assembly.FullName);
        if (!string.Equals(outMod.Assembly.Name.ToString(), TargetAssemblyName, StringComparison.Ordinal))
            throw new Exception("输出程序集名被改变: " + outMod.Assembly.Name);
        if (_origAsmVersion != null && outMod.Assembly.Version != _origAsmVersion)
            throw new Exception("输出程序集版本被改变: " + outMod.Assembly.Version);
        Console.WriteLine("  OK 程序集名/版本未变");

        // 类型总数（加法式策略：与原来完全一致，不删除任何 TypeDef）
        int outAllTypes = CountTypes(outMod.Types);
        Console.WriteLine("  typeDefs(all)=" + outAllTypes + "  expected=" + _origAllTypeCount);
        if (outAllTypes != _origAllTypeCount)
            throw new Exception("类型总数不符（加法式策略应保持原值）：期望 " + _origAllTypeCount + "，实际 " + outAllTypes);

        // 4 个 EmbeddedResource：名称 + 长度
        if (outMod.Resources.Count != _origResources.Count)
            throw new Exception("资源数不符：" + outMod.Resources.Count + " vs " + _origResources.Count);
        foreach (Resource r in outMod.Resources)
        {
            string name = r.Name.ToString();
            uint origLen;
            if (!_origResources.TryGetValue(name, out origLen))
                throw new Exception("输出资源名在原 exe 中不存在: " + name);
            EmbeddedResource er = r as EmbeddedResource;
            if (er == null) throw new Exception("输出资源不是 EmbeddedResource: " + name);
            if (er.Length != origLen)
                throw new Exception("资源长度被改变: " + name + " " + origLen + " -> " + er.Length);
            Console.WriteLine("  resource OK " + name + " len=" + er.Length);
        }
        Console.WriteLine("  OK 4 个 EmbeddedResource 全部保留（名称+长度一致）");

        // 目标类型
        TypeDef tt = outMod.Find(TargetTypeFullName, false);
        if (tt == null) throw new Exception("输出 exe 中找不到目标类型: " + TargetTypeFullName);
        Console.WriteLine("  target type: methods=" + tt.Methods.Count + " fields=" + tt.Fields.Count +
                          " props=" + tt.Properties.Count + " events=" + tt.Events.Count +
                          " nested=" + tt.NestedTypes.Count + " ifaces=" + tt.Interfaces.Count);
        int expectedMethods = _origTargetMethodCount + _addedMethodCount;
        int expectedFields = _origTargetFieldCount + _addedFieldCount;
        Console.WriteLine("  期望 methods=" + expectedMethods + "（原 " + _origTargetMethodCount +
                          " + 新 " + _addedMethodCount + "） fields=" + expectedFields +
                          "（原 " + _origTargetFieldCount + " + 新 " + _addedFieldCount + "）");
        if (tt.Methods.Count != expectedMethods)
            throw new Exception("目标类型方法数不符：期望 " + expectedMethods + "，实际 " + tt.Methods.Count);
        if (tt.Fields.Count != expectedFields)
            throw new Exception("目标类型字段数不符：期望 " + expectedFields + "，实际 " + tt.Fields.Count);
        if (tt.Properties.Count != _origTargetPropCount)
            throw new Exception("目标类型属性数变化: " + tt.Properties.Count + " vs " + _origTargetPropCount);
        if (tt.Events.Count != _origTargetEventCount)
            throw new Exception("目标类型事件数变化: " + tt.Events.Count + " vs " + _origTargetEventCount);
        if (tt.NestedTypes.Count != _origTargetNestedNames.Count)
            throw new Exception("目标类型嵌套类型数变化: " + tt.NestedTypes.Count + " vs " + _origTargetNestedNames.Count);
        if (tt.Interfaces.Count != 1)
            throw new Exception("目标类型接口数变化: " + tt.Interfaces.Count);

        // 收集输出目标类型的方法；旧方法必须全部仍在，替换/新增方法核对指令数
        Dictionary<string, MethodDef> outMethods = new Dictionary<string, MethodDef>(StringComparer.Ordinal);
        HashSet<string> outMethodNames = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < tt.Methods.Count; i++)
        {
            MethodDef m = tt.Methods[i];
            string name = m.Name.ToString();
            string key = MethodKey(name, m.MethodSig);
            if (outMethods.ContainsKey(key))
                throw new Exception("输出目标类型方法(名+签名)重复: " + key);
            outMethods.Add(key, m);
            outMethodNames.Add(name);
            int wantInstr;
            if (_expectedInstrCounts.TryGetValue(key, out wantInstr))
            {
                if (!m.HasBody || m.Body == null || m.Body.Instructions.Count != wantInstr)
                    throw new Exception("替换/新增方法指令数不符: " + key + " 期望 " + wantInstr +
                                        " 实际 " + (m.HasBody && m.Body != null ? m.Body.Instructions.Count : -1));
            }
            else
            {
                // 未改动的旧方法：body/指令数必须与原 exe 一致
                int origInstr;
                if (!_origTargetMethodInstrCounts.TryGetValue(key, out origInstr))
                    throw new Exception("输出目标类型出现未知方法（既非原方法也非 shim 映射方法）: " + key);
                int got = m.HasBody && m.Body != null ? m.Body.Instructions.Count : 0;
                if (got != origInstr)
                    throw new Exception("旧方法 body 被意外改动: " + key + " 原指令数 " + origInstr + " 实际 " + got);
            }
        }
        // 原目标类型的所有旧方法必须全部仍在
        int missingOld = 0;
        for (int i = 0; i < _origTargetMethodKeys.Count; i++)
        {
            if (!outMethods.ContainsKey(_origTargetMethodKeys[i]))
            {
                missingOld++;
                if (missingOld <= 5) Console.WriteLine("  FAIL 旧方法丢失: " + _origTargetMethodKeys[i]);
            }
        }
        if (missingOld != 0) throw new Exception("有 " + missingOld + " 个原目标类型方法在输出中丢失");
        Console.WriteLine("  OK 原目标类型 " + _origTargetMethodKeys.Count + " 个旧方法全部保留（body 未改动）");

        // 10 个保留 + 87 个 ob_ 新方法与 shim 对应方法签名/指令数一致
        int checkedCount = 0;
        for (int i = 0; i < _shimType.Methods.Count; i++)
        {
            MethodDef sm = _shimType.Methods[i];
            MethodDef tm;
            if (!_methodMap.TryGetValue(sm, out tm))
                throw new Exception("shim 方法未映射: " + sm.FullName);
            string key = MethodKey(tm.Name.ToString(), RemapMethodSig(sm.MethodSig));
            if (!outMethods.ContainsKey(key))
                throw new Exception("输出目标类型缺少 shim 映射方法: " + key);
            checkedCount++;
        }
        Console.WriteLine("  OK 目标类型 " + checkedCount + " 个 shim 映射方法（名称+签名+指令数）与 shim 一致");

        // 旧字段 / 旧嵌套类型必须全部仍在
        HashSet<string> outFieldNames = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < tt.Fields.Count; i++) outFieldNames.Add(tt.Fields[i].Name.ToString());
        for (int i = 0; i < _origTargetFieldKeys.Count; i++)
        {
            if (!outFieldNames.Contains(_origTargetFieldKeys[i]))
                throw new Exception("原目标类型字段丢失: " + _origTargetFieldKeys[i]);
        }
        HashSet<string> outNestedFullNames = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < tt.NestedTypes.Count; i++) outNestedFullNames.Add(tt.NestedTypes[i].FullName);
        for (int i = 0; i < _origTargetNestedNames.Count; i++)
        {
            if (!outNestedFullNames.Contains(_origTargetNestedNames[i]))
                throw new Exception("原目标类型嵌套类型丢失: " + _origTargetNestedNames[i]);
        }
        Console.WriteLine("  OK 原目标类型 " + _origTargetFieldKeys.Count + " 个字段 / " +
                          _origTargetNestedNames.Count + " 个嵌套类型全部保留");

        // 其它类型也检查 body（除 PInvoke/Runtime 方法）
        int totalMethods = 0;
        int bodyMethods = 0;
        bool anyBadBody = false;
        foreach (TypeDef t in outMod.GetTypes())
        {
            for (int i = 0; i < t.Methods.Count; i++)
            {
                MethodDef m = t.Methods[i];
                totalMethods++;
                if (m.RVA == 0) continue;
                bodyMethods++;
                if (!m.HasBody || m.Body == null || m.Body.Instructions.Count == 0)
                {
                    if (!anyBadBody)
                        Console.WriteLine("  FAIL 空 body: " + m.FullName);
                    anyBadBody = true;
                }
            }
        }
        if (anyBadBody) throw new Exception("输出 exe 存在空 body（RVA 非 0 但无指令）");
        Console.WriteLine("  OK 模块方法 " + totalMethods + " 个，其中 RVA 非 0 的 " + bodyMethods + " 个均有指令");

        // 不得残留 OneBotShim 引用
        foreach (AssemblyRef ar in outMod.GetAssemblyRefs())
        {
            if (string.Equals(ar.Name.ToString(), ShimAssemblyName, StringComparison.Ordinal))
                throw new Exception("输出 exe 残留 OneBotShim AssemblyRef");
        }
        int shimTypeRefs = 0;
        foreach (TypeRef tr in outMod.GetTypeRefs())
        {
            string f;
            try { f = tr.FullName; }
            catch (Exception) { continue; }
            if (f != null && f.StartsWith("OneBotShim", StringComparison.Ordinal))
            {
                shimTypeRefs++;
                if (shimTypeRefs <= 5) Console.WriteLine("  FAIL 残留 TypeRef: " + f);
            }
        }
        if (shimTypeRefs != 0) throw new Exception("输出 exe 残留 " + shimTypeRefs + " 个 OneBotShim TypeRef");
        Console.WriteLine("  OK 无 OneBotShim 程序集/类型残留");

        // 注意：additive 策略下原 exe 的不可达垃圾 IL（null 操作数）按原样保留，
        // 因此这里不再检查 null 操作数 —— rt4/rt6 已证明 NoThrow writer 写回后 CLR 可正常启动。

        // 目标类型 3 个属性 getter 存在
        string[] requiredGetters = new string[] { "get_IsConnected", "get_StatusText", "get_StatusBackground" };
        for (int i = 0; i < requiredGetters.Length; i++)
        {
            if (!outMethodNames.Contains(requiredGetters[i]))
                throw new Exception("目标类型缺少属性 getter: " + requiredGetters[i]);
        }
        // CommandHandler 保持原有成员（.ctor/Invoke/BeginInvoke/EndInvoke + 混淆器加的 .cctor）
        TypeDef ch = null;
        for (int i = 0; i < tt.NestedTypes.Count; i++)
        {
            if (string.Equals(tt.NestedTypes[i].Name.ToString(), "CommandHandler", StringComparison.Ordinal))
            {
                ch = tt.NestedTypes[i];
                break;
            }
        }
        if (ch == null) throw new Exception("输出目标类型缺少嵌套类型 CommandHandler");
        List<string> chKeys = new List<string>();
        for (int i = 0; i < ch.Methods.Count; i++)
            chKeys.Add(MethodKey(ch.Methods[i].Name.ToString(), ch.Methods[i].MethodSig));
        chKeys.Sort(StringComparer.Ordinal);
        Console.WriteLine("  CommandHandler methods=" + ch.Methods.Count + " (原=" + _origCommandHandlerMethodKeys.Count + ")");
        bool chSame = chKeys.Count == _origCommandHandlerMethodKeys.Count;
        for (int i = 0; chSame && i < chKeys.Count; i++)
            if (!string.Equals(chKeys[i], _origCommandHandlerMethodKeys[i], StringComparison.Ordinal)) chSame = false;
        if (!chSame) throw new Exception("CommandHandler 成员签名被改变（原 " + _origCommandHandlerMethodKeys.Count + "，输出 " + chKeys.Count + "）");
        Console.WriteLine("  OK CommandHandler 成员签名未变（含原委托 .cctor）");

        // [已禁用] HTTP BaseAddress 修复校验：补丁未注入（原因见上）。

        if (_fallbackMemberRefCount > 0)
            _warnings.Add("共有 " + _fallbackMemberRefCount + " 个 MemberRef 走外部重建路径（多为 mscorlib/Newtonsoft 外部成员，属正常）");

        Console.WriteLine("  OK 回读校验全部通过");
    }

    // ----------------------------------------------------------------- utils

    // 原 exe 被混淆器注入了不可达的垃圾 IL：约 3752 个方法各含一条 `call`，
    // 其 4 字节操作数不是合法 metadata token（被混淆器替换成 ASCII 垃圾字节），
    // dnlib 读入后 operand == null，直接写回时 dnlib 会报 "Instruction operand is null"。
    // 经可达性分析确认这些指令全部不可达（每条都被前一条 br.s 跳过，且不是 EH 边界），
    // 写回前原地替换为 nop（保持指令数不变、不改变任何可达路径行为）。
    private static void NormalizeUnreachableJunkOperands()
    {
        List<TypeDef> types = new List<TypeDef>();
        CollectTypes(_target.Types, types);
        int methodsChanged = 0;
        int replaced = 0;
        int reachableBad = 0;
        int ehBad = 0;
        List<string> badSamples = new List<string>();
        for (int ti = 0; ti < types.Count; ti++)
        {
            TypeDef t = types[ti];
            IList<MethodDef> methods = t.Methods;
            for (int mi = 0; mi < methods.Count; mi++)
            {
                MethodDef m = methods[mi];
                if (!m.HasBody || m.Body == null) continue;
                CilBody body = m.Body;
                IList<Instruction> instrs = body.Instructions;
                if (instrs.Count == 0) continue;
                bool hasNull = false;
                for (int i = 0; i < instrs.Count; i++)
                {
                    Instruction ins = instrs[i];
                    if (ins.Operand == null && ins.OpCode.OperandType != OperandType.InlineNone) { hasNull = true; break; }
                }
                if (!hasNull) continue;

                HashSet<Instruction> reachable = ComputeReachableInstructions(body);
                HashSet<Instruction> boundaries = new HashSet<Instruction>();
                for (int i = 0; i < body.ExceptionHandlers.Count; i++)
                {
                    ExceptionHandler eh = body.ExceptionHandlers[i];
                    if (eh.TryStart != null) boundaries.Add(eh.TryStart);
                    if (eh.TryEnd != null) boundaries.Add(eh.TryEnd);
                    if (eh.HandlerStart != null) boundaries.Add(eh.HandlerStart);
                    if (eh.HandlerEnd != null) boundaries.Add(eh.HandlerEnd);
                    if (eh.FilterStart != null) boundaries.Add(eh.FilterStart);
                }

                bool changed = false;
                for (int i = 0; i < instrs.Count; i++)
                {
                    Instruction ins = instrs[i];
                    if (ins.Operand != null || ins.OpCode.OperandType == OperandType.InlineNone) continue;
                    if (reachable.Contains(ins))
                    {
                        reachableBad++;
                        if (badSamples.Count < 10)
                            badSamples.Add(m.FullName + " IL_" + ins.Offset.ToString("X4") + " " + ins.OpCode.Name + " (reachable)");
                        continue;
                    }
                    if (boundaries.Contains(ins))
                    {
                        ehBad++;
                        if (badSamples.Count < 10)
                            badSamples.Add(m.FullName + " IL_" + ins.Offset.ToString("X4") + " " + ins.OpCode.Name + " (eh-boundary)");
                        continue;
                    }
                    instrs[i] = Instruction.Create(OpCodes.Nop);
                    replaced++;
                    changed = true;
                }
                if (changed) methodsChanged++;
            }
        }
        Console.WriteLine("  null 操作数替换为 nop: " + replaced + " 条，涉及方法 " + methodsChanged + " 个");
        Console.WriteLine("  未替换（可达/EH 边界）: 可达 " + reachableBad + "，EH " + ehBad);
        for (int i = 0; i < badSamples.Count; i++) Console.WriteLine("  BAD " + badSamples[i]);
        if (reachableBad + ehBad != 0)
            throw new Exception("存在可达或 EH 边界的 null 操作数，无法安全归一化（可达 " + reachableBad + "，EH " + ehBad + "）");
        if (replaced == 0) _warnings.Add("未发现需要归一化的 null 操作数指令");
    }

    private static void CollectTypes(IList<TypeDef> types, List<TypeDef> list)
    {
        for (int i = 0; i < types.Count; i++)
        {
            list.Add(types[i]);
            CollectTypes(types[i].NestedTypes, list);
        }
    }

    private static HashSet<Instruction> ComputeReachableInstructions(CilBody body)
    {
        HashSet<Instruction> reachable = new HashSet<Instruction>();
        Queue<Instruction> queue = new Queue<Instruction>();
        IList<Instruction> instrs = body.Instructions;
        if (instrs.Count == 0) return reachable;
        AddReachable(instrs[0], reachable, queue);
        for (int i = 0; i < body.ExceptionHandlers.Count; i++)
        {
            ExceptionHandler eh = body.ExceptionHandlers[i];
            if (eh.TryStart != null) AddReachable(eh.TryStart, reachable, queue);
            if (eh.HandlerStart != null) AddReachable(eh.HandlerStart, reachable, queue);
            if (eh.FilterStart != null) AddReachable(eh.FilterStart, reachable, queue);
        }
        while (queue.Count > 0)
        {
            Instruction ins = queue.Dequeue();
            int idx = instrs.IndexOf(ins);
            Instruction next = (idx >= 0 && idx + 1 < instrs.Count) ? instrs[idx + 1] : null;
            OpCode op = ins.OpCode;
            if (op.Code == Code.Switch)
            {
                Instruction[] targets = ins.Operand as Instruction[];
                if (targets != null)
                {
                    for (int i = 0; i < targets.Length; i++) AddReachable(targets[i], reachable, queue);
                }
                AddReachable(next, reachable, queue);
                continue;
            }
            if (op.FlowControl == FlowControl.Branch)
            {
                AddReachable(ins.Operand as Instruction, reachable, queue);
                continue;
            }
            if (op.FlowControl == FlowControl.Cond_Branch)
            {
                AddReachable(ins.Operand as Instruction, reachable, queue);
                AddReachable(next, reachable, queue);
                continue;
            }
            if (op.FlowControl == FlowControl.Return || op.FlowControl == FlowControl.Throw || op.FlowControl == FlowControl.Break)
                continue;
            if (op.Code == Code.Endfinally || op.Code == Code.Endfilter) continue;
            AddReachable(next, reachable, queue);
        }
        return reachable;
    }

    private static void AddReachable(Instruction ins, HashSet<Instruction> reachable, Queue<Instruction> queue)
    {
        if (ins == null) return;
        if (reachable.Add(ins)) queue.Enqueue(ins);
    }

    private static void Section(string title)
    {
        Console.WriteLine();
        Console.WriteLine("=== " + title + " ===");
    }

    private static string Sha256File(string path)
    {
        using (FileStream fs = File.OpenRead(path))
        using (SHA256 sha = SHA256.Create())
        {
            byte[] hash = sha.ComputeHash(fs);
            StringBuilder sb = new StringBuilder(hash.Length * 2);
            for (int i = 0; i < hash.Length; i++) sb.Append(hash[i].ToString("X2"));
            return sb.ToString();
        }
    }

    /// <summary>收集 dnlib writer 的非致命事件（NoThrow 语义下仍记录，便于验收确认 PreserveAll 路径无错误）。</summary>
    private sealed class CollectingLogger : ILogger
    {
        public int ErrorCount;
        public int WarningCount;
        public List<string> Messages = new List<string>();

        public bool IgnoresEvent(LoggerEvent loggerEvent)
        {
            return loggerEvent == LoggerEvent.Info || loggerEvent == LoggerEvent.Verbose || loggerEvent == LoggerEvent.VeryVerbose;
        }

        public void Log(object sender, LoggerEvent loggerEvent, string format, object[] args)
        {
            if (loggerEvent == LoggerEvent.Error) ErrorCount++;
            else if (loggerEvent == LoggerEvent.Warning) WarningCount++;
            if (Messages.Count < 20)
            {
                string msg = (args == null || args.Length == 0) ? format : string.Format(format, args);
                Messages.Add(loggerEvent.ToString() + ": " + msg);
            }
        }
    }
}
