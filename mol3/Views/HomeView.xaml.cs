using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace mol3.Views
{
    public sealed partial class HomeView : Page
    {
        private string _connectionString = (App.Current as App).ConnectionString;
        public HomeView()
        {
            this.InitializeComponent();
        }

        private void getName(object sender, RoutedEventArgs e, string name)
        {
            List<string> nameList = new List<string>();
            if (name == "admin")
            {
                this.Frame.Navigate(typeof(AdminView));
            }
            else
            {
                const string getNamesQuery = "SELECT naam from kanidaat";
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        if (conn.State == System.Data.ConnectionState.Open)
                        {
                            using (SqlCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = getNamesQuery;
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        nameList.Add(reader.GetString(0).ToLower());
                                    }
                                }
                            }
                        }
                    }
                }
                catch(Exception eSql)
                {
                    Debug.WriteLine($"Exception: {eSql.Message}");
                }

                if (nameList.Contains(name.ToLower()))
                {
                    Payload payload = new Payload();
                    payload.item1 = name;
                    payload.item2 = 0;
                    this.Frame.Navigate(typeof(MakeTestView), payload);
                }
            }
        }

        private void onEnterPress(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                string name = nameBox.Text;
                getName(sender, e, name);
            }
        }


    }
}
