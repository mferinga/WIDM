using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class EditTest : Page
    {
        public EditTest()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Test test;
            if (e.Parameter is int && (int)e.Parameter > 0)
            {
                int testId = (int)e.Parameter;
                test = GetSpecificTest(((App.Current as App).ConnectionString), testId);
                if (test != null)
                {
                    this.testId.Text = "Test ID = " + test.id;
                    this.testName.Text = "Test Name = " + test.testnaam;
                }
            }
        }

        public Test GetSpecificTest(string connectionString, int testId)
        {
            const string GetTestQuery = "select id, testnaam from test where id = @testId";
            var test = new Test();
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Parameters.Add("@testId", SqlDbType.Int).Value = testId;
                            cmd.CommandText = GetTestQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    test.id = reader.GetInt32(0);
                                    test.testnaam = reader.GetString(1);
                                }
                            }
                        }
                    }
                }
                return test;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine($"Exception: {eSql.Message}");
            }
            return null;
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ViewTests));
        }
    }
}
