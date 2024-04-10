﻿
using Telegram.Bot.Types;
using TelegramBot.Class;

namespace TelegramBot.Interfaces
{
    public interface IExtension
    {
        /// <summary>
        /// Command list which extension can support
        /// </summary>
        Command[] Commands { get; }
        /// <summary>
        /// Extension Name
        /// </summary>
        string Name { get; }
        void Handle(InputCommand command, Update update, TUser querier, Group group);
        void Init();
        void Save();
    }
}