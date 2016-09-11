using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;

namespace TestDataBases
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<DataBase> DBnames;
        public MainWindow()
        {
            InitializeComponent();
            DBnames = GetDbList();
            treeView.ItemsSource = DBnames;
        }

        public ObservableCollection<DataBase> GetDbList()
        {
            try
            {
                DataTable table = new DataTable("DataBases");
                
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString))
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter("SELECT name FROM sys.databases WHERE database_id > 4", con);
                    adapter.Fill(table);
                    con.Close();
                }

                ObservableCollection<DataBase> listDBs = new ObservableCollection<DataBase>();
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    DataBase db = new DataBase(table.Rows[i][0].ToString());
                    listDBs.Add(db);
                }
                return listDBs;
            }
            catch(Exception)
            {
                return null;
            }
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(e.NewValue is UserDataTable)
            {
                UserDataTable dt = e.NewValue as UserDataTable;
                try
                {
                    DataSet ds = new DataSet();
                    string query = string.Format("USE {0} SELECT TOP 10 * FROM {1};", dt.ParentDBName, dt.Table.TableName);
                    using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString))
                    {
                        con.Open();
                        SqlDataAdapter adapter = new SqlDataAdapter(query, con);
                        adapter.Fill(ds);
                        con.Close();
                    }
                    dataGrid.ItemsSource = ds.Tables[0].DefaultView;
                }
                catch(Exception)
                {
                    MessageBox.Show("Can not get data");
                }
            }
        }

        private void treeView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if(item != null)
            {
                item.Focus();
                if(item.DataContext is UserDataTable)
                {
                    treeView.ContextMenu = (ContextMenu)Resources["ContextMenuDelete"];
                }
                else if(item.DataContext is DataBase)
                {
                    treeView.ContextMenu = (ContextMenu)Resources["ContextMenuCreate"];
                }
            }
        }

        static DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
                source = VisualTreeHelper.GetParent(source);
            return source;
        }

        private void DeleteTable_Click(object sender, RoutedEventArgs e)
        {
            UserDataTable delTable = (UserDataTable)treeView.SelectedItem;
            string query = string.Format("USE {0} DROP TABLE {1}", delTable.ParentDBName, delTable.Table.TableName);
            SimpleSqlComand(query);
            for (int i = 0; i < DBnames.Count; i++)
            {
                if (DBnames[i].Name.Equals(delTable.ParentDBName))
                {
                    DBnames[i].RefreshTableList();
                    break;
                }
            }
            dataGrid.ItemsSource = null;
        }
        private void CreateTable_Click(object sender, RoutedEventArgs e)
        {
            CreateTableForm inputForm = new CreateTableForm();
            inputForm.ShowDialog();
            if (!inputForm.enteredText.Equals(""))
            {
                DataBase currentDB = (DataBase)treeView.SelectedItem;
                string query = string.Format("USE {0} CREATE TABLE _{1} (id int NOT NULL)", currentDB.Name, inputForm.enteredText);
                SimpleSqlComand(query);
                for (int i = 0; i < DBnames.Count; i++)
                {
                    if(DBnames[i].Name.Equals(currentDB.Name))
                    {
                        DBnames[i].RefreshTableList();
                        break;
                    }
                }
            }
            else
            {
                return;
            }
        }

        private void SimpleSqlComand(string query)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString))
                {
                    con.Open();
                    SqlCommand delComand = new SqlCommand(query, con);
                    delComand.ExecuteNonQuery();
                    con.Close();
                }
            }
            catch(Exception)
            {
                MessageBox.Show("Can not execute sql query");
            }
        }
    }
}
