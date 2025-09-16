using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

namespace RailwayTermsGame
{
    internal class GameUser
    {
        private int teamNumber;
        private int scoreFirstTeam;
        private int scoreSecondTeam;
        private int timer;
        private int numberGamesFirstTeam;
        private int numberGamesSecondTeam;
        private int winningGame;
        private int winningGameYA;
        private string typeWords;
        private TelegramBotClient client;
        private CancellationTokenSource cts;
        private Task taskTimer;
        private Random rnd = new Random();
        private string randomWord;
        private YandexChat yandexChat = new YandexChat();
        bool role;
        

        private List<Message> botWordMessages = new List<Message>();
        private List<Message> userGameMessages = new List<Message>();
        private bool startTimer;
        private bool openSettings;
        private List<string> listRailwayWords = new List<string>();
        private List<string> listEverydayWords = new List<string>();
        private List<string> listRailwayWordsEasy = new List<string>();
        private Message botMessageKeybord = new Message();
        private Message messageTimer;
        private Message rulesGame;
        private Message rulesVideo;
        private Message settingsMessage;
        private Message settingsInfoMessage;
        private Keyboard keyboardM = new Keyboard();
        private bool startGameYAGPT;
        private bool clickingGuess;
        List<object> messegesYA = new List<object>();


        public GameUser(List<string> listRailwayWordsEasy, List<string> listRailwayWords, List<string> listEverydayWords, TelegramBotClient client)
        {
            Client = client;
            TeamNumber = 1;
            ScoreSecondTeam = 0;
            ScoreFirstTeam = 0;
            NumberGamesFirstTeam = 0;
            NumberGamesSecondTeam = 0;
            ListRailwayWords = listRailwayWords;
            ListEverydayWords = listEverydayWords;
            ListRailwayWordsEasy = listRailwayWordsEasy;

            Cts = new CancellationTokenSource();
            TaskTimer = TimerMonitoring(Cts.Token);
            StartTimer = false;
            OpenSettings = false;
            Timer = 60;
            WinningGame = 60;
            WinningGameYA = 8;
            TypeWords = "жд";
            StartGameYAGPT = false;
            ClickingGuess = false;
            
            


        }

        public int TeamNumber { get => teamNumber; set => teamNumber = value; }
        public int ScoreFirstTeam { get => scoreFirstTeam; set => scoreFirstTeam = value; }
        public int ScoreSecondTeam { get => scoreSecondTeam; set => scoreSecondTeam = value; }
        public List<Message> BotWordMessages { get => botWordMessages; set => botWordMessages = value; }
        public List<Message> UserGameMessages { get => userGameMessages; set => userGameMessages = value; }
        public bool StartTimer { get => startTimer; set => startTimer = value; }

        public int Timer { get => timer; set => timer = value; }
        public int NumberGamesFirstTeam { get => numberGamesFirstTeam; set => numberGamesFirstTeam = value; }
        public int NumberGamesSecondTeam { get => numberGamesSecondTeam; set => numberGamesSecondTeam = value; }
        public List<string> ListRailwayWords { get => listRailwayWords; set => listRailwayWords = value; }
        public Message BotMessageKeybord { get => botMessageKeybord; set => botMessageKeybord = value; }
        public Message MessageTimer { get => messageTimer; set => messageTimer = value; }
        public TelegramBotClient Client { get => client; set => client = value; }
        public CancellationTokenSource Cts { get => cts; set => cts = value; }
        public Message RulesGame { get => rulesGame; set => rulesGame = value; }
        public Message RulesVideo { get => rulesVideo; set => rulesVideo = value; }
        public Task TaskTimer { get => taskTimer; set => taskTimer = value; }
        internal Keyboard KeyboardM { get => keyboardM; set => keyboardM = value; }
        public Message SettingsMessage { get => settingsMessage; set => settingsMessage = value; }
        public Message SettingsInfoMessage { get => settingsInfoMessage; set => settingsInfoMessage = value; }
        public bool OpenSettings { get => openSettings; set => openSettings = value; }
        public int WinningGame { get => winningGame; set => winningGame = value; }
        public string TypeWords { get => typeWords; set => typeWords = value; }
        public List<string> ListEverydayWords { get => listEverydayWords; set => listEverydayWords = value; }
        public List<string> ListRailwayWordsEasy { get => listRailwayWordsEasy; set => listRailwayWordsEasy = value; }
        public bool StartGameYAGPT { get => startGameYAGPT; set => startGameYAGPT = value; }
        public string RandomWord { get => randomWord; set => randomWord = value; }
        public List<object> MessegesYA { get => messegesYA; set => messegesYA = value; }
        public int WinningGameYA { get => winningGameYA; set => winningGameYA = value; }
        public bool ClickingGuess { get => clickingGuess; set => clickingGuess = value; }
        public bool Role { get => role; set => role = value; }

        public string GetRandomWord()
        {
           
            
            if (TypeWords == "быт")
            {
                RandomWord = ListEverydayWords[rnd.Next(0, ListEverydayWords.Count)];
            }
            else if (TypeWords == "ждп")
            {
               RandomWord  = ListRailwayWords[rnd.Next(0, ListRailwayWords.Count)];
            }
            else
            {
                RandomWord = ListRailwayWordsEasy[rnd.Next(0, listRailwayWordsEasy.Count)];
            }

            return "➡️" + RandomWord;
        }

        // Роль угадывающего
        public async Task<string>  GetResponseExplanatory()
        {
           // Начало использование не забудь рассписать роли угадывателя 
            MessegesYA.Add(new
            {
                role = "system",
                text = "Ты играешь в настольную игру Alias и ты работник РЖД. Пытаешься объяснять не используя однкоренные и не используя созвучные слова. Само слово не писать."
                
            });
            MessegesYA.Add(new
            {
                role = "user",
                text = $"Обясни - {RandomWord}"
            });
           
            string response = $"Количество слов: {RandomWord.Split(" ").Length} \n" +   await  yandexChat.SendRequestToYandexGPT(MessegesYA);
                       
            MessegesYA.Add(new
            {
                role = "assistant",
                text = response
            });
            return response;
        }
        public async Task<string> GetResponseExplanatory(string msg)
        {
            MessegesYA.Add(new
            {
                role = "user",
                text = $"Продолжи объяснение, по другому  и больше криатива"

            });
            string response = await yandexChat.SendRequestToYandexGPT(MessegesYA);
            MessegesYA.Add(new
            {
                role = "assistant",
                text = response
            });
            return response;
        }


        // Отгадывающий 
        public async Task<string> GetResponseGuessing(string msg)
        {
            if (MessegesYA.Count == 0)
            {

                MessegesYA.Add(new
                {
                    role = "system",
                    text = "Ты играешь в настольную игру Alias и ты работник РЖД. Пытаешься угадать слова."

                });
                MessegesYA.Add(new
                {
                    role = "user",
                    text = $"Угадай - {msg}"
                });

                string response = await yandexChat.SendRequestToYandexGPT(MessegesYA);

                MessegesYA.Add(new
                {
                    role = "assistant",
                    text = response
                });
                return response;
            }
            else
            {
                MessegesYA.Add(new
                {
                    role = "user",
                    text = $"Продолжи угадывать. {msg}"

                });
                string response = await yandexChat.SendRequestToYandexGPT(MessegesYA);
                MessegesYA.Add(new
                {
                    role = "assistant",
                    text = response
                });
                return response;

            }
        }


       



        public async Task TimerMonitoring(CancellationToken token)
        {

            StartTimer = true;           
            messageTimer = await Client.SendTextMessageAsync(BotWordMessages[0].Chat.Id, $"⏳ Запущен таймер: {Timer} секунд");
            await Task.Delay(Timer * 1000, token);
            StartTimer = false;
            await Client.DeleteMessageAsync(MessageTimer.Chat.Id, MessageTimer.Id);
            await Client.DeleteMessageAsync(BotWordMessages[0].Chat.Id, BotMessageKeybord.MessageId);
            BotWordMessages.Add(await Client.SendTextMessageAsync(BotWordMessages[0].Chat.Id, $"Время ⏰ вышло, отгадывают обе команды 🤼"));
            BotMessageKeybord = await Client.SendMessage(BotWordMessages[0].Chat.Id, CommandsMsg.selectAnAction, replyMarkup: KeyboardM.GemeTwoPlayMarkup);
            if (TeamNumber == 1)
            {
                NumberGamesFirstTeam++;
            }
            else
            {
                NumberGamesSecondTeam++;
            }
        }

        public async void StopTimer()
        {

            if (StartTimer)
            {
                try
                {
                    if (MessageTimer != null)
                    {
                        await Client.DeleteMessageAsync(MessageTimer.Chat.Id, MessageTimer.Id);
                    }
                }
                catch
                {

                    await Client.SendTextMessageAsync(BotWordMessages[0].Chat.Id, CommandsMsg.errorBot);
                    Console.WriteLine($"У пользователя {BotWordMessages[0].Chat.Id} возникли проблемы!");
                    Console.Write("> ");
                }
                Cts.Cancel();
            }
            StartTimer = false;
            Cts = new CancellationTokenSource();
        }

        // Завершении игры при условии проверок
        public async void FinihGameMetod()
        {
            if (NumberGamesFirstTeam == NumberGamesSecondTeam)
            {
                if (ScoreFirstTeam >= WinningGame && ScoreFirstTeam == ScoreSecondTeam)
                {
                    ScoreFirstTeam = 0;
                    await Client.DeleteMessageAsync(BotWordMessages[0].Chat.Id, BotMessageKeybord.MessageId);
                    BotWordMessages.Add(await Client.SendTextMessageAsync(BotWordMessages[0].Chat.Id, $"Победила ДРУЖБА! 🤝"));
                    BotMessageKeybord = await Client.SendMessage(BotWordMessages[0].Chat.Id, CommandsMsg.selectAnAction, replyMarkup: KeyboardM.FinishMarkup);
                    //NumberGamesFirstTeam = 0;
                    //NumberGamesSecondTeam = 0;
                }
                else if (ScoreFirstTeam >= WinningGame && ScoreSecondTeam < ScoreFirstTeam)
                {
                    ScoreFirstTeam = 0;
                    await Client.DeleteMessageAsync(BotWordMessages[0].Chat.Id, BotMessageKeybord.MessageId);
                    BotWordMessages.Add(await Client.SendTextMessageAsync(BotWordMessages[0].Chat.Id, $"Победила Команда1 🏆"));
                    BotMessageKeybord = await Client.SendMessage(BotWordMessages[0].Chat.Id, CommandsMsg.selectAnAction, replyMarkup: KeyboardM.FinishMarkup);
                    //NumberGamesFirstTeam = 0;
                    //NumberGamesSecondTeam = 0;
                }
                else if (ScoreSecondTeam >= WinningGame && ScoreFirstTeam < ScoreSecondTeam)
                {
                    await Client.DeleteMessageAsync(BotWordMessages[0].Chat.Id, BotMessageKeybord.MessageId);
                    BotWordMessages.Add(await Client.SendTextMessageAsync(BotWordMessages[0].Chat.Id, $"Победила Команда2 🏆"));
                    BotMessageKeybord = await Client.SendMessage(BotWordMessages[0].Chat.Id, CommandsMsg.selectAnAction, replyMarkup: KeyboardM.FinishMarkup);
                    //NumberGamesFirstTeam = 0;
                    //NumberGamesSecondTeam = 0;
                }
            }
        }

        public async Task<bool> FinihGameYA()
        {
            
            if (WinningGameYA == NumberGamesFirstTeam)
            {
                await Client.DeleteMessageAsync(BotWordMessages[0].Chat.Id, BotMessageKeybord.MessageId);
                BotWordMessages.Add(await Client.SendTextMessageAsync(BotWordMessages[0].Chat.Id, $"Вы угодали {ScoreFirstTeam} из {WinningGameYA}  слов! 🏆 "));
                BotMessageKeybord = await Client.SendMessage(BotWordMessages[0].Chat.Id, CommandsMsg.selectAnAction, replyMarkup: KeyboardM.FinishMarkup);              
                return false;
                              
            }
            else
            {
                return true;
            }

            
            

        }




    }


}
