using System;
using System.Collections.Generic;
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
    public sealed partial class MakeTestView : Page
    {

        private Person _testPerson;
        private string _connectionString = (App.Current as App).ConnectionString;
        public MakeTestView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is string)
            {
                string nameTestPerson = (string)e.Parameter;
                _testPerson = getTestPerson(nameTestPerson, _connectionString);
                TestPersonDisplay.Text = "Id = " + _testPerson.Id + ", Naam = " + _testPerson.Name + ", is de mol: " + _testPerson.isMol;
            }
        }

        private Person getTestPerson(string name, string connectionString)
        {
            Person person = new Person();
            const string getTestPersonQuery = "SELECT id, naam, ismol FROM kanidaat WHERE naam = @personsName";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
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
    }
}
