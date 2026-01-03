using System.Collections.Generic;

namespace Resto.Front.Api.CustomerScreen.View
{
    public static class Translations
    {
        public static readonly Dictionary<LanguageEnum, Dictionary<string, string>> Data =
            new Dictionary<LanguageEnum, Dictionary<string, string>>
            {
                [LanguageEnum.Russian] = new Dictionary<string, string>
                {
                    ["Scanning_Title"] = "Подготовьтесь к сканированию",
                    ["Scanning_Subtitle"] = "Поставьте блюда на зону, уберите лишние предметы",
                    ["Scanning_Button"] = "ПОСТАВИЛ",
                    ["Error_Title"] = "Произошла ошибка",
                    ["Error_Button"] = "Попробовать снова",
                    ["Success_Title"] = "Запрос выполнен успешно!",
                    ["Loading"] = "Ожидайте..",

                    ["Error_Message"] = "Произошла ошибка",

                    ["Success_AddedDishes"] = "Добавленные блюда:",
                    ["Repeat_Scan"] = "Повторить сканирование",
                    ["Pay"] = "Оплатить",

                    ["Payment_Title"] = "Выберите способ оплаты",
                    ["Payment_Dishes"] = "Блюда в заказе:",
                    ["Payment_Cash"] = "Наличные",

                    ["Thanks"] = "Спасибо за оплату!",
                },

                [LanguageEnum.Kazakh] = new Dictionary<string, string>
                {
                    ["Scanning_Title"] = "Сканерге дайындалыңыз",
                    ["Scanning_Subtitle"] = "Тағамдарды аймаққа қойып, артық заттарды алып тастаңыз",
                    ["Scanning_Button"] = "ҚОЙДЫМ",
                    ["Error_Title"] = "Қате пайда болды",
                    ["Error_Button"] = "Қайта көру",
                    ["Success_Title"] = "Сұраныс сәтті орындалды!",
                    ["Loading"] = "Күтіңіз..",

                    ["Error_Message"] = "Қате пайда болды",

                    ["Success_AddedDishes"] = "Қосылған тағамдар:",
                    ["Repeat_Scan"] = "Қайта сканерлеу",
                    ["Pay"] = "Төлеу",

                    ["Payment_Title"] = "Төлем әдісін таңдаңыз",
                    ["Payment_Dishes"] = "Тапсырыстағы тағамдар:",
                    ["Payment_Cash"] = "Қолма-қол",

                    ["Thanks"] = "Төлеміңізге рахмет!",
                }
            };
    }
}

