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
                    ["Welcome_Title"] = "КАССА САМООБСЛУЖИВАНИЯ",
                    ["Scanning_Title"] = "Подготовьтесь к сканированию",
                    ["Scanning_Subtitle"] = "Поставьте блюда на зону, уберите лишние предметы",
                    ["Scanning_Button"] = "ПОСТАВИЛ",
                    ["Error_Title"] = "Произошла ошибка",
                    ["Error_Button"] = "Попробовать снова",
                    ["Success_Title"] = "Запрос выполнен успешно!",
                    ["Loading"] = "Ожидайте..",
                },
                [LanguageEnum.Kazakh] = new Dictionary<string, string>
                {
                    ["Welcome_Title"] = "ӨЗІНЕ-ӨЗІ ҚЫЗМЕТ КАССАСЫ",
                    ["Scanning_Title"] = "Сканерге дайындалыңыз",
                    ["Scanning_Subtitle"] = "Тағамдарды аймаққа қойып, артық заттарды алып тастаңыз",
                    ["Scanning_Button"] = "ҚОЙДЫМ",
                    ["Error_Title"] = "Қате пайда болды",
                    ["Error_Button"] = "Қайта көру",
                    ["Success_Title"] = "Сұраныс сәтті орындалды!",
                    ["Loading"] = "Күтіңіз..",
                }
            };
    }
}
