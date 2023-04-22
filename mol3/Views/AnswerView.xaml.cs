using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
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
using static System.Net.Mime.MediaTypeNames;

namespace mol3.Views
{
    public sealed partial class AnswerView : Page
    {
        private Question _editedVraag;
        private string _connectionString = (App.Current as App).ConnectionString;
        public AnswerView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is int && (int)e.Parameter > 0)
            {
                int vraagId = (int)e.Parameter;
                _editedVraag = GetSpecificQuestion(_connectionString, vraagId);
                QuestionText.Text = _editedVraag.vraagTekst;
                AnswerList.ItemsSource = getAllAnswers(_connectionString, _editedVraag.id);
            }
        }
        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(EditSpecificTest), _editedVraag.testId);
        }
        private void DeleteTest_Click(object sender, RoutedEventArgs e)
        {
            string antwoordIdString = checkDeleteInput.Text;
            bool isNumeric = int.TryParse(antwoordIdString, out int antwoordId);
            if (isNumeric)
            {
                DeleteAnswer(_connectionString, antwoordId);
                AnswerList.ItemsSource = getAllAnswers(_connectionString, _editedVraag.id);
                checkDeleteInput.Text = "";
            }
        }
        private void InsertAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            if (AnswerTextInput.Text.Trim().Length > 0)
            {
                int correct;
                correct = AnswerCorrectCheckBox.IsChecked == true ? 1 : 0;
                
                string answerString = AnswerTextInput.Text;
                int vraagId = _editedVraag.id;

                InsertAnswer(_connectionString, vraagId, answerString, correct);
                
                AnswerList.ItemsSource = getAllAnswers(_connectionString, vraagId);
            }
        }
        private Question GetSpecificQuestion(string connectionString, int vraagId)
        {
            const string getQuestionQuery = "select id, testid, vraagTekst from vraag where id = @vraagId";
            Question question = new Question();

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
                            cmd.CommandText = getQuestionQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    question.id = reader.GetInt32(0);
                                    question.testId = reader.GetInt32(1);
                                    question.vraagTekst = reader.GetString(2);
                                }
                            }
                        }
                    }
                }
                return question;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine(eSql.Message);
            }
            return null;
        }
        private ObservableCollection<Answer> getAllAnswers(string connectionString, int vraagId)
        {
            const string GetAnswersQuery = "select id, vraagId, antwoordTekst, correct from antwoord where vraagId = @vraagId";
            var answers = new ObservableCollection<Answer>();
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
                            cmd.CommandText = GetAnswersQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Answer answer = new Answer();
                                    answer.id = reader.GetInt32(0);
                                    answer.vraagId = reader.GetInt32(1);
                                    answer.antwoordTekst = reader.GetString(2);

                                    //check value of bit and convert it to boolean
                                    answer.correct = reader.GetBoolean(3);

                                    answers.Add(answer);
                                }
                            }
                        }
                    }
                }
                return answers;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine(eSql);
            }
            return null;
        }
        private void InsertAnswer(string connectionString, int vraagId, string antwoordTekst, int isCorrect)
        {
            const string InsertAnswerQuery = "insert into antwoord values(@vraagId, @antwoordTekst, @correct)";
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
                            cmd.Parameters.Add("@antwoordTekst", SqlDbType.VarChar).Value = antwoordTekst;
                            cmd.Parameters.Add("@correct", SqlDbType.Bit).Value = isCorrect;
                            cmd.CommandText = InsertAnswerQuery;  
                            cmd.ExecuteNonQuery();

                            AnswerTextInput.Text = "";
                            AnswerCorrectCheckBox.IsChecked = false;
                        }
                    }
                }
            }
            catch (Exception eSql)
            {
                Debug.Write(eSql);
            }
        }
        private void DeleteAnswer(string connectionString, int antwoordId)
        {
            const string DeleteAnwerQuery = "delete from antwoord where id=@antwoordId";
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Parameters.Add("@antwoordId", SqlDbType.Int).Value = antwoordId;
                            cmd.CommandText = DeleteAnwerQuery;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception eSql)
            {
                Debug.WriteLine(eSql);
            }
        }
    }
}
