using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using RailwayTermsGame;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace RailwayTermsGame
{
    internal class GameOrders
    {

        private TelegramBotClient client;
        private Dictionary<long, GameUser> games;
        private Keyboard keyboardM = new Keyboard();



        // Конструктор класса
        public GameOrders(TelegramBotClient client, Dictionary<long, GameUser> games)
        {
            Client = client;
            Games = games;

        }
        // Свойства
        public TelegramBotClient Client { get => client; set => client = value; }
        internal Keyboard KeyboardM { get => keyboardM; set => keyboardM = value; }
        internal Dictionary<long, GameUser> Games { get => games; set => games = value; }



        //Методы
        public async void ExecutingCommand(Message msg, GameUser gameUser)
        {


            {
                try
                {
                    // Настрока игры с  проверкой открыты ли настройки или нет 
                    if (gameUser.OpenSettings & msg.Text != CommandsMsg.closeSettings)
                    {
                        try
                        {
                            int n = int.Parse(msg.Text[0].ToString());
                            switch (n)
                            {
                                case 1:
                                    gameUser.Timer = int.Parse(msg.Text.Substring(2));
                                    break;
                                case 2:
                                    gameUser.WinningGame = int.Parse(msg.Text.Substring(2));
                                    break;
                                case 3:
                                    gameUser.TypeWords = msg.Text.Substring(2);
                                    break;
                                case 4:
                                    gameUser.WinningGameYA = int.Parse(msg.Text.Substring(2));
                                    break;

                            }
                        }
                        catch
                        {
                        }
                        await Client.DeleteMessageAsync(msg.Chat.Id, msg.Id);
                        await Client.DeleteMessageAsync(msg.Chat.Id, gameUser.SettingsInfoMessage.Id);
                        gameUser.SettingsInfoMessage = await Client.SendTextMessageAsync(msg.Chat.Id, $"Настройки:\n" +
                                                                                                      $"1. Время раунда: {gameUser.Timer} c.\n" +
                                                                                                      $"2. Победа к-во очков: {gameUser.WinningGame}.\n" +
                                                                                                      $"3. Тип слов: {gameUser.TypeWords} \n" +
                                                                                                      $"4. C YndexGPT к-во слов: {gameUser.WinningGameYA}\n" +
                                                                                                      $"Для изменения настроек введите номер пунка и введите параметр через _ например 2_70\n" +
                                                                                                      $"Пункт 3 имеет три состояния /жд/ термины /ждп/ жд термины уровень профи и /быт/ слова");
                    }
                    else
                    {


                        switch (msg.Text)
                        {
                            case "/start":
                                gameUser.BotMessageKeybord = await Client.SendMessage(msg.Chat.Id, CommandsMsg.selectAnAction, replyMarkup: keyboardM.StartMarkup);
                                break;
                            case CommandsMsg.newGame:
                                await Client.DeleteMessageAsync(msg.Chat.Id, gameUser.BotMessageKeybord.MessageId);
                                await Client.DeleteMessageAsync(msg.Chat.Id, msg.MessageId);
                                gameUser.BotMessageKeybord = await Client.SendMessage(msg.Chat.Id, CommandsMsg.selectAnAction, replyMarkup: KeyboardM.GemeMarkup);
                                gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"Счтет: {gameUser.ScoreFirstTeam}:{gameUser.ScoreSecondTeam}"));
                                gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"Отвечает Команда {gameUser.TeamNumber}:"));
                                break;
                            case CommandsMsg.completion:
                                gameUser.StopTimer();
                                await Client.DeleteMessageAsync(msg.Chat.Id, msg.MessageId);
                                await Client.DeleteMessageAsync(msg.Chat.Id, gameUser.BotMessageKeybord.MessageId);
                                gameUser.ScoreFirstTeam = 0;
                                gameUser.ScoreSecondTeam = 0;
                                gameUser.NumberGamesFirstTeam = 0;
                                gameUser.NumberGamesSecondTeam = 0;
                                gameUser.TeamNumber = 1;
                                gameUser.BotMessageKeybord = await Client.SendMessage(msg.Chat.Id, CommandsMsg.selectAnAction, replyMarkup: KeyboardM.StartMarkup);
                                foreach (Message message in gameUser.UserGameMessages)
                                {
                                    await Client.DeleteMessageAsync(msg.Chat.Id, message.MessageId);
                                }
                                gameUser.UserGameMessages.Clear();
                                foreach (Message message in gameUser.BotWordMessages)
                                {
                                    await Client.DeleteMessageAsync(msg.Chat.Id, message.MessageId);
                                }
                                gameUser.BotWordMessages.Clear();
                                break;
                            case CommandsMsg.beginning:
                                await Client.DeleteMessageAsync(msg.Chat.Id, msg.MessageId);
                                await Client.DeleteMessageAsync(msg.Chat.Id, gameUser.BotMessageKeybord.MessageId);
                                gameUser.TimerMonitoring(gameUser.Cts.Token);
                                gameUser.BotMessageKeybord = await Client.SendMessage(msg.Chat.Id, CommandsMsg.selectAnAction, replyMarkup: KeyboardM.GemePlayMarkup);
                                gameUser.StartTimer = true;
                                gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"{gameUser.GetRandomWord()}"));
                                break;
                            case CommandsMsg.skipped:
                                gameUser.UserGameMessages.Add(msg);
                                if (gameUser.TeamNumber == 1)
                                {
                                    gameUser.ScoreFirstTeam = gameUser.ScoreFirstTeam - 1;
                                }
                                else
                                {
                                    gameUser.ScoreSecondTeam = gameUser.ScoreSecondTeam - 1;
                                }
                                gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"{gameUser.GetRandomWord()}"));
                                break;
                            case CommandsMsg.guessed:
                                gameUser.UserGameMessages.Add(msg);
                                if (gameUser.TeamNumber == 1)
                                {
                                    gameUser.ScoreFirstTeam = gameUser.ScoreFirstTeam + 1;
                                }
                                else
                                {
                                    gameUser.ScoreSecondTeam = gameUser.ScoreSecondTeam + 1;
                                }
                                gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"{gameUser.GetRandomWord()}"));
                                break;
                            case CommandsMsg.teamOne:
                                gameUser.ScoreFirstTeam = gameUser.ScoreFirstTeam + 1;
                                await Client.DeleteMessageAsync(msg.Chat.Id, gameUser.BotMessageKeybord.MessageId);
                                await Client.DeleteMessageAsync(msg.Chat.Id, msg.MessageId);
                                gameUser.TeamNumber = gameUser.TeamNumber == 1 ? 2 : 1;
                                gameUser.BotMessageKeybord = await Client.SendMessage(msg.Chat.Id, CommandsMsg.selectAnAction, replyMarkup: KeyboardM.GemeMarkup);
                                gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"Счет: {gameUser.ScoreFirstTeam}:{gameUser.ScoreSecondTeam}"));
                                gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"Отвечает Команда {gameUser.TeamNumber}:"));
                                gameUser.FinihGameMetod();
                                break;
                            case CommandsMsg.teamTwo:
                                gameUser.ScoreSecondTeam = gameUser.ScoreSecondTeam + 1;
                                await Client.DeleteMessageAsync(msg.Chat.Id, gameUser.BotMessageKeybord.MessageId);
                                await Client.DeleteMessageAsync(msg.Chat.Id, msg.MessageId);
                                gameUser.TeamNumber = gameUser.TeamNumber == 1 ? 2 : 1;
                                gameUser.BotMessageKeybord = await Client.SendMessage(msg.Chat.Id, CommandsMsg.selectAnAction, replyMarkup: KeyboardM.GemeMarkup);
                                gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"Счет: {gameUser.ScoreFirstTeam}:{gameUser.ScoreSecondTeam}"));
                                gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"Отвечает Команда {gameUser.TeamNumber}:"));
                                gameUser.FinihGameMetod();
                                break;
                            case CommandsMsg.reset:
                                await Client.DeleteMessageAsync(msg.Chat.Id, gameUser.BotMessageKeybord.MessageId);
                                await Client.DeleteMessageAsync(msg.Chat.Id, msg.MessageId);
                                gameUser.TeamNumber = gameUser.TeamNumber == 1 ? 2 : 1;
                                gameUser.BotMessageKeybord = await Client.SendMessage(msg.Chat.Id, CommandsMsg.selectAnAction, replyMarkup: KeyboardM.GemeMarkup);
                                gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"Счет: {gameUser.ScoreFirstTeam}:{gameUser.ScoreSecondTeam}"));
                                gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"Отвечает Команда {gameUser.TeamNumber}:"));
                                gameUser.FinihGameMetod();
                                break;
                            case CommandsMsg.rules:
                                await Client.DeleteMessageAsync(msg.Chat.Id, gameUser.BotMessageKeybord.MessageId);
                                await Client.DeleteMessageAsync(msg.Chat.Id, msg.MessageId);
                                FileStream fileStream = new FileStream("rules_game.txt", FileMode.Open);
                                StreamReader streamReader = new StreamReader(fileStream);
                                gameUser.RulesGame = await Client.SendTextMessageAsync(msg.Chat.Id, streamReader.ReadToEnd());
                                fileStream.Close();
                                FileStream videoStream = new FileStream("rules_video.mp4", FileMode.Open);
                                gameUser.RulesVideo = await Client.SendVideo(msg.Chat.Id, videoStream);
                                videoStream.Close();
                                gameUser.BotMessageKeybord = await Client.SendMessage(msg.Chat.Id, CommandsMsg.selectAnAction, replyMarkup: KeyboardM.BreakingRulesMarkup);
                                break;
                            case CommandsMsg.closeRules:
                                await Client.DeleteMessageAsync(msg.Chat.Id, gameUser.BotMessageKeybord.MessageId);
                                await Client.DeleteMessageAsync(msg.Chat.Id, msg.MessageId);
                                await Client.DeleteMessageAsync(msg.Chat.Id, gameUser.RulesGame.Id);
                                await Client.DeleteMessageAsync(msg.Chat.Id, gameUser.RulesVideo.Id);
                                gameUser.BotMessageKeybord = await Client.SendMessage(msg.Chat.Id, CommandsMsg.selectAnAction, replyMarkup: KeyboardM.StartMarkup);
                                break;

                            case CommandsMsg.settings:
                                await Client.DeleteMessageAsync(msg.Chat.Id, msg.MessageId);
                                await Client.DeleteMessageAsync(msg.Chat.Id, gameUser.BotMessageKeybord.MessageId);
                                gameUser.OpenSettings = true;
                                gameUser.SettingsInfoMessage = await Client.SendTextMessageAsync(msg.Chat.Id, $"Настройки:\n" +
                                                                                                              $"1. Время раунда: {gameUser.Timer} c.\n" +
                                                                                                              $"2. Победа к-во очков: {gameUser.WinningGame}.\n" +
                                                                                                              $"3. Тип слов: {gameUser.TypeWords} \n" +
                                                                                                              $"4. C YndexGPT к-во слов: {gameUser.WinningGameYA}\n" +
                                                                                                              $"Для изменения настроек введите номер пункта и введите параметр например 2_70\n" +
                                                                                                              $"Пункт 3 имеет три состояния /жд/ - термины /ждп/ - термины уровень профи и /быт/ слова");
                                gameUser.BotMessageKeybord = await Client.SendMessage(msg.Chat.Id, $"Выберете действие ⌨️", replyMarkup: KeyboardM.BreakingSettingsMarkup);

                                break;
                            case CommandsMsg.closeSettings:
                                gameUser.OpenSettings = false;
                                await Client.DeleteMessageAsync(msg.Chat.Id, msg.MessageId);
                                await Client.DeleteMessageAsync(msg.Chat.Id, gameUser.SettingsInfoMessage.Id);
                                await Client.DeleteMessageAsync(msg.Chat.Id, gameUser.BotMessageKeybord.MessageId);
                                gameUser.BotMessageKeybord = await Client.SendMessage(msg.Chat.Id, $"Выберете действие ⌨️", replyMarkup: KeyboardM.StartMarkup);
                                break;

                            case CommandsMsg.newGameYAGPT:

                                
                                FileStream fileStreamApiKey = new FileStream("apiyandexkey.txt", FileMode.Open);
                                StreamReader streamReaderApiKey = new StreamReader(fileStreamApiKey);

                                string ApiKey = streamReaderApiKey.ReadLine();

                                if (ApiKey == null)
                                {
                                    Console.WriteLine($"Файлы apiyandexkey.txt не содержит APIKey, FolderID, ModelUri ");
                                    await Client.DeleteMessageAsync(msg.Chat.Id, msg.Id);
                                    await Client.DeleteMessageAsync(msg.Chat.Id, gameUser.BotMessageKeybord.MessageId);
                                    gameUser.BotMessageKeybord = await Client.SendMessage(msg.Chat.Id, $"Выберете действие ⌨️", replyMarkup: KeyboardM.StartMarkup);
                                }
                                else
                                {
                                    gameUser.StartGameYAGPT = true;
                                    await Client.DeleteMessageAsync(msg.Chat.Id, msg.MessageId);
                                    await Client.DeleteMessageAsync(msg.Chat.Id, gameUser.BotMessageKeybord.MessageId);
                                    gameUser.BotMessageKeybord = await Client.SendMessage(msg.Chat.Id, $"Выберете действие ⌨️", replyMarkup: KeyboardM.GemeMarkupYA);                                    
                                }
                                streamReaderApiKey.Close();
                                break;
                            default:
                                await Client.DeleteMessageAsync(msg.Chat.Id, msg.Id);
                                break;
                        }
                    }
                }
                catch
                {
                    // await Client.DeleteMessageAsync(msg.Chat.Id, msg.Id);
                    if (Games.ContainsKey(msg.Chat.Id))
                    {
                        Games.Remove(msg.Chat.Id);
                    }

                    await Client.SendTextMessageAsync(msg.Chat.Id, CommandsMsg.errorBot);
                    Console.WriteLine($"{DateTime.Now.ToString("dd.MM.yyyy_HH:mm:ss")} У пользователя {msg.Chat.Id} возникли проблемы!");
                    Console.Write("> ");
                }
                ;

            }

        }



    }
}

