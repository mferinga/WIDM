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
using System.Collections.ObjectModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace mol3.Views
{
    public sealed partial class EditSpecificTest : Page
    {
        private Test _editedTest;
        public EditSpecificTest()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is int && (int)e.Parameter > 0)
            {
                int testId = (int)e.Parameter;
                _editedTest = GetSpecificTest(((App.Current as App).ConnectionString), testId);
                if (_editedTest != null)
                {
                    //assigning textboxes to the test which is edited at the moment, so the user can see which test is edited.
                    this.testId.Text = "Test ID = " + _editedTest.id;
                    this.testName.Text = "Test Name = " + _editedTest.testnaam;

                    QuestionList.ItemsSource = GetQuestions((App.Current as App).ConnectionString, testId);
                }
            }
        }

        public Test GetSpecificTest(string connectionString, int testId)
        {
            const string GetTestQuery = "SELECT id, testnaam FROM test WHERE id = @testId";
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

        private void DeleteTest_Click(object sender, RoutedEventArgs e)
        {
            string vraagIdString = checkDeleteInput.Text;
            bool isNumeric = int.TryParse(vraagIdString, out int vraagId);
            if (isNumeric)
            {
                DeleteWidmQuestion(vraagId, ((App.Current as App).ConnectionString));
                QuestionList.ItemsSource = GetQuestions((App.Current as App).ConnectionString, _editedTest.id);
                checkDeleteInput.Text = "";
            }
        }

        public ObservableCollection<Question> GetQuestions(string connectionString, int testId)
        {
            const string getQuestionsQuery = "SELECT test.testnaam, vraag.id, test.id, vraag.vraagTekst FROM test FULL JOIN vraag ON test.id = vraag.testid WHERE test.id = @testId";
            var questions = new ObservableCollection<Question>();
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
                            cmd.CommandText = getQuestionsQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    //make temp question attribute
                                    var question = new Question();
                                    var test = new Test();

                                    //read values from reader
                                    test.testnaam = reader.GetString(0);
                                    question.id = reader.GetInt32(1);
                                    question.testId = reader.GetInt32(2);
                                    question.vraagTekst = reader.GetString(3);

                                    question.test = test;
                                    questions.Add(question);
                                }
                            }
                        }
                    }
                }
                return questions;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine($"Exception: {eSql.Message}");
            }
            return null;
        }

        public void DeleteWidmQuestion(int vraagId, string connectionString)
        {
            const string DeleteQuestionQuery = "delete from vraag where id = @vraagId";
            const string DeleteAllAnswers = "delete from antwoord where vraagId = @vraagId";
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Parameters.Add("@vraagId", SqlDbType.Int).Value = vraagId;
                            cmd.CommandText = DeleteAllAnswers;
                            cmd.ExecuteNonQuery();
                            cmd.CommandText = DeleteQuestionQuery;
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
            this.Frame.Navigate(typeof(ViewTests));
        }

        private void InsertTest_Click(object sender, RoutedEventArgs e)
        {
            if (QuestionTextBox.Text.Trim().Length > 0)
            {
                string conString = (App.Current as App).ConnectionString;
                int testId = _editedTest.id;
                string vraagTekst = QuestionTextBox.Text;
                InsertQuestion(conString, testId, vraagTekst);
                QuestionList.ItemsSource = GetQuestions(conString, testId);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            string EditTestString = EditTextBox.Text;
            bool isNumeric = int.TryParse(EditTestString, out int vraagId);
            if (isNumeric)
            {
                this.Frame.Navigate(typeof(AnswerView), vraagId);
            }
        }

        private void InsertQuestion(string connectionString, int testId, string vraagTesks)
        {
            string InsertQuery = "insert into vraag values(@testId, @vraagTekst)";
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
                            cmd.Parameters.Add("@vraagTekst", SqlDbType.VarChar).Value = vraagTesks;
                            cmd.CommandText = InsertQuery;
                            cmd.ExecuteNonQuery(); 
                            
                            //clearing the textBox
                            QuestionTextBox.Text = "";
                        }
                    }
                }
                
            }
            catch (Exception eSql)
            {
                Debug.WriteLine($"Exception: {eSql.Message}");
            }
        }

        private void onEnterPressDelete(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                string vraagIdString = checkDeleteInput.Text;
                bool isNumeric = int.TryParse(vraagIdString, out int vraagId);
                if (isNumeric)
                {
                    DeleteWidmQuestion(vraagId, ((App.Current as App).ConnectionString));
                    QuestionList.ItemsSource = GetQuestions((App.Current as App).ConnectionString, _editedTest.id);
                    checkDeleteInput.Text = "";
                }
            }
        }

        private void onEnterPressEdit(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                string EditTestString = EditTextBox.Text;
                bool isNumeric = int.TryParse(EditTestString, out int vraagId);
                if (isNumeric)
                {
                    this.Frame.Navigate(typeof(AnswerView), vraagId);
                }
            }

        }
    }
}