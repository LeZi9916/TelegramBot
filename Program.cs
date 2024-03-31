﻿using System;
using System.Threading.Tasks;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Diagnostics;
using System.Net.Http;
using System.Net;
using TelegramBot.Class;
using System.IO;
using static TelegramBot.Image;
using System.Collections.Generic;
using static TelegramBot.ChartHelper;

namespace TelegramBot
{
    enum DebugType
    {
        Debug,
        Info,
        Warning,        
        Error
    }
    internal partial class Program
    {
        internal static TelegramBotClient botClient;
        internal static string Token = "";
        static string BotUsername = "";
        static DateTime startTime;
        internal static BotCommand[] BotCommands;
        static void Test()
        {
            List<KNode> data = new List<KNode>
            {
                new KNode { Date = DateTime.Now.AddDays(-4), Open = 100, High = 110, Low = 90, Close = 105 },
                new KNode { Date = DateTime.Now.AddDays(-3), Open = 105, High = 115, Low = 95, Close = 110 },
                new KNode { Date = DateTime.Now.AddDays(-2), Open = 110, High = 120, Low = 100, Close = 115 },
                new KNode { Date = DateTime.Now.AddDays(-1), Open = 115, High = 125, Low = 105, Close = 120 },
                new KNode { Date = DateTime.Now, Open = 120, High = 130, Low = 110, Close = 125 },
                new KNode { Date = DateTime.Now, Open = 120, High = 130, Low = 110, Close = 125 },
            };
            List<int> XSamples = new List<int>()
            {
                1210,1220,1230,1240,1250,1260
            };
            List<int> YSamples = new List<int>()
            {
                130,115,100,85,70
            };
            var helper = new CandlestickChartHelper<int,int>(data, XSamples, YSamples);
            helper.Draw("KLineChart.png");
        }
        static void Main(string[] args)
        {
            Test();
            Monitor.Init();
            Config.Init();
            MaiMonitor.Init();
            startTime = DateTime.Now;
            Config.Load(Path.Combine(Config.DatabasePath, "Proxy.config"),out string proxyStr);
            HttpClient httpClient = new(new SocketsHttpHandler
            {
                //Proxy = Config.Load<WebProxy>(Path.Combine(Config.DatabasePath, "Proxy.config")),
                Proxy = new WebProxy(proxyStr)
                {
                    Credentials = new NetworkCredential("", "")
                },
                UseProxy = true,
            }); ;
            botClient = new TelegramBotClient(Token, httpClient);
            botClient.ReceiveAsync(
                updateHandler: UpdateHandleAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: new ReceiverOptions()
                {
                    AllowedUpdates = Array.Empty<UpdateType>()
                }
            );
            Debug(DebugType.Info, "Connecting to telegram...");

            while (BotUsername == "")
            {
                
                try
                {
                    BotUsername = botClient.GetMeAsync().Result.Username;
                    BotCommands = botClient.GetMyCommandsAsync().Result;
                    if (string.IsNullOrEmpty(BotUsername))
                        Debug(DebugType.Info, "Connect failure,retrying...");
                    else
                    {
                        Debug(DebugType.Info, "Connect Successful");
                        break;
                    }
                }
                catch
                {
                    Debug(DebugType.Info, "Connect failure,retrying...");
                }
            }            
            while(true)
                Console.ReadKey();
        }
        static async void MessageHandleAsync(ITelegramBotClient botClient, Update update)
        {
            await Task.Run(() =>
            {
                Stopwatch stopwatch = new();
                stopwatch.Reset();
                stopwatch.Start();
                try
                {                    
                    var message = update.Message;
                    var chat = message.From;
                    var userId = message.From.Id;

                    var text = message.Text is null ? message.Caption ?? "" : message.Text;
                    CommandPreHandle(text.Split(" ",StringSplitOptions.RemoveEmptyEntries), update);

                    Debug(DebugType.Debug, $"Received message:\n" +
                        $"Chat Type: {message.Chat.Type}\n" +
                        $"Message Type: {message.Type}\n" +
                        $"Form User: {chat.FirstName} {chat.LastName}[@{chat.Username}]({userId})\n" +
                        $"Content: {message.Text ?? ""}\n");
                }
                catch(Exception e)
                {
                    Debug(DebugType.Error, $"Failure to receive message : \n{e.Message}\n{e.StackTrace}");
                }
                stopwatch.Stop();
                Config.TotalHandleCount++;
                Config.TimeSpentList.Add((int)stopwatch.ElapsedMilliseconds);
            });
        }
        static async Task UpdateHandleAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {            
            try
            {
                FindUser(update);
                FindGroup(update);

                switch (update.Type)
                {
                    case UpdateType.Message:
                        MessageHandleAsync(botClient, update);
                        break;
                    case UpdateType.InlineQuery:
                        break;
                    case UpdateType.ChosenInlineResult:
                        break;
                    case UpdateType.CallbackQuery:
                        break;
                    case UpdateType.EditedMessage:
                        break;
                    case UpdateType.ChannelPost:
                        break;
                    case UpdateType.EditedChannelPost:
                        break;
                    case UpdateType.ShippingQuery:
                        break;
                    case UpdateType.PreCheckoutQuery:
                        break;
                    case UpdateType.Poll:
                        break;
                    case UpdateType.PollAnswer:
                        break;
                    case UpdateType.MyChatMember:
                        break;
                    case UpdateType.ChatMember:
                        break;
                    case UpdateType.ChatJoinRequest:
                        break;
                    case UpdateType.Unknown:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Debug(DebugType.Error, $"Failure to handle message : \n{e.Message}\n{e.StackTrace}");
                return;
            }            
            
            await Task.Delay(0);
        }
        static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception e, CancellationToken cancellationToken)
        {
            Debug(DebugType.Error, $"From internal exception : \n{e.Message}\n{e.StackTrace}");
            return Task.CompletedTask;
        }
        static async void FindUser(Update update)
        {
            await Task.Run(() => 
            {
                //var message = update.Message ?? update.EditedMessage ?? update.ChannelPost;
                //if (message is null)
                //    return;

                //User[] userList = new User[4];
                User[] userList = TUser.GetUsers(update);
                //userList[0] = message.From;
                //userList[1] = message.ForwardFrom;
                //if (message.ReplyToMessage is not null)
                //{
                //    userList[2] = message.ReplyToMessage.From;
                //    userList[3] = message.ReplyToMessage.ForwardFrom;
                //}

                if (userList is null)
                    return;

                foreach (var user in userList)
                {
                    if (user is null)
                        continue;
                    if (Config.UserIdList.Contains(user.Id))
                    {
                        TUser.Update(update);
                        continue;
                    }

                    Config.AddUser(TUser.FromUser(user));

                    Debug(DebugType.Info, $"Find New User:\n" +
                    $"Name: {user.FirstName} {user.LastName}\n" +
                    $"isBot: {user.IsBot}\n" +
                    $"Username: {user.Username}\n" +
                    $"isPremium: {user.IsPremium}");
                }
            });
        }
        static async void FindGroup(Update update)
        {
            await Task.Run(() => 
            {
                try
                {
                    var chat = update.Message?.Chat ?? update.EditedMessage?.Chat;

                    if (chat is null)
                        return;
                    else if (chat.Type is not (ChatType.Group or ChatType.Supergroup))
                        return;

                    var groupId = chat.Id;

                    if (Config.GroupIdList.Contains(groupId))
                        return;

                    var group = new Group()
                    {
                        Id = groupId,
                        Name = chat.Title,
                        Username = chat.Username,                        
                    };
                    Config.GroupList.Add(group);
                    Config.GroupIdList.Add(groupId);
                    Config.SaveData();
                    Debug(DebugType.Debug, $"Find New Group:\n" +
                        $"Name: {group.Name}\n" +
                        $"Id: {groupId}\n" +
                        $"Username：{group.Username}\n");
                }
                catch(Exception e)
                {
                    Debug(DebugType.Error, $"Find a new group,but cannon add to list: \n{e.Message}\n{e.StackTrace}");
                }
            });
        }
        public static async void Debug(DebugType type, string message)
        {
            await Task.Run(() =>
            {
                var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var _type = type switch
                {
                    DebugType.Info => "Info",
                    DebugType.Warning => "Warning",
                    DebugType.Debug => "Debug",
                    DebugType.Error => "Error",
                    _ => "Unknow"
                };

                if (_type == "Unknow")
                    return;

                Console.WriteLine($"[{time}][{_type}] {message}");
                LogManager.WriteLog($"[{time}][{_type}] {message}");
            });
        }
    }
}
