using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ServerlessSlackBot;

public static class Utilities
{
    public static async Task<string> ReadStreamAsync(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var text = await reader.ReadToEndAsync();
        return text;
    }

    // ログ出力
    // logger 自体のLoglevelも使えるが、messageに含まれてtextPayload扱いになってしまうのを避けられなかった。
    // LoggerでjsonPayloadの機能を使えるようにあえて標準出力向けをラップして利用する。
    // SEE: https://cloud.google.com/functions/docs/monitoring/logging?hl=ja
    // SEE: https://cloud.google.com/logging/docs/reference/v2/rest/v2/LogEntry#logseverity
    private static void Log(string message, Object datas, LogLevel level)
    {
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(new
        {
            message,
            severity = level.ToString(),
            datas
        });
        Console.WriteLine(json);
    }
    
    public static void LogTrace(string message, Object datas = null)
    {
        Log(message, datas, LogLevel.Trace);
    }
    
    public static void LogDebug(string message, Object datas = null)
    {
        Log(message, datas, LogLevel.Debug);
    }
    
    public static void LogInformation(string message, Object datas = null)
    {
        Log(message, datas, LogLevel.Information);
    }
    
    public static void LogWarning(string message, Object datas = null)
    {
        Log(message, datas, LogLevel.Warning);
    }
    
    public static void LogError(string message, Object datas = null)
    {
        Log(message, datas, LogLevel.Error);
    }
    
    public static void LogCritical(string message, Object datas = null)
    {
        Log(message, datas, LogLevel.Critical);
    }
}