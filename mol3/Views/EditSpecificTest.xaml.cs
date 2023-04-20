using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
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
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditSpecificTest : Page
    {
        public EditSpecificTest()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Question question;
            if (e.Parameter is int && (int)e.Parameter > 0)
            {
                int testId = (int)e.Parameter;
                question = GetSpecificTest(((App.Current as App).ConnectionString), testId);
                if (question != null)
                {
                    this.testId.Text = "Test ID = " + question.testId;
                    this.testName.Text = "Test Name = " + question.test.testnaam;
                    TestList.ItemsSource = GetSpecificTest((App.Current as App).ConnectionString, testId);
                }
            }
        }

        private void DeleteTest_Click(object sender, RoutedEventArgs e)
        {
            string testIdString = checkDeleteInput.Text;
            bool isNumeric = int.TryParse(testIdString, out int testId);
            if (isNumeric)
            {
                DeleteWidmQuestion(testId, ((App.Current as App).ConnectionString));
                //TestList.ItemsSource = GetSpecificTest((App.Current as App).ConnectionString);
            }
        }

        public Question GetSpecificTest(string connectionString, int testId)
        {
            const string GetTestQuery = "SELECT test.testnaam, vraag.id, test.id, vraag.vraagTekst FROM test FULL JOIN vraag ON test.id = vraag.testid WHERE test.id = @testId";
            var question = new Question();
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
                                    test.testnaam = reader.GetString(0);
                                    question.testId = reader.GetInt32(2);
                                }
                            }
                        }
                    }
                }
                question.test = test;
                return question;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine($"Exception: {eSql.Message}");
            }
            return null;
        }

        public void DeleteWidmQuestion(int testId, string connectionString)
        {
            throw new NotImplementedException();
            // string DeleteTestQuery = "delete from test where id = @testId";
            // try
            // {
            //     using (var conn = new SqlConnection(connectionString))
            //     {
            //         conn.Open();
            //         if (conn.State == System.Data.ConnectionState.Open)
            //         {
            //             using (SqlCommand cmd = conn.CreateCommand())
            //             {
            //                 cmd.Parameters.Add("@testId", SqlDbType.Int).Value = testId;
            //                 cmd.CommandText = DeleteTestQuery;
            //                 cmd.ExecuteNonQuery();
            //             }
            //         }
            //     }
            // }
            // catch (Exception eSql)
            // {
            //     Debug.WriteLine($"Exception: {eSql.Message}");
            // }
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ViewTests));
        }
    }
}