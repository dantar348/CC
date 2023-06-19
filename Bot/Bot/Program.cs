using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            App app = new App();
            await app.StartProgram();
        }
    }
}












