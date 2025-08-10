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
using System.Windows.Controls;

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

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public CustomerScreenWindow()
        {
            InitializeComponent();
            //SizeChanged += CustomerScreenWindow_SizeChanged;
            //StateChanged += CustomerScreenWindow_StateChanged;
            //Closing += CustomerScreenWindow_Closing;

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

    public class ScreenTemplateSelector : DataTemplateSelector
    {
        public DataTemplate WelcomeTemplate { get; set; }
        public DataTemplate LoadingTemplate { get; set; }
        public DataTemplate SuccessTemplate { get; set; }
        public DataTemplate ErrorTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            PluginContext.Log.Info("SelectTemplate called with: " + item);

            if (item is ScreenType screenType)
            {
                switch (screenType)
                {
                    case ScreenType.Welcome:
                        return WelcomeTemplate;
                    case ScreenType.Loading:
                        return LoadingTemplate;
                    case ScreenType.Success:
                        return SuccessTemplate;
                    case ScreenType.Error:
                        return ErrorTemplate;
                    default:
                        return WelcomeTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }

    public enum ScreenType
    {
        Welcome,
        Loading,
        Success,
        Error
    }
}
