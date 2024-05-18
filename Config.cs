﻿using AquaTools;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using File = System.IO.File;
using Telegram.Bot;
using Group = NekoBot.Types.Group;
using NekoBot.Types;

namespace NekoBot
{
    public static class Config
    {
        static DateTime Up = DateTime.Now;
        public static string AppPath { get => Environment.CurrentDirectory; }
        public static string LogsPath { get => Path.Combine(AppPath, "logs"); }
        public static string DatabasePath { get => Path.Combine(AppPath, "Database"); }
        public static string TempPath { get => Path.Combine(AppPath, "Temp"); }
        public static string LogFile { get => Path.Combine(LogsPath, $"{Up.ToString("yyyy-MM-dd HH-mm-ss")}.log"); }
        public static HotpAuthenticator Authenticator { get; set; } = new HotpAuthenticator();

        public static bool EnableAutoSave { get; set; } = true;
        public static int AutoSaveInterval { get; set; } = 900000;

        public static long TotalHandleCount { get; set; } = 0;
        public static List<long> TimeSpentList { get; set; } = new();

        public static List<long> GroupIdList { get; set; } = new();
        public static List<Group> GroupList { get; set; } = new();
        public static List<long> UserIdList { get; set; } = new() { 1136680302 };
        public static List<User> TUserList { get; set; } = new()
        {
            new User()
            {
                Id = 1136680302,
                FirstName = "Tanuoxi",
                LastName = null,
                Level = Permission.Root
            }
        };
        public static List<KeyChip> keyChips { get; set; } = new()
        {
            new KeyChip()
            {
                PlaceId = 2120,
                PlaceName = "SUPER101潮漫北流店",
                RegionId = 28,
                RegionName = "广西",
                KeyChipId = "A63E-01E14596415"
            },
            new KeyChip()
            {
                PlaceId = 1,
                PlaceName = "Unknow",
                RegionId = 1,
                RegionName = "Unknow",
                KeyChipId = "A63E-01E14150010"
            }
        };

        public static void Init()
        {
            Check();
            if (File.Exists(Path.Combine(DatabasePath, "UserList.data")))
                TUserList = Load<List<User>>(Path.Combine(DatabasePath, "UserList.data"));
            if (File.Exists(Path.Combine(DatabasePath, "UserIdList.data")))
                UserIdList = Load<List<long>>(Path.Combine(DatabasePath, "UserIdList.data"));
            if (File.Exists(Path.Combine(DatabasePath, "TotalHandleCount.data")))
                TotalHandleCount = Load<long>(Path.Combine(DatabasePath, "TotalHandleCount.data"));
            if (File.Exists(Path.Combine(DatabasePath, "TimeSpentList.data")))
                TimeSpentList = Load<List<long>>(Path.Combine(DatabasePath, "TimeSpentList.data"));
            if (File.Exists(Path.Combine(DatabasePath, "GroupList.data")))
                GroupList = Load<List<Group>>(Path.Combine(DatabasePath, "GroupList.data"));
            if (File.Exists(Path.Combine(DatabasePath, "GroupIdList.data")))
                GroupIdList = Load<List<long>>(Path.Combine(DatabasePath, "GroupIdList.data"));
            if (File.Exists(Path.Combine(DatabasePath, "HotpAuthenticator.data")))
                Authenticator = Load<HotpAuthenticator>(Path.Combine(DatabasePath, "HotpAuthenticator.data"));
            if (File.Exists(Path.Combine(DatabasePath, "token.config")))
                Core.Token = File.ReadAllText(Path.Combine(DatabasePath, "token.config"));
            else
            {
                Core.Debug(DebugType.Error, "Config file isn't exist");
                Environment.Exit(-1);
            }
            AutoSave();
        }
#nullable enable
        public static Group? SearchGroup(long groupId)
        {
            if (!GroupIdList.Contains(groupId))
                return null;
            var result = GroupList.Where(u => u.Id == groupId).ToArray();

            return result[0];
        }
        public static User? SearchUser(long userId)
        {
            if (!UserIdList.Contains(userId))
                return null;

            var result = TUserList.Where(u => u.Id == userId).ToArray();

            return result[0];
        }
#nullable disable
        public static void AddUser(User user)
        {
            TUserList.Add(user);
            UserIdList.Add(user.Id);
            Save(Path.Combine(DatabasePath, "UserList.data"), TUserList);
            Save(Path.Combine(DatabasePath, "UserIdList.data"), UserIdList);
        }
        public static async void AutoSave()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(AutoSaveInterval);
                    if (!EnableAutoSave)
                        break;
                    Core.Debug(DebugType.Debug, "Auto save data start");
                    SaveData();
                }
            });
        }
        public static async void SaveData()
        {
            Save(Path.Combine(DatabasePath, "UserList.data"), TUserList);
            Save(Path.Combine(DatabasePath, "UserIdList.data"), UserIdList);
            Save(Path.Combine(DatabasePath, "TotalHandleCount.data"), TotalHandleCount);
            Save(Path.Combine(DatabasePath, "TimeSpentList.data"), TimeSpentList);
            Save(Path.Combine(DatabasePath, "GroupList.data"), GroupList);
            Save(Path.Combine(DatabasePath, "GroupIdList.data"), GroupIdList);
            Save(Path.Combine(DatabasePath, "HotpAuthenticator.data"), Authenticator);
            ScriptManager.Save();
            Core.BotCommands = await Core.botClient.GetMyCommandsAsync();
        }
        static void Check()
        {
            if (!Directory.Exists(LogsPath))
                Directory.CreateDirectory(LogsPath);
            if (!Directory.Exists(DatabasePath))
                Directory.CreateDirectory(DatabasePath);
            if (!Directory.Exists(TempPath))
                Directory.CreateDirectory(TempPath);
        }


        public static async void Save<T>(string path, T target, bool debugMessage = true)
        {
            try
            {
                var fileStream = File.Open(path, FileMode.Create);
                await fileStream.WriteAsync(Encoding.UTF8.GetBytes(ToJsonString(target)));
                fileStream.Close();
                if (debugMessage)
                    Core.Debug(DebugType.Info, $"Saved File {path}");
            }
            catch (Exception e)
            {

                Core.Debug(DebugType.Error, $"Saving File \"{path}\" Failure:\n{e.Message}");
            }
        }
        public static T Load<T>(string path) where T : new()
        {
            try
            {
                var json = File.ReadAllText(path);
                var result = FromJsonString<T>(json);

                Core.Debug(DebugType.Info, $"Loaded File: {path}");
                return result;
            }
            catch (Exception e)
            {
                Core.Debug(DebugType.Error, $"Loading \"{path}\" Failure:\n{e.Message}");
                return new T();
            }
        }
        public static void Load<T>(string path, out T obj)
        {
            try
            {
                var json = File.ReadAllText(path);
                obj = FromJsonString<T>(json);

                Core.Debug(DebugType.Info, $"Loaded File: {path}");

            }
            catch (Exception e)
            {
                Core.Debug(DebugType.Error, $"Loading \"{path}\" Failure:\n{e.Message}");
                obj = default;
            }
        }
        public static string ToJsonString<T>(T target) => JsonSerializer.Serialize(target);
        public static T FromJsonString<T>(string json) => JsonSerializer.Deserialize<T>(json);
    }
}
