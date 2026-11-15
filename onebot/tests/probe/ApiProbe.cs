using System;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

class ApiProbe
{
    static Task<Robot.Common.CommandExecResult> Handler(JObject body)
    {
        Console.WriteLine("HANDLER CALLED: " + body.ToString(Newtonsoft.Json.Formatting.None));
        var tcs = new TaskCompletionSource<Robot.Common.CommandExecResult>();
        tcs.SetResult(null);
        return tcs.Task;
    }

    static void Main()
    {
        Console.WriteLine("Is64BitProcess=" + Environment.Is64BitProcess);
        Console.WriteLine("BaseDir=" + AppDomain.CurrentDomain.BaseDirectory);

        Type cer = typeof(Robot.Common.CommandExecResult);
        Console.WriteLine("=== Robot.Common.CommandExecResult: " + cer.FullName + " public=" + cer.IsPublic + " base=" + cer.BaseType);
        foreach (ConstructorInfo ci in cer.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            Console.WriteLine("CTOR " + ci);
        foreach (FieldInfo fi in cer.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            Console.WriteLine("FIELD " + fi.FieldType.FullName + " " + fi.Name + " public=" + fi.IsPublic);
        foreach (PropertyInfo pi in cer.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            Console.WriteLine("PROP " + pi.PropertyType.FullName + " " + pi.Name + " get=" + (pi.GetGetMethod(true) != null) + " set=" + (pi.GetSetMethod(true) != null));
        foreach (MethodInfo mi in cer.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            Console.WriteLine("METH " + mi);

        Type handlerT = typeof(WeComConsoleServer.Core.WxWebSocketClient.CommandHandler);
        Console.WriteLine("=== CommandHandler: " + handlerT.FullName + " public=" + handlerT.IsPublic);
        Console.WriteLine("Invoke: " + handlerT.GetMethod("Invoke"));

        Type wx = typeof(WeComConsoleServer.Core.WxWebSocketClient);
        Console.WriteLine("=== WxWebSocketClient: " + wx.FullName + " public=" + wx.IsPublic + " base=" + wx.BaseType);
        foreach (ConstructorInfo ci in wx.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            Console.WriteLine("CTOR " + ci);
        foreach (EventInfo ei in wx.GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            Console.WriteLine("EVENT " + ei.EventHandlerType.FullName + " " + ei.Name);

        Console.WriteLine("=== instantiate CommandExecResult ===");
        try
        {
            object o = Activator.CreateInstance(cer);
            Console.WriteLine("Activator OK: " + o);
        }
        catch (Exception ex) { Console.WriteLine("Activator FAIL: " + ex.GetType().Name + ": " + ex.Message); }

        Console.WriteLine("=== construct handler + WxSocketClient ===");
        try
        {
            var handler = new WeComConsoleServer.Core.WxWebSocketClient.CommandHandler(Handler);
            Console.WriteLine("handler OK: " + handler.Target + " / " + handler.Method);
            var shim = new OneBotShim.WxSocketClient("test", "", (Func<string>)null, (Func<string>)null, (Func<string>)null, handler);
            Console.WriteLine("shim ctor OK. IsConnected=" + shim.IsConnected + " StatusText=" + shim.StatusText + " StatusBackground=" + shim.StatusBackground);
            Console.WriteLine("TrySendEvent(no self_id 11041)=" + shim.TrySendEvent("{\"type\":11041,\"client\":1,\"data\":{\"conversation_id\":\"R:1\",\"content\":\"x\",\"sender\":\"1\",\"server_id\":1}}"));
            Console.WriteLine("TrySendEvent(bad json)=" + shim.TrySendEvent("{bad"));
            shim.Dispose();
            Console.WriteLine("dispose OK");
        }
        catch (Exception ex) { Console.WriteLine("FAIL: " + ex.GetType().Name + ": " + ex); }

        Console.WriteLine("=== test WxWebSocketClient base instantiation? ===");
        try
        {
            object o = Activator.CreateInstance(typeof(WeComConsoleServer.Core.WxWebSocketClient));
            Console.WriteLine("base Activator OK: " + o);
        }
        catch (Exception ex) { Console.WriteLine("base Activator FAIL: " + ex.GetType().Name + ": " + ex.Message); }
    }
}
