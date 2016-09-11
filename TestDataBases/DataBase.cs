using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace TestDataBases
{
    public class DataBase
    {
        public string Name { get; }

        public ObservableCollection<UserDataTable> Tables { get; set; }

        public DataBase(string name)
        {
            Name = name;
            Tables = new ObservableCollection<UserDataTable>();
            GetTables();
        }

        public void RefreshTableList()
        {
            Tables.Clear();
            GetTables();
        }
        private void GetTables()
        {
            try
            {
                string query = string.Format("USE {0} ", Name);
                query += "SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_TYPE LIKE '%TABLE%'";
                DataTable table = new DataTable("Tables");

                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString))
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, con);
                    adapter.Fill(table);
                    con.Close();
                }

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    //добавляем названия таблиц
                    string nameTable = table.Rows[i][0].ToString();
                    Tables.Add(new UserDataTable(Name, nameTable));
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Can not get data");
                return;
            }
        }
    }
}
