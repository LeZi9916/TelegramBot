﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using YamlDotNet.Serialization;
using File = System.IO.File;

namespace NekoBot.Types;
public class Proxy
{
    public bool UseProxy { get; set; } = false;
    public string? Address { get; set; } = null;
}
public class Analyzer
{
    public long TotalHandleCount { get; set; } = 0;
    public long TotalHandleTime { get; set; } = 0;
}
public class BotIdentity
{
    public string Token { get; set; } = "";
    public Proxy Proxy { get; set; } = new();
}
public class BotInfo
{
    public required ITelegramBotClient Client { get; set; }
    public required string Username { get; set; }
    public BotCommand[] BotCommands { get; set; } = Array.Empty<BotCommand>();
}
public struct DatabaseInfo
{
    /// <summary>
    /// Path of database file
    /// <para>FileDatabase Only</para>
    /// </summary>
    public string? Path { get; set; }
    public string? Address { get; set; }
    public int? Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Database { get; set; }
    /// <summary>
    /// Type of database
    /// </summary>
    public required DatabaseType Type { get; set; }
}
public class Config
{
    [YamlIgnore]
    public static DateTime Up { get; set; }
    [YamlIgnore]
    public static string AppPath { get => Environment.CurrentDirectory; }
    [YamlIgnore]
    public static string LogsPath { get => Path.Combine(AppPath, "logs"); }
    [YamlIgnore]
    public static string DatabasePath { get => Path.Combine(AppPath, "Database"); }
    [YamlIgnore]
    public static string TempPath { get => Path.Combine(AppPath, "Temp"); }
    [YamlIgnore]
    public static string ScriptPath { get => Path.Combine(AppPath, "Scripts"); }
    [YamlIgnore]
    public static string LogFile { get => Path.Combine(LogsPath, $"{Up.ToString("yyyy-MM-dd HH-mm-ss")}.log"); }
    [YamlIgnore]
    public static string ConfigPath { get => Path.Combine(AppPath, "NekoBot.conf"); }

    public HotpAuthenticator Authenticator { get; set; } = new HotpAuthenticator();
    public DatabaseInfo? Database { get; set; } = null;
    public bool DbAutoSave { get; set; } = true;
    public int AutoSaveInterval { get; set; } = 600;
    public List<BotIdentity> BotIdentitys { get; set; } = new();
    public Analyzer Analyzer { get; set; } = new();
    public string[] Assembly { get; set; } = Array.Empty<string>();
    [YamlIgnore]
    bool isRunning = false;

    internal async void AutoSave()
    {
        if (!Core.Config.DbAutoSave || isRunning)
            return;
        while (true)
        {
            isRunning = true;
            var fileStream = File.Open(ConfigPath, FileMode.Create);
            await fileStream.WriteAsync(Encoding.UTF8.GetBytes(Serializer.Yaml.Serialize(this)));
            fileStream.Close();
            Core.Debug(DebugType.Info, "Config saved");
            await Task.Delay(Core.Config.AutoSaveInterval * 1000);
        }
    }
    public static void Check()
    {
        var subScript = new string[] { "Database", "Module", "Handler", "Serializer", "Library" };
        if (!Directory.Exists(LogsPath))
            Directory.CreateDirectory(LogsPath);
        if (!Directory.Exists(DatabasePath))
            Directory.CreateDirectory(DatabasePath);
        if (!Directory.Exists(TempPath))
            Directory.CreateDirectory(TempPath);
        if (!Directory.Exists(ScriptPath))
            Directory.CreateDirectory(ScriptPath);

        foreach (var dir in subScript)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(Path.Combine(ScriptPath, dir));
        }
    }
}
