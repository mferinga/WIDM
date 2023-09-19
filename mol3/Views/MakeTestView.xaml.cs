using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace mol3.Views
{
    public sealed partial class MakeTestView : Page
    {

        private Person _testPerson;
        private string _connectionString = (App.Current as App).ConnectionString;
        private int? _lastMadeTest;
        private int currentQuestion;
        private Uri _imagePath = new Uri("ms-appx:///Assets/img/button_img.png");

        private Test _currentTest;
        private List<Question> _vragenList = new List<Question>();
        private List<Button> _buttonList = new List<Button>();

        private string _selectedAnswer;

        public MakeTestView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Payload)
            {
                Payload payload = (Payload)e.Parameter;
                string nameTestPerson = payload.item1;
                currentQuestion = payload.item2;
                _testPerson = getListsAndPerson(nameTestPerson);
                getTestVragen();
                QuestionText.Text = _vragenList[currentQuestion].vraagTekst;
                LoadAnswers();
                
            }
        }

        private void LoadAnswers()
        {
            int i = 0;
            foreach(Answer a in _vragenList[currentQuestion].answers)
            {
                Button b = new Button();
                b.Name = $"{a.antwoordTekst}";
                b.Background = new SolidColorBrush(Windows.UI.Colors.Brown);
                b.Padding = new Thickness(30, 30, 30, 30);
                b.HorizontalAlignment = HorizontalAlignment.Left;
                b.VerticalAlignment = VerticalAlignment.Top;
                b.Click += ChooseAnswer;
                Thickness marginImage = b.Margin;
                marginImage.Top = 200 + (i * 85);
                marginImage.Left = 75;
                b.Margin = marginImage;
                this.gridpanel.Children.Add(b);
                SaveButton(b);

                CheckBox c = new CheckBox();
                c.Name = $"checkbox{i}";
                c.HorizontalAlignment = HorizontalAlignment.Left;
                c.VerticalAlignment = VerticalAlignment.Top;
                Thickness marginc = c.Margin;
                marginc.Top = 200 + (i * 87.5);
                marginc.Left = 77.5;
                c.Margin = marginc;
                c.Opacity = 0;
                this.gridpanel.Children.Add(c);
                

                TextBlock textBlock = new TextBlock();
                textBlock.Text = a.antwoordTekst;
                textBlock.FontSize = 26;
                textBlock.Width = 100;
                textBlock.Height = 100;
                textBlock.HorizontalAlignment = HorizontalAlignment.Left;
                textBlock.VerticalAlignment = VerticalAlignment.Top;
                Thickness marginText = textBlock.Margin;
                marginText.Top = 215 + (i * 85);
                marginText.Left = 175;
                textBlock.Margin = marginText;
                textBlock.Name = $"answer{i}";
                
                this.gridpanel.Children.Add(textBlock);
                i++;
            }
        }

        private void SaveButton(Button b)
        {
            _buttonList.Add(b);
        }

        private void ChooseAnswer(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b.Opacity != 0.5)
            {
                foreach(var c in _buttonList)
                {
                    if(c.Opacity == 0.5) c.Opacity = 1;

                }
            }
            b.Opacity = 0.5;
            this._selectedAnswer = b.Name;
        }

        private Person getListsAndPerson(string name)
        {
            Person person = new Person();
            const string getTestPersonQuery = "SELECT id, naam, ismol FROM kanidaat WHERE naam = @personsName";
            const string getLastMadeTest = "SELECT MAX(testid) AS lastTest FROM kanidaatvraag LEFT JOIN kanidaat ON kanidaatvraag.kanidaatid = kanidaat.id WHERE kanidaat.naam = @personsName";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Parameters.Add("@personsName", SqlDbType.VarChar).Value = name;
                            cmd.CommandText = getTestPersonQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    person.Id = reader.GetInt32(0);
                                    person.Name = reader.GetString(1);
                                    person.isMol = reader.GetBoolean(2);
                                }
                            }
                            cmd.CommandText = getLastMadeTest;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                int lastTest = reader.GetOrdinal("lastTest");
                                while (reader.Read())
                                {
                                    _lastMadeTest = reader.IsDBNull(lastTest) ? (int?)null : reader.GetInt32(0);
                                }
                            }

                        }
                    }
                }

                return person;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine(eSql);
            }

            return null;
        }

        private void getTestVragen()
        {
            if (_lastMadeTest == null)
            {
                getTest(1);
                for (int i = 0; i < _vragenList.Count; i++)
                {
                    int vraagId = _vragenList[i].id;
                    _vragenList[i].answers = getAnswersForQuestion(vraagId);
                }
            }
            else
            {
                int testId = (int)_lastMadeTest + 1;
                getTest(testId);
                for (int i = 0; i < _vragenList.Count; i++)
                {
                    int vraagId = _vragenList[i].id;
                    _vragenList[i].answers = getAnswersForQuestion(vraagId);
                }
                
            }
        }

        private void getTest(int testId)
        {
            const string getTestQuery = "SELECT id, testnaam FROM test WHERE id = @testId";
            const string getAllQuestionForTest = "SELECT id, vraagTekst FROM vraag WHERE testid = @testid";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Parameters.Add("@testId", SqlDbType.Int).Value = testId;
                            cmd.CommandText = getTestQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    _currentTest = new Test
                                    {
                                        id = reader.GetInt32(0),
                                        testnaam = reader.GetString(1)
                                    };
                                }
                            }
                            cmd.CommandText = getAllQuestionForTest;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Question q = new Question();
                                    q.id = reader.GetInt32(0);
                                    q.vraagTekst = reader.GetString(1);
                                    _vragenList.Add(q);
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception eSql)
            {
                Debug.WriteLine(eSql.Message);
            }

        }

        private List<Answer> getAnswersForQuestion(int vraagId)
        {
            List<Answer> answerList = new List<Answer>();
            const string getAnswer = "SELECT id, antwoordTekst, correct FROM antwoord WHERE vraagId = @vraagId";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Parameters.Add("@vraagId", SqlDbType.Int).Value = vraagId;
                            cmd.CommandText = getAnswer;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Answer a = new Answer();
                                    a.id = reader.GetInt32(0);
                                    a.antwoordTekst = reader.GetString(1);
                                    a.correct = reader.GetBoolean(2);
                                    answerList.Add(a);
                                }
                            }
                        }
                    }
                    return answerList;
                }
            }
            catch (Exception eSql)
            {
                Debug.WriteLine(eSql.Message);
            }

            return null;
        }


    }
}
