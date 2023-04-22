using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel.Channels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using mol3.Views;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace mol3
{
    public sealed partial class ViewTests : Page
    {
        public ViewTests()
        {
            this.InitializeComponent();
            TestList.ItemsSource = GetTests((App.Current as App).ConnectionString);
        }

        private void InsertTest_Click(object sender, RoutedEventArgs e)
        {
            InsertWidmTest(TestName.Text, (App.Current as App).ConnectionString);
            TestList.ItemsSource = GetTests((App.Current as App).ConnectionString);
        }

        private void DeleteTest_Click(object sender, RoutedEventArgs e)
        {
            string testIdString = checkDeleteInput.Text;
            bool isNumeric = int.TryParse(testIdString, out int testId);
            if (isNumeric)
            {
                DeleteWidmTest(testId, ((App.Current as App).ConnectionString));
                TestList.ItemsSource = GetTests((App.Current as App).ConnectionString);
                checkDeleteInput.Text = "";
            }
        }

        public ObservableCollection<Test> GetTests(string connectionString)
        {
            const string GetTestQuery = "select id, testnaam from test";
            var tests = new ObservableCollection<Test>();
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetTestQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var test = new Test();
                                    test.id = reader.GetInt32(0);
                                    test.testnaam = reader.GetString(1);
                                    tests.Add(test);
                                }
                            }
                        }
                    }
                }
                return tests;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine($"Exception: {eSql.Message}");
            }
            return null;
        }

        public string InsertWidmTest(string testnaam, string connectionString)
        {
            string InsertTestQuery = "insert into test values('" + testnaam + "');";
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = InsertTestQuery;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                return "Test Inserted successfully!";
            }
            catch (Exception eSql)
            {
                Debug.WriteLine($"Exception: {eSql.Message}");
            }

            return "Test Failed to insert";
        }

        public void DeleteWidmTest(int testId, string connectionString)
        {
            //Because the test and question tables are related the questions need to be deleted as well
            string DeleteQuestionsQuery = "DELETE FROM vraag WHERE testid = @testId";
            string DeleteTestQuery = "delete from test where id = @testId";
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
                            cmd.CommandText = DeleteQuestionsQuery;
                            cmd.ExecuteNonQuery();
                            cmd.CommandText = DeleteTestQuery;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception eSql)
            {
                Debug.WriteLine($"Exception: {eSql.Message}");
            }
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AdminView));
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            string EditTestString = EditTextBox.Text;
            bool isNumeric = int.TryParse(EditTestString, out int testId);
            if (isNumeric)
            {
                this.Frame.Navigate(typeof(EditSpecificTest), testId);
            }
        }
    }
}
