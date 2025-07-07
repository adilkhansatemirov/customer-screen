using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using static Resto.Front.Api.CustomerScreen.View.CustomerScreenWindow;

namespace Resto.Front.Api.CustomerScreen.View
{
    /// <summary>
    /// Interaction logic for CustomerScreenWindow.xaml
    /// </summary>
    public partial class CustomerScreenWindow
    {
        public bool CanBeClosed = false;

        public CustomerScreenWindow()
        {
            InitializeComponent();
            SizeChanged += CustomerScreenWindow_SizeChanged;
            StateChanged += CustomerScreenWindow_StateChanged;
            Closing += CustomerScreenWindow_Closing;
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
            PluginContext.Log.InfoFormat("Customer window state changed. Window state is {0}", WindowState);
            if (WindowState == System.Windows.WindowState.Minimized)
                WindowState = System.Windows.WindowState.Maximized;
        }

        void CustomerScreenWindow_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            PluginContext.Log.InfoFormat("Customer window size changed. Window state is {0}", WindowState);
            if (WindowState == System.Windows.WindowState.Minimized)
                WindowState = System.Windows.WindowState.Maximized;
        }

        public void ChangeSumChanged(decimal sum)
        {
            ctlResultSum.ChangeSumChanged(sum);
        }

        private async void ApiRequestButton_Click(object sender, RoutedEventArgs e)
        {
            PluginContext.Log.Info("ApiRequestButton_Click fired 1");
            await MakeApiRequestAsync();
            PluginContext.Log.Info("ApiRequestButton_Click fired");
        }

        private async Task MakeApiRequestAsync()
        {
            PluginContext.Log.Info("ApiRequestButton_Click fired 2");
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var url = "https://reqres.in/api/users?page=1";
                    var response = await httpClient.GetStringAsync(url);
                    var users = JsonConvert.DeserializeObject<UserResponse>(response);

                    // Выводим в лог
                    foreach (var user in users.data)
                    {
                        PluginContext.Log.Info(user.ToString());
                    }
                    PluginContext.Log.Info(response.ToString());
                    PluginContext.Log.Info(users.ToString());

                    // Показываем первые 10 задач
                    ApiResultList.ItemsSource = users.data;
                }
            }
            catch (Exception ex)
            {
                ApiResultList.ItemsSource = new[] { "Ошибка при запросе: " + ex.Message };
            }
        }

        public class User
        {
            public int id { get; set; }
            public string email { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }

            public override string ToString()
            {
                return $"{first_name} {last_name} ({email})";
            }
        }

        public class UserResponse
        {
            public List<User> data { get; set; }
        }
    }
}
