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
using static System.Net.Mime.MediaTypeNames;

namespace mol3.Views
{
    public sealed partial class AnswerView : Page
    {
        private Question _editedVraag;
        public AnswerView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is int && (int)e.Parameter > 0)
            {
                int vraagId = (int)e.Parameter;
                _editedVraag = GetSpecificQuestion(((App.Current as App).ConnectionString), vraagId);
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
    }
}
