using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace RailwayTermsGame
{
    internal  class Keyboard

    {
        

        private ReplyKeyboardMarkup startMarkup = new ReplyKeyboardMarkup(true)
                   .AddButton(CommandsMsg.newGame )
                   .AddNewRow(CommandsMsg.newGameYAGPT)
                   .AddNewRow(CommandsMsg.rules, CommandsMsg.settings);
        private ReplyKeyboardMarkup gemeMarkup = new ReplyKeyboardMarkup(true)
                    .AddButton(CommandsMsg.beginning)
                    .AddNewRow(CommandsMsg.completion);

        private ReplyKeyboardMarkup gemeMarkupYA = new ReplyKeyboardMarkup(true)
                  .AddButton(CommandsMsg.guessedYA)
                  .AddNewRow(CommandsMsg.explainYA)
                  .AddNewRow(CommandsMsg.completion);
        private ReplyKeyboardMarkup gemePlayMarkup = new ReplyKeyboardMarkup(true)
                    .AddNewRow(CommandsMsg.skipped, CommandsMsg.guessed)
                   .AddNewRow(CommandsMsg.completion);
        private ReplyKeyboardMarkup gemePlayYandexGPT = new ReplyKeyboardMarkup(true)
                    .AddNewRow(CommandsMsg.reset)
                   .AddNewRow(CommandsMsg.completion);
        private ReplyKeyboardMarkup gemeTwoPlayMarkup = new ReplyKeyboardMarkup(true)
                    .AddNewRow(CommandsMsg.teamOne, CommandsMsg.teamTwo)
                   .AddNewRow(CommandsMsg.reset, CommandsMsg.completion);
        private ReplyKeyboardMarkup finishMarkup = new ReplyKeyboardMarkup(true)
                    .AddButton(CommandsMsg.completion);
        private ReplyKeyboardMarkup breakingRulesMarkup = new ReplyKeyboardMarkup(true)
                   .AddButton(CommandsMsg.closeRules);
        private ReplyKeyboardMarkup breakingSettingsMarkup = new ReplyKeyboardMarkup(true)
                 .AddButton(CommandsMsg.closeSettings);

        private InlineKeyboardMarkup guessInlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{CommandsMsg.guessed}", "button_clicked") // Кнопка с callback_data
                }                
            });




        public ReplyKeyboardMarkup StartMarkup { get => startMarkup; set => startMarkup = value; }
        public ReplyKeyboardMarkup GemeMarkup { get => gemeMarkup; set => gemeMarkup = value; }
        public ReplyKeyboardMarkup GemePlayMarkup { get => gemePlayMarkup; set => gemePlayMarkup = value; }
        public ReplyKeyboardMarkup GemeTwoPlayMarkup { get => gemeTwoPlayMarkup; set => gemeTwoPlayMarkup = value; }
        public ReplyKeyboardMarkup FinishMarkup { get => finishMarkup; set => finishMarkup = value; }
        public ReplyKeyboardMarkup BreakingRulesMarkup { get => breakingRulesMarkup; set => breakingRulesMarkup = value; }
        public ReplyKeyboardMarkup BreakingSettingsMarkup { get => breakingSettingsMarkup; set => breakingSettingsMarkup = value; }
        public ReplyKeyboardMarkup GemePlayYandexGPT { get => gemePlayYandexGPT; set => gemePlayYandexGPT = value; }
        public InlineKeyboardMarkup GuessInlineKeyboard { get => guessInlineKeyboard; set => guessInlineKeyboard = value; }
        public ReplyKeyboardMarkup GemeMarkupYA { get => gemeMarkupYA; set => gemeMarkupYA = value; }
    }
}
