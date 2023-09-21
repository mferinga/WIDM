using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Timers;
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
        private int _currentQuestion;
        private readonly Uri _imagePath = new Uri("ms-appx:///Assets/img/button_img.png");

        private Test _currentTest;
        private List<Question> _vragenList = new List<Question>();
        private List<Button> _buttonList = new List<Button>();
        private List<Answer> _answerList = new List<Answer>();

        private Answer _selectedAnswer;
        private Stopwatch _questionTimer = new Stopwatch();
        private double _timeQuestion;

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
                _currentQuestion = payload.item2;
                _testPerson = getListsAndPerson(nameTestPerson);
                getTestVragen();
                if(_currentQuestion == -1)
                {
                    _currentQuestion = _vragenList.First().id;
                }
                QuestionText.Text = _vragenList[_currentQuestion].vraagTekst;
                LoadAnswers();
                StartTimer();
            }
        }

        private void LoadAnswers()
        {
            int i = 0;
            foreach(Answer a in _answerList.Where(a => a.vraagId == _currentQuestion))
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

        private void StartTimer()
        {
            _questionTimer.Start();
        }

        private void StopTimer()
        {
            _questionTimer.Stop();
            TimeSpan ts = _questionTimer.Elapsed;
            _timeQuestion = ts.TotalMilliseconds;
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
            this._selectedAnswer = this._answerList.First(answer => answer.antwoordTekst.Equals(b.Name));
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
                    getAnswersForQuestion(vraagId);
                    _vragenList[i].answers = _answerList;
                }
            }
            else
            {
                int testId = (int)_lastMadeTest + 1;
                getTest(testId);
                for (int i = 0; i < _vragenList.Count; i++)
                {
                    int vraagId = _vragenList[i].id;
                    getAnswersForQuestion(vraagId);
                    _vragenList[i].answers = _answerList;
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

        private void getAnswersForQuestion(int vraagId)
        {
            const string getAnswer = "SELECT id, vraagid, antwoordTekst, correct FROM antwoord WHERE vraagId = @vraagId";
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
                                    a.vraagId = reader.GetInt32(1);
                                    a.antwoordTekst = reader.GetString(2);
                                    a.correct = reader.GetBoolean(3);
                                    _answerList.Add(a);
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

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            StopTimer();
            const string InsertAnswerQuery = "insert into kanidaatvraag values(@kanidaatid, @vraagid, @antwoordid, @testid, @tijd, @wasCorrect)";
            if(_selectedAnswer != null)
            {
                try
                {
                    using(var  conn = new SqlConnection(_connectionString))
                    {
                        conn.Open() ;
                        if(conn.State == System.Data.ConnectionState.Open)
                        {
                            using(SqlCommand cmd = conn.CreateCommand())
                            {
                                cmd.Parameters.Add("@kanidaatid", SqlDbType.Int).Value = _testPerson.Id;
                                cmd.Parameters.Add("@vraagid", SqlDbType.Int).Value = _currentQuestion;
                                cmd.Parameters.Add("@antwoordid", SqlDbType.Int).Value = _selectedAnswer.id;
                                cmd.Parameters.Add("@testid", SqlDbType.Int).Value = _currentTest.id;
                                cmd.Parameters.Add("@tijd", SqlDbType.Decimal).Value = _timeQuestion;
                                cmd.Parameters.Add("@wasCorrect", SqlDbType.Bit).Value = _selectedAnswer.correct;
                                cmd.CommandText = InsertAnswerQuery;
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
                catch (Exception eSql)
                {
                    Debug.Write(eSql.Message);
                }
            }
            else
            {
                Debug.Write("Er moet een antwoord worden gekozen!");
            }
        }
    }
}
