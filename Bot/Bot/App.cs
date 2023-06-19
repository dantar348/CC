using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;


namespace Bot
{
    public class App
    {
        public async Task StartProgram()
        {
            List<BotUpdate> botUpdates = new List<BotUpdate>();
            List<StickerSets> stickerSets = new List<StickerSets>();
            List<BotCommand> commandsList = new List<BotCommand>();

            commandsList.Add(new BotCommand { Command = "start", Description = "Начать использование бота" });
            commandsList.Add(new BotCommand { Command = "create", Description = "Создать Стикер Пак" });
            commandsList.Add(new BotCommand { Command = "sets", Description = "Ваши Стикер Паки" });
            commandsList.Add(new BotCommand { Command = "add", Description = "Добавить Стикеров В Пак" });

            UpdateT updateT = new UpdateT();
            string filePath = "updates.json";
            string filePathStickerSet = "stickerSet.json";

            var botClient = new TelegramBotClient("6125350302:AAECiOwG1HkjpWCO7X6_ac7j-Flxug7KjMY");

            using CancellationTokenSource cts = new();
            var me = await botClient.GetMeAsync();
            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
            };

            botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            cts.Cancel();

            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {
                if (update.Message is not { } message)
                    return;
                var chatId = update.Message.Chat.Id;


                if (update.Message.Text == "/Update")
                {
                    BotCommand[] currentCommands = await botClient.GetMyCommandsAsync();
                    Dictionary<string, BotCommand> currentCommandsDict = currentCommands.ToDictionary(cmd => cmd.Command);
                    foreach (BotCommand newCommand in commandsList)
                    {
                        if (currentCommandsDict.TryGetValue(newCommand.Command, out BotCommand existingCommand))
                        {
                            if (existingCommand.Description != newCommand.Description)
                            {
                                existingCommand.Description = newCommand.Description;
                            }
                        }
                        else
                        {
                            currentCommands = currentCommands.Append(newCommand).ToArray();
                        }
                    }
                    await botClient.SetMyCommandsAsync(currentCommands);
                }

                var _botUpdate = new BotUpdate
                {
                    text = update.Message.Text,
                    id = update.Message.Chat.Id,
                    username = update.Message.Chat.Username
                };

                botUpdates.Add(_botUpdate);
                await updateT.AppendToFileAsync(filePath, botUpdates);
                botUpdates.Clear();
                if (update.Message.Text != "Выход")
                {
                    switch (updateT.createSet)
                    {
                        case 0:
                            switch (update.Message.Text)
                            {
                                case "/create":
                                    await СreateSet(updateT.createSet);
                                    break;
                                case "/sets":
                                    await StickerSets();
                                    break;
                                case "/add":
                                    await AddStickersToSet(updateT.createSet);
                                    break;
                                default:
                                    return;
                            }
                            break;
                        case 1:
                            await СreateSet(updateT.createSet);
                            break;
                        case 2:
                            await СreateSet(updateT.createSet);
                            break;
                        case 3:
                            await СreateSet(updateT.createSet);
                            break;
                        case 5:
                            await AddStickersToSet(updateT.createSet);
                            break;
                        case 6:
                            await AddStickersToSet(updateT.createSet);
                            break;
                        default:
                            return;
                    }
                }
                else
                {
                    updateT.createSet = 0;
                    updateT.stickerSetNameGlobal = "";
                    updateT.newTitle = "";
                    return;
                }

                async Task СreateSet(int set)
                {
                    try
                    {
                        List<InputSticker> stickers = new List<InputSticker>();

                        if (set == 0)
                        {
                            await botClient.SendTextMessageAsync(chatId, "Введите название набора стикеров (title):");
                            updateT.createSet = 1;
                        }
                        else if (set == 1)
                        {
                            updateT.newTitle = update.Message.Text;
                            await botClient.SendTextMessageAsync(chatId, "Отправьте Стикер (Это Обязательно)");
                            updateT.createSet = 2;
                        }
                        else if (set == 2 && update.Message.Sticker != null)
                        {
                            string stickerSetName = $"Packs{updateT.CountElementsInJsonFile(filePathStickerSet) + 1}_by_Stickerim_Bot";
                            string newTitleUpdate = updateT.newTitle + updateT.title;
                            string stickerEmojis1 = "👍"; // Эмодзи для стикера 1
                            stickers.Add(new InputSticker(InputFile.FromFileId(update.Message.Sticker.FileId), new List<string> { stickerEmojis1 }));
                            await botClient.CreateNewStickerSetAsync(chatId, stickerSetName, newTitleUpdate, stickers, StickerFormat.Static, StickerType.Regular);
                            Console.WriteLine("Набор стикеров успешно создан!");
                            string markdownText = $"[{newTitleUpdate}]({"http://t.me/addstickers/" + stickerSetName})";
                            await botClient.SendTextMessageAsync(chatId, markdownText, parseMode: ParseMode.Markdown);
                            var _stickerSet = new StickerSets
                            {
                                stickerSetName = stickerSetName,
                                userId = chatId,
                                title = newTitleUpdate
                            };

                            stickerSets.Add(_stickerSet);
                            await updateT.AppendToFileAsync(filePathStickerSet, stickerSets);
                            stickerSets.Clear();
                            updateT.createSet = 0;
                            updateT.newTitle = "";
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(chatId, "Повторите процедуру вписав /create - вы должны отправить 1 стикер");
                            updateT.createSet = 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при получении наборов стикеров: {ex.Message}");
                    }
                }

                async Task AddStickersToSet(int set)
                {
                    if (updateT.createSet == 0)
                    {
                        await botClient.SendTextMessageAsync(chatId, "Отправьте мне ссылку на ваш набор");
                        updateT.createSet = 5;
                    }
                    else if (updateT.createSet == 5)
                    {
                        int checker = 0;
                        JArray filteredItems = await updateT.FindItemsByUserIdFromFileAsync(filePathStickerSet, chatId);

                        foreach (JObject item in filteredItems)
                        {
                            string stickerSetName = item.Value<string>("stickerSetName");
                            string stickerSetUserId = item.Value<string>("userId");
                            string stickerSetTitle = item.Value<string>("title");
                            string stickerSetUrlHttp = $"http://t.me/addstickers/{stickerSetName}";
                            string stickerSetUrlHttps = $"https://t.me/addstickers/{stickerSetName}";

                            if (stickerSetUserId == chatId.ToString())
                            {
                                if (stickerSetUrlHttp == update.Message.Text || stickerSetUrlHttps == update.Message.Text)
                                {
                                    await botClient.SendTextMessageAsync(chatId, "Отправляйте мне ваши стикеры");
                                    updateT.stickerSetNameGlobal = stickerSetName;
                                    updateT.stickerSetTitleGlobal = stickerSetTitle;
                                    updateT.createSet = 6;
                                    checker++;
                                }
                            }
                        }
                        if (checker == 0)
                        {
                            await botClient.SendTextMessageAsync(chatId, "Это не ваш набор.");
                            updateT.createSet = 0;
                        }
                    }
                    else if (updateT.createSet == 6 && update.Message.Sticker != null)
                    {
                        string stickerSetTitle = updateT.stickerSetTitleGlobal;
                        string stickerSetUrl = $"http://t.me/addstickers/{updateT.stickerSetNameGlobal}";
                        var inlineKeyboardButton = new InlineKeyboardButton(stickerSetTitle)
                        {
                            Url = stickerSetUrl,
                        };
                        string stickerEmojis1 = "👍";
                        await botClient.AddStickerToSetAsync(chatId, updateT.stickerSetNameGlobal, new InputSticker(InputFile.FromFileId(update.Message.Sticker.FileId), new List<string> { stickerEmojis1 }));
                        var inlineKeyboardMarkup = new InlineKeyboardMarkup(inlineKeyboardButton);
                        await botClient.SendTextMessageAsync(chatId, "Отправляйте мне ваши стикеры", replyMarkup: inlineKeyboardMarkup);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId, "Вы вышли из Меню добавления стикеров - отправьте вашу команду :) ");
                        updateT.stickerSetNameGlobal = "";
                        updateT.stickerSetTitleGlobal = "";
                        updateT.createSet = 0;
                    }
                }
                async Task StickerSets()
                {
                    try
                    {
                        JArray filteredItems = await updateT.FindItemsByUserIdFromFileAsync(filePathStickerSet, chatId);
                        var inlineKeyboardButtons = new List<InlineKeyboardButton>();

                        foreach (JObject item in filteredItems)
                        {
                            string stickerSetName = item.Value<string>("stickerSetName");
                            string stickerSetTitle = item.Value<string>("title");
                            string stickerSetUrl = $"http://t.me/addstickers/{stickerSetName}";
                            var inlineKeyboardButton = new InlineKeyboardButton(stickerSetTitle)
                            {
                                Url = stickerSetUrl,
                            };

                            inlineKeyboardButtons.Add(inlineKeyboardButton);
                        }

                        var inlineKeyboardMarkup = new InlineKeyboardMarkup(inlineKeyboardButtons.Select(button => new[] { button }).ToArray());
                        await botClient.SendTextMessageAsync(chatId, "Sticker Sets:", replyMarkup: inlineKeyboardMarkup);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при получении наборов стикеров: {ex.Message}");
                    }
                }
            }

            Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
            {
                var ErrorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };

                Console.WriteLine(ErrorMessage);
                return Task.CompletedTask;
            }
        }
    }
}

/*                if (message.Sticker is { } messageSticker)
                {
                    Message message2 = await botClient.SendStickerAsync(
                    chatId: chatId,
                    sticker: InputFile.FromFileId(messageSticker.FileId),
                    cancellationToken: cancellationToken);

                    var _botUpdate = new BotUpdate
                    {
                        text = update.Message.Sticker.FileId,
                        id = update.Message.Chat.Id,
                        username = update.Message.Chat.Username
                    };

                    botUpdates.Add(_botUpdate);
                    await updateT.AppendToFileAsync(filePath, botUpdates);
                    botUpdates.Clear();
                }*/

// Echo received message text
/*Message sentMessage2 = await botClient.SendTextMessageAsync(
    chatId: chatId,
    text: "You said:\n" + update.Message.Text,
    cancellationToken: cancellationToken);*/


/* var firstStickerFileId = stickerSet.Stickers.First().FileId;
await botClient.SendStickerAsync(
   chatId: chatId,
   sticker: InputFile.FromFileId(firstStickerFileId),
   cancellationToken: cancellationToken
    );*/
// Получение списка наборов стикеров

// Создание списка стикеров


// Загрузка стикера 1 на сервер Telegram и получение file_id
/*                        

                        // Загрузка стикера 2 на сервер Telegram и получение file_id
                        string stickerFileId2 = "CAACAgIAAxkBAAEB3UxkjQe5fI7lYZiodnmElaYpOxvVOwACqBwAAtUwgUrvZSzvfo3P-i8E"; // Замените на фактический file_id
                        string stickerEmojis2 = "😄"; // Эмодзи для стикера 2
                        stickers.Add(new InputSticker(InputFile.FromFileId(stickerFileId2), new List<string> { stickerEmojis2 }));*/

// Создание набора стикеров

// Send cancellation request to stop bot