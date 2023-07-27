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
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace mol3.Views
{
    public sealed partial class MakeTestView : Page
    {

        private Person _testPerson;
        private string _connectionString = (App.Current as App).ConnectionString;
        private List<KanidaatVraag> _kanidaatVragen = new List<KanidaatVraag>();
        private int? _lastMadeTest;
        public MakeTestView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is string)
            {
                string nameTestPerson = (string)e.Parameter;
                _testPerson = getListsAndPerson(nameTestPerson);
                TestPersonDisplay.Text = "Id = " + _testPerson.Id + ", Naam = " + _testPerson.Name + ", is de mol: " + _testPerson.isMol + ", " + _kanidaatVragen.Count;
            }
        }

        private Person getListsAndPerson(string name)
        {
            Person person = new Person();
            const string getTestPersonQuery = "SELECT id, naam, ismol FROM kanidaat WHERE naam = @personsName";
            const string getKanidaatVraag = "SELECT kanidaatid, vraagid, antwoordid, testid FROM kanidaatvraag";
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
                            cmd.CommandText = getKanidaatVraag;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var kanidaatVraag = new KanidaatVraag();
                                    kanidaatVraag.kanidaatId = reader.GetInt32(0);
                                    kanidaatVraag.vraagId = reader.GetInt32(1);
                                    kanidaatVraag.antwoordId = reader.GetInt32(2);
                                    kanidaatVraag.testId = reader.GetInt32(3);
                                    _kanidaatVragen.Add(kanidaatVraag);
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

        private Test getTestVragen()
        {
            if (_lastMadeTest == null)
            {

            }
            return null;
        }
    }
}
