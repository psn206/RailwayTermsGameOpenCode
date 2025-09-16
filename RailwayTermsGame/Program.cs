using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using RailwayTermsGame;
using Microsoft.VisualBasic;
using System.Runtime.ConstrainedExecution;
using System.Reflection.PortableExecutable;
using System.IO;

internal class Program
{


    private static TelegramBotClient client;
    private static Dictionary<long, GameUser> games = new Dictionary<long, GameUser>();
    private static List<string> listRailwayWords = new List<string>();
    private static List<string> listEverydayWords = new List<string>();
    private static List<string> listRailwayWordsEasy = new List<string>();
    private static GameOrders gameOrders;
    private static GameOrdersYAGPT gameOrdersYAGPT;



    [Obsolete]
    private static void Main(string[] args)
    {

        FileStream fileStreamTokenBot = new FileStream("token_bot.txt", FileMode.Open);
        StreamReader streamReaderTokenBot = new StreamReader(fileStreamTokenBot);

     string  token = streamReaderTokenBot.ReadLine() ;
       
       if (token == null)
        {
            Console.WriteLine($"Не найден токен  для бота\nПропишете токе в первой строке текстового файла token_bot.txt\nПерезапустите приложение!! ");
            Console.ReadKey();
            Environment.Exit(1);
        }
        streamReaderTokenBot.Close();

        using var cts = new CancellationTokenSource();
        try
        {
          
            client = new TelegramBotClient(token, cancellationToken: cts.Token);
        }
        catch(Exception e)
        {
            Console.WriteLine("Возможное не верно указан токен!");
            Console.WriteLine(e);
            Console.ReadKey();
            Environment.Exit(1);
        }
        gameOrders = new GameOrders(client, games);
        gameOrdersYAGPT = new GameOrdersYAGPT(client, games);
        listRailwayWordsEasy = ListWordReturn("railwayWordsEasy.txt");
        listRailwayWords = ListWordReturn("railwayWords.txt");
        listEverydayWords = ListWordReturn("everydayWords.txt");


        FileStream fileStreamUsers = new FileStream("users.txt", FileMode.Open);
        StreamReader streamReaderUsers = new StreamReader(fileStreamUsers);
        string user;
        while ((user = streamReaderUsers.ReadLine()) != null)
        {
            if (!games.ContainsKey(long.Parse(user.Split(' ')[0])))
            {
                games.Add(long.Parse(user.Split(' ')[0]), new GameUser(listRailwayWordsEasy, listRailwayWords, listEverydayWords, client));
            }
        }
        streamReaderUsers.Close();

        
        client.OnMessage += OnMessage;
        client.OnUpdate += OnUpdate;

        Console.WriteLine($"RailwayTermsGame is running... Press Enter to terminate");
        string input;
        while (true)
        {
            Console.Write("> ");
            input = Console.ReadLine();
            switch (input)
            {
                case "help":
                    Console.WriteLine("r_load - загружает слова из railwayWordsEasy");
                    Console.WriteLine("rp_load - загружает слова из railwayWords.txt");
                    Console.WriteLine("e_load - загружает слова из everydayWords.txt");
                    Console.WriteLine("msg_load - отправляет всем сообщения из msg_load.txt");

                    break;
                case "rp_load":
                    listRailwayWords = ListWordReturn("railwayWords.txt");
                    Console.WriteLine("railwayWords  обновлены!");
                    break;
                case "r_load":
                    listRailwayWords = ListWordReturn("railwayWordsEasy.txt");
                    Console.WriteLine("railwayWordsEasy  обновлены!");
                    break;
                case "e_load":
                    listEverydayWords = ListWordReturn("everydayWords.txt");
                    Console.WriteLine("everydayWords обновлены !");
                    break;
                case "msg_load":
                    SendMessageEveryone();
                    Console.WriteLine("Сообщение отправлено!");

                    break;
                default:
                    Console.WriteLine("Такой команды не существует!");
                    break;
            }

        }
        //Console.ReadLine();
        cts.Cancel();
    }

    [Obsolete]
    static async Task OnMessage(Message msg, UpdateType type)
    {

        if (!games.ContainsKey(msg.Chat.Id))
        {
            games.Add(msg.Chat.Id, new GameUser(listRailwayWordsEasy, listRailwayWords, listEverydayWords, client));

            if (!UserListContains(msg.Chat.Id))
            {
                StreamWriter writerUser = new StreamWriter("users.txt", true);
                writerUser.WriteLine($"{msg.Chat.Id} {msg.Chat.Username} {msg.Chat.FirstName} {msg.Chat.LastName} ");
                writerUser.Close();
            }
            gameOrders.ExecutingCommand(msg, games[msg.Chat.Id]);
        }
        else if (games[msg.Chat.Id].StartGameYAGPT)
        {
            
                switch (msg.Text)
                {
                    case CommandsMsg.guessedYA:
                        games[msg.Chat.Id].Role = true;
                        gameOrdersYAGPT.ExecutingCommand(msg, games[msg.Chat.Id]); // игра с YA
                        break;
                    case CommandsMsg.explainYA:
                        games[msg.Chat.Id].Role = false;
                        gameOrdersYAGPT.ExecutingCommand(msg, games[msg.Chat.Id]); // игра с YA
                        break;
                    default:
                        gameOrdersYAGPT.ExecutingCommand(msg, games[msg.Chat.Id]); // игра с YA
                        break;
                }
            
          
           
        }
        else
        {
            gameOrders.ExecutingCommand(msg, games[msg.Chat.Id]);
        }


    }

    static List<string> ListWordReturn(string filePath)
    {
        List<string> listWords = new List<string>();
        FileStream fileStream = new FileStream(filePath, FileMode.Open);
        StreamReader streamReader = new StreamReader(fileStream);
        string words;
        while ((words = streamReader.ReadLine()) != null)
        {
            listWords.Add(words);
        }
        streamReader.Close();
        return listWords;

    }

    static bool UserListContains(long msgID)
    {
        bool flag = false;
        FileStream fileStreamUsers = new FileStream("users.txt", FileMode.Open);
        StreamReader streamReaderUsers = new StreamReader(fileStreamUsers);
        string user;
        while ((user = streamReaderUsers.ReadLine()) != null)
        {
            if (msgID == long.Parse(user.Split(' ')[0]))
            {
                flag = true;
                break;
            }
            else
            {
                flag = false;
            }
        }
        streamReaderUsers.Close();
        return flag;
    }

    static async Task OnUpdate(Update update)
    {
        if (update.Type == UpdateType.CallbackQuery)
        {
          
            var callbackQuery = update.CallbackQuery;
            if (games[callbackQuery.Message.Chat.Id].ClickingGuess)
            {               
                if (callbackQuery.Data == "button_clicked")
                {
                    games[callbackQuery.Message.Chat.Id].ScoreFirstTeam++;
                    games[callbackQuery.Message.Chat.Id].ClickingGuess = false;                  

                    if (games[callbackQuery.Message.Chat.Id].Role)
                    {
                        if (!games[callbackQuery.Message.Chat.Id].StartGameYAGPT)
                        {
                            Message msg = games[callbackQuery.Message.Chat.Id].BotWordMessages.Last();
                            client.EditMessageTextAsync(msg.Chat.Id, msg.Id, $"Вы угодали {games[msg.Chat.Id].ScoreFirstTeam} из {games[msg.Chat.Id].WinningGameYA}  слов! 🏆 ");
                        }
                        
                    }
                    else
                    {
                        if (!games[callbackQuery.Message.Chat.Id].StartGameYAGPT)
                        {
                            Message msg = games[callbackQuery.Message.Chat.Id].BotWordMessages.Last();
                            client.EditMessageTextAsync(msg.Chat.Id, msg.Id, $"Вы объяснили {games[msg.Chat.Id].ScoreFirstTeam} из {games[msg.Chat.Id].WinningGameYA}  слов! 🏆 ");
                        }
                        else
                        {
                           gameOrdersYAGPT.GuessedYes(callbackQuery.Message.Chat.Id, games[callbackQuery.Message.Chat.Id]);

                        }
                    }

                }
            }
        }
    }

    static async Task SendMessageEveryone()
    {
        FileStream fileStreamMsgUsers = new FileStream("msg_load.txt", FileMode.Open);
        StreamReader streamReaderMsgUsers = new StreamReader(fileStreamMsgUsers);
        string msgUsers = streamReaderMsgUsers.ReadToEnd();
        streamReaderMsgUsers.Close();

        FileStream fileStreamUsers = new FileStream("users.txt", FileMode.Open);
        StreamReader streamReaderUsers = new StreamReader(fileStreamUsers);
        string user;
        while ((user = streamReaderUsers.ReadLine()) != null)
        {
            long IDUserChat = long.Parse(user.Split(' ')[0]);
            await client.SendTextMessageAsync(IDUserChat, $"‼️Инфо.сообщение‼️:\n{msgUsers}");
        }
        streamReaderUsers.Close();
    }

}



