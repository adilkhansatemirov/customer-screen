using Newtonsoft.Json;
using Resto.Front.Api.Data.Assortment;
using Resto.Front.Api.Editors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace Resto.Front.Api.CustomerScreen.View
{
    public partial class CustomerScreenWindow : Window, INotifyPropertyChanged
    {
        public bool CanBeClosed = false;

        public event PropertyChangedEventHandler PropertyChanged;

        private ScreenType currentScreen = ScreenType.Welcome;
        public ScreenType CurrentScreen
        {
            get => currentScreen;
            set
            {
                currentScreen = value;
                PluginContext.Log.Info("CurrentScreen set to: " + currentScreen);
                OnPropertyChanged(nameof(CurrentScreen));
            }
        }

        private LanguageEnum selectedLanguage;
        public LanguageEnum SelectedLanguage
        {
            get => selectedLanguage;
            set
            {
                selectedLanguage = value;
                OnPropertyChanged(nameof(SelectedLanguage));
                OnPropertyChanged("Item[]"); // refresh bindings for dictionary lookups
            }
        }

        private ObservableCollection<User> users;
        public ObservableCollection<User> Users
        {
            get => users;
            set
            {
                users = value;
                OnPropertyChanged(nameof(Users));
            }
        }

        private string errorMessage;
        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        // Translation indexer
        public string this[string key]
        {
            get
            {
                if (Translations.Data.ContainsKey(SelectedLanguage) &&
                    Translations.Data[SelectedLanguage].ContainsKey(key))
                {
                    return Translations.Data[SelectedLanguage][key];
                }
                return $"[{key}]"; // fallback so you can see missing keys
            }
        }

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public CustomerScreenWindow()
        {
            InitializeComponent();
            DataContext = this;
            CurrentScreen = ScreenType.Welcome;
            PluginContext.Log.Info("Set screen to: " + CurrentScreen);
        }

        void CustomerScreenWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            PluginContext.Log.InfoFormat("Customer window try to close.");
            if (CanBeClosed)
            {
                PluginContext.Log.InfoFormat("Customer window closed.");
                return;
            }
            e.Cancel = true;
            PluginContext.Log.InfoFormat("Customer window closing aborted.");
        }

        private void CustomerScreenWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Maximized;
        }

        void CustomerScreenWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Maximized;
        }

        public void ChangeSumChanged(decimal sum)
        {
            //ctlResultSum.ChangeSumChanged(sum);
        }

        private void RussianLanguageButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedLanguage = LanguageEnum.Russian;
            CurrentScreen = ScreenType.Scanning;
        }

        private void KazakhLanguageButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedLanguage = LanguageEnum.Kazakh;
            CurrentScreen = ScreenType.Scanning;
        }

        private async void ApiRequestButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentScreen = ScreenType.Loading;
            try
            {
                await MakeApiRequestAsync();
                CurrentScreen = ScreenType.Success;
                var credentials = PluginContext.Operations.GetDefaultCredentials();
                var editSession = PluginContext.Operations.CreateEditSession();
                var newOrder = editSession.CreateOrder(null);
                editSession.ChangeOrderOriginName("Customer Screen", newOrder);
                var guest1 = editSession.AddOrderGuest("Bratishka", newOrder);
                var firstProduct = PluginContext.Operations.GetActiveProducts().FirstOrDefault();
                editSession.AddOrderProductItem(2m, firstProduct, newOrder, guest1, null);
                var result = PluginContext.Operations.SubmitChanges(editSession, credentials);
            }
            catch (Exception ex)
            {
                PluginContext.Log.Info("API request failed: " + ex.Message);
                ErrorMessage = ex.Message;
                CurrentScreen = ScreenType.Error;
            }
        }

        private async Task MakeApiRequestAsync()
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("x-api-key", "reqres-free-v1");
                var url = "https://reqres.in/api/users?page=1";
                var response = await httpClient.GetStringAsync(url);
                var users = JsonConvert.DeserializeObject<UserResponse>(response);
                Users = new ObservableCollection<User>(users.data);
            }
        }

        private void RepeatScanButton_Click(object sender, RoutedEventArgs e)
        {
            PluginContext.Log.Info("Repeat scan clicked.");
            CurrentScreen = ScreenType.Scanning;
        }

        private void PayButton_Click(object sender, RoutedEventArgs e)
        {
            PluginContext.Log.Info("Pay clicked.");
            CurrentScreen = ScreenType.Payment;
        }

        private async void KaspiButton_Click(object sender, RoutedEventArgs e)
        {
            //PluginContext.Log.Info("Kaspi payment selected.");
            //MessageBox.Show("Оплата через Kaspi успешно выполнена!", "Kaspi", MessageBoxButton.OK, MessageBoxImage.Information);
            await Task.Delay(1000); // short delay to simulate processing
            CurrentScreen = ScreenType.Final;
            await Task.Delay(1000);
            CurrentScreen = ScreenType.Welcome;
        }

        private async void CashButton_Click(object sender, RoutedEventArgs e)
        {
            //PluginContext.Log.Info("Cash payment selected.");
            //MessageBox.Show("Оплата наличными успешно выполнена!", "Наличные", MessageBoxButton.OK, MessageBoxImage.Information);
            await Task.Delay(1000);
            CurrentScreen = ScreenType.Final;
            await Task.Delay(1000);
            CurrentScreen = ScreenType.Welcome;
        }

        public class User
        {
            public int id { get; set; }
            public string email { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }

            public override string ToString() => $"{first_name} {last_name} ({email})";
        }

        public class UserResponse
        {
            public List<User> data { get; set; }
        }
    }

    public class ScreenTemplateSelector : System.Windows.Controls.DataTemplateSelector
    {
        public System.Windows.DataTemplate LanguageSelectorTemplate { get; set; }
        public System.Windows.DataTemplate ScanningTemplate { get; set; }
        public System.Windows.DataTemplate LoadingTemplate { get; set; }
        public System.Windows.DataTemplate SuccessTemplate { get; set; }
        public System.Windows.DataTemplate PaymentTemplate { get; set; }
        public System.Windows.DataTemplate ErrorTemplate { get; set; }
        public System.Windows.DataTemplate FinalTemplate { get; set; }

        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            if (item is ScreenType screenType)
            {
                switch (screenType)
                {
                    case ScreenType.Welcome:
                        return LanguageSelectorTemplate;
                    case ScreenType.Scanning:
                        return ScanningTemplate;
                    case ScreenType.Loading:
                        return LoadingTemplate;
                    case ScreenType.Success:
                        return SuccessTemplate;
                    case ScreenType.Payment:
                        return PaymentTemplate;
                    case ScreenType.Error:
                        return ErrorTemplate;
                    case ScreenType.Final:
                        return FinalTemplate;
                }
            }
            return base.SelectTemplate(item, container);
        }
    }

    public enum LanguageEnum
    {
        Russian,
        Kazakh
    }

    public enum ScreenType
    {
        Welcome,
        Scanning,
        Loading,
        Success,
        Error,
        Payment,
        Final
    }
}
