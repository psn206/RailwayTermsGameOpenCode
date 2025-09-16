using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RailwayTermsGame
{
    class GameOrdersYAGPT
    {
        private TelegramBotClient client;
        private Dictionary<long, GameUser> games;
        private Keyboard keyboardM = new Keyboard();
        string response;

        public GameOrdersYAGPT(TelegramBotClient client, Dictionary<long, GameUser> games)
        {
            Client = client;
            Games = games;

        }

        public TelegramBotClient Client { get => client; set => client = value; }
        public string Response { get => response; set => response = value; }
        internal Dictionary<long, GameUser> Games { get => games; set => games = value; }
        internal Keyboard KeyboardM { get => keyboardM; set => keyboardM = value; }

        public async void ExecutingCommand(Message msg, GameUser gameUser)
        {
            try
            {
                if (gameUser.Role)
                {

                    switch (msg.Text)
                    {
                        case CommandsMsg.guessedYA:
                            await Client.DeleteMessageAsync(msg.Chat.Id, msg.MessageId);
                            await Client.DeleteMessageAsync(msg.Chat.Id, gameUser.BotMessageKeybord.MessageId);
                            gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"Объясняет слова YndexGPT"));
                            gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"<tg-spoiler>{gameUser.GetRandomWord()}</tg-spoiler>", parseMode: ParseMode.Html));
                            Response = await gameUser.GetResponseExplanatory();
                            gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"YndexGPT:\n {Response}"));
                            gameUser.BotMessageKeybord = await Client.SendMessage(msg.Chat.Id, $" Введите текст или выберете действие ⌨️", replyMarkup: KeyboardM.GemePlayYandexGPT);

                            break;
                        case CommandsMsg.reset:
                            await Client.DeleteMessageAsync(msg.Chat.Id, msg.MessageId);
                            gameUser.StartGameYAGPT = await gameUser.FinihGameYA();
                            if (gameUser.StartGameYAGPT)
                            {
                                gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"Следуюшие слово:"));
                                gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"<tg-spoiler>{gameUser.GetRandomWord()}</tg-spoiler>", parseMode: ParseMode.Html));
                                gameUser.MessegesYA.Clear();
                                Response = await gameUser.GetResponseExplanatory();
                                gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"YndexGPT:\n {Response}"));
                                gameUser.NumberGamesFirstTeam++;
                            }

                            break;
                        case CommandsMsg.completion:
                            await Client.DeleteMessageAsync(msg.Chat.Id, msg.MessageId);
                            await Client.DeleteMessageAsync(msg.Chat.Id, gameUser.BotMessageKeybord.MessageId);
                            gameUser.StartGameYAGPT = false;
                            gameUser.ClickingGuess = false;
                            gameUser.MessegesYA.Clear();
                            gameUser.ScoreFirstTeam = 0;
                            gameUser.NumberGamesFirstTeam = 0;
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
                        default:

                            gameUser.UserGameMessages.Add(msg);
                            if (msg.Text.ToUpper() == gameUser.RandomWord.ToUpper())
                            {
                                gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, "Ура угадано!😉\nСледуюшие слово:"));
                                gameUser.ScoreFirstTeam++;
                                gameUser.NumberGamesFirstTeam++;
                                gameUser.StartGameYAGPT = await gameUser.FinihGameYA();

                                if (gameUser.StartGameYAGPT)
                                {
                                    gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"<tg-spoiler>{gameUser.GetRandomWord()}</tg-spoiler>", parseMode: ParseMode.Html));
                                    gameUser.MessegesYA.Clear();
                                    Response = await gameUser.GetResponseExplanatory();
                                    gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"YndexGPT:\n {Response}"));

                                }
                            }
                            else
                            {
                                if (gameUser.MessegesYA.Count() < 7) // 7 - 3 попытки, 9 - 5 попыток
                                {
                                    Response = await gameUser.GetResponseExplanatory(msg.Text);
                                    gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"YndexGPT:\n {response}"));
                                }
                                else
                                {
                                    gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"Вы исчерпали попытки 😕\n Было загадано: ➡️{gameUser.RandomWord}\n\nЕсли Вы угадали, но были трудности с вводом ответа, нажмите:", replyMarkup: keyboardM.GuessInlineKeyboard));
                                    gameUser.NumberGamesFirstTeam++;
                                    gameUser.ClickingGuess = true;
                                    gameUser.StartGameYAGPT = await gameUser.FinihGameYA();

                                    if (gameUser.StartGameYAGPT)
                                    {
                                        gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"Следуюшие слово:"));
                                        gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"<tg-spoiler>{gameUser.GetRandomWord()}</tg-spoiler>", parseMode: ParseMode.Html));
                                        gameUser.MessegesYA.Clear();
                                        Response = await gameUser.GetResponseExplanatory();
                                        gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"YndexGPT:\n {Response}"));
                                    }

                                }
                            }
                            break;
                    }
                }
                else // Загдываю слова боту ___________________// 
                {
                    gameUser.UserGameMessages.Add(msg);
                    

                    switch (msg.Text)
                    {
                        case CommandsMsg.explainYA:
                           
                            await Client.DeleteMessageAsync(msg.Chat.Id, gameUser.BotMessageKeybord.MessageId);
                            gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"Объясняете слова Вы:"));
                            gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"{gameUser.GetRandomWord()}"));
                            gameUser.BotMessageKeybord = await Client.SendMessage(msg.Chat.Id, $" Введите текст или выберете действие ⌨️", replyMarkup: KeyboardM.GemePlayYandexGPT);
                            break;
                        case CommandsMsg.reset:                         
                            gameUser.MessegesYA.Clear();
                            gameUser.NumberGamesFirstTeam++;
                            gameUser.StartGameYAGPT = await gameUser.FinihGameYA();
                            if (gameUser.StartGameYAGPT)
                            {
                                gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"Следующие слово:"));
                                gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"{gameUser.GetRandomWord()}"));
                            }
                            break;
                        case CommandsMsg.completion:

                            await Client.DeleteMessageAsync(msg.Chat.Id, gameUser.BotMessageKeybord.MessageId);
                            gameUser.StartGameYAGPT = false;
                            gameUser.ClickingGuess = false;
                            gameUser.MessegesYA.Clear();
                            gameUser.ScoreFirstTeam = 0;
                            gameUser.NumberGamesFirstTeam = 0;
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
                        default:

                            if (gameUser.MessegesYA.Count() < 5)
                            {
                                Response = await gameUser.GetResponseGuessing(msg.Text);
                                gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"YndexGPT:\n {Response}", replyMarkup: keyboardM.GuessInlineKeyboard));
                                gameUser.ClickingGuess = true;

                            }
                            else if (gameUser.MessegesYA.Count() < 7)
                            {
                                Response = await gameUser.GetResponseGuessing(msg.Text);
                                gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"YndexGPT:\n {Response}", replyMarkup: keyboardM.GuessInlineKeyboard));
                                gameUser.ClickingGuess = true;
                                gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"Вы исчерпали попытки 😕!\n Либо слово угадано либо необходимо сбросить!"));

                            }
                            else
                            {
                                gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(msg.Chat.Id, $"Вы исчерпали попытки 😕!\n Либо слово угадано либо необходимо сбросить!"));
                            }
                            break;
                    }
                }
            }
            catch
            {
                if (Games.ContainsKey(msg.Chat.Id))
                {
                    Games.Remove(msg.Chat.Id);
                }

                await Client.SendTextMessageAsync(msg.Chat.Id, CommandsMsg.errorBot);
                Console.WriteLine($"{DateTime.Now.ToString("dd.MM.yyyy_HH:mm:ss")} У пользователя {msg.Chat.Id} возникли проблемы!");
                Console.Write("> ");
            }

        }

        public async void GuessedYes(long IDUserChat, GameUser gameUser)
        {

            gameUser.ClickingGuess = false;
            gameUser.NumberGamesFirstTeam++;
            gameUser.MessegesYA.Clear();
            gameUser.StartGameYAGPT = await gameUser.FinihGameYA();
            if (gameUser.StartGameYAGPT)
            {
                gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(IDUserChat, $"Следующие слово:"));
                gameUser.BotWordMessages.Add(await Client.SendTextMessageAsync(IDUserChat, $"{gameUser.GetRandomWord()}"));

            }

        }
    }
}
