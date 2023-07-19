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
using Windows.Networking;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace mol3.Views
{
    public sealed partial class PersonView : Page
    {
        private string _connectionString = (App.Current as App).ConnectionString;
        
        public PersonView()
        {
            this.InitializeComponent();
            PersonList.ItemsSource = GetPersons(_connectionString);
        }
        private void DeleteTest_Click(object sender, RoutedEventArgs e)
        {
            string personIdString = checkDeleteInput.Text;
            bool isNumeric = int.TryParse(personIdString, out int personId);
            if (isNumeric)
            {
                DeletePerson(personId, _connectionString);
                PersonList.ItemsSource = GetPersons(_connectionString);
                checkDeleteInput.Text = "";
            }
        }
        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AdminView));
        }
        private void InsertPerson_OnClick(object sender, RoutedEventArgs e)
        {
            if (PersonName.Text.Trim().Length > 0)
            {
                int isMol;
                isMol = PersonIsMolCheckBox.IsChecked == true ? 1 : 0;

                string personName = PersonName.Text;

                InsertKanidaat(_connectionString, personName, isMol);

                PersonList.ItemsSource = GetPersons(_connectionString);
            }
        }
        public ObservableCollection<Person> GetPersons(string connectionString)
        {
            const string GetPersonsQuery = "select id, naam, ismol from kanidaat";
            var persons = new ObservableCollection<Person>();
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetPersonsQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var person = new Person();
                                    person.Id = reader.GetInt32(0);
                                    person.Name = reader.GetString(1);
                                    person.isMol = reader.GetBoolean(2);
                                    persons.Add(person);
                                }
                            }
                        }
                    }
                }
                return persons;
            }
            catch (Exception eSql)
            {
                Debug.WriteLine($"Exception: {eSql.Message}");
            }
            return null;
        }
        private void DeletePerson(int personId, string connectionString)
        {
            string DeletePersonQuery = "delete from kanidaat where id = @personId";
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Parameters.Add("@personId", SqlDbType.Int).Value = personId;
                            cmd.CommandText = DeletePersonQuery;
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
        private void InsertKanidaat(string connectionString, string personName, int isMol)
        {
            const string InsertPersonQuery = "insert into kanidaat values(@personName, @isMol)";
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Parameters.Add("@personName", SqlDbType.VarChar).Value = personName;
                            cmd.Parameters.Add("@isMol", SqlDbType.Bit).Value = isMol;
                            cmd.CommandText = InsertPersonQuery;
                            cmd.ExecuteNonQuery();

                            PersonName.Text = "";
                            PersonIsMolCheckBox.IsChecked = false;
                        }
                    }
                }
            }
            catch (Exception eSql)
            {
                Debug.Write(eSql);
            }
        }
        private void onEnterPressDelete(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                string personIdString = checkDeleteInput.Text;
                bool isNumeric = int.TryParse(personIdString, out int personId);
                if (isNumeric)
                {
                    DeletePerson(personId, _connectionString);
                    PersonList.ItemsSource = GetPersons(_connectionString);
                    checkDeleteInput.Text = "";
                }
            }
        }
    }
}
