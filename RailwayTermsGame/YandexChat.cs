using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace RailwayTermsGame
{
    class YandexChat
    {
        private static HttpClient client = new HttpClient();

        private  string apiKey; 
        private  string folderId;  
        private  string modelUri;  


        public YandexChat()
        {
            FileStream fileStreamApiKey = new FileStream("apiyandexkey.txt", FileMode.Open);
            StreamReader streamReaderApiKey = new StreamReader(fileStreamApiKey);
           
            string str;
            int i = 0;

            while ((str = streamReaderApiKey.ReadLine()) != null)
            {
                switch (i)
                {
                    case 0:
                        ApiKey = str;
                        break;
                    case 1:
                        FolderId = str;
                        break;
                    case 2:
                        ModelUri = str;
                        break;
                }
                i++;
            }
            streamReaderApiKey.Close();

        }

        public  string ApiKey { get => apiKey; set => apiKey = value; }
        public  string FolderId { get => folderId; set => folderId = value; }
        public  string ModelUri { get => modelUri; set => modelUri = value; }
        public static HttpClient Client { get => client; set => client = value; }


        public async Task<string> SendRequestToYandexGPT(List<object> ListMsg)  // string inputMsg
        {
            object[] msg = new object[ListMsg.Count];
            ListMsg.CopyTo(msg);
            try
            {
                // Подготовка JSON-тела запроса
                var requestData = new
                {
                    modelUri = ModelUri,
                    completionOptions = new
                    {
                        stream = false, // Установите в true для потоковой передачи
                        temperature = 0.6f,
                        maxTokens = 2000
                    },
                    messages = msg

                };

                string jsonRequest = JsonSerializer.Serialize(requestData);

                // Создание HTTP-запроса
                var request = new HttpRequestMessage(HttpMethod.Post, "https://llm.api.cloud.yandex.net/foundationModels/v1/completion");
                request.Headers.Authorization = new AuthenticationHeaderValue("Api-Key", ApiKey);
                request.Headers.Add("x-folder-id", FolderId);
                request.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // Отправка запроса и получение ответа
                HttpResponseMessage response = await Client.SendAsync(request);
                response.EnsureSuccessStatusCode(); //  Вызовет исключение, если код ответа не успешный (2xx)

                string responseBody = await response.Content.ReadAsStringAsync();

                // Десериализация ответа
                using (JsonDocument document = JsonDocument.Parse(responseBody))
                {
                    return document.RootElement
                               .GetProperty("result")
                               .GetProperty("alternatives")[0]
                               .GetProperty("message")
                               .GetProperty("text")
                               .GetString() ?? "Диалог не состоялся. Ошибка!";
                }
            }
            catch
            {
                return "Нейросеть сегодня не в духе. Ошибка!";
            }

        }

    }
}
